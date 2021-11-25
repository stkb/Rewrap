#!/usr/bin/env sh
":" //; exec node --input-type=module - "$@" < "$0"

import CP from 'child_process'
import FS from 'fs'
import logUpdate from 'log-update'

const operations = {
  'clean': "  removes previous build artifacts",
  'build': "  development build",
  'test': "   runs tests (builds first)",
  'prod': "   production build (also runs tests)",
  'watch': "  runs build & test whenever source files are changed",
  'package': "(implies prod) creates a VSIX package",
  'publish': "(implies clean, package) creates a package and publishes it",
  'version': "Gets current extension version updates to given version"
}
const components = ['core', 'vscode']

// File paths
const corePkg = readJsonFile ('./core/package.json')
const coreProd = 'core/' + corePkg.module
const coreTestPkg = readJsonFile ('./core/test/package.json')
const coreTestDev = 'core/test/' + coreTestPkg.main
const coreTestProd = 'core/test/' + coreTestPkg.prod
const vscodePkg = readJsonFile ('./vscode/package.json')
const vscodeMain = 'vscode/' + vscodePkg.main
const vscodeSrc = 'vscode/src'
const vscodeVsix = '.obj/Rewrap-VSCode.vsix'


//========== PROCESS INPUT ARGS ==========//


let args = process.argv.slice(2);
const supplied = (x) => args.includes(x)
const suppliedAny = (...xs) => xs.some(supplied)
const suppliedAll = (...xs) => xs.every(supplied)

if (!suppliedAny (...Object.keys(operations)) && !suppliedAny (...components)) {
  showHelpAndExit ()
}

const [targetCore, targetVSCode]
  = suppliedAll (...components) ? [true, true]
  : supplied ('core') ? [true, false]
  : supplied ('vscode') ? [false, true]
  : [true, true]
const watch = suppliedAny ('watch')
const verbose = suppliedAny ('--verbose', '-v')
let cleanRun, testsRun


//========== TASKS ==========//


// Prereqs. Make sure base (tool) dependencies are installed
if (verbose) {
  run ('', "dotnet --version")
  run ('', "node --version")
  run ('', "npm --version")
}
run ("", "dotnet tool restore")
if (notExists ('node_modules')) run ("Installing NPM modules", "npm install")

// Run given task
if (supplied ('clean')) clean ()

if (supplied ('version')) getSetVersion ()
else if (supplied ('publish')) publish ()
else if (supplied ('package')) package_ ()
else if (supplied ('prod')) productionBuild ()
else if (supplied ('test')) runTests ()
else if (supplied ('build')) devBuild ()
else if (!supplied ('clean')) devBuild ()


function getSetVersion ()
{
  const pVSC = 'vscode/package.json', reVSC = /"version"\: "(.*?)"/i
  let cnt = FS.readFileSync (pVSC, 'utf8'), newVer = args.find(s => s.startsWith("1."))
  if (!newVer) exit (0, cnt.match(reVSC)[1])

  let parts = newVer.split(/[-.]/)
  if (parts[3] == 'alpha' || parts[3] == 'beta') {
    parts[3] = parseInt(parts[4]) + (parts[3] == 'alpha' ? 100 : 200)
  }
  else if (parts[3] == null) {
    parts[3] = 300

    for (let p of ['README.md', 'vscode/README.md'])
      replaceInFile(p, /version: (.+?)\*/, newVer)

    let pre = parts[2] == "0" ? "## " : "### "
    replaceInFile('CHANGELOG.md', /(## Unreleased)/, pre + newVer)
  }
  else exit (1, "Invalid version")

  replaceInFile (pVSC, reVSC, newVer)

  const vsVer = parts.slice(0, 4).join('.')
  replaceInFile ('vs/source.extension.vsixmanifest', /" Version="(.*?)"/, vsVer)

  run ("", 'npm install', {cwd:'./vscode'})
  console.log ("Version changed to " + newVer)

  function replaceInFile (path, re, rep) {
    const cnt = FS.readFileSync (path, 'utf8'), match = cnt.match(re)
    if (!match) return;
    FS.writeFileSync (path, cnt.replace(match[0], match[0].replace(match[1], rep)))
  }
}


function publish () {
  let stats
  try { stats = FS.statSync(vscodeVsix) } catch {
    exit (1, `No VSIX file at ${vscodeVsix}. Run './do package' and test the VSIX file first.`)
  }
  if(stats.mtimeMs < (new Date()).valueOf() - 600000)
    exit (1, `${vscodeVsix} file is older than 10 mins. Recreate it with: ./do package`)

    run ("Publishing to OpenVSX", `ovsx publish ${vscodeVsix} -p ${process.env.OVSX_PAT}`, {showOutput: true})
    run ("Publishing to VS Code", `vsce publish -i ${vscodeVsix}`, {showOutput: true})
    log ("Published!")
}

function package_ () {
  clean ()
  productionBuild ()
  run ("Creating VSIX", `vsce package -o ../${vscodeVsix} 2>&1`, {cwd:'./vscode'})
  log (`${vscodeVsix} created.`)
}

function clean () {
  if (cleanRun) { return } else { cleanRun = true }

  if (targetCore) {
    removeDirs ('.obj/.net/core/bin', 'core/dist')
    removeFiles (coreTestProd, coreTestProd + '.map')
  }
  if (targetVSCode) removeDirs ('vscode/dist', 'vscode/node_modules')
  log ("Cleaned.")
}

function productionBuild () {
  runTests ({production: true})
}

function runTests ({production} = {}) {
  if (testsRun) { return } else { testsRun = true }

  buildCore ({production})
  if (targetCore) {
    if (production && outdated (coreTestProd, coreTestDev))
      run ("Bundling Core tests with Parcel", parcel `build core/test`)
    const msg = 'Core build complete. Running tests:'
    run (msg, `node core/test${production ? '/prod': ''}`, {showOutput: true})
  }

  if (targetVSCode) {
    buildVSCode ({production})
    if (process.env.TERM_PROGRAM == 'vscode') log ("Can't run VS Code tests inside VS Code")
    else run ("Running VS Code tests", 'node vscode.test/run.cjs')
  }
}

function devBuild () {
  buildCore ()
  if (targetVSCode) buildVSCode()
  if (!watch) log ("Dev build complete.")
}


//========== STEPS ==========//


function buildCore ({production} = {}) {
  const paj = x => `.obj/.net/${x}/project.assets.json`
  if (notExists (paj ('core'), paj ('test')))
    run ("Restoring dependencies", "dotnet restore core/Core.Test.fsproj")

  const fableArgs = 'core/Core.Test.fsproj -o core/dist/dev --noRestore'

  if (watch) {
    const cmd = `dotnet fable watch ${fableArgs} --runWatch "node core/test"`
    runAsync ("Fable watching...", cmd)
  }
  else {
    if (outdated (coreTestDev, 'core')) {
      const output = run ("Building with Fable", `dotnet fable ${fableArgs}`)
      const warnings = output.split(/\r?\n/).filter(s => s.includes("warning"))
      if (warnings.length) {
        warnings.forEach(w => console.error (w))
        exit (1)
      }
    }
    if (production && outdated (coreProd, 'core')) run ("Parcel bundling Core", parcel `build core`)
  }
}

function buildVSCode({production} = {}) {
  if (notExists ('vscode/node_modules'))
    run ("Installing NPM modules", 'npm install', {cwd:'./vscode'})

  const srcMap = vscodeMain + '.map'
  // Crude way to check if last build was production mode
  const lastBuildProduction = ! FS.existsSync (srcMap)
  if (!!production == lastBuildProduction && ! outdated (vscodeMain, vscodeSrc, 'core')) return

  if (watch) {
    runAsync ("TypeScript watching...", npx `tsc -w -p vscode --noEmit`)
    runAsync ("Parcel watching...", parcel `watch vscode`)
  }
  else {
    run ("Typechecking TypeScript", npx `tsc -p vscode --noEmit`)
    if (production) {
      run ("Linting TypeScript", npx `eslint vscode --ext .ts`)
      run ("Bundling with Parcel", parcel `build vscode --no-source-maps`)
      FS.rmSync (srcMap, {force:true})
    }
    else run ("Parcel bundling VS Code Extension", parcel `build vscode --no-optimize`)
  }
}


//========== HELPER FUNCTIONS ==========//


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

/** If given file/folder(s) don't exist */
function notExists (...ps) { return ps.some(p => !FS.existsSync(p)) }

/** Removes given folder(s) */
function removeDirs(...paths) {
  for(let p of paths)
    if (FS.existsSync (p)) run (`Removing dir ${p}`, npx `rimraf ${p}`)
}

/** Removes given file(s) */
function removeFiles (...paths) {
  paths.filter (p => FS.existsSync (p))
    .map (p => { log (`Removing file ${p}`); FS.rmSync (p, {force:true}) })
}

function readJsonFile (p) {
  return JSON.parse(FS.readFileSync(p, 'utf8'))
}


/** Runs a shell command (sync) */
function run (msg, cmd, {cwd, showOutput} = {}) {
  if (msg) log (msg)
  if (verbose) { log (cmd); if (cwd) log("cwd: " + cwd) }

  try {
    const output = CP.execSync(cmd, {encoding: 'utf8', cwd}).trimEnd()
    if (showOutput || verbose) console.log (output)
    return output
  }
  catch (err) {
    console.error (`Error running: ${cmd}`)
    console.error (err.stderr)
    // dotnet cli writes errors to stdout
    console.error (err.stdout)
    process.exit (1)
  }
}

/** Runs a shell command (async) */
function runAsync (msg, cmd, {cwd, showOutput} = {}) {
  if (msg) console.log (msg)
  const child = CP.exec(cmd, {encoding: 'utf8', cwd}, onExit)
  if (showOutput || verbose) child.stdout.pipe(process.stdout, {end:false})
  child.stderr.pipe(process.stderr, {end:false})

  function onExit (error) { if (error) exit (1, error) }
}


function log (msg) {
  verbose ? console.log (msg) : logUpdate(msg)
}

function exit (code, msg) {
  if (msg) { code == 0 ? console.log(msg) : console.error (msg) }
  process.exit (code)
}

/** Builds parcel command */
function parcel ([args]) { return npx `parcel ${args} --cache-dir .obj/parcel` }

/** Builds npx command */
function npx (arr1, ...arr2) {
  for (var cmd = 'npx --silent ', i = 0; i < arr1.length || i < arr2.length; i++) {
    if (arr1[i]) cmd += arr1[i]
    if (arr2[i]) cmd += arr2[i]
  }
  return cmd
}


/** Shows help text and exits. For when invalid args are given */
function showHelpAndExit () {
  const opStrs = Object.entries(operations).map(([k,v]) => `${k}   ${v}`).join("\n    ")
  const msg = [
    "Usage: ./do (<operation> | <component> | <option>)...",
    "- Operations:\n    " + opStrs,
    `- Components: ${components.join(", ")}`,
    `- Options: --verbose (-v)`,
    "",
    "If no operations are given, does a 'build'. If no components are given,",
    "does all components. Always does builds as necessary.",
    "",
    "Examples:",
    "    ./do build core",
    "    ./do package",
  ]
  console.log('\n' + msg.join('\n') + '\n')
  process.exit(1)
}
