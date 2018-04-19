// Gets editor settings from the environment
module.exports = { getSettings, getWrappingColumns }

const { workspace } = require('vscode')

function getSettings(editor)
{
    const setting = settingGetter(editor)
    const settings = {
        column: 0, // Not used and will later be removed
        columns: getWrappingColumns(editor),
        tabWidth: editor.options.tabSize,
        doubleSentenceSpacing: setting('rewrap.doubleSentenceSpacing'),
        wholeComment: setting('rewrap.wholeComment'),
        reformat: setting('rewrap.reformat'),
    }
    return validateSettings(settings)
}

/** Gets an array of the available wrapping column(s) from the user's settings.
 */
function getWrappingColumns(editor) 
{
    const setting = settingGetter(editor)
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

    // Check tab width
    if(!Number.isInteger(settings.tabWidth) || settings.tabWidth < 1) {
        console.warn(
            "Rewrap: tab width is an invalid value (%o). " +
            "Using the default of (4) instead.", settings.tabWidth
        )
        settings.tabWidth = 4
    }

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
