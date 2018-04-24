const vscode = require('vscode')
const { Position, Range, Selection, commands, workspace, window } = vscode
const Environment = require('./Environment')
const { adjustSelections } = require('./FixSelections')
const Rewrap = require('./compiled/Core/Types')
const Core = require('./compiled/Core/Main')

/** Function to activate the extension. */
exports.activate = function activate(context)
{
    // Register the commands
    context.subscriptions.push
        ( commands.registerTextEditorCommand
            ( 'rewrap.rewrapComment', rewrapCommentCommand )
        , commands.registerTextEditorCommand
            ( 'rewrap.rewrapCommentAt', rewrapCommentAtCommand )
        , commands.registerCommand
            ( 'rewrap.toggleAutoWrap', toggleAutoWrapCommand )
        )

    /** Standard rewrap command */
    function rewrapCommentCommand(editor) 
    {
        doWrap(editor, Core.rewrap).then(Core.saveDocState)
    }
    
    let customWrappingColumn = 99
    /** Does a rewrap, but first prompts for a custom wrapping column to use. */
    function rewrapCommentAtCommand(editor)
    {
        return getCustomColumn(customWrappingColumn.toString())
            .then(wrapWithCustomColumn)

        // Prompts the user for a value. initialValue {string} may be undefined.
        function getCustomColumn(initialValue)
        {
            return window.showInputBox({
                prompt: "Wrap selected text at this column",
                value: initialValue,
                placeHolder: "Enter a number greater than zero"
            })
                .then(inputStr => {
                    if(inputStr == null) return null
                    else {
                        const inputInt = parseInt(inputStr)
                        return (isNaN(inputInt) || inputInt < 1)
                            ? getCustomColumn(undefined)
                            : inputInt
                    }
                })
        }

        function wrapWithCustomColumn(customColumn) 
        {
            if(!customColumn) return
            doWrap(editor, Core.rewrap, customColumn)
        }
    }

    let changeHook
    if(context.globalState.get('autoWrap')) {
        toggleAutoWrapCommand();
    }

    /** Auto-wrap automatically wraps the current line when space or enter is
     *  pressed after the wrapping column. */
    function toggleAutoWrapCommand()
    {
        if(changeHook) {
            changeHook.dispose()
            changeHook = null
            context.globalState.update('autoWrap', false)
                .then(() => window.setStatusBarMessage("Auto-wrap: Off", 7000))
        }
        else {
            changeHook = workspace.onDidChangeTextDocument(checkChange)
            context.globalState.update('autoWrap', true)
                .then(() => window.setStatusBarMessage("Auto-wrap: On", 7000))
        }
    }
}

/** Catches any error and displays a friendly message to the user. */
const catchErr = err => {
    console.error("====== Rewrap: Error ======")
    console.log(err)
    console.error(
        "^^^^^^ Rewrap: Please report this (with a copy of the above lines) ^^^^^^\n" +
        "at https://github.com/stkb/vscode-rewrap/issues"
    )       
    vscode.window.showInformationMessage(
        "Sorry, there was an error in Rewrap. " +
        "Go to: Help -> Toggle Developer Tools -> Console " +
        "for more information."
    )
}

/** Gets an object representing the state of the document and selections */
const getDocState = (document, selections) => {
    // Conversion of selections is needed for equality operations within
    // Fable code
    return new Rewrap.DocState
        ( document.fileName
        , document.languageId
        , document.version
        , selections.map(vscodeToRewrapSelection)
        )

    function vscodeToRewrapSelection(s) 
    {
        return new Rewrap.Selection
            ( new Rewrap.Position(s.anchor.line, s.anchor.character)
            , new Rewrap.Position(s.active.line, s.active.character)
            )
    }
}

/** Applies an edit to the document. Also fixes the selections afterwards */
const applyEdit = (editor, selections, edit) => {
    const doc = editor.document
    const range = doc.validateRange
            ( new Range(edit.startLine, 0, edit.endLine, Number.MAX_VALUE) )
    const lines = Array.from(new Array(doc.lineCount))
        .map((_, i) => doc.lineAt(i).text)

    // vscode takes care of converting to \r\n if necessary
    const text = edit.lines.join('\n')
    return editor.edit(editBuilder => editBuilder.replace(range, text))
        .then(() => {
            editor.selections =
                adjustSelections(lines, selections, [edit])
            return getDocState(doc, editor.selections)
        })
}

/** Collects the information for a wrap from the editor, passes it to the
 *  wrapping code, and then applies the result to the document. If an edit
 *  is applied, returns an updated DocState object, else returns null.
 */
const doWrap = (editor, wrapFn, customColumn = undefined) => {
    const document = editor.document
    try {
        const docState = getDocState(document, editor.selections)
        let settings = Environment.getSettings(editor)
        settings.column = customColumn
            || Core.getDocWrappingColumn(docState, settings.columns)
        const docLine = i =>
            i < document.lineCount ? document.lineAt(i).text : null

        // Don't call wrapFn in a promise: it causes performance/race
        // conditions when using auto-wrap
        const edit = wrapFn(docState, settings, docLine)
        if(!edit.lines.length) return Promise.resolve()
        
        return applyEdit(editor, docState.selections, edit).then(null, catchErr)
    }
    catch(err) { catchErr(err) }
}

const checkChange = e => {
    // Make sure we're in the active document
    const editor = window.activeTextEditor
    if(!editor || !e || editor.document !== e.document) return
    
    const document = e.document;

    // Haven't come across a case where # of changes is != 1 but if it
    // happens we can't handle it.
    if(e.contentChanges.length != 1) return

    // Trigger only on space or enter pressed
    const lastChange = e.contentChanges[e.contentChanges.length - 1]
    const triggers = [' ', '\n', '\r\n']
    if(!triggers.includes(lastChange.text)) return
    
    // Multiple selections aren't supported atm
    if(editor.selections.length > 1) return
    if(!editor.selection.isEmpty) return

    // Here editor.selection is still at the position it would be before
    // the typed character is added. After the wrap we need to move it
    // the equivalent of a space or enter press.

    // Check if cursor is past the wrapping column
    const pos = editor.selection.active
    if
        ( Core.cursorBeforeWrappingColumn
            ( document.fileName
            , editor.options.tabSize
            , document.lineAt(pos.line).text
            , pos.character
            , () => Environment.getWrappingColumns(editor)[0]
            )
        )
    {
        return
    }

    // If we got this far, do the wrap
    doWrap(editor, Core.autoWrap).then(adjustSelection)

    function adjustSelection(docState) 
    {
        if(!docState) return

        const {line, character} = docState.selections[0].active
        const newPos = lastChange.text == " "
            ? new Position(line, character + 1)
            : new Position(line + 1, 0)
        editor.selection = new Selection(newPos, newPos)
    }
}
