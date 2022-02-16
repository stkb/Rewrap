import {getWrappingColumn, maybeAutoWrap} from './Core'
import {Memento, ThemeColor, workspace, window, TextDocumentChangeEvent, TextEditor, ConfigurationChangeEvent} from 'vscode'
import {buildEdits, catchErr, docLine, docType} from './Common'
import {EditorSettings, getCoreSettings, getEditorSettings} from './Settings'

/** Handler that's called if the text in the active editor changes */
const checkChange = async (e: TextDocumentChangeEvent) => {
  // Make sure we're in the active document
  const editor = window.activeTextEditor
  if (!editor || !e || editor.document !== e.document) return
  const doc = e.document;

  // We only want to trigger on normal typing and input with IME's, not other sorts of
  // edits. With normal typing the range (text insertion point) and selection will be both
  // empty and equal to each other (the selection state is still from *before* the edit).
  // IME's make edits where the range is not empty (as text is replaced), but the
  // selection should still be empty. We can also restrict it to single-line ranges (this
  // filters out in particular undo edits immediately after an auto-wrap).
  if (editor.selections.length != 1) return
  if (!editor.selection.isEmpty) return
  // There's more than one change if there were multiple selections, or a whole line is
  // moved up/down with alt+up/down
  if (e.contentChanges.length != 1) return
  const {text: newText, range, rangeLength} = e.contentChanges[0]
  if (rangeLength > 0) return

  try {
    const file = docType(doc)
    const settings = getCoreSettings(editor, cs => getWrappingColumn(file.path, cs))
    // maybeAutoWrap does more checks: that newText isn't empty, but is only whitespace.
    // Don't call this in a promise: it causes timing issues.
    const edit = maybeAutoWrap(file, settings, newText, range.start, docLine(doc))
    if (!edit.isEmpty) return editor.edit (builder => buildEdits(doc, edit, builder))
  }
  catch (err) { catchErr(err) }
}

/** Notification that shows autowrap status in status bar */
namespace Notification {
  const sbItem = window.createStatusBarItem(0, 101)
  const defaultColor = new ThemeColor('statusBar.foreground')
  let timeout // Used for the text notification that hides after a few secs

  /** Override must be true or false */
  export function maybeShow (settings: EditorSettings, override: boolean, wasToggled) {
    const hideAfterTimeout = () => { sbItem.hide(); clearTimeout(timeout) }
    hideAfterTimeout()

    const enabled = settings.autoWrap.enabled
    const onOffText = enabled.value != override ? "on" : "off"
    if (settings.autoWrap.notification === 'icon') {
      sbItem.tooltip = makeTooltip(settings, override)
      sbItem.text = "A$(word-wrap)"
      sbItem.color = override ?
        (enabled.value ? 'gray' : 'orange') : defaultColor
      enabled.value || override ? sbItem.show() : sbItem.hide()
    }
    else if (wasToggled) {
      sbItem.text = `Auto-wrap: ${onOffText}`
      sbItem.show()
      timeout = setTimeout(hideAfterTimeout, 5000)
    }
  }

  function makeTooltip (settings: EditorSettings, override: boolean) : string {
    function str ({name, value, origin}, vFn, text?, showName = false)
    {
      const scopes = ["default", "user", "workspace", "workspace folder"]
      const lang = origin.language ? `[${origin.language}] ` : ""
      const n = showName ? `'${name}' ` : ""
      text = text || name.split('.').slice[-1][0] + ":"
      return `${text} ${vFn(value)} (${lang}${n}${scopes[origin.scope]} setting)`
    }
    const bStr = (x:boolean): string => x ? "on" : "off"
    const colsStr = (cols: number[]) =>
      cols.length > 1 ? "columns: " + cols : "column " + cols[0]

    const lines: string[] = []
    const awEnabled = settings.autoWrap.enabled
    if (override) {
      const onOffText = bStr(awEnabled.value != override)
      lines.push (`Auto-wrap toggled ${onOffText} for this document`)
      lines.push (`Normally: ${bStr(awEnabled.value)}`)
    }
    else lines.push (str(awEnabled, bStr, "Auto-wrap:"))

    lines.push (str(settings.columns, colsStr, "Wrapping at", true))
    return lines.join ("\n")
  }
}

let changeHook

const setDocumentAutoWrap =
  (wsState: Memento, doToggle: boolean) => async (editor: TextEditor) =>
{
  const settings = getEditorSettings(editor), enabled = settings.autoWrap.enabled
  // For every document, we store if autowrap has been toggled on or off. This translates
  // into a value for whether it has been overridden from the current settings.
  const override: boolean = await (async function (){
    const key = editor.document.uri + ':autoWrap.enabled'
    let val: boolean | undefined = wsState.get(key)
    if (doToggle) {
      val = val === undefined || val === enabled.value ? !enabled.value : undefined
      await wsState.update(key, val)
    }
    return val !== undefined
  } ())

  Notification.maybeShow(settings, override, doToggle)

  const isOn = enabled.value != override
  if (isOn && !changeHook)
    changeHook = workspace.onDidChangeTextDocument (checkChange)
  else if (!isOn && changeHook) {
    changeHook.dispose()
    changeHook = null
  }
}


export default function (memento: Memento, subscriptions) {
  const onChangeEditor = setDocumentAutoWrap(memento, false)
  const changeIfAffects = (e: ConfigurationChangeEvent) => (ed: TextEditor) =>
    e.affectsConfiguration('rewrap.autoWrap', ed.document) && onChangeEditor(ed)
  const ifActiveDoc = fn => window.activeTextEditor && fn(window.activeTextEditor)

  window.onDidChangeActiveTextEditor(e => e && onChangeEditor(e), undefined, subscriptions)
  workspace.onDidChangeConfiguration(e => ifActiveDoc(changeIfAffects(e)), undefined, subscriptions)

  ifActiveDoc(onChangeEditor)
  return {
    editorToggle: setDocumentAutoWrap(memento, true),
  }
}
