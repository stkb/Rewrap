'use strict'

// Gets editor settings from the environment
module.exports = getSettings

const {workspace} = require('vscode')

/** Gets and validates a settings object from vscode's configuration. Doing this
 *  is not normally expensive. */
function getSettings(editor)
{
    const docID = editor.document.uri.toString()
    const config = workspace.getConfiguration('', editor.document)
    const setting = name => config.get(name)
    const checkColumn = col =>
        !Number.isInteger(col) || col < 1 ? warnings.column(docID, col, 0)
        : col > 120 ? warnings.largeColumn(docID, col, col)
        : col
    const checkTabSize = size =>
        !Number.isInteger(size) || size < 1 ? warnings.tabSize(docID, size, 4) : size

    return {
        columns: getWrappingColumns(setting).map(checkColumn),
        doubleSentenceSpacing: setting('rewrap.doubleSentenceSpacing'),
        wholeComment: setting('rewrap.wholeComment'),
        reformat: setting('rewrap.reformat'),
        tabWidth: checkTabSize(editor.options.tabSize),
    }
}

/** Gets an array of the available wrapping column(s) from the user's settings.
 */
const getWrappingColumns = getSetting => {
    let wcSetting, rulers
    const rulerValue = r => r.column != undefined ? r.column : r
    return (wcSetting = getSetting('rewrap.wrappingColumn')) ? [wcSetting]
        // Rulers might be {"column": 80, "color": "#000000"} objects
        : (rulers = getSetting('editor.rulers'))[0] ? rulers.map(rulerValue)
        : [getSetting('editor.wordWrapColumn')] // default for this is already 80
}

/** Deals with writing warnings for invalid values. */
const warnings = (() => {
    // For each invalid value for each document, remember that we've warned so
    // that we don't flood the console with the same warnings
    let cache = {}

    const warn = (setting, msg) => (docID, val, def) => {
        const key = docID + "|" + setting + "|" + val
        if (!cache[key]) { cache[key] = true; console.warn("Rewrap: " + msg, val, def) }
        return def
    }

    const column = warn('wrappingColumn',
        "wrapping column is set at '%o'. This will be treated as infinity.")
    const largeColumn = warn('wrappingColumn',
        "wrapping column is a rather large value (%d).")
    const tabSize = warn('tabSize',
        "tab size is an invalid value (%o). Using the default of (%d) instead.")

    return {column, largeColumn, tabSize}
})()
