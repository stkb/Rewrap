import {maybeChangeWrappingColumn, rewrap} from './Core'
import {buildEdit, catchErr, docType, docLine, getDocState, onDocumentChange} from './Common'
import {TextEditor, commands, window, TextEditorEdit, workspace, TextDocumentChangeEvent} from 'vscode'
import {getCoreSettings, getEditorSettings} from './Settings'
import AutoWrap from './AutoWrap'

export {activate, getCoreSettings, getEditorSettings}

/** Function to activate the extension. */
async function activate(context) {
  const autoWrap = AutoWrap(context.workspaceState)

  // Register the commands
  context.subscriptions.push
    ( commands.registerTextEditorCommand('rewrap.rewrapComment', rewrapCommentCommand)
    , commands.registerTextEditorCommand('rewrap.rewrapCommentAt', rewrapCommentAtCommand)
    , commands.registerTextEditorCommand('rewrap.toggleAutoWrap', autoWrap.editorToggle)
    )

  workspace.onDidChangeTextDocument(onDocumentChange)

  /** Standard rewrap command */
  function rewrapCommentCommand(editor: TextEditor, editBuilder: TextEditorEdit) {
    doWrap(editor, editBuilder)
  }

  let customWrappingColumn = 0;
  /** Does a rewrap, but first prompts for a custom wrapping column to use. */
  async function rewrapCommentAtCommand(editor: TextEditor, editBuilder: TextEditorEdit)
  {
    let columnStr = customWrappingColumn > 0 ?
      customWrappingColumn.toString() : undefined

    columnStr = await window.showInputBox({
      prompt: "Enter a column number to wrap the selection to. Leave blank to remove wrapping instead.",
      value: columnStr, placeHolder: "",
    })
    if(columnStr === undefined) return // The user pressed cancel

    customWrappingColumn = parseInt(columnStr) || 0
    return doWrap(editor, editBuilder, customWrappingColumn);
  }
}

/** Collects the information for a wrap from the editor, passes it to the
 *  wrapping code, and then applies the result to the document. If an edit
 *  is applied, returns an updated DocState object, else returns null.
 *  Takes an optional customColumn to wrap at.
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
    buildEdit (editor, editBuilder, edit, isNaN(customColumn))
  }
  catch(err) { catchErr(err) }
}
