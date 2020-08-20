const {DocState, Position, Selection} = require('./core/Types')
import {Rewrap, applyEdit, catchErr, docType, docLine} from './Common'
const Rewrap: Rewrap = require('./core/Main')
import * as vscode from 'vscode'
import {commands, window} from 'vscode'
import {getCoreSettings} from './Settings'
import AutoWrap from './AutoWrap'
import fixSelections from './FixSelections'


/** Function to activate the extension. */
exports.activate = async function activate(context)
{
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
        doWrap(editor).then(() => Rewrap.saveDocState(getDocState(editor)))
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

/** Converts a selection-like object to a rewrap Selection object */
const rewrapSelection = s =>
    new Selection
        ( new Position(s.anchor.line, s.anchor.character)
        , new Position(s.active.line, s.active.character)
        )

/** Gets an object representing the state of the document and selections. When a
 *  standard wrap is done, the state is compared with the state after the last
 *  wrap. If they are equal, and there are multiple rulers for the document, the
 *  next ruler is used for wrapping instead. */
const getDocState = editor => {
    const doc = editor.document
    // Conversion of selections is needed for equality operations within Fable
    // code
    return new DocState
        ( docType(doc).path
        , doc.version
        , editor.selections.map(rewrapSelection)
        )
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
            customColumn : Rewrap.maybeChangeWrappingColumn(docState, cs)
        let settings = getCoreSettings(editor, toCol)
        const selections = editor.selections.map(rewrapSelection)

        const edit = Rewrap.rewrap(docType(doc), settings, selections, docLine(doc))
        return applyEdit(editor, edit).then(null, catchErr)
    }
    catch(err) { catchErr(err) }
}
