import { Position, Selection, TextDocument } from 'vscode'

/** After wrapping is performed to the document in an editor, vscode sometimes
 *  does something strange to the selection(s) from before the edit.  
 *  (https://github.com/stkb/vscode-rewrap/issues/4)  
 *  The selection gets expanded to the whole of the last line, plus the line
 *  break before it. This only happens if the selection was within the text that
 *  ends up on the last line of the section, after wrapping.
 *
 *  We don't fix this completely here but take care of the 90% case, which is
 *  when the text selection is empty (just a cursor), to avoid the overwriting
 *  problem described in issue 4. We do it by creating a list of all the empty
 *  selections before the edits, along with how far they are from the end of the
 *  line they're on. We can then look at the same selections after editing, and
 *  if they have been messed with by vscode (no longer empty), restore them to
 *  being empty again, and at the same position they were from the end of the
 *  line.
 */
export { saveSelections, restoreSelections }


/** Returns an object that contains selections that might need fixing later */
function saveSelections
  ( editor: Editor
  ): Selections
{
  const map = new Map<number, number>()
      , document = editor.document
  
  editor.selections.forEach((s, i) => {
    if(s.isEmpty) map.set(i, distanceFromLineEnd(s, document))
  })
  
  return map
}


/** Modifies an editor's selections, given a set of old selections, to fix the
 *  ones that have were empty but no longer are */
function restoreSelections
  ( editor: Editor, oldSelections: Selections
  ): void
{
  const document = editor.document
  
  const newSelections = 
         editor.selections
          .map((ns, i) => {
            if(oldSelections.has(i) && !ns.isEmpty) {
              return restoreSelection(document, ns, oldSelections.get(i))
            }
            else return ns
          })
  editor.selections = newSelections
}


/** Map of the index that a selection was at, in an editor's list of selections,
 *  together with how far it was from the end of the line */
type Selections = Map<number, number>
type Editor = { document: TextDocument, selections: Selection[] }


function distanceFromLineEnd
  ( selection : Selection, document: TextDocument
  ): number
{
  const line = document.lineAt(selection.end)
  return line.text.length - selection.end.character
}


/** Returns an empty selection at a specified point from the line's end */
function restoreSelection
  ( document: TextDocument, selection: Selection, fromLineEnd: number
  ): Selection
{
  // The selection only gets modified by vscode if its end ends up on the last
  // line of the text after wrapping. So we don't need to worry about if
  const endPos = selection.end
      , line = document.lineAt(endPos)
      , newPos = new Position(endPos.line, line.text.length - fromLineEnd)
      
  return new Selection(newPos, newPos)
}