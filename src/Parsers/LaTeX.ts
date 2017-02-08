import * as vscode from 'vscode'

import DocumentProcessor, { Edit, WrappingOptions } from '../DocumentProcessor'
import { positionAt, offsetAt } from '../Position'
import { 
  containsActualText, prefixSize, textAfterPrefix, trimEnd, trimInsignificantEnd 
} from '../Strings'
import Section, { SectionToEdit, section } from '../Section'
import { LineType } from '../Wrapping'


export default class LaTeX extends DocumentProcessor
{
  options = {
    start: null as string,
    end: null as string,
    line: '%',
    plainTextAsPrimary: true
  }


  findSections
    ( docLines: string[], tabSize: number
    ): Section[]
  {
    const ws = '[ \\t]*'
        , leadingWS = '^' + ws
        , { start, end, line, plainTextAsPrimary } = this.options
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
        
    const sections = [] as Section[]
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
        sections.push(
          section(
            adjustMultiLineCommentEndLine(sectionLines, end),
            sectionStart,
            false,
            /^[ \t]*[#*]?[ \t]*/,
            selectLinePrefixMaker(sectionText),
            new RegExp('^[ \\t]*' + start + '[ \\t]*')
          )
        )
      }
      // Single-line comments (//)
      else if(linePrefixRegex && sectionText.match(linePrefixRegex)) {
        sections.push(
          section(
            sectionLines, sectionStart, false, new RegExp(leadingWS + line + ws)
          )
        )
      }
      // Other text
      else {
        const plainSections = 
                plainSectionsFromLines([], sectionStart, sectionLines)
        sections.splice(sections.length, 0, ...plainSections)
      }
    }

    return sections
  }

  lineType(line: string) 
  {
    const trimmed = line.trim()
    if(trimmed.endsWith('\\\\') || trimmed.endsWith('\\hline')) {
      return new LineType.Wrap(false, true)
    }
    else {
      return new LineType.Wrap(false, false)
    }
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


function plainSectionsFromLines
  ( sections: Section[], lineIndex: number, lines: string[] 
  ): Section[]
{
  if(lines.length == 0) {
    return sections
  }
  else {
    const [first, ...rest] = lines
    const command = lineCommand(first)

    if(isEmptyCommand(command)) {
      sections.push(createPlainSection(command, lineIndex, [first]))
      return plainSectionsFromLines(sections, lineIndex + 1, rest)
    }
    else {
      let splitPoint = rest.findIndex(l => !!lineCommand(l))
      if(splitPoint == -1) splitPoint = rest.length
      
      sections.push(
        createPlainSection(command, lineIndex, [first, ...rest.slice(0, splitPoint)]))
      
      return plainSectionsFromLines(
        sections, lineIndex + splitPoint + 1, rest.slice(splitPoint))
    }
  }
}

// Listing all of these might be impossible but we can get the main ones
const emptyCommands = ["begin", "documentclass", "section", "subsection", "end"]

/** Returns whether a command should be "empty" (ie have no text content after
 *  it). If this is so, the line immediately afterwards will always start a new
 *  paragraph when wrapping. */
function isEmptyCommand(name: string)
{
  if(!name) return false
  else return emptyCommands.indexOf(name.toLowerCase()) > -1
}


const inlineCommands =
  ["cite", "dots", "emph", "href", "latex", "latexe", "ref", "verb"]

/** If the line starts with a "block" command, return its name. If it starts
 *  with an "inline" or no command, return null. */
function lineCommand(line: string): string 
{
  const match = line.match(/^\s*\\([a-z]+|\S)/)
      , command = match && match[1]

  if(command && inlineCommands.indexOf(command.toLowerCase()) > -1) {
    return null
  }
  else {
    return command
  }
}

function createPlainSection
  ( command: string, lineIndex: number, lines: string[]
  ): Section
{
  const [first, ...rest] = lines
      , regex = /^\s*/
  let firstLinePrefix = first.match(regex)[0]

  if(command) {
    const linePrefix =
            (rest[0] && rest[0].match(regex)[0]) || firstLinePrefix
  
    // If it's a hanging indent we need to adjust the the flp
    if(linePrefix.length > firstLinePrefix.length) {
      firstLinePrefix = first.substr(0, linePrefix.length)
    }

    return {
      startAt: lineIndex,
      firstLinePrefix,
      linePrefix,
      lines: [
        first.substr(linePrefix.length),
        ...rest.map(l => textAfterPrefix(l, regex))
      ]
    }
  }
  else {
    return {
      startAt: lineIndex,
      linePrefix: firstLinePrefix,
      lines: lines.map(l => textAfterPrefix(l, regex))
    }
  }
}

