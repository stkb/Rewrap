import * as assert from 'assert'
import { extname, join } from 'path'
import { readFile } from 'mz/fs'

import { Position, Selection, Uri, window, workspace } from 'vscode'
import { TextEditor, TextDocument } from './mocks'

import { wrapSomething } from '../src/Main'

export default fileTest


function fileTest(inputPath: string, expectedPath: string) 
{
  return getFileText(inputPath)
    .then(extractSelectionOffsets)
    .then(([text, offsets]) => {
      const editor = new TextEditor(new TextDocument(text, inputPath))
      
      return applyEdits(offsets)(editor)
    })
    .then(document => {
      const actualText = document.getText()
      
      return getFileText(expectedPath)
        .then(expectedText => {
          expectedText = expectedText.split(/\r?\n/).join(document.eol)
          
          assert.equal(actualText, expectedText)
        })
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
    return wrapSomething(editor)
      .then(() => editor.document)
  }
}


function extractSelectionOffsets(text: string): [string, number[]] 
{
  const 
    sections = text.split(/\^/), offsets = [] as number[]
    
  for(let i = 0, s = 0; i < sections.length - 1; i++) {
    s += sections[i].length
    offsets.push(s)
  }

  return [sections.join(''), offsets]
}


function getFileText(name: string): Thenable<string> {
  return readFile(path(name)).then(data => data.toString())
}


function offsetsToSelections
  ( doc: TextDocument
  , [fst, snd, ...offsets]: number[]
  ): Selection[] 
{
  return (offsets.length ? offsetsToSelections(doc, offsets) : [])
    .concat(new Selection(doc.positionAt(fst), doc.positionAt(snd)))
}


function path(file: string) {
  return join(__dirname, '../../test/fixture', file) 
}