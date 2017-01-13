import { Position, Range } from 'vscode'
import { containsActualText, textAfterPrefix } from './Strings'


export { SectionToEdit }


export default class Section
{
  constructor
    ( lines: string[]
    , public startAt: number
    , lineRegex = /^[ \t]*/
    , defaultLinePrefix = (flp: string) => flp
    , firstLineRegex = lineRegex )
  {
    const rawLines = lines
    let linePrefix: string, firstLinePrefix: string

    // Get firstLinePrefix from first line
    firstLinePrefix = rawLines[0].match(firstLineRegex)[0]
    
    // Get linePrefix from the first line after that that has text
    const firstMiddleLineWithText = rawLines.slice(1).find(containsActualText)
    if(firstMiddleLineWithText) {
      linePrefix = firstMiddleLineWithText.match(lineRegex)[0]
    }
    else {
      linePrefix = defaultLinePrefix(firstLinePrefix)
    }
    
    if(linePrefix.length < firstLinePrefix.length 
      || containsActualText(rawLines[0])
    ) {
      const prefixLength = Math.min(linePrefix.length, firstLinePrefix.length)
      linePrefix = linePrefix.substr(0, prefixLength)
      firstLinePrefix = firstLinePrefix.substr(0, prefixLength)
    }
    
    this.lines =
      rawLines
        .map((line, i) => 
          i === 0 
            ? line.substr(linePrefix.length)
            : textAfterPrefix(line, lineRegex, linePrefix.length)
        )
    this.linePrefix = linePrefix
    this.firstLinePrefix = firstLinePrefix
  }
  
  
  lines: string[]
  linePrefix: string
  firstLinePrefix: string

  
  static sectionsInSelections = sectionsInSelections
}


/** Set of data needed to do a wrapping edit; namely the section and the
 *  selection range */
interface SectionToEdit {
  section: Section
  selection: Range
}


/** Gets all the sections that are touched by the given selections */
function sectionsInSelections
  ( primarySections: Section[], secondarySections: Section[], selections: Range[]
  ): SectionToEdit[]
{
  return selections
    .flatMap(sel => {
      const priSectionsInSelection = 
        sectionsInSelection(sel, primarySections)
        
      if(priSectionsInSelection.length) return priSectionsInSelection
      
      const secSectionsInSelection = 
        sectionsInSelection(sel, secondarySections)
      
      if(secSectionsInSelection.length) return secSectionsInSelection
       
      return []
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