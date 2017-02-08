import { Range } from 'vscode'
import Section, { SectionToEdit } from './Section'
import { containsActualText, prefixSize, trimEnd, trimInsignificantEnd } from './Strings'
import { LineType, wrapLinesDetectingTypes } from './Wrapping'


/** Represents wrapping options */
export interface WrappingOptions { 
  wrappingColumn: number, 
  tabSize: number,
  doubleSentenceSpacing: boolean,
}


/** Represents an edit to be made to a document */
export interface Edit { startLine: number, endLine: number, lines: string[] }


/** Base class for different sorts of document handlers */ 
abstract class DocumentProcessor 
{
  abstract findSections
    ( docLines: string[], tabSize: number
    ): Section[]
    
  editSection
    ( options: WrappingOptions
    , sectionToEdit: SectionToEdit
    ): Edit
  {
    const { wrappingColumn, tabSize, doubleSentenceSpacing } = options
        , { section, selection } = sectionToEdit
        , wrappingWidth = 
            wrappingColumn - prefixSize(tabSize, section.linePrefix)

    const lines =
      Section.linesToWrap(section, selection)
        .map(trimInsignificantEnd)
        .apply(ls => 
            wrapLinesDetectingTypes(
              wrappingWidth, this.lineType, doubleSentenceSpacing, ls)
          )
        .map((line, i) => {
          const prefix = 
            selection.start.line + i === section.startAt
              ? (section.firstLinePrefix || section.linePrefix)
              : section.linePrefix
          // If the line is empty then trim all trailing ws from the prefix
          return (line ? prefix : trimEnd(prefix)) + line
         })
      
    return { 
      startLine: selection.start.line,
      endLine: selection.end.line,
      lines,
    }  
  }

  /** Gets the LineType of a line */
  lineType(text: string): LineType 
  {
    if(
      // No text on line
      !containsActualText(text) ||
      
      // After implementing trimInsignificantStart, make this 2 spaces
      // Don't forget ^ priority
      /^[ \t]/.test(text) ||
        
      // Whole line is a single xml tag
      /^<[^!][^>]*>$/.test(text)
    ) {
      return new LineType.NoWrap()
    }
    
    else {
      let breakBefore = false, breakAfter = false
      
      // Start and end xml tag on same line
      if(/^<[^>]+>[^<]*<\/[^>+]>$/.test(text)) {
        [breakBefore, breakAfter] = [true, true]
      }
      
      else {
        // Starts with xml or @ tag
        if(/^[@<]/.test(text)) breakBefore = true
        
        // Ends with (at least) 2 spaces
        if(/  $/.test(text)) breakAfter = true
      }
      
      return new LineType.Wrap(breakBefore, breakAfter)
    }
  }

}

export default DocumentProcessor