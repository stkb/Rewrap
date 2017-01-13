import * as vscode from 'vscode'

import DocumentProcessor, { Edit, WrappingOptions } from '../DocumentProcessor'
import { positionAt, offsetAt } from '../Position'
import { 
  containsActualText, prefixSize, textAfterPrefix, trimEnd, trimInsignificantEnd 
} from '../Strings'
import Section, { SectionToEdit } from '../Section'
import { wrapLinesDetectingTypes } from '../Wrapping'

type CommentMarkers = { start?: string, end?: string, line?: string }


export default class Standard extends DocumentProcessor
{
  constructor(public commentMarkers: CommentMarkers) 
  {
    super()
  }
  
  findSections
    ( docLines: string[], tabSize: number
    ) : { primary: Section[], secondary: Section[] }
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
        , docText = docLines.join('\n') + '\n'
    let match: RegExpExecArray

    while(match = combinedRegex.exec(docText)) {
      const sectionText = match[0]
          , sectionStart = positionAt(docLines, match.index).line
          , sectionEnd = positionAt(docLines, match.index + sectionText.length).line
          , sectionLines = docLines.slice(sectionStart, sectionEnd + 1)

      // Multi-line comments (/* .. */)
      if(multiLinePrefixRegex && sectionText.match(multiLinePrefixRegex)) 
      {
        primarySections.push(
          new Section(
            adjustMultiLineCommentEndLine(sectionLines, end),
            sectionStart, 
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
            sectionLines, sectionStart, new RegExp(leadingWS + line + ws)
          )
        )
      }
      // Other text
      else {
        plainSectionsFromLines(sectionLines, sectionStart, tabSize)
          .forEach(s => secondarySections.push(s))
      }
    }

    return { primary: primarySections, secondary: secondarySections }
  }
  
  /** Edits the comment to rewrap the selected lines. If no edit needs doing,
   *  return null */
  editSection
    ( options: WrappingOptions
    , { section, selection }: SectionToEdit
    ): Edit
  {
    const edit = 
            super.editSection(options, { section, selection })
    return edit
  }
}



/** Adjusts the end line index of a multi-line comment section. Excludes the 
 *  last line if it's just an end-comment marker with no text before it. */
function adjustMultiLineCommentEndLine
  ( lines: string[]
  , endPattern: string
  ): string[]
{
  // We can't have a section less than 1 line
  if(lines.length == 1) {
    return lines
  }
  else {
    const endLineText = lines[lines.length - 1]
        , match = endLineText.match(endPattern)
    if(match && !containsActualText(endLineText.substr(0, match.index)))
    {
      return lines.slice(0, -1)
    }
    else return lines
  }
}

/** Gets a line prefix maker function for multiline comments. Handles the
 *  special cases of javadoc and coffeedoc */
function selectLinePrefixMaker
  ( sectionText: string
  ) : (flp: string) => string
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
  ( lines: string[], startLine: number, tabSize: number
  ) : Section[]
{
  const sections = [] as Section[]
  let subSectionStart = 0

  for(var i = 1; i < lines.length; i++) {
    if( normalizedIndent(lines[i], tabSize) 
        != normalizedIndent(lines[subSectionStart], tabSize) )
    {
      sections.push(new Section(lines.slice(subSectionStart, i), subSectionStart + startLine))
      subSectionStart = i
    }
  }

  sections.push(new Section(lines.slice(subSectionStart, i), subSectionStart + startLine))
  return sections
}


/** Indents of 0 or 1 space are counted as the same indent level, also 2-3
 *  spaces, 4-5 spaces etc. Tab render width is taken into account. */
function normalizedIndent(lineText: string, tabSize: number) 
{
  const indentChars = lineText.match(/^\s*/)[0]
      , indentSize = prefixSize(tabSize, indentChars)
  return Math.floor(indentSize / 2)
}