import * as vscode from 'vscode'
import { Range, TextDocument } from 'vscode'
import DocumentProcessor, { Edit } from './DocumentProcessor'
import { 
  containsActualText, prefixSize, textAfterPrefix, trimEnd, trimInsignificantEnd 
} from './Strings'
import Section, { SectionToEdit } from './Section'
import { wrapLinesDetectingTypes } from './Wrapping'


export default class BasicLanguage extends DocumentProcessor
{
  constructor(public commentMarkers: CommentMarkers) 
  {
    super()
  }
  
  findSections(doc: TextDocument)
    : { primary: Section[], secondary: Section[] }
  {
    const ws = '[ \\t]*'
        , leadingWS = '^' + ws
        , { start, end, line } = this.commentMarkers
        , startOrLine = 
            [start, line].filter(s => !!s).join('|')
        , plainPattern =
            leadingWS + '(?!' + startOrLine + ')\\S[^]*?' +
            '(?=\\n' + leadingWS + '(' + startOrLine  + '|$))'
        , multiLinePattern = 
            start && end && 
              leadingWS + start + '[^]+?' + end
        , singleLinePattern = 
            line &&
              leadingWS + line + '[^]+?$(?!\\r?\\n' + leadingWS + line +')'
        , combinedPattern = 
            [plainPattern, multiLinePattern, singleLinePattern]
              .filter(p => !!p)
              .join('|')
        , combinedRegex = new RegExp(combinedPattern, 'mg')
        
    const multiLinePrefixRegex = 
            start && new RegExp(leadingWS + start)
        , linePrefixRegex =
            line && new RegExp(leadingWS + line)
        
    const primarySections = [] as Section[]
        , secondarySections = [] as Section[]
        , text = doc.getText() + '\n'
    let match: RegExpExecArray

    while(match = combinedRegex.exec(text)) {
      const sectionText = match[0]
          , startAt = doc.positionAt(match.index).line
          , endAt = doc.positionAt(match.index + sectionText.length).line

      if(multiLinePrefixRegex && sectionText.match(multiLinePrefixRegex)) {
        primarySections.push(
          new Section(
            doc, startAt, endAt, 
            /^[ \t]*[#*]?[ \t]*/,
            selectLinePrefixMaker(sectionText),
            new RegExp('^[ \\t]*' + start + '[ \\t]*')
          )
        )
      }
      else if(linePrefixRegex && sectionText.match(linePrefixRegex)) {
        primarySections.push(
          new Section(doc, startAt, endAt, new RegExp(leadingWS + line + ws))
        )
      }
      else {
        secondarySections.push(new Section(doc, startAt, endAt))
      }
    }

    return { primary: primarySections, secondary: secondarySections }
  }
  
  /** Edits the comment to rewrap the selected lines. If no edit needs doing, return null */
  editSection
    ( wrappingColumn: number
    , tabSize: number
    , { section, selection }: SectionToEdit
    ): Edit
  {
    const edit = 
            super.editSection(wrappingColumn, tabSize, { section, selection })
            
    // Final tweak for jsdoc/coffeedoc comments: ignore the last line
    if(edit.lines.length > 1) {
      const lastLine = edit.lines[edit.lines.length - 1].trim()
      if(lastLine.match(/^\*\s+\//) || lastLine.match(/^[#\*]\s+##/)) {
        edit.endLine--
        edit.lines.pop()
      }
    }
    
    return edit
  }
}


type CommentMarkers = { start?: string, end?: string, line?: string }



/** Gets a line prefix maker function for multiline comments. Handles the
 *  special cases of javadoc and coffeedoc */
function selectLinePrefixMaker(sectionText: string)
  : (flp: string) => string
{
  const trimmedText = sectionText.trim()
  if(trimmedText.startsWith('/**') || trimmedText.startsWith('###*')) {
    return (flp => flp.replace(/\S+/, ' * '))
  }
  else {
    return (fpl => fpl.match(/^[ \t]*/)[0])
  }
}