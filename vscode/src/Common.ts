import {DocType} from './Core'
import {Range, Selection, TextDocument, window} from 'vscode'
import fixSelections from './FixSelections'
import GetCustomMarkers from './CustomLanguage'
const getCustomMarkers = GetCustomMarkers()


/** Converts a selection-like object to a vscode Selection object */
const vscodeSelection = s =>
    new Selection(s.anchor.line, s.anchor.character, s.active.line, s.active.character)

/** Applies an edit to the document. Also fixes the selections afterwards. If
 * the edit is empty this is a no-op */
export function applyEdit (editor, edit) {
    if (!edit.lines.length) return Promise.resolve()

    const selections = edit.selections.map(vscodeSelection)
    const doc = editor.document
    const docVersion = doc.version
    const oldLines = Array(edit.endLine - edit.startLine + 1).fill(null)
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
                editor.selection = new Selection(wholeRange.start, wholeRange.end)
            }
            else editor.selections = fixSelections(oldLines, selections, edit)
        })
}

/** Catches any error and displays a friendly message to the user. */
export function catchErr(err) {
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

/** Returns a function for the given document, that gets the line at the given
 *  index. */
export function docLine(document) {
    return i => i < document.lineCount ? document.lineAt(i).text : null
}

/** Gets the path and language of the document. These are used to determine the
 *  parser used for it. */
export function docType(document: TextDocument): DocType {
    const path = document.fileName, language = document.languageId
    return {path, language, getMarkers: () => getCustomMarkers(language)}
}
