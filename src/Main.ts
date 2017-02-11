import * as vscode from 'vscode'
import { 
  ExtensionContext, Range, Selection, TextDocument, TextEditor, TextEditorEdit,
  commands, workspace,
} from 'vscode'
require('./extensions')

import DocumentProcessor, { Edit, WrappingOptions } from './DocumentProcessor'
import { fromDocument } from './DocumentTypes'
import { adjustSelections } from './FixSelections'
import Section from './Section'
import Environment from './Environment'


export { activate, getEditsAndSelections, wrapSomething }


/** Is called when the extension is activated, the very first time the
 *  command is executed */
function activate(context: ExtensionContext) 
{
  context.subscriptions.push(
    commands.registerTextEditorCommand(
      'rewrap.rewrapComment', 
      editor => {
        const options = Environment.getOptions(editor)
        return wrapSomething(editor, options).catch(catchErr)
      }
    )
  )
}


function catchErr(err: any): void
{
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
}


/** Finds the processor for the document and does the wrapping */
async function wrapSomething
  ( editor: TextEditor, options: WrappingOptions
  )
{
  const documentProcessor = fromDocument(editor.document)
      , documentLines = 
          Array.range(0, editor.document.lineCount)
            .map(i => editor.document.lineAt(i).text)

  const [edits, newSelections] = 
          getEditsAndSelections
            ( documentProcessor, documentLines, editor.selections, options
            )
  
  await editor.edit(eb => applyEdits(edits, editor.document, eb))

  editor.selections = newSelections
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


/** Gets the edits to be made to a document and the positions the selections
 *  should be in afterwards. */
function getEditsAndSelections
  ( documentProcessor: DocumentProcessor
  , documentLines: string[]
  , selections: Selection[]
  , options: WrappingOptions
  ) : [ Edit[], Selection[] ]
{
  const sections = 
          documentProcessor.findSections(documentLines, options.tabSize)  
      , sectionsToEdit = 
          Section.sectionsInSelections(sections, selections)

  // Edits should be kept in ascending order, for `adjustSelections`. For
  // applying the edits with `editor.edit` it doesn't matter.
  const edits = 
          sectionsToEdit
            .map(sectionToEdit => 
                  documentProcessor.editSection(options, sectionToEdit))

  // Get the adjusted selections to apply after the edits are done
  const adjustedSelections = 
          adjustSelections(documentLines, selections, edits)


  return [ edits, adjustedSelections ]
}


/** Defines a TextEditor with the minimum set of features needed to do wrapping
 *  on a document. Used for tests. */
export interface TextEditorLike {
  document: vscode.TextDocument
  edit(callback: (editBuilder: TextEditorEdit) => void): Thenable<boolean>
  options: vscode.TextEditorOptions
  selections: vscode.Selection[]
}