// Gets editor settings from the environment

import {TextDocument, workspace, TextEditor, WorkspaceConfiguration} from 'vscode'

export interface EditorSettings {
  autoWrap: AutoWrapSettings
  columns: Setting<number[]>
  tabWidth: number
  doubleSentenceSpacing: boolean
  reformat: boolean
  wholeComment: boolean
}

export interface AutoWrapSettings
  { enabled: Setting<boolean>; notification: 'icon' | 'text' }

export interface Setting<T>
  { name: string; value: T; origin: {scope: number, language: string | null} }


/** Gets and validates a settings object from vscode's configuration. Doing this is not
 *  normally expensive. */
export function getCoreSettings (editor: TextEditor, fn: (cols:number[]) => number) {
  const settings = getEditorSettings (editor)
  return {
    column: fn(settings.columns.value),
    doubleSentenceSpacing: settings.doubleSentenceSpacing,
    wholeComment: settings.wholeComment,
    reformat: settings.reformat,
    tabWidth: settings.tabWidth,
  }
}

export function getEditorSettings (editor: TextEditor) : EditorSettings {
  const docID = editor.document.uri.toString()
  const config = workspace.getConfiguration('', editor.document)
  const setting = <T>(name) => config.get<T>(name) as T
  const checkTabSize = size =>
    !Number.isInteger(size) || size < 1 ? Warnings.tabSize(docID, size, 4) : size
  return {
    autoWrap: getAutoWrapSettings(config, editor.document.languageId),
    columns: getWrappingColumns(config, editor.document),
    doubleSentenceSpacing: setting('rewrap.doubleSentenceSpacing'),
    wholeComment: setting('rewrap.wholeComment'),
    reformat: setting('rewrap.reformat'),
    tabWidth: checkTabSize(editor.options.tabSize),
  }
}

/** Gets the onSave setting for the wrap-on-save feature */
export function getOnSaveSetting (document: TextDocument) : boolean {
  return workspace.getConfiguration('', document).get('rewrap.onSave', false)
}

const getAutoWrapSettings =
  (config: WorkspaceConfiguration, lang: string) : AutoWrapSettings =>
({
  enabled: settingWithOrigin(config, lang)('rewrap.autoWrap.enabled'),
  notification: config.get('rewrap.autoWrap.notification', 'icon'),
})

/** Gets an array of the available wrapping column(s) from the user's settings. */
function getWrappingColumns
  (config: WorkspaceConfiguration, doc: TextDocument) : Setting<number[]>
{
  const checkColumn = (col: number) =>
    !Number.isInteger(col) || col < 1 ? Warnings.column(doc, col, 0)
    : col > 120 ? Warnings.largeColumn(doc, col, col)
    : col
  const get = settingWithOrigin (config, doc.languageId)

  {
    const s = get<number>('rewrap.wrappingColumn')
    if (s.value) return {...s, value: [checkColumn(s.value)]}
  } {
    const s = get<number[]>('editor.rulers')
    const rValue = (r): number => checkColumn(r.column != undefined ? r.column : r)
    if (s.value && s.value[0]) return {...s, value: s.value.map(rValue)}
  } {
    const s = get<number>('editor.wordWrapColumn')
    return {...s, value: [checkColumn(s.value)]}
  }
}

const settingWithOrigin =
  (config: WorkspaceConfiguration, lang: string) => <T>(name: string) : Setting<T> =>
{
  const scopes = ['default', 'global', 'workspace', 'workspaceFolder']
  const info = config.inspect(name)
  if (!info) throw `Tried to get undefined setting: ${name}`
  for (let language of [lang, null]) {
    for (let scope = 3; scope >= 0; scope--) {
      const key = scopes[scope] + (language ? 'LanguageValue' : 'Value')
      if (info[key] !== undefined)
        return {name, value: info[key], origin: {scope, language}}
    }
  }
  throw `Failed to get info for setting: ${name}`
}

/** Deals with writing warnings for invalid values. */
namespace Warnings {
  // For each invalid value for each document, remember that we've warned so that we don't
  // flood the console with the same warnings
  let cache = {}

  const warn = (setting, msg) => <T>(doc, val: T, def): T => {
    const key = doc.uri.toString() + "|" + setting + "|" + val
    if (!cache[key]) { cache[key] = true; console.warn("Rewrap: " + msg, val, def) }
    return def
  }

  export const column = warn('wrappingColumn',
    "wrapping column is set at '%o'. This will be treated as infinity.")
  export const largeColumn = warn('wrappingColumn',
    "wrapping column is a rather large value (%d).")
  export const tabSize = warn('tabSize',
    "tab size is an invalid value (%o). Using the default of (%d) instead.")
}
