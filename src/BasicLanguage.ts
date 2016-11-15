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
  
  findSections(doc: TextDocument, tabSize: number)
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
              leadingWS + line + '[^]*?$(?!\\r?\\n' + leadingWS + line +')'
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
          , startLine = doc.positionAt(match.index).line
      let endLine = doc.positionAt(match.index + sectionText.length).line

      // Multi-line comments (/* .. */)
      if(multiLinePrefixRegex && sectionText.match(multiLinePrefixRegex)) {
        endLine = adjustMultiLineCommentEndLine(doc, startLine, endLine, end)
        primarySections.push(
          new Section(
            doc, startLine, endLine, 
            /^[ \t]*[#*]?[ \t]*/,
            selectLinePrefixMaker(sectionText),
            new RegExp('^[ \\t]*' + start + '[ \\t]*')
          )
        )
      }
      // Single-line comments (//)
      else if(linePrefixRegex && sectionText.match(linePrefixRegex)) {
        primarySections.push(
          new Section(
            doc, startLine, endLine, new RegExp(leadingWS + line + ws)
          )
        )
      }
      // Other text
      else {
        plainSectionsFromLines(doc, startLine, endLine, tabSize)
          .forEach(s => secondarySections.push(s))
      }
    }

    return { primary: primarySections, secondary: secondarySections }
  }
  
  /** Edits the comment to rewrap the selected lines. If no edit needs doing,
   *  return null */
  editSection
    ( wrappingColumn: number
    , tabSize: number
    , { section, selection }: SectionToEdit
    ): Edit
  {
    const edit = 
            super.editSection(wrappingColumn, tabSize, { section, selection })
    return edit
  }
}


type CommentMarkers = { start?: string, end?: string, line?: string }

/** Adjusts the end line index of a multi-line comment section. Excludes the 
 *  last line if it's just an end-comment marker with no text before it. */
function adjustMultiLineCommentEndLine
  ( doc: TextDocument
  , startLine: number
  , endLine: number
  , endPattern: string
  ): number
{
  // We can't have a section less than 1 line
  if(endLine === startLine) {
    return endLine
  }
  else {
    const lineText = doc.lineAt(endLine).text
        , match = lineText.match(endPattern)
    if(match && !containsActualText(lineText.substr(0, match.index)))
    {
      return endLine - 1
    }
    else return endLine
  }
}

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


/** Separates a plain text section, further into multiple sections,
 *  distinguished by line indent. */
function plainSectionsFromLines
  ( doc: TextDocument, startLine: number, endLine: number, tabSize: number
  ) : Section[]
{
  const sections = [] as Section[]

  for(var i = startLine + 1; i <= endLine; i++) {
    if(normalizedIndent(doc, i, tabSize) !== normalizedIndent(doc, startLine, tabSize)) {
      sections.push(new Section(doc, startLine, i - 1))
      startLine = i
    }
  }

  sections.push(new Section(doc, startLine, endLine))
  return sections
}


/** Indents of 0 or 1 space are counted as the same indent level, also 2-3
 *  spaces, 4-5 spaces etc. Tab render width is taken into account. */
function normalizedIndent(doc: TextDocument, lineIndex: number, tabSize: number) 
{
  const line = doc.lineAt(lineIndex)
      , indentChars = line.text.substr(0, line.firstNonWhitespaceCharacterIndex)
      , indentSize = prefixSize(tabSize, indentChars)
  return Math.floor(indentSize / 2)
}