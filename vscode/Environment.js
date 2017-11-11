// Gets editor settings from the environment
module.exports = { getSettings }

const { workspace } = require('vscode')


function getSettings(editor)
{
    return {
        column: 0, // Not used and will later be removed
        columns: getWrappingColumns(editor),
        tabWidth: editor.options.tabSize,
        doubleSentenceSpacing: getSetting(editor, 'rewrap.doubleSentenceSpacing'),
        wholeComment: getSetting(editor, 'rewrap.wholeComment'),
        reformat: getSetting(editor, 'rewrap.reformat'),
    }
}


/** Gets the wrapping column(s) from the user's settings. The result is always
 *  returned as an array. */
function getWrappingColumns(editor) 
{
    let extensionColumn, rulers

    if(extensionColumn = getSetting(editor, 'rewrap.wrappingColumn'))
        return [extensionColumn]
    else if((rulers = getSetting(editor, 'editor.rulers'))[0])
        return rulers
    else
        return [getSetting(editor, 'editor.wordWrapColumn')]
        // The default for this is already 80
}

/** Gets a setting from vscode. Tries to find a setting for the appropriate
 *  language for the editor. */
function getSetting(editor, setting)
{
  const language = editor.document.languageId
      , config = workspace.getConfiguration()
      , languageSection = config.get('[' + language + ']')

  return languageSetting(languageSection, setting.split('.')) 
          || config.get(setting)
}

function languageSetting(obj, pathParts)
{
  if(!pathParts.length) return undefined

  const [next, ...rest] = pathParts
  if(obj) {
    return obj[pathParts.join('.')] || languageSetting(obj[next], rest)
  }
  else {
    return undefined
  }
}