// Gets editor settings from the environment
module.exports = getSettings

const { workspace } = require('vscode')
let cache = {}

/** Gets a settings object from vscode's configuration. Doing this is not
 *  normally expensive, but an object for each document is cached to prevent the
 *  console being flooded with warnings (vscode issue #48225). */
function getSettings(editor)
{
    const key = editor.document.uri
    if(!cache[key]) {
        const setting = settingGetter(editor)
        const settings = {
            columns: getWrappingColumns(setting),
            doubleSentenceSpacing: setting('rewrap.doubleSentenceSpacing'),
            wholeComment: setting('rewrap.wholeComment'),
            reformat: setting('rewrap.reformat'),
        }
        cache[key] = validateSettings(settings)
    }
    return Object.assign
        ({tabWidth: validateTabSize(editor.options.tabSize)}, cache[key])
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

/** Since the settings come from the user's own settings.json file, there may be
 *  invalid values. */
function validateSettings(settings) 
{
    // Check all columns
    settings.columns = settings.columns.map(checkWrappingColumn)
    return settings;

    function checkWrappingColumn(col)
    {
        if(!Number.isInteger(col) || col < 1) {
            console.warn(
                "Rewrap: wrapping column is an invalid value (%o). " +
                "Using a default of (80) instead.", col
            )
            col = 80
        }
        else if(col > 120) {
            console.warn(
                "Rewrap: wrapping column is a rather large value (%d).", col
            )
        }
        return col
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
