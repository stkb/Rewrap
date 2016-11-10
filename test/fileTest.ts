import * as assert from 'assert'
import { extname, join } from 'path'
import { readFile } from 'mz/fs'

import { 
  Position, Selection, TextDocument, TextEditor, Uri,
  commands, window, workspace
} from 'vscode'

import { wrapSomething } from '../src/Main'

export default fileTest


/** Takes text from an input file and puts it in an editor and wraps it.
 *  Compares it with the contents of another file */
async function fileTest(inputPath: string, expectedPath: string) 
{
  const generatedPath = inputPath + '.generated' + extname(inputPath)
      , uri = Uri.parse('untitled:' + path(generatedPath))

  // Get input text from file and extract selection offsets (marked with '^'
  // characters)
  const inputTextWithSelectionMarkers = await getFileText(inputPath)
      , [inputText, selectionOffsets] = 
          extractSelectionOffsets(inputTextWithSelectionMarkers)

  // Open new editor and paste in input text      
  const doc = await workspace.openTextDocument(uri)
      , editor = await window.showTextDocument(doc)
  await editor.edit(eb => eb.insert(new Position(0, 0), inputText))

  // Set selections and perform Rewrap
  applySelections(editor, selectionOffsets)
  await wrapSomething(editor, 80)

  // Compare wrapped text to expected
  const actualText = normalizeLineEndings(doc.getText())
      , expectedText = normalizeLineEndings(await getFileText(expectedPath))
  assert.equal(actualText, expectedText)
}


/** Takes an array of selection offsets and sets the selections of an editor
 *  from them. */
function applySelections(editor: TextEditor, offsets: number[])
{
  if(offsets.length) {
    editor.selections = offsetsToSelections(editor.document, offsets)
  }
  else {
    editor.selection = 
      new Selection
        ( new Position ( 0, 0 )
        , editor.document.validatePosition
            ( new Position ( Number.MAX_VALUE, Number.MAX_VALUE ) )
        )
  }
}


/** Extracts selection offsets from the given text. The selection starts and
 *  ends are marked by the '^' in the text. */
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


/** Converts a list of offsets for a document into a list of selections */
function offsetsToSelections
  ( doc: TextDocument
  , [fst, snd, ...offsets]: number[]
  ): Selection[] 
{
  return (offsets.length ? offsetsToSelections(doc, offsets) : [])
    .concat(new Selection(doc.positionAt(fst), doc.positionAt(snd)))
}


/** Normalizes line endings of a string to \n */
function normalizeLineEndings(text: string) 
{
  return text.split(/\r?\n/).join('\n')
}


/** Makes a full path from the given file name. */
function path(file: string) {
  return join(__dirname, '../../test/fixture', file) 
}