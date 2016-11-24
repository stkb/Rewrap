export { adjustSelections }

// When we replace lines of text in the editor with new text that's been
// wrapped, the original cursor position/selections can get messed up; ie. the
// cursor isn't by the word it used to be.
//
// This module does a diff of the old and new text (using the js module
// fast-diff), and uses it to calculate where the selections should be after
// wrapping, so they can be re-applied in the editor after the edit(s) have been
// done.

import { Position, Range, Selection } from 'vscode'
const fd = require('fast-diff')

/** An array of operation-string tuples. Operation is -1 for removed, 1 for
 *  added and 0 for unchanged text. */
type Diff = [number, string][]

import { Edit } from './DocumentProcessor'
import { offsetAt, positionAt } from './Position'


/** Given lines of original text, a set of selections and a set of edits,
 *  returns the positions of the selections for after the edits have been
 *  applied. */
function adjustSelections
  ( lines: string[], selections: Selection[], edits: Edit[]
  ) : Selection[]
{
  let runningLineGrowth = 0

  for(let edit of edits) 
  {
    const { startLine, endLine } = edit
        , newStartLine = startLine + runningLineGrowth
        , oldLines = lines.slice(startLine, endLine + 1)
        , diff = fd(oldLines.join('\n'), edit.lines.join('\n'))
        , oldLineCount = endLine - startLine + 1
        , newLineCount = edit.lines.length
        , rangeLineGrowth = newLineCount - oldLineCount

    selections = selections.map(s => {
      const points = [s.anchor, s.active]
        .map(p => {
          // For selection points in the edit range, adjust from the diff
          if(p.line >= newStartLine && p.line <= endLine + runningLineGrowth) {
            const oldOffset = offsetAt(oldLines, p.translate(-newStartLine))
                , newOffset = newOffsetFromOld(oldOffset, diff)
            p = positionAt(edit.lines, newOffset).translate(newStartLine)
          }
          // For selection points after the range, adjust with rangeLineGrowth
          else if(p.line > endLine + runningLineGrowth) {
            p = p.translate(rangeLineGrowth)
          }
          return p
        })
      return new Selection(points[0], points[1])
    })

    runningLineGrowth += rangeLineGrowth
  }

  return selections
}


/** Gets the new offset of a position, given and old offset and a diff between
 *  old and new text. */
function newOffsetFromOld(offset: number, diff: Diff) : number 
{
  // Count up chars from parts of the diff until we get to the original offset.
  // Keep count of the delta between old & new text from added & removed chars.
  let runningOffset = 0, delta = 0
  for(let [operation, text] of diff) 
  {
    if(operation !== 1) {
      if(runningOffset + text.length > offset) break
      runningOffset += text.length
    }

    delta += operation * text.length
  }

  return offset + delta
}