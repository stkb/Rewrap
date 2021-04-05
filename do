#!/usr/bin/env node

const CP = require('child_process')
const FS = require('fs')

const operations = ['clean', 'build', 'watch', 'test', 'package']
const components = ['core', 'vscode']

// File paths
const corePkg = require ('./core/package.json')
const coreTestDev = 'core/' + corePkg.source
const coreTestProd = 'core/' + corePkg.main
const vscodePkg = require ('./vscode/package.json')
const vscodeMain = 'vscode/' + vscodePkg.main
const vscodeSrc = 'vscode/src'

// Process input args
let args = process.argv.slice(2);
const supplied = (x) => args.includes(x)
const suppliedAny = (...xs) => xs.some(supplied)
const suppliedAll = (...xs) => xs.every(supplied)
const {production, verbose} = processArgs ()


/** Entry point */
function main () {
  // Make sure base (tool) dependencies are installed
  run ("", "dotnet tool restore")
  if (notExists ('node_modules'))
    run ("Installing NPM modules", "npm install")

  // Clean
  if (supplied ('clean')) {
    if (supplied ('core')) rmDir ('core/bin')
    if (supplied ('vscode')) rmDir ('vscode/bin', 'vscode/node_modules')
  }

  // Build Core
  if (supplied ('build')) buildCore ()

  // Test Core
  if (supplied ('test'))
    run ('Running Core tests', `node ${production ? 'core': coreTestDev}`, {showOutput: true})

  // Build VS Code
  if (suppliedAll ('build', 'vscode')) buildVSCode ()

  // Test VS Code
  if (suppliedAll ('test', 'vscode') && process.env.TERM_PROGRAM != 'vscode')
    run ("Running VS Code tests", 'node vscode.test/run')

  // Package up VSIX
  if (supplied ('package'))
    run ('Creating VSIX', npx `vsce package -o Rewrap-VSCode.vsix`, {cwd:'./vscode'})

  // Watch
  if (supplied ('watch')) {
    buildCore ({watch: true})
    if (supplied ('vscode')) {
      runAsync ("TypeScript watching...", npx `tsc -w -p vscode --noEmit`)
      runAsync ("Parcel watching...", npx `parcel watch vscode`)
    }
  }
}


/** Builds the Core JS files */
function buildCore ({watch} = {}) {
    const paj = x => `core/obj/${x}/project.assets.json`
    if (notExists (paj ('core'), paj ('test')))
      run ("Restoring Core dependencies", "dotnet restore core/Core.Test.fsproj")
    
    const fableArgs = 'core/Core.Test.fsproj -o core/bin/Debug/js --noRestore'

    if (outdated (coreTestDev, 'core')) run ("Fable building Core", `dotnet fable ${fableArgs}`)

    if (watch) {
      const cmd = `dotnet fable watch ${fableArgs} --runWatch "node ${coreTestDev}"`
      runAsync ("Fable watching...", cmd)
    }
    else if (production && supplied ('test') && outdated (coreTestProd, coreTestDev))
      run ("Parcel bundling Core", npx `parcel build core`)
}


/** Builds the VS Code extension */
function buildVSCode() {
  if (notExists ('vscode/node_modules'))
    run ("Installing VS Code Extension NPM modules", 'npm install', {cwd:'./vscode'})

  const srcMap = vscodeMain + '.map'
  // Crude way to check if last build was production mode
  const lastBuildProduction = ! FS.existsSync (srcMap)
  if (production == lastBuildProduction && ! outdated (vscodeMain, vscodeSrc, 'core')) return

  run ("Typechecking TypeScript", npx `tsc -p vscode --noEmit`)
  if (production) {
    run ("Linting TypeScript", npx `eslint vscode --ext .ts`)
    run ("Parcel bundling", npx `parcel build vscode --no-source-maps`)
    FS.rmSync (srcMap, {force:true})
  }
  else run ("Parcel bundling", npx `parcel build vscode --no-optimize`)
}


/** If given file/folder(s) don't exist */
const notExists = (...ps) => ps.some(p => !FS.existsSync(p))


/** Runs a command under npx */
const npx = ([cmd], a1 = '') => 'npx --silent ' + cmd + a1


/** Checks if the target is outdated compared with the source(s) */
function outdated (target, ...sources) {
  function lastModified (path) {
    const s = FS.statSync(path, {throwIfNoEntry: false})
    if (!s) return -Infinity
    if (s.isDirectory()) {
      const times =
        FS.readdirSync (path, {encoding: 'utf8', withFileTypes: true})
        . filter(x => x.isFile())
        . map (x => lastModified (path + "/" + x.name))
      return Math.max(...times)
    }
    else return s.mtimeMs
  }

  const sourcesLatestTimestamp = Math.max(...sources.map(lastModified))
  return lastModified (target) <= sourcesLatestTimestamp
}


/** Processes the args given to this script. Returns production and verbose values */
function processArgs () {
  if (suppliedAny ('-pv', '-vp')) args.push('-p', '-v')
  const production =
    suppliedAny ('--production', '-p') || process.env.NODE_ENV == 'production'
  const verbose = suppliedAny ('--verbose', '-v')

  // If not supplied any (valid) options, show help
  if (!(production || suppliedAny (...operations) || suppliedAny (...components)))
    showHelpAndExit ()

  // If no other operations given then 'build' is default

  if (! suppliedAny (...operations)) args.push('build')
  // If no components given then do for all
  if (! suppliedAny (...components)) args.push(...components)
  if (supplied ('test')) args.push('build')
  if (supplied ('package')) args.push('build', 'core', 'vscode')

  return {production, verbose}
}


/** Removes given folder(s) */
function rmDir(...paths) {
  for(let p of paths)
    if (FS.existsSync (p)) run ('Cleaning ' + p, npx `rimraf ${p}`)
}


/** Runs a shell command (sync) */
function run (msg, cmd, {cwd, showOutput} = {}) {
  console.log(msg)
  try {
    const output = CP.execSync(cmd, {encoding: 'utf8', cwd})
    if (showOutput || verbose) console.log(output)
  }
  catch (err) {
    console.error(err.stderr)
    process.exit (1)
  }
}


/** Runs a shell command (async) */
function runAsync (msg, cmd, {cwd, showOutput} = {}) {
  console.log(msg)
  const child = CP.exec(cmd, {encoding: 'utf8', cwd}, onExit)
  if (showOutput || verbose) child.stdout.pipe(process.stdout, {end:false})
  child.stderr.pipe(process.stderr, {end:false})

  function onExit (error) {
    if (error) {
      console.error(error)
      process.exit (1)
    }
  }
}


/** Shows help text and exits. For when invalid args are given */
function showHelpAndExit () {
  const msg = [
    "Usage: ./do (<operation> | <component> | <option>)...",
    `- Operations: ${operations.join(", ")}`,
    `- Components: ${components.join(", ")}`,
    `- Options: --production (-p), --verbose (-v)`,
    "",
    "If no operations are given, does a 'build'. If no components are given, does all components.",
    "Adding the --production flag does the operation in production mode.",
    "",
    "Examples:",
    "    ./do build core",
    "    ./do package --production",
  ]
  console.log('\n' + msg.join('\n') + '\n')
  process.exit(1)
}


// Run main function
main ()
