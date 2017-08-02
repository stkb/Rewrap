const createEdit = require('../fable/Main').rewrap

let doWrap

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


    // Table of document wrapping columns
    const docWrappingColumns = {}

    let lastWrap = {
        uri: null,
        version: null,
        selections: null,
    }

    /**
     * Standard rewrap command
     */
    function rewrapCommentCommand(editor) 
    {
        const document = editor.document
        const uri = document.uri
        const settings = Environment.getSettings(editor)
        const columns = settings.columns

        if(columns.length > 1) {
            if( uri == lastWrap.uri 
                && document.version == lastWrap.version
                && JSON.stringify(editor.selections) == lastWrap.selections
            ) {
                const nextColIndex = 
                    (columns.indexOf(docWrappingColumns[uri]) + 1) % columns.length
                docWrappingColumns[uri] = columns[nextColIndex]
            } else if (!docWrappingColumns[uri]) {
                docWrappingColumns[uri] = columns[0]
            }

            settings.column = docWrappingColumns[uri]
        } else {
            settings.column = settings.columns[0]
        }

        doWrap(editor, validateSettings(settings))
            .then(saveWrapInfo)

        function saveWrapInfo() {
            lastWrap = { 
                uri, 
                version: document.version, 
                selections: JSON.stringify(editor.selections), 
            } 
        }
    }


    /**
     * Does a rewrap, but first prompts for a custom wrapping column to use.
     */
    function rewrapCommentAtCommand(editor)
    {
        return getCustomColumn(customWrappingColumn.toString())
            .then(customColumn => 
                    customColumn 
                        ? Object.assign
                            ( Environment.getSettings(editor)
                            , { column: customColumn }
                            )
                        : null
                )
            .then(options => 
                    options ? doWrap(editor, options).catch(catchErr) : null
                )

        /**
         * Prompts the user for a column value.
         * @param {string} initialValue - Value to prepopulate the input box
         * with. Can be undefined.
         */
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
    }

    /** Used in the rewrapCommmentAt command */
    let customWrappingColumn = 99


    /** 
     * Collects the information for a wrap from the editor, passes it to the
     * wrapping code, and then applies the result to the document.
     */
    function doWrap(editor, options)
    {
        const document = editor.document
        const selections = editor.selections
        const lines = 
            Array.from(new Array(document.lineCount))
                .map((_, i) => document.lineAt(i).text)

        return Promise.resolve(
            createEdit
                ( document.languageId
                , document.fileName
                , selections
                , options
                , lines
                )
            )
            .then(applyEdit)
            .then(edit =>
                editor.selections = adjustSelections(lines, selections, [edit])
            )
            .catch(catchErr)

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


    function validateSettings(settings) 
    {
        // Check wrapping column
        if(!Number.isInteger(settings.column) || settings.column < 1) {
            console.warn(
                "Rewrap: wrapping column is an invalid value (%o). " +
                "Using the default of (80) instead.", settings.column
            )
            settings.column = 80
        }
        else if(settings.column > 120) {
            console.warn(
                "Rewrap: wrapping column is a rather large value (%d).",
                settings.column
            )
        }

        // Check tab width
        if(!Number.isInteger(settings.tabWidth) || settings.tabWidth < 1) {
            console.warn(
                "Rewrap: tab width is an invalid value (%o). " +
                "Using the default of (4) instead.", settings.tabWidth
            )
            settings.tabWidth = 4
        }
        if(settings.tabWidth > settings.column / 2) {
            console.warn(
                "Rewrap: tabSize is (%d) and wrappingColumn is (%d). " +
                "Unexpected results may occur.", settings.tabWidth, settings.column
            )
        }

        return settings;
    }
}