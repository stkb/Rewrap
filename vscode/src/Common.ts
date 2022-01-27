import {DocState, DocType, Edit, saveDocState} from './Core'
import {Range, Selection, TextDocument, TextDocumentChangeEvent, TextEditor, TextEditorEdit, window} from 'vscode'
import fixSelections from './FixSelections'
import GetCustomMarkers from './CustomLanguage'
const getCustomMarkers = GetCustomMarkers()


/** Converts a selection-like object to a vscode Selection object */
const vscodeSelection = (s: Selection) =>
  new Selection(s.anchor.line, s.anchor.character, s.active.line, s.active.character)

/** Gets the range for the whole document */
const getDocRange = (doc: TextDocument) => doc.validateRange
    (new Range(0, 0, Number.MAX_SAFE_INTEGER, Number.MAX_SAFE_INTEGER))

/** Gets an object representing the state of the document and selections. When a standard
 *  wrap is done, the state is compared with the state after the last wrap. If they are
 *  equal, and there are multiple rulers for the document, the next ruler is used for
 *  wrapping instead. */
export const getDocState = (editor: TextEditor) : DocState => {
  const doc = editor.document, selections = editor.selections
  return {filePath: docType(doc).path, version: doc.version, selections}
}

/** Builds the vscode edits that apply an Edit to the document. Also sets the needed data
 *  to fix the selections afterwards. If the edit is empty this is a no-op */
export function buildEdit
  (editor: TextEditor, editBuilder: TextEditorEdit, edit: Edit, saveState: boolean) : void
{
  if (edit.isEmpty) return

  const selections = edit.selections.map(vscodeSelection)
  const doc = editor.document
  const oldLines = Array(edit.endLine - edit.startLine + 1).fill(null)
    .map((_, i) => doc.lineAt(edit.startLine + i).text)
  const wholeDocSelected = selections[0].isEqual (getDocRange (doc))
  const range =
    doc.validateRange (new Range(edit.startLine, 0, edit.endLine, Number.MAX_VALUE))
  // vscode takes care of converting to \r\n if necessary

  editBuilder.replace (range, edit.lines.join('\n'))
  fixSelectionsData =
    {editor, oldLines, edit, selections: (wholeDocSelected ? [] : selections), saveState}
}

/** Catches any error and displays a friendly message to the user. */
export function catchErr (err) {
  console.error("====== Rewrap: Error ======")
  console.log(err)
  console.error(
    "^^^^^^ Rewrap: Please report this (with a copy of the above lines) ^^^^^^\n" +
    "at https://github.com/stkb/vscode-rewrap/issues"
  )
  window.showInformationMessage(
    "Sorry, there was an error in Rewrap. " +
    "Go to: Help -> Toggle Developer Tools -> Console " +
    "for more information."
  )
}

/** Returns a function for the given document, that gets the line at the given index. */
export function docLine (document: TextDocument) {
  return (i: number) => i < document.lineCount ? document.lineAt(i).text : null
}

/** Gets the path and language of the document. These are used to determine the parser
 *  used for it. */
export function docType (document: TextDocument): DocType {
    const path = document.fileName, language = document.languageId
    return {path, language, getMarkers: () => getCustomMarkers(language)}
}

type FixSelectionsData =
  { editor: TextEditor, oldLines: string[], selections: Selection[],
    edit: Edit, saveState: boolean }
let fixSelectionsData : FixSelectionsData | undefined

export function onDocumentChange (e: TextDocumentChangeEvent) {
  if (! fixSelectionsData) return
  const {editor, oldLines, selections, edit, saveState} = fixSelectionsData
  if (editor !== window.activeTextEditor) { fixSelectionsData = undefined; return }
  if (e.document !== editor.document) return
  fixSelectionsData = undefined

  if (selections.length === 0) {
    const wholeRange = getDocRange (editor.document)
    editor.selection = new Selection (wholeRange.start, wholeRange.end)
  }
  else editor.selections = fixSelections (oldLines, selections, edit)

  if (saveState) saveDocState (getDocState (editor))
}
