import { Range, TextDocument } from 'vscode'
import Section, { SectionToEdit } from './Section'
import { prefixSize, trimEnd, trimInsignificantEnd } from './Strings'
import { wrapLinesDetectingTypes } from './Wrapping'

/** Base class for different sorts of document handlers */ 
abstract class DocumentProcessor 
{
  abstract findSections(doc: TextDocument)
    : { primary: Section[], secondary: Section[] }
    
  editSection
    ( wrappingColumn: number
    , tabSize: number
    , sectionToEdit: SectionToEdit
    ): Edit
  {
    const { section, selection } = sectionToEdit
        , wrappingWidth = 
            wrappingColumn - prefixSize(tabSize, section.linePrefix)

    const lines =
      linesToWrap(section, selection)
        .map(trimInsignificantEnd)
        .apply(ls => wrapLinesDetectingTypes(wrappingWidth, ls))
        .map((line, i) => {
          const prefix = 
            selection.start.line + i === section.startAt
              ? section.firstLinePrefix
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
}

export default DocumentProcessor


/** Represents an edit to be made to a document */
export interface Edit { startLine: number, endLine: number, lines: string[] }


/** Gets the lines that need wrapping, given a section and selection range */
function linesToWrap(section: Section, selection: Range): string[]
{
  return section.lines
    .filter((line, i) => {
      const row = section.startAt + i
      return row >= selection.start.line && row <= selection.end.line
    })
}