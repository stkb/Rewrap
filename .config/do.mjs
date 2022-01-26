import CP from 'child_process'
import FS from 'fs'
import { setTimeout } from 'timers/promises'
import { Octokit } from '@octokit/rest'
import logUpdate from 'log-update'


const operations = {
  'clean': "      removes previous build artifacts",
  'build': "      development build",
  'test': "       runs tests (builds first)",
  'prod': "       production build (also runs tests)",
  'watch': "      runs build & test whenever source files are changed",
  'package': "    (implies prod) creates a VSIX package",
  'prepublish': " (implies clean, prod) starts steps to publish a release",
  'publish': "    (implies package) creates a package and publishes it",
  'version': "    Gets current extension version updates to given version",
}
const components = ['core', 'vscode']

// File paths (these are relative to cwd not this file)
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


/** Main function (invoked at end of file) */
async function main () {
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
  else if (supplied ('prepublish')) prepublish ()
  else if (supplied ('package')) package_ ()
  else if (supplied ('prod')) productionBuild ()
  else if (supplied ('test')) runTests ()
  else if (supplied ('build')) devBuild ()
  else if (!supplied ('clean')) devBuild ()
}


//========== TASKS ==========//


function getSetVersion () {
  const newVerStr = args.find(s => s.startsWith("1")) // Hacky
  if (!newVerStr) exit (0, Version.fromVSC().toString())
  else Version.fromString(newVerStr).applyToFiles()
}

async function publish () {
  const expectedDiff =
    'CHANGELOG.md|vs/source.extension.vsixmanifest|vscode/package-lock.json|vscode/package.json'
  if (run("", 'git diff --name-only').replaceAll(/\r?\n/g, '|') !== expectedDiff)
    exit (1, "Unexpected changed files. Run ./do prepublish")

  const ver = Version.fromVSC ()
  const changesText = await readChangesText()
  const m = changesText.match(/^#+ ([\d.]+)/), clVer = m && m[1]
  if (ver.toString() != clVer)
    exit (1, `Newest entry in changelog doesn't match version ${ver}`)

  package_ ()

  console.log (`Will really publish v${ver} (${ver.isStable ? 'stable' : 'pre-release'}) in 10 seconds! Press Ctrl+C to cancel`)
  await setTimeout (10000)

  if (ver.isStable) {
    // do some other checks; not sure
  }
  else {
    run ("", `git commit -am "v${ver}"`)
    run ("", 'git tag beta --force && git push --atomic --force origin prerelease beta')
    log ("Publishing GitHub release")
    let body = changesText + (await readFile ('docs/prerelease-versions.md'))
    await githubRelease (await unwrapText (body))
  }

  const pre = ver.isStable ? "" : "--pre-release"
  run ("Publishing to VS Code", npx `vsce publish ${pre} -i ${vscodeVsix}`, {showOutput: true})
  if (ver.isStable)
    run ("Publishing to OpenVSX", `ovsx publish ${vscodeVsix} -p ${process.env.OVSX_PAT}`, {showOutput: true})
  log ("Published!")
}

async function prepublish () {
  const dirty = run ("Checking working directory status", 'git status --porcelain')
  if (dirty) exit (1, "Working directory must be clean before prepublish")

  clean ()
  productionBuild ()

  let ver = Version.fromVSC ()
  if (ver.isStable) {
    run ("", 'git checkout prerelease -- CHANGELOG.md && git reset') // Reset to unstage changelog
  }
  else {
    run ("", 'git cherry-pick beta && git reset --mixed HEAD~1') // Avoid using caret char
  }
  ver = Version.fromVSC().bumpMinor()
  ver.applyToFiles ()
  const msg =
    "Next steps:\n- Build VS version\n- Make changes to changelog\nThen run ./do publish"
  exit (0, msg)
}

function package_ () {
  productionBuild ()
  const pre = Version.fromVSC().isStable ? "" : "--pre-release"
  run ("Creating VSIX", npx `vsce package ${pre} -o ../${vscodeVsix}`, {cwd:'./vscode'})
  log (`VSIX file created at ${vscodeVsix}`)
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
    else run ("Running VS Code tests", 'node vscode/test/run.cjs')
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

function createVSCReadme () {
  const ver = Version.fromVSC ()
  let cnt = FS.readFileSync ('README.md', 'utf8')
  cnt = cnt.replaceAll('.svg', '.png')
  if (!ver.isStable) cnt = cnt.replace (RegExp (ver.major + ".x"), ver)
  FS.writeFileSync ('vscode/README.md', cnt)
}

async function githubRelease (body) {
  const github = new Octokit({auth: process.env.GITHUB_PAT}).repos
  const ownerRepo = {owner: "stkb", repo: "Rewrap"}
  const ver = Version.fromVSC ()

  let old_release
  try { old_release = await github.getReleaseByTag({tag: "beta", ...ownerRepo}) }
  catch { }
  if (old_release) {
    await github.deleteRelease({release_id: old_release.data.id, ...ownerRepo})
  }

  // Create new release
  const release_opts = {name: "v" + ver, tag_name: "beta", prerelease: true, body, ...ownerRepo}
  const release_id = (await github.createRelease(release_opts)).data.id

  // Upload new assets
  const uploadAsset = async ({path, name}) => {
    let data
    try { data = await FS.promises.readFile(path) }
    catch { console.warn("Couldn't read from %s", path); return }

    const headers = {"content-type": "application/vsix", "content-length": data.length}
    return github.uploadReleaseAsset ({headers, release_id, name, data, ...ownerRepo})
  }
  const vsVsix = {name: `Rewrap-VS-${ver}.vsix`, path: 'vs/bin/Release/Rewrap-VS.vsix'}
  await Promise.all ([uploadAsset (vsVsix)])
}

const readChangesText = async () =>
  (await readFile ('CHANGELOG.md')).match(/^#{2,4} .*?(?=^###? )/ms)[0]


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

const readFile = async (p) => FS.promises.readFile (p, 'utf8')

function readJsonFile (p) {
  return JSON.parse(FS.readFileSync(p, 'utf8'))
}

async function unwrapText (text) {
  const Rewrap = await import('../core/dist/index.js')
  const docType = {path: "", language: "markdown", getMarkers: () => null}
  const lines = text.split(/\r?\n/), settings = {column: 0}, docLine = i => lines[i]
  const edit = Rewrap.rewrap(docType, settings, [], docLine)
  lines.splice(edit.startLine, edit.endLine - edit.startLine + 1, ...edit.lines)
  return lines.join("\n")
}

/** Runs a shell command (sync) */
function run (msg, cmd, {cwd, showOutput} = {}) {
  if (msg) log (msg + "...")
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


class Version {
  constructor (major, minor) {
    this.major = major
    this.minor = minor
    this.isStable = major === 1 || major % 2 === 0
  }

  static #path = 'vscode/package.json'
  static #re = /"version"\: "(.*?)"/i

  static fromVSC () {
    let cnt = FS.readFileSync (Version.#path, 'utf8')
    return Version.fromString (cnt.match(Version.#re)[1])
  }

  static fromString (s) {
    let parts = s.split(".")
    if (parts[0] == "1") parts = parts.slice(1)
    return new Version (parseInt(parts[0]), parseInt(parts[1]))
  }

  applyToFiles () {
    replaceInFile (Version.#path, Version.#re, this.toString(3))
    replaceInFile ('vs/source.extension.vsixmanifest', /" Version="(.*?)"/, this.toString(4))
    if (this.isStable) replaceInFile('README.md', /version <b>(.+?)</i, this.toString())
    else replaceInFile('README.md', /pre-release <b>(.+?)</i, this.major + ".x")
    createVSCReadme ()

    run ("", 'npm install', {cwd:'./vscode'})

    function replaceInFile (path, re, rep) {
      const cnt = FS.readFileSync (path, 'utf8'), match = cnt.match(re)
      if (!match) return;
      FS.writeFileSync (path, cnt.replace(match[0], match[0].replace(match[1], rep)))
    }

  }

  bumpMajor () { return new Version (this.major + 1, 0) }
  bumpMinor () { return new Version (this.major, this.minor + 1) }

  toString (places = 0) {
    let [s, remainingPlaces] = this.major < 17 ? ["1.", places-3] : ["", places-2]
    s += this.major + "." + this.minor
    while (remainingPlaces-- > 0) s += ".0"
    return s
  }
}


main ()
