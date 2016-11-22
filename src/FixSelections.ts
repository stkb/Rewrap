import { Position, Range, Selection } from 'vscode'
const fd = require('fast-diff')
type Diff = [number, string][]

import { Edit } from './DocumentProcessor'
import { offsetAt, positionAt } from './Position'


export default adjustSelections


function adjustSelections
  ( lines: string[], selections: Selection[], edits: Edit[]
  ) : Selection[]
{
  let runningLineGrowth = 0

  for(var i = 0; i < edits.length; i++) {
    const edit = edits[i]
        , { startLine, endLine } = edit
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


// function getText
//   ( lines: string[], startLine: number, endLine: number
//   ) : string
// {
//   return lines.slice(startLine, endLine + 1).join('\n')
// }


function newOffsetFromOld(offset: number, diff: Diff) : number 
{
  let count = 0, delta = 0
  for(var i = 0; i < diff.length; i++) {
    let [operation, text] = diff[i]

    if(operation !== 1) {
      if(count + text.length > offset) break
      count += text.length
    }

    delta += operation * text.length
  }

  return offset + delta
}