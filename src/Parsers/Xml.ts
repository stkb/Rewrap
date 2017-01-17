import DocumentProcessor from '../DocumentProcessor'
import * as DocumentTypes from '../DocumentTypes'
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
  constructor(public embeddedLanguages: boolean) 
  {
    super()
  }

  findSections
    ( docLines: string[], tabSize: number, 
    ): Section[]
  {
    let state : LineState = new InWhitespace()
      , row: number
    const sections = [] as Section[]
    
    for(row = 0; row < docLines.length; row++) 
    {
      state = getStateFromLineBegin(docLines, row, state, sections)

      state = checkIfCommentEndsOnThisLine(docLines, row, state, sections)


      if(this.embeddedLanguages && state instanceof InParagraph) {
        const contentType = scriptOrCssStartsOnThisLine(docLines[row])
        if(contentType) {
          sections.push(Section.fromDocument(docLines, state.start, row, false))
          row += parseEmbeddedLanguage(tabSize, contentType, row + 1, docLines.slice(row + 1), sections)
          state = new InWhitespace()
        }
      }    
    }
    
    if(state instanceof InParagraph) {
      sections.push(Section.fromDocument(docLines, state.start, row - 1, false))
    }

    return sections
  }
}
function scriptOrCssStartsOnThisLine(line: string): string
{
  const match = line.match(/<(SCRIPT|STYLE)\b/i)
  return match && match[1].toUpperCase()
}


/** Returns the number of lines consumed. */
function parseEmbeddedLanguage
  ( tabSize: number
  , contentType: string
  , startRow: number
  , lines: string[]
  , sections: Section[]
  ): number
{
  const endTagRegex = new RegExp('</'+ contentType + '\\b', 'i')
  
  let endRow = lines.findIndex(s => !!s.match(endTagRegex))
  // If end tag isn't found it probably hasn't been typed yet. So we reat all
  // the remaining lines as the embedded language.
  if(endRow == -1) endRow = lines.length
  
  const language = contentType == "STYLE" ? "css" : "javascript"
      , processor = DocumentTypes.fromLanguage(language)
      , newSections = 
          processor.findSections(lines.slice(0, endRow), tabSize)
                      .map(s => ({...s, startAt: s.startAt + startRow}))

  sections.splice(sections.length, 0, ...newSections)
  return endRow
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
      Section.fromDocument(
        docLines,
        state.start,
        row,
        false,
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
      sections.push(Section.fromDocument(docLines, prevState.start, row - 1, false))
      return new InComment(row)
    }
    else if(lineText.trim() === '') {
      sections.push(Section.fromDocument(docLines, prevState.start, row - 1, false))
      return new InWhitespace()
    }
    else if(Math.abs(prevState.indent - lineIndent) >= 2) {
      sections.push(Section.fromDocument(docLines, prevState.start, row - 1, false))
      return new InParagraph(row, lineIndent)
    }
  }
  return prevState
}