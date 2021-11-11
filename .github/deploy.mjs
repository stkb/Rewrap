import FS from 'fs'
import Xml2JS from 'xml2js'
import { Octokit } from '@octokit/rest'
import * as Rewrap from '../core/dist/index.js'


//================ General stuff ================

/** Builds a path from root dir */
const fromRoot = (x) => new URL (x, (new URL("..", import.meta.url)))


//================ Getting version numbers ================

const getVersion = async () => {
  const cnt = await FS.promises.readFile (fromRoot ("vscode/package.json"), 'utf8')
  return JSON.parse(cnt).version
}

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
  const settings = {column: 0}
  const selection = {anchor: {line: 0, character: 0}, active: {line:lines.length, character: 0}}
  const docLine = i => lines[i]
  const edit = Rewrap.rewrap(docType, settings, [selection], docLine)
  lines.splice(edit.startLine, edit.endLine - edit.startLine + 1, ...edit.lines)
  return lines.join("\n")
}

const getBetaReleaseText = () => `
**This pre-release version has the following changes:**
${getChangesText ()}

#### Installing ####

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

const github = new Octokit({auth: process.env.GITHUB_PAT}).repos
const ownerRepo = {owner: "stkb", repo: "Rewrap"}

async function uploadGithubBetaRelease (version, vscVsix, vsVsix) {
  let old_release
  try { old_release = await github.getReleaseByTag({tag: "beta", ...ownerRepo}) }
  catch { }
  if (old_release) {
    await github.deleteRelease({release_id: old_release.data.id, ...ownerRepo})
  }

  // Create new release
  const release_opts = {name: "v" + version, tag_name: "beta", prerelease: true, 
                        body: getBetaReleaseText (), ...ownerRepo}
  const release_id = (await github.createRelease(release_opts)).data.id

  // Upload new assets
  const uploadAsset = async ({path, name}) => {
    let data
    try { data = await FS.promises.readFile(path) }
    catch { console.warn("Couldn't read from %s", path); return }

    const headers = {"content-type": "application/vsix", "content-length": data.length}
    return github.uploadReleaseAsset ({headers, release_id, name, data, ...ownerRepo})
  }
  await Promise.all ([uploadAsset (vscVsix), uploadAsset (vsVsix)])
}


//================ Main ================

(async function() {
  const version = await getVersion (), vsVersion = await getVsVersion ()

  uploadGithubBetaRelease
    ( version
    , {name: `Rewrap-VSCode-${version}.vsix`, path: fromRoot (`vscode/Rewrap-VSCode.vsix`)}
    , {name: `Rewrap-VS-${version}_${vsVersion}`, path: fromRoot ("vs/bin/Release/Rewrap-VS.vsix")}
    )
})()
