const vscode = require('vscode')
const { Range, commands, workspace, window } = vscode
const getSettings = require('./Settings')
const getCustomMarkers = require('./CustomLanguage')()
const fixSelections = require('./FixSelections')
const { DocState, Position, Selection } = require('./compiled/Types')
const Rewrap = require('./compiled/Main')

/** Function to activate the extension. */
exports.activate = async function activate(context)
{
    const toggleAutoWrapCommand =
        await initAutoWrap (workspace, context.globalState, window)

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
            value: columnStr,
            placeHolder: ""
        })
        if(columnStr === undefined) return // The user pressed cancel

        customWrappingColumn = parseInt(columnStr) || 0
        return doWrap(editor, customWrappingColumn);
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

/** Gets the path and language of the document. These are used to determine the
 *  parser used for it. */
const docType = document =>
    ( { path: document.fileName
      , language: document.languageId
      , getMarkers: () => getCustomMarkers(document.languageId)
      }
    )

/** Converts a selection-like object to a vscode Selection object */
const vscodeSelection = s =>
    new vscode.Selection
        (s.anchor.line, s.anchor.character, s.active.line, s.active.character)

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

/** Returns a function for the given document, that gets the line at the given
 *  index. */
const docLine =
    document => i => i < document.lineCount ? document.lineAt(i).text : null

/** Applies an edit to the document. Also fixes the selections afterwards. If
 * the edit is empty this is a no-op */
const applyEdit = (editor, edit) => {
    if (!edit.lines.length) return Promise.resolve()

    const selections = edit.selections.map(vscodeSelection)
    const doc = editor.document
    const docVersion = doc.version
    const oldLines = Array(edit.endLine - edit.startLine + 1).fill()
        .map((_, i) => doc.lineAt(edit.startLine + i).text)
    const getDocRange = () => doc.validateRange
            (new Range(0, 0, Number.MAX_SAFE_INTEGER, Number.MAX_SAFE_INTEGER))
    const wholeDocSelected = selections[0].isEqual(getDocRange())

    return editor
        .edit(editBuilder => {
            // Execution of this callback is delayed. Check the document is
            // still as we expect it.
            // todo: see if vscode already makes this check anyway
            if(doc.version !== docVersion) return false

            const range = doc.validateRange
                ( new Range(edit.startLine, 0, edit.endLine, Number.MAX_VALUE) )
            // vscode takes care of converting to \r\n if necessary
            const text = edit.lines.join('\n')

            return editBuilder.replace(range, text)
        })
        .then(didEdit => {
            if(!didEdit) return
            if(wholeDocSelected) {
                const wholeRange = getDocRange()
                editor.selection =
                    new vscode.Selection(wholeRange.start, wholeRange.end)
            }
            else editor.selections = fixSelections(oldLines, selections, edit)
        })
}

/** Collects the information for a wrap from the editor, passes it to the
 *  wrapping code, and then applies the result to the document. If an edit
 *  is applied, returns an updated DocState object, else returns null.
 *  Takes an optional customColumn to wrap at.
 */
const doWrap = (editor, customColumn) => {
    const doc = editor.document
    try {
        const docState = getDocState(editor)
        let settings = getSettings(editor)
        settings.column = !isNaN(customColumn) ? customColumn
            : Rewrap.maybeChangeWrappingColumn(docState, settings.columns)
        const selections = editor.selections.map(rewrapSelection)

        const edit = Rewrap.rewrap(docType(doc), settings, selections, docLine(doc))
        return applyEdit(editor, edit).then(null, catchErr)
    }
    catch(err) { catchErr(err) }
}

/********** Auto-Wrap **********/

const checkChange = e => {
    // Make sure we're in the active document
    const editor = window.activeTextEditor
    if(!editor || !e || editor.document !== e.document) return
    const doc = e.document;

    // We only want to trigger on normal typing and input with IME's, not other
    // sorts of edits. With normal typing the range (text insertion point) and
    // selection will be both empty and equal to each other (the selection state
    // is still from *before* the edit). IME's make edits where the range is not
    // empty (as text is replaced), but the selection should still be empty. We
    // can also restrict it to single-line ranges (this filters out in
    // particular undo edits immediately after an auto-wrap).
    if(editor.selections.length != 1) return
    if(!editor.selection.isEmpty) return
    // There's more than one change if there were multiple selections,
    // or a whole line is moved up/down with alt+up/down
    if(e.contentChanges.length != 1) return
    const { text: newText, range, rangeLength } = e.contentChanges[0]
    if(rangeLength > 0) return

    try {
        const file = docType(doc)
        let settings = getSettings(editor)
        settings.column = Rewrap.getWrappingColumn(file.path, settings.columns)

        // maybeAutoWrap does more checks: that newText isn't empty, but is only
        // whitespace. Don't call this in a promise: it causes timing issues.
        const edit =
            Rewrap.maybeAutoWrap(file, settings, newText, range.start, docLine(doc))
        return applyEdit(editor, edit).then(null, catchErr)
    }
    catch(err) { catchErr(err) }
}

const initAutoWrap = async (workspace, extState, window) => {
    const config = workspace.getConfiguration('rewrap.autoWrap')
    const getEnabledSetting = () => config.inspect('enabled').globalValue
    const getEnabled = () => { // can still return null/undefined
        const s = getEnabledSetting ()
        return s !== undefined ? s : extState.get('autoWrap')
    }

    let changeHook
    const setEnabled = async (on) => {
        if (on && !changeHook)
            changeHook = workspace.onDidChangeTextDocument(checkChange)
        else if (!on && changeHook) {
            changeHook.dispose()
            changeHook = null
        }
        const s = getEnabledSetting ()
        if (s === undefined) await extState.update('autoWrap', on)
        else {
            await extState.update('autoWrap', null)
            if (s !== on) await config.update('enabled', on, true)
        }
        await window.setStatusBarMessage(`Auto-wrap: ${on?'On':'Off'}`, 7000)
    }

    const checkState = async () => setEnabled (getEnabled ())
    workspace.onDidChangeConfiguration(e => {
        if (e.affectsConfiguration('rewrap.autoWrap')) checkState ()
    })
    await checkState ()
    return async () => { setEnabled (!getEnabled()) }
}
