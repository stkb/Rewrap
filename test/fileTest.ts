import * as assert from 'assert'
import { extname, join } from 'path'
import { readFile } from 'fs'

import { Position, Selection, TextDocument, TextEditor, Uri, window, workspace } from 'vscode'

import rewrapComment from '../src/rewrapComments'


export default fileTest

function fileTest(input: string, expected: string) 
{
  const generated = input + '.generated' + extname(input)

  return getFileText(input)
    .then(extractSelectionOffsets)
    .then(([text, offsets]) => {
      const uri = Uri.parse('untitled:' + path(generated))
      
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
      getFileText(expected)
        .then(expected => [actual, expected])
    )
    .then(([actual, expected]) => {
      assert.equal(actual, expected)
    })
}


function applyEdits
  ( offsets: number[]
  ): (editor: TextEditor) => Thenable<TextDocument>
{
  return function(editor) {
    if(offsets.length) {
      editor.selections = offsetsToSelections(editor.document, offsets)
    }
    else {
      editor.selection = new Selection(0, 0, Number.MAX_VALUE, Number.MAX_VALUE)
    }
    return rewrapComment(editor)
      .then(() => editor.document)
  }
}


function extractSelectionOffsets(text: string): [string, number[]] 
{
  const 
    sections = text.split(/\^/), offsets = []
    
  for(let i = 0, s = 0; i < sections.length - 1; i++) {
    s += sections[i].length
    offsets.push(s)
  }

  return [sections.join(''), offsets]
}


function getFileText(name: string): Thenable<string> {
  return new Promise((resolve, reject) =>
    readFile(path(name), (err, data) =>
      err ?
        reject(err) :
        resolve(
          data.toString()
            .replace(/\r?\n/g, process.platform === 'win32' ? '\r\n' : '\n')
        )
    )
  )
}


function offsetsToSelections
  ( doc: TextDocument
  , [fst, snd, ...offsets]: number[]
  ): Selection[] 
{
  return (offsets.length ? offsetsToSelections(doc, offsets) : [])
    .concat(new Selection(doc.positionAt(fst), doc.positionAt(snd)))
}


function path(file) {
  return join(__dirname, '../../test/fixture', file) 
}