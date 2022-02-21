import {getWrappingColumn, maybeChangeWrappingColumn, rewrap} from './Core'
import {buildEdits, catchErr, docType, docLine, getDocState} from './Common'
import vscode, {TextEditor, TextEditorEdit, commands, window, workspace} from 'vscode'
import {getCoreSettings, getEditorSettings, getOnSaveSetting} from './Settings'
import AutoWrap from './AutoWrap'

export {activate, getCoreSettings, getEditorSettings}

/** Function to activate the extension. */
async function activate (context: vscode.ExtensionContext) {
  const autoWrap = AutoWrap(context.workspaceState, context.subscriptions)

  // Register the commands
  context.subscriptions.push
    ( commands.registerTextEditorCommand('rewrap.rewrapComment', rewrapCommentCommand)
    , commands.registerTextEditorCommand('rewrap.rewrapCommentAt', rewrapCommentAtCommand)
    , commands.registerTextEditorCommand('rewrap.toggleAutoWrap', autoWrap.editorToggle)
    , workspace.onWillSaveTextDocument(onSaveDocument)
    )
}


/** Standard rewrap command */
function rewrapCommentCommand (editor: TextEditor, editBuilder: TextEditorEdit) {
  doWrap(editor, editBuilder)
}


let customWrappingColumn = 0;

/** Does a rewrap, but first prompts for a custom wrapping column to use. */
async function rewrapCommentAtCommand (editor: TextEditor)
{
  let columnStr = customWrappingColumn > 0 ?
    customWrappingColumn.toString() : undefined

  columnStr = await window.showInputBox({
    prompt: "Enter a column number to wrap the selection to. Leave blank to remove wrapping instead.",
    value: columnStr, placeHolder: "",
  })
  if (columnStr === undefined) return // The user pressed cancel

  customWrappingColumn = parseInt(columnStr) || 0
  // Since this is an async function, we have to use editor.edit
  editor.edit (editBuilder => doWrap (editor, editBuilder, customWrappingColumn))
}


function onSaveDocument (e: vscode.TextDocumentWillSaveEvent) {
  if (e.reason !== vscode.TextDocumentSaveReason.Manual) return
  if (! getOnSaveSetting (e.document)) return
  // We need an editor for the tab size. So for now we have to look for it.
  const editor = window.visibleTextEditors.find(ed => ed.document === e.document)
  if (!editor) return

  const file = docType (e.document)
    , settings = getCoreSettings (editor, cs => getWrappingColumn(file.path, cs))
    , edit = rewrap (file, settings, [], docLine(e.document))
    , edits = buildEdits (e.document, edit)

  e.waitUntil(Promise.resolve(edits))
}


/** Collects the information for a wrap from the editor, passes it to the wrapping code,
 *  and then applies the result to the document. If an edit is applied, returns an updated
 *  DocState object, else returns null. Takes an optional customColumn to wrap at.
 */
const doWrap = (editor: TextEditor, editBuilder: TextEditorEdit, customColumn?) => {
  const doc = editor.document
  try {
    const docState = getDocState(editor)
    const toCol = cs => !isNaN(customColumn) ?
      customColumn : maybeChangeWrappingColumn(docState, cs)
    const settings = getCoreSettings(editor, toCol)
    const selections = editor.selections

    const edit = rewrap(docType(doc), settings, selections, docLine(doc))
    buildEdits (doc, edit, editBuilder, isNaN(customColumn))
  }
  catch (err) { catchErr(err) }
}
