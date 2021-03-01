const {resolve} = require('path')
const {runTests} = require('vscode-test')

async function main() {
    try {
        const extensionDevelopmentPath = resolve(__dirname, '..')
        const extensionTestsPath = resolve(__dirname, 'tests')
        const workspace = resolve(__dirname, 'fixture')
        const launchArgs = [workspace, '--disable-extensions']

        // Download VS Code, unzip it and run the integration test
        await runTests({extensionDevelopmentPath, extensionTestsPath, launchArgs})
    } catch (err) {
        console.error("Failed to run tests")
        console.error(err)
        process.exit(1)
    }
}

main();
