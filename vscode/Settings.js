// Gets editor settings from the environment
module.exports = getSettings

const { workspace } = require('vscode')

/** Gets a settings object from vscode's configuration. Doing this is not
 *  normally expensive. */
function getSettings(editor)
{
    const setting = settingGetter(editor)
    const settings = {
        columns: getWrappingColumns(setting),
        doubleSentenceSpacing: setting('rewrap.doubleSentenceSpacing'),
        wholeComment: setting('rewrap.wholeComment'),
        reformat: setting('rewrap.reformat'),
        tabWidth: validateTabSize(editor.options.tabSize),
    }
    return validateSettings(editor.document.uri, settings);
}

/** Gets an array of the available wrapping column(s) from the user's settings.
 */
function getWrappingColumns(setting)
{
    let extensionColumn, rulers

    if(extensionColumn = setting('rewrap.wrappingColumn'))
        return [extensionColumn]
    else if((rulers = setting('editor.rulers'))[0])
        return rulers
    else
        return [setting('editor.wordWrapColumn')]
        // The default for this is already 80
}

// For each invalid value for each document, remember that we've warned so that
// we don't flood the console with the same warnings
let warningCache = {}

/** Since the settings come from the user's own settings.json file, there may be
 *  invalid values. */
function validateSettings(docID, settings)
{
    // Check all columns
    settings.columns = settings.columns.map(checkWrappingColumn)
    return settings;

    function checkWrappingColumn(col)
    {
        if(!Number.isInteger(col) || col < 1) {
            warn(
                "Rewrap: wrapping column is set at '%o'. " +
                "This will be treated as infinity.", col
            )
            col = 0
        }
        else if(col > 120) {
            warn("Rewrap: wrapping column is a rather large value (%d).", col)
        }
        return col
    }

    function warn(msg, val) {
        const key = docID + "|" + val
        if(warningCache[key]) return
        console.warn(msg, val)
        warningCache[key] = true
    }
}

function validateTabSize(size) {
    // Check tab width
    if(!Number.isInteger(size) || size < 1) {
        console.warn(
            "Rewrap: tab size is an invalid value (%o). " +
            "Using the default of (4) instead.", size
        )
        return 4
    }
    else return size
}

/** Returns a function for getting a setting. That function looks first for a
 *  language-specific setting before finding the general. */
function settingGetter(editor)
{
    const language = editor.document.languageId
        , config = workspace.getConfiguration('', editor.document.uri)
        , languageSection = config.get('[' + language + ']')

    return setting =>
        langSetting(languageSection, setting.split('.')) || config.get(setting)

    function langSetting(obj, pathParts)
    {
        if(!pathParts.length) return undefined

        const [next, ...rest] = pathParts
        if(obj) {
            return obj[pathParts.join('.')] || langSetting(obj[next], rest)
        }
        else {
            return undefined
        }
    }
}

// Invalidate cache if configuration changed. This is pretty crude and we could
// use e.affectsConfiguration(section, uri) to be a bit smarter.
workspace.onDidChangeConfiguration(e => cache = {})
