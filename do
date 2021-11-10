#!/usr/bin/env node

const CP = require('child_process')
const FS = require('fs')

const operations = ['clean', 'build', 'watch', 'test', 'package']
const components = ['core', 'vscode']

// File paths
const corePkg = require ('./core/package.json')
const coreProd = 'core/' + corePkg.module
const coreTestPkg = require ('./core/test/package.json')
const coreTestDev = 'core/test/' + coreTestPkg.main
const coreTestProd = 'core/test/' + coreTestPkg.prod
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
    if (supplied ('core')) {
      rmDir ('.obj/.net/core/bin', 'core/dist')
      ;[coreTestProd, coreTestProd + '.map'].forEach(x => FS.rmSync (x, {force:true}))
    }
    if (supplied ('vscode')) rmDir ('vscode/dist', 'vscode/node_modules')
  }

  // Build Core
  if (supplied ('build')) buildCore ()

  // Test Core
  if (supplied ('test')) {
    if (production && outdated (coreTestProd, coreTestDev))
      run ("Parcel bundling Core.Test", parcel `build core/test`)
    run ('Running Core tests', `node core/test${production ? '/prod': ''}`, {showOutput: true})
  }

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
      runAsync ("Parcel watching...", parcel `watch vscode`)
    }
  }
}


/** Builds the Core JS files */
function buildCore ({watch} = {}) {
  const paj = x => `.obj/.net/${x}/project.assets.json`
  if (notExists (paj ('core'), paj ('test')))
    run ("Restoring Core dependencies", "dotnet restore core/Core.Test.fsproj")
  
  const fableArgs = 'core/Core.Test.fsproj -o core/dist/dev --noRestore'

  if (outdated (coreTestDev, 'core')) run ("Fable building Core", `dotnet fable ${fableArgs}`)

  if (watch) {
    const cmd = `dotnet fable watch ${fableArgs} --runWatch "node core/test"`
    runAsync ("Fable watching...", cmd)
  }
  else if (production && outdated (coreProd, 'core'))
    run ("Parcel bundling Core", parcel `build core`)
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
    run ("Parcel bundling", parcel `build vscode --no-source-maps`)
    FS.rmSync (srcMap, {force:true})
  }
  else run ("Parcel bundling VS Code Extension", parcel `build vscode --no-optimize`)
}


/** If given file/folder(s) don't exist */
const notExists = (...ps) => ps.some(p => !FS.existsSync(p))


/** Runs a command under npx */
const npx = (arr1, ...arr2) => {
  for (var cmd = 'npx --silent ', i = 0; i < arr1.length || i < arr2.length; i++) {
    if (arr1[i]) cmd += arr1[i]
    if (arr2[i]) cmd += arr2[i]
  }
  return cmd
}


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


/** Runs Parcel */
const parcel = ([args]) => npx `parcel ${args} --cache-dir .obj/parcel`

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
  if (verbose) {
    console.log(cmd)
    if (cwd) console.log("cwd: " + cwd)
  }
  try {
    const output = CP.execSync(cmd, {encoding: 'utf8', cwd})
    if (showOutput || verbose) console.log(output)
  }
  catch (err) {
    console.error(`Error running: ${cmd}`)
    console.error(err.stderr)
    // dotnet cli writes errors to stdout
    console.error(err.stdout)

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
