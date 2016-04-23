import * as vscode from 'vscode'
import { commands, ExtensionContext, workspace, Range, TextEditorEdit } from 'vscode'
require('./extensions')

import BasicLanguage from './BasicLanguage'
import wrappingHandler from './documentTypes'
import { saveSelections, restoreSelections } from './FixSelections'
import Section from './Section'


/** Is called when the extension is activated, the very first time the
 *  command is executed */
export function activate(context: ExtensionContext) 
{
  context.subscriptions.push(
    commands.registerTextEditorCommand('rewrap.rewrapComment', wrapSomething)
  )
}


/** Finds the processor for the document and does the wrapping */
export function wrapSomething
  ( editor: TextEditorLike, _?: TextEditorEdit, wrappingColumn?: number
  ): Thenable<void>
{
  const handler = wrappingHandler(editor.document)
      , tabSize = editor.options.tabSize
  wrappingColumn = wrappingColumn || getWrappingColumn()
  
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
  
  return (
    editor
      .edit(builder => 
        edits.forEach(e => 
          builder.replace(
            new Range(e.startLine, 0, e.endLine, Number.MAX_VALUE),
            e.lines.join('\n')
          )
        )
      )
      .then(() => restoreSelections(editor, oldSelections))
  )
}


/** Defines a TextEditor with the minimum set of features needed to do wrapping
 *  on a document. Used for tests. */
export interface TextEditorLike {
  document: vscode.TextDocument
  edit(callback: (editBuilder: TextEditorEdit) => void): Thenable<boolean>
  options: vscode.TextEditorOptions
  selections: vscode.Selection[]
}


/** Gets the wrapping column (eg 80) from the user's setting */
function getWrappingColumn() {
  const editorColumn =
        workspace.getConfiguration('editor').get<number>('wrappingColumn')
    , extensionColumn =
        workspace.getConfiguration('rewrap').get<number>('wrappingColumn')

  return extensionColumn
    || (0 < editorColumn && editorColumn <= 120) && editorColumn
    || 80
}