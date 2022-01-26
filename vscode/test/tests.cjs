const assert = require('assert')
const {commands, window, workspace} = require('vscode')
const {getCoreSettings} = require('../dist/Extension.js')

const testSettings = async (language, expected) => {
  const doc = await workspace.openTextDocument({language, content: ""})
  const editor = await window.showTextDocument(doc)
  const actual = getCoreSettings(editor, cs => cs[0])
  assert.deepStrictEqual(actual, expected)
  await commands.executeCommand("workbench.action.closeActiveEditor")
}

const expectedSettings = {
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

const testWrapping = async (language, content, expected) => {
  const doc = await workspace.openTextDocument({language, content})
  await window.showTextDocument(doc)
  await commands.executeCommand("editor.action.selectAll")
  await commands.executeCommand("rewrap.rewrapComment")
  assert.equal(doc.getText(), expected)
  await commands.executeCommand("workbench.action.closeActiveEditor")
}

exports.run = async () => {
  // Settings tests
  // breaks when run in parallel (Promise.all)
  for([l, e] of Object.entries(expectedSettings)) { await testSettings(l,e) }

  // Wrapping tests
  await testWrapping ("javascript", "// a\n// b", "// a b")
}
