import {DocState, maybeChangeWrappingColumn, rewrap, saveDocState} from './Core'
import {applyEdit, catchErr, docType, docLine} from './Common'
import {TextEditor, commands, window} from 'vscode'
import {getCoreSettings} from './Settings'
import AutoWrap from './AutoWrap'

export {activate, getCoreSettings}

/** Function to activate the extension. */
async function activate(context) {
    const autoWrap = AutoWrap(context.workspaceState)

    // Register the commands
    context.subscriptions.push
        ( commands.registerTextEditorCommand('rewrap.rewrapComment', rewrapCommentCommand)
        , commands.registerTextEditorCommand('rewrap.rewrapCommentAt', rewrapCommentAtCommand)
        , commands.registerTextEditorCommand('rewrap.toggleAutoWrap', autoWrap.editorToggle)
        )

    /** Standard rewrap command */
    function rewrapCommentCommand(editor)
    {
        doWrap(editor).then(() => saveDocState(getDocState(editor)))
    }

    let customWrappingColumn = 0;
    /** Does a rewrap, but first prompts for a custom wrapping column to use. */
    async function rewrapCommentAtCommand(editor)
    {
        let columnStr = customWrappingColumn > 0 ?
            customWrappingColumn.toString() : undefined

        columnStr = await window.showInputBox({
            prompt: "Enter a column number to wrap the selection to. Leave blank to remove wrapping instead.",
            value: columnStr, placeHolder: "",
        })
        if(columnStr === undefined) return // The user pressed cancel

        customWrappingColumn = parseInt(columnStr) || 0
        return doWrap(editor, customWrappingColumn);
    }
}

/** Gets an object representing the state of the document and selections. When a
 *  standard wrap is done, the state is compared with the state after the last
 *  wrap. If they are equal, and there are multiple rulers for the document, the
 *  next ruler is used for wrapping instead. */
const getDocState = (editor: TextEditor) : DocState => {
    const doc = editor.document, selections = editor.selections
    return {filePath: docType(doc).path, version: doc.version, selections}
}

/** Collects the information for a wrap from the editor, passes it to the
 *  wrapping code, and then applies the result to the document. If an edit
 *  is applied, returns an updated DocState object, else returns null.
 *  Takes an optional customColumn to wrap at.
 */
const doWrap = (editor, customColumn?) => {
    const doc = editor.document
    try {
        const docState = getDocState(editor)
        const toCol = cs => !isNaN(customColumn) ?
            customColumn : maybeChangeWrappingColumn(docState, cs)
        let settings = getCoreSettings(editor, toCol)
        const selections = editor.selections

        const edit = rewrap(docType(doc), settings, selections, docLine(doc))
        return applyEdit(editor, edit).then(null, catchErr)
    }
    catch(err) { catchErr(err) }
}
