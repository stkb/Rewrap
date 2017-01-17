import { Position, Range } from 'vscode'
import { containsActualText, textAfterPrefix } from './Strings'

interface Section {
  startAt: number
  lines: string[]
  isSecondary: boolean
  linePrefix: string
  firstLinePrefix?: string
}

abstract class Section
{
  static fromDocument = fromDocument
  static section = section
  static sectionsInSelections = sectionsInSelections
}

export default Section


/** Creates a Section object */
export function section
  ( lines: string[]
  , startAt: number
  , isSecondary: boolean
  , lineRegex = /^[ \t]*/
  , defaultLinePrefix = (flp: string) => flp
  , firstLineRegex = lineRegex
  ): Section
{
  let linePrefix: string, firstLinePrefix: string

  // Get firstLinePrefix from first line
  firstLinePrefix = lines[0].match(firstLineRegex)[0]
  
  // Get linePrefix from the first line after that that has text
  const firstMiddleLineWithText = lines.slice(1).find(containsActualText)
  if(firstMiddleLineWithText) {
    linePrefix = firstMiddleLineWithText.match(lineRegex)[0]
  }
  else {
    linePrefix = defaultLinePrefix(firstLinePrefix)
  }
  
  if(linePrefix.length < firstLinePrefix.length 
    || containsActualText(lines[0])) 
  {
    const prefixLength = Math.min(linePrefix.length, firstLinePrefix.length)
    linePrefix = linePrefix.substr(0, prefixLength)
    firstLinePrefix = firstLinePrefix.substr(0, prefixLength)
  }
  
  const bareLines =
          lines
            .map((line, i) => 
              i === 0 
                ? line.substr(linePrefix.length)
                : textAfterPrefix(line, lineRegex, linePrefix.length)
            )
  return { 
    startAt,
    lines: bareLines,
    isSecondary,
    linePrefix,
    firstLinePrefix
  }
}


/** Creates a Section object from the given document lines and start & end
 *  points. */
export function fromDocument
  ( docLines: string[]
  , startAt: number
  , endAt: number
  , isSecondary: boolean
  , lineRegex = /^[ \t]*/
  , defaultLinePrefix = (flp: string) => flp
  , firstLineRegex = lineRegex
  ): Section
{
  return section(docLines.slice(startAt, endAt + 1), startAt, isSecondary,
                  lineRegex, defaultLinePrefix, firstLineRegex)
}




/** Set of data needed to do a wrapping edit; namely the section and the
 *  selection range */
export interface SectionToEdit {
  section: Section
  selection: Range
}


/** Gets all the sections that are touched by the given selections */
function sectionsInSelections
  ( sections: Section[], selections: Range[]
  ): SectionToEdit[]
{
  return selections
    .flatMap(sel => {
      const sectionsInSel = sectionsInSelection(sel, sections)
          , priSectionsInSel = sectionsInSel.filter(s => !s.section.isSecondary)

      return priSectionsInSel.length ? priSectionsInSel : sectionsInSel
    })
}


/** Gets all the sections that are touched by the given selection */
function sectionsInSelection(selection: Range, sections: Section[])
  : SectionToEdit[]
{
  return sections
    .map(s => sectionAndSelectionIntersection(selection, s))
    .filter(ss => !!ss)
}


/** Gets the intersection of a section and selection */
function sectionAndSelectionIntersection(selection: Range, section: Section)
  : SectionToEdit
{
  const intersection = {
    start: Math.max(section.startAt, selection.start.line),
    end: Math.min(section.startAt + section.lines.length - 1, selection.end.line),
  }
  
  if(intersection.start <= intersection.end) {
    const range = selection.isEmpty 
      ? new Range(section.startAt, 0, section.startAt + section.lines.length - 1, Number.MAX_VALUE)
      : new Range(intersection.start, 0, intersection.end, Number.MAX_VALUE)
    return { section, selection: range }
  }
  else return null
}