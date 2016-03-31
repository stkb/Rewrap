import { TextDocument } from 'vscode'
import DocumentProcessor from './DocumentProcessor'
import Section from './Section'


/** Processor for xml & html files */
export default class Sgml extends DocumentProcessor
{
  findSections(doc: TextDocument)
    : { primary: Section[], secondary: Section[] } 
  {
    class InComment { 
      constructor(public start: number) {}
    }
    class InParagraph { 
      constructor(public start: number, public indent: number) {}
    }
    class InWhitespace {}
    
    let state : InComment | InParagraph | InWhitespace = new InWhitespace()
      , row: number
    const sections = [] as Section[]
    
    for(row = 0; row < doc.lineCount; row++) {
      const line = doc.lineAt(row)
          , lineIndent = line.firstNonWhitespaceCharacterIndex
      let stateCopy = state // This is needed for the TS type guards
      
      if(stateCopy instanceof InWhitespace) {
        if(line.text.match(/^[ \t]*<!--/)) {
          state = new InComment(row)
        }
        else if(!line.isEmptyOrWhitespace) {
          state = new InParagraph(row, lineIndent)
        }
      }
      else if(stateCopy instanceof InParagraph) {
        if(line.text.match(/^[ \t]*<!--/)) {
          sections.push(new Section(doc, stateCopy.start, row - 1))
          state = new InComment(row)
        }
        else if(line.isEmptyOrWhitespace) {
          sections.push(new Section(doc, stateCopy.start, row - 1))
          state = new InWhitespace()
        }
        else if(Math.abs(stateCopy.indent - lineIndent) >= 2) {
          sections.push(new Section(doc, stateCopy.start, row - 1))
          state = new InParagraph(row, lineIndent)
        }
      }
      
      stateCopy = state
      if(stateCopy instanceof InComment) {
        if(line.text.match(/-->/)) {
          sections.push(
            new Section(
              doc, stateCopy.start, row, 
              /^[ \t]*/, 
              flp => flp.match(/^[ \t]*/)[0],
              /^[ \t]*<!--[ \t]*/ 
            )
          )
          state = new InWhitespace()
        }
      }
    }
    
    if(state instanceof InParagraph) {
      sections.push(new Section(doc, state.start, row - 1))
    }

    return { primary: sections, secondary: []}
  }
}