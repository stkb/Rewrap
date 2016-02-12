import * as assert from 'assert'
import { join } from 'path'
import { readFile, readdirSync } from 'fs'

import { commands, Position, Selection, TextDocument, TextEditor, Uri, window, workspace } from 'vscode'
import rewrapComment from '../src/rewrapComments'

const path = (file) => join(__dirname, '../../test/fixture', file)

suite("Wrapping", () => {
  
  const dataFiles = readdirSync(path('.'))
    .filter(name => name.startsWith('data.'))
  
  dataFiles
    .map(name => name.split('.')[1])
    .forEach(doTest)
});

function doTest(ext: string) {
  test(ext, function() {
    return getFileText('data.' + ext)
      .then(extractSelectionOffsets)
      .then(([text, offsets]) => {
        const uri = Uri.parse('untitled:' + path('generated.' + ext))
        return workspace.openTextDocument(uri)
          .then(window.showTextDocument)
          .then(editor =>
              editor.edit(eb => eb.insert(new Position(0, 0), text))
                .then(() => editor)
          )
          .then(applyEdits(offsets))
      })
      .then(doc => doc.getText())
      .then(actual =>
        getFileText('expected.80.' + ext)
          .then(expected => [actual, expected])
      )
      .then(([actual, expected]) => {
        assert.equal(actual, expected)
      })
  })
}

const applyEdits =
  (offsets: number[]) =>
  (editor: TextEditor) :
  Thenable<TextDocument> =>
{
  if(offsets.length) {
    editor.selections = offsetsToSelections(editor.document, offsets)
    return rewrapComment(editor)
      .then(() => editor.document)
  }
  else {
    return Promise.resolve(editor.document)
  }
}

const getFileText = (name: string): Thenable<string> =>
  new Promise((resolve, reject) =>
    readFile(path(name), (err, data) =>
      err 
        ? reject(err) 
        : resolve(
            data.toString()
              .replace(/\r?\n/g, process.platform === 'win32' ? '\r\n' : '\n')
          )
    )
  )

const extractSelectionOffsets =
  (text: string) :
  [string, number[]] =>
{
  const sections = text.split(/\^/), offsets = []
  for(let i = 0, s = 0; i < sections.length - 1; i++) {
    s += sections[i].length
    offsets.push(s)
  }
  
  return [sections.join(''), offsets]
}

const offsetsToSelections =
  (doc: TextDocument, [fst, snd, ...offsets]: number[]) :
  Selection[] =>
    (offsets.length ? offsetsToSelections(doc, offsets): [])
      .concat(new Selection(doc.positionAt(fst), doc.positionAt(snd)))
