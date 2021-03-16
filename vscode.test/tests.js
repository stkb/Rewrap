const assert = require('assert')
const {window, workspace} = require('vscode')
const {getCoreSettings} = require('../vscode/bin/Extension.js')

const testSettings = async (language, expected) => {
    const doc = await workspace.openTextDocument({language, content: ""})
    const editor = await window.showTextDocument(doc)
    const actual = getCoreSettings(editor, cs => cs[0])
    assert.deepStrictEqual(actual, expected)
}

const settingsData = {
    plaintext: {
        column: 45,
        doubleSentenceSpacing: true,
        wholeComment: false,
        reformat: true,
        tabWidth: 2,
    },
    markdown: {
        column: 60,
        doubleSentenceSpacing: false,
        wholeComment: true,
        reformat: false,
        tabWidth: 6,
    },
}

exports.run = async () => {
    // breaks when run in parallel (Promise.all)
    for([l, e] of Object.entries(settingsData)) { await testSettings(l,e) }
}
