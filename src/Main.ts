import * as vscode from 'vscode'
import { 
  ExtensionContext,  Range, TextDocument, TextEditor, TextEditorEdit,
  commands, workspace,
} from 'vscode'
require('./extensions')

import { Edit } from './DocumentProcessor'
import BasicLanguage from './BasicLanguage'
import wrappingHandler from './documentTypes'
import { saveSelections, restoreSelections } from './FixSelections'
import Section from './Section'


/** Is called when the extension is activated, the very first time the
 *  command is executed */
export function activate(context: ExtensionContext) 
{
  context.subscriptions.push(
    commands.registerTextEditorCommand(
      'rewrap.rewrapComment', 
      editor => {
        // Tried doing this as wrapSomething().then(undefined, errback) but it
        // didn't catch errors.
        try {
          return wrapSomething(editor)
        }
        catch (err) {
          console.error("Rewrap: Something happened.")
          console.log(err)
          console.error(
            "Rewrap: Please report this (with a screenshot of this log) at " +
            "https://github.com/stkb/vscode-rewrap/issues"
          )       
          vscode.window.showInformationMessage(
            "Sorry, there was an error in Rewrap. " +
            "Go to: Help -> Toggle Developer Tools -> Console " +
            "for more information."
          )
          return null
        }
      }
    )
  )
}


/** Finds the processor for the document and does the wrapping */
export async function wrapSomething
  ( editor: TextEditorLike, wrappingColumn?: number
  )
{
  wrappingColumn = wrappingColumn || getWrappingColumn()
  const handler = wrappingHandler(editor.document)
      , tabSize = getTabSize(editor, wrappingColumn)

  const sections = handler.findSections(editor.document)  
      , sectionsToEdit = 
          Section.sectionsInSelections(
            sections.primary, sections.secondary, editor.selections
          )
  const edits = 
    sectionsToEdit
    .map(sectionToEdit => 
      handler.editSection(wrappingColumn, tabSize, sectionToEdit)
     )
    // sort edits in reverse range order
    .sort((e1, e2) => e1.startLine > e2.startLine ? -1 : 1)
  
  const oldSelections = saveSelections(editor)
  
  const success = await editor.edit(eb => applyEdits(edits, editor.document, eb))
  restoreSelections(editor, oldSelections)
}


function applyEdits(edits: Edit[], document: TextDocument, builder: TextEditorEdit)
{
  edits.forEach(e => {
    const range = 
            document.validateRange(
              new Range(e.startLine, 0, e.endLine, Number.MAX_VALUE)
            )
        , text = e.lines.join('\n')
    builder.replace(range, text)
  })
}


/** Defines a TextEditor with the minimum set of features needed to do wrapping
 *  on a document. Used for tests. */
export interface TextEditorLike {
  document: vscode.TextDocument
  edit(callback: (editBuilder: TextEditorEdit) => void): Thenable<boolean>
  options: vscode.TextEditorOptions
  selections: vscode.Selection[]
}


/** Gets the tab size from the editor, according to the user's settings.
 *  Sanitizes the input. */
function getTabSize(editor: TextEditorLike, wrappingColumn: number): number 
{
  let tabSize = editor.options.tabSize as number
  
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
function getWrappingColumn(): number {
  const editorColumn =
        workspace.getConfiguration('editor').get<number>('wrappingColumn')
    , extensionColumn =
        workspace.getConfiguration('rewrap').get<number>('wrappingColumn')

  let wrappingColumn =
        extensionColumn
        || (0 < editorColumn && editorColumn <= 120) && editorColumn
        || 80
  
  if(!Number.isInteger(wrappingColumn) || wrappingColumn < 1) {
    console.warn(
      "Rewrap: wrappingColumn is an invalid value (%o). " +
      "Using the default of (80) instead.", wrappingColumn
    )
    wrappingColumn = 80
  }

  return wrappingColumn
}