const Path = require ("path")
const FS = require ("fs")
const Xml2JS = require ("xml2js")
const { Octokit } = require ("@octokit/rest")
const Rewrap = require('root-require')('./vscode/compiled/Main')
const { Position, Selection } = require('root-require')('./vscode/compiled/Types')


//================ General stuff ================

/** Builds a path from root dir */
const fromRoot = (...args) => Path.resolve (__dirname, "..", ...args)


//================ Getting version numbers ================

const getVersion = () => require('root-require')('package.json').version
const getVsVersion = () =>
  FS.promises.readFile (fromRoot ("vs/source.extension.vsixmanifest"), 'utf8')
    . then (Xml2JS.parseStringPromise)
    . then (x => x.PackageManifest.Metadata[0].Identity[0].$.Version)


//================ Getting release text ================

const getChangesText = () => {
  const wholeFile = FS.readFileSync (fromRoot ("CHANGELOG.md"), 'utf8')
  let lines = wholeFile.split("\n")
  lines = lines.slice(lines.findIndex(s => s.startsWith("##")) + 1)
  lines = lines.slice(0, lines.findIndex(s => s.startsWith("##")))
  
  // 'Unwrap' for github markdown
  const docType = {path: "", language: "markdown", getMarkers: () => null}
  const settings = { column: 0 }
  const selections = [new Selection(new Position(0, 0), new Position(999,999))]
  const docLine = i => lines[i]
  const edit = Rewrap.rewrap(docType, settings, selections, docLine)
  lines.splice(edit.startLine, edit.endLine - edit.startLine + 1, ...edit.lines)
  return lines.join("\n")
}

const getBetaReleaseText = () => `
This pre-release version has the following changes:
${getChangesText ()}

### Beta versions ###

You can test pre-release (beta) versions by downloading and installing the \
.vsix file from the "Assets" section below. Be sure to choose the correct \
version (**VSCode** or **VS**).

- **VSCode**: press \`F1\` and run the command \`Extensions: Install from \
  VSIX...\`. Choose the downloaded .vsix file. You'll be prompted to \
  restart/reload.

- **Visual Studio**: Run the downloaded .vsix file

You can stick with the beta version until the final version is released to \
the marketplace. When this happens, you'll be automatically upgraded to the \
released version, and you'll be back to stable versions.

(The version number for the VS version is different because Visual Studio \
extensions don't support pre-release version numbers. But they are the same \
version.)
`


//================ Uploading to GitHub ================

const github = new Octokit({
  auth: process.env.GITHUB_PAT
}).repos
const ownerRepo = {owner: "stkb", repo: "Rewrap"}

async function uploadGithubBetaRelease (version, vscVsix, vsVsix) {
  const release_id = (await github.getReleaseByTag({tag: "beta", ...ownerRepo})).data.id

  // Delete old assets
  const assets = (await github.listReleaseAssets ({release_id, ...ownerRepo})).data
  await Promise.all (assets.map (a => github.deleteReleaseAsset({asset_id: a.id, ...ownerRepo})))

  // Upload new assets
  const uploadAsset = async ({path, name}) => {
    const data = await FS.promises.readFile(path)
    const headers = {"content-type": "application/vsix", "content-length": data.length}
    return github.uploadReleaseAsset ({headers, release_id, name, data, ...ownerRepo})
  }
  await Promise.all ([uploadAsset (vscVsix), uploadAsset (vsVsix)])

  // Update name & text body
  return github.updateRelease
    ({release_id, name: "v" + version, body: getBetaReleaseText (), ...ownerRepo})
}


//================ Main ================

(async function() {
  const version = getVersion (), vsVersion = await getVsVersion ()

  uploadGithubBetaRelease
    ( version
    , {name: `Rewrap-VSCode-${version}.vsix`, path: fromRoot (`rewrap-${version}.vsix`)}
    , {name: `Rewrap-VS-${version}_${vsVersion}`, path: fromRoot ("vs/bin/Release/Rewrap-VS.vsix")}
    )
})()
