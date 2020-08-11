const assert = require('assert')
const {window, workspace} = require('vscode')
const getSettings = require('../vscode/compiled/Settings').default

const testSettings = async (language, expected) => {
    const doc = await workspace.openTextDocument({language, content: ""})
    const editor = await window.showTextDocument(doc)
    const actual = getSettings(editor)
    assert.deepStrictEqual(actual, expected)
}

const settingsData = {
    plaintext: {
        column: undefined,
        columns: [45, 90],
        doubleSentenceSpacing: true,
        wholeComment: false,
        reformat: true,
        tabWidth: 2,
    },
    markdown: {
        column: undefined,
        columns: [60],
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
