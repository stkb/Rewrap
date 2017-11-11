const Rewrap = require('./compiled/Core/Types')
const Core = require('./compiled/Core/Main')

/**
 * Function to activate the extension.
 */
exports.activate = function activate(context)
{
    const { Range, commands, workspace, window } = require('vscode')
    const Environment = require('./Environment')
    const { adjustSelections } = require('./FixSelections')

    // Register the commands
    context.subscriptions.push
        ( commands.registerTextEditorCommand
            ( 'rewrap.rewrapComment', rewrapCommentCommand )
        , commands.registerTextEditorCommand
            ( 'rewrap.rewrapCommentAt', rewrapCommentAtCommand )
        )

    /**
     * Standard rewrap command
     */
    function rewrapCommentCommand(editor) 
    {
        doWrap(editor).then(Core.saveDocState)
    }


    /**
     * Does a rewrap, but first prompts for a custom wrapping column to use.
     */
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

            doWrap(editor, { columns: [customColumn] })
        }
    }

    /** Used in the rewrapCommmentAt command */
    let customWrappingColumn = 99


    /**
     * Collects the information for a wrap from the editor, passes it to the
     * wrapping code, and then applies the result to the document. Returns an
     * updated DocState object.
     */
    function doWrap(editor, settingOverrides)
    {
        const document = editor.document
        const docState = getDocState()
        const lines = 
            Array.from(new Array(document.lineCount))
                .map((_, i) => document.lineAt(i).text)
        const settings = 
            Object.assign(Environment.getSettings(editor), settingOverrides)

        return Promise.resolve()
            .then(() => Core.rewrap(docState, settings, lines))
            .then(applyEdit)
            .then(edit => {
                editor.selections = adjustSelections(lines, docState.selections, [edit])
                return getDocState()
            })
            .catch(catchErr)


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
            if(edit.lines.length) {
                const range =
                    document.validateRange
                        ( new Range(edit.startLine, 0, edit.endLine, Number.MAX_VALUE) )
                const text = 
                    edit.lines.join('\n')

                return editor.edit(editBuilder => editBuilder.replace(range, text))
                    .then(_ => edit)
            }
            else return edit
        }
    }


    /**
     * Catches an error and displays a friendly message to the user.
     */
    function catchErr(err)
    {
        console.error("Rewrap: Something happened.")
        console.log(err)
        console.error(
            "Rewrap: Please report this (with a screenshot of this log) at " +
            "https://github.com/stkb/vscode-rewrap/issues"
        )       
        vscode.window.showInformationMessage(
            "Sorry, there was an error in Rewrap. " +
            "Go to: Help -> Toggle Developer Tools -> Console " +
            "for more information."
        )
    }
}