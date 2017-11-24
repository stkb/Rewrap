const Rewrap = require('./compiled/Core/Types')
const Core = require('./compiled/Core/Main')

/** Function to activate the extension. */
exports.activate = function activate(context)
{
    const vscode = require('vscode')
    const { Position, Range, Selection, commands, workspace, window } = vscode
    const Environment = require('./Environment')
    const { adjustSelections } = require('./FixSelections')

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
    /** Auto-wrap automatically wraps the current line when space or enter is
     *  pressed after the wrapping column. */
    function toggleAutoWrapCommand()
    {
        if(changeHook) {
            changeHook.dispose()
            changeHook = null
            window.setStatusBarMessage("Auto-wrap: Off", 5000)
        }
        else {
            changeHook = workspace.onDidChangeTextDocument(checkChange)
            window.setStatusBarMessage("Auto-wrap: On", 5000)
        }

        function checkChange(e)
        {
            const document = e.document;
            // Make sure we're in the active document
            const editor = window.activeTextEditor
            if(editor.document !== e.document) return

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
    }
    
    
    /** Collects the information for a wrap from the editor, passes it to the
     *  wrapping code, and then applies the result to the document. If an edit
     *  is applied, returns an updated DocState object, else returns null.
     */
    function doWrap(editor, wrapFn, customColumn = undefined)
    {
        const document = editor.document
        try {
            let settings = Environment.getSettings(editor)
            if(customColumn) Object.assign(settings, {columns: [customColumn]})
            
            const docState = getDocState()
            const lines = Array.from(new Array(document.lineCount))
                .map((_, i) => document.lineAt(i).text)

            // Don't call wrapFn in a promise: it causes performance/race
            // conditions when using auto-wrap
            const edit = wrapFn(docState, settings, lines)
            if(!edit.lines.length) return Promise.resolve()
            
            return applyEdit(edit)
                .then(() => {
                    editor.selections =
                    adjustSelections(lines, docState.selections, [edit])
                    return getDocState()
                })
                .then(null, catchErr)
        }
        catch(err) { catchErr(err) }

        function getDocState() 
        {
            // Conversion of selections is needed for equality operations within
            // Fable code
            return new Rewrap.DocState
                ( document.fileName
                , document.languageId
                , document.version
                , editor.selections.map(vscodeToRewrapSelection)
                )

            function vscodeToRewrapSelection(s) 
            {
                return new Rewrap.Selection
                    ( new Rewrap.Position(s.anchor.line, s.anchor.character)
                    , new Rewrap.Position(s.active.line, s.active.character)
                    )
            }
        }

        function applyEdit(edit) 
        {
            const range =
                document.validateRange
                    ( new Range(edit.startLine, 0, edit.endLine, Number.MAX_VALUE) )

            // vscode takes care of converting to \r\n if necessary
            const text = edit.lines.join('\n')
            return editor.edit(editBuilder => editBuilder.replace(range, text))
        }

        // Catches any error and displays a friendly message to the user.
        function catchErr(err)
        {
            console.error("==Rewrap==")
            console.log(err)
            console.error(
                "Rewrap: Please report this (with a screenshot of this log) at " +
                "https://github.com/stkb/vscode-rewrap/issues"
            )       
            console.error("==========")
            vscode.window.showInformationMessage(
                "Sorry, there was an error in Rewrap. " +
                "Go to: Help -> Toggle Developer Tools -> Console " +
                "for more information."
            )
        }
    }
}