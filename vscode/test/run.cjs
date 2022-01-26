const {resolve} = require('path')
const {downloadAndUnzipVSCode, runTests} = require('@vscode/test-electron')

async function main() {
  try {
    const cachePath = resolve(__dirname, '../../.obj/vscode-test')
    const extensionDevelopmentPath = resolve(__dirname, '..')
    const extensionTestsPath = resolve(__dirname, 'tests.cjs')
    const workspace = resolve(__dirname, 'fixture')
    const launchArgs = [workspace, '--disable-extensions']

    // Download manually so we can choose the location
    vscodeExecutablePath = await downloadAndUnzipVSCode({cachePath})

    // Run the integration tests
    await runTests({extensionDevelopmentPath, extensionTestsPath, launchArgs, reuseMachineInstall: true, vscodeExecutablePath})
  }
  catch (err) {
    console.error("Failed to run tests")
    console.error(err)
    process.exit(1)
  }
}

main()
