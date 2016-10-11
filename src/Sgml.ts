import { TextDocument, TextLine } from 'vscode'
import DocumentProcessor from './DocumentProcessor'
import Section from './Section'

class InComment { 
  constructor(public start: number) {}
}
class InParagraph { 
  constructor(public start: number, public indent: number) {}
}
class InWhitespace {
  private __InWhiteSpace = true // Discriminator field; TypeScript issue#10934
}

type LineState = InComment | InParagraph | InWhitespace

/** Processor for xml & html files */
export default class Sgml extends DocumentProcessor
{
  findSections(doc: TextDocument)
    : { primary: Section[], secondary: Section[] } 
  {
    let state : LineState = new InWhitespace()
      , row: number
    const sections = [] as Section[]
    
    for(row = 0; row < doc.lineCount; row++) {
      const line = doc.lineAt(row)
          , lineIndent = line.firstNonWhitespaceCharacterIndex

      state = getStateFromLineBegin(doc, row, line, lineIndent, state, sections)

      state = checkIfCommentEndsOnThisLine(doc, row, line, state, sections)
    }
    
    if(state instanceof InParagraph) {
      sections.push(new Section(doc, state.start, row - 1))
    }

    return { primary: sections, secondary: []}
  }
}


function checkIfCommentEndsOnThisLine
  ( doc: TextDocument
  , row: number
  , line: TextLine
  , state: LineState
  , sections: Section[]
  ): LineState 
{
  if(state instanceof InComment && line.text.match(/-->/)) {
    sections.push(
      new Section(
        doc, state.start, row, 
        /^[ \t]*/, 
        flp => flp.match(/^[ \t]*/)[0],
        /^[ \t]*<!--[ \t]*/ 
      )
    )
    return new InWhitespace()
  }
  else return state
}

function getStateFromLineBegin
  ( doc: TextDocument
  , row: number
  , line: TextLine
  , lineIndent: number
  , prevState: LineState
  , sections: Section[]
  ): LineState 
{
  if(prevState instanceof InWhitespace) {
    if(line.text.match(/^[ \t]*<!--/)) {
      return new InComment(row)
    }
    else if(!line.isEmptyOrWhitespace) {
      return new InParagraph(row, lineIndent)
    }
  }
  else if(prevState instanceof InParagraph) {
    if(line.text.match(/^[ \t]*<!--/)) {
      sections.push(new Section(doc, prevState.start, row - 1))
      return new InComment(row)
    }
    else if(line.isEmptyOrWhitespace) {
      sections.push(new Section(doc, prevState.start, row - 1))
      return new InWhitespace()
    }
    else if(Math.abs(prevState.indent - lineIndent) >= 2) {
      sections.push(new Section(doc, prevState.start, row - 1))
      return new InParagraph(row, lineIndent)
    }
  }
  return prevState
}