import DocumentProcessor from '../DocumentProcessor'
import Section from '../Section'

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
export default class Xml extends DocumentProcessor
{
  findSections
    ( docLines: string[], tabSize: number, 
    ) : { primary: Section[], secondary: Section[] } 
  {
    let state : LineState = new InWhitespace()
      , row: number
    const sections = [] as Section[]
    
    for(row = 0; row < docLines.length; row++) {
      

      state = getStateFromLineBegin(docLines, row, state, sections)

      state = checkIfCommentEndsOnThisLine(docLines, row, state, sections)
    }
    
    if(state instanceof InParagraph) {
      sections.push(new Section(docLines.slice(state.start, row), state.start))
    }

    return { primary: sections, secondary: []}
  }
}


function checkIfCommentEndsOnThisLine
  ( docLines: string[]
  , row: number
  , state: LineState
  , sections: Section[]
  ): LineState 
{
  if(state instanceof InComment && docLines[row].match(/-->/)) {
    sections.push(
      new Section(
        docLines.slice(state.start, row + 1),
        state.start, 
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
  ( docLines: string[]
  , row: number
  , prevState: LineState
  , sections: Section[]
  ): LineState 
{
  const lineText = docLines[row]
      , lineIndent = lineText.match(/^\s*/)[0].length

  if(prevState instanceof InWhitespace) {
    if(lineText.match(/^[ \t]*<!--/)) {
      return new InComment(row)
    }
    else if(lineText.trim() !== '') {
      return new InParagraph(row, lineIndent)
    }
  }
  else if(prevState instanceof InParagraph) {
    if(lineText.match(/^[ \t]*<!--/)) {
      sections.push(new Section(docLines.slice(prevState.start, row), prevState.start))
      return new InComment(row)
    }
    else if(lineText.trim() === '') {
      sections.push(new Section(docLines.slice(prevState.start, row), prevState.start))
      return new InWhitespace()
    }
    else if(Math.abs(prevState.indent - lineIndent) >= 2) {
      sections.push(new Section(docLines.slice(prevState.start, row), prevState.start))
      return new InParagraph(row, lineIndent)
    }
  }
  return prevState
}