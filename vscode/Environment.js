// Gets editor settings from the environment
module.exports = { getSettings }

const { workspace } = require('vscode')


function getSettings(editor)
{
    const column = getWrappingColumn(editor)
    
    return {
        column,
        tabWidth: getTabSize(editor, column),
        doubleSentenceSpacing: getSetting(editor, 'rewrap.doubleSentenceSpacing'),
        wholeComment: getSetting(editor, 'rewrap.wholeComment'),
        reformat: getSetting(editor, 'rewrap.reformat'),
    }
}


/** Gets the tab size from the editor, according to the user's settings.
 *  Sanitizes the input. */
function getTabSize(editor, wrappingColumn) 
{
  let tabSize = editor.options.tabSize
  
  if(!Number.isInteger(tabSize) || tabSize < 1) {
    console.warn(
      "Rewrap: tabSize is an invalid value (%o). " +
      "Using the default of (4) instead.", tabSize
    )
    tabSize = 4
  }
  
  if(tabSize > wrappingColumn / 2) {
    console.warn(
      "Rewrap: tabSize is (%d) and wrappingColumn is (%d). " +
      "Unexpected results may occur.", tabSize, wrappingColumn
    )
  }

  return tabSize
}


/** Gets the wrapping column (eg 80) from the user's settings.  
 *  Sanitizes the input. */
function getWrappingColumn(editor) 
{
  const extensionColumn = getSetting(editor, 'rewrap.wrappingColumn')
      , rulers = getSetting(editor, 'editor.rulers')
      , editorColumn = getSetting(editor, 'editor.wordWrapColumn')

  // The default for editor.wordWrapColumn is already 80
  let wrappingColumn = extensionColumn || rulers[0] || editorColumn
  
  if(!Number.isInteger(wrappingColumn) || wrappingColumn < 1) {
    console.warn(
      "Rewrap: wrapping column is an invalid value (%o). " +
      "Using the default of (80) instead.", wrappingColumn
    )
    wrappingColumn = 80
  }
  else if(wrappingColumn > 120) {
    console.warn(
      "Rewrap: wrapping column is a rather large value (%d).", wrappingColumn
    )
  }

  return wrappingColumn
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