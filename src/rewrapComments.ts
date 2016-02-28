import wrap = require('greedy-wrap')

import { languages, Position, Range, Selection, TextDocument, TextEditor, workspace } from 'vscode'

import { getCommentsRegex, getLanguage } from './languageInfo'


/** Function called by the rewrap.rewrapComment command */
export default function rewrapComments(editor: TextEditor): Thenable<void> 
{
  return fixComments(
    editor, getDocumentCommentRanges(editor.document), editor.selections)
}


/** Checks whether a line of text contains actual words etc, not just symbols */
function containsActualText(lineText: string): boolean {
  const text = lineText.trim()
  // This exception needed for Ruby
  return text !== '=begin' && text !== '=end' && /\w/.test(text)
}


/** Edits the comment to rewrap the selected lines */
function editComment
  ( editor: TextEditor, commentRange: Range, selection: Range
  ): Thenable<boolean>
{
  // Treat empty selection as for the whole comment
  if(selection.isEmpty) {
    selection = commentRange
  }
  
  const commentStart = commentRange.start.line
      , commentEnd = commentRange.end.line
      , selectionStart = selection.start.line
      , selectionEnd = selection.end.line

  const doc = editor.document
      , commentInfo = getCommentInfo(doc, commentStart, commentEnd)
      , { lineRegex, linePrefix, firstLinePrefix } = commentInfo
      , startAt = Math.max(selectionStart, commentInfo.startAt)
      , endAt = Math.min(selectionEnd, commentInfo.endAt)
      
  const buffer = [] as string[]
      , outputLines = [] as string[]
      , wrappingWidth = getWrappingColumn() - linePrefix.length
  
  for(let index = startAt; index <= endAt; index++) {
    const line = doc.lineAt(index).text

    const textAfter = firstLinePrefix && index === startAt ?
            trimEnd(line.substr(linePrefix.length)) :
            getTextAfterPrefix(line, lineRegex, linePrefix.length)
            
    if(containsActualText(textAfter)) {

      // Extra indent        
      if(/^[ \t]/.test(textAfter)) {
        writeBuffer()
        outputLines.push(textAfter)
      }
      // Xml tag and nothing else
      else if(/^<[^>]+>$/.test(textAfter)) {
        writeBuffer()
        buffer.push(textAfter)
        writeBuffer()
      }
      // xml/doc tag
      else if(/^[@<]/.test(textAfter)) {
        writeBuffer()
        buffer.push(textAfter)
      }
      // Otherwise
      else {
        buffer.push(textAfter)
      }
    }
    else {
      writeBuffer()
      outputLines.push(textAfter)
    }
  }
  writeBuffer()

  // Do the actual edit
  if(startAt <= endAt) {
    const replacementRange = new Range(startAt, 0, endAt, Number.MAX_VALUE)
        , processedText = outputLines
            .map((line, i) => {
              const prefix = firstLinePrefix && i === 0 ?
                      firstLinePrefix.substr(0, linePrefix.length) : linePrefix
              return trimEnd(prefix + line)
            })
            // Vscode converts to \r\n if necessary
            .join('\n')
    
    return editor.edit(eb => eb.replace(replacementRange, processedText))
  }
  else {
    return Promise.resolve(false)
  }
  
  function writeBuffer() {
    if(buffer.length) {
      const textToWrap = buffer.join(' ')
          , wrappedLines = wrap(textToWrap, {width: wrappingWidth}).split('\n')
      wrappedLines.forEach(l => outputLines.push(l))    
      buffer.length = 0
    }
  }
}


/** If a selection is found that is in the given comment range, fix that
 *  comment with regard to the selection */
function fixCommentIfInASelection
  ( editor: TextEditor, range: Range, selections: Selection[]
  ): Thenable<boolean>
{
  const selection = selections.find(s => !!s.intersection(range))
  return selection ? 
    editComment(editor, range, selection) : 
    Promise.resolve(true)
}


/** Rewraps comments with given ranges & selections */
function fixComments
  ( editor: TextEditor, ranges: Range[], selections: Selection[]
  ): Thenable<void>
{
  // If no comments or selections, do nothing
  if(ranges.length === 0 || selections.length === 0) {
      return Promise.resolve()
  }
  // Make sure ranges and selections are in reverse order.
  // Am assuming this is needed to stop the edits messing up existing ranges.
  // TODO: Make sure this assumption is correct.
  else if(ranges.length > 1 && ranges[0].start.isBefore(ranges[1].start)) {
    return fixComments(editor, ranges.reverse(), selections)
  }
  else if(selections.length > 1 
    && selections[0].start.isBefore(selections[1].start)) 
  {
    return fixComments(editor, ranges, selections.reverse())
  }
  // Process first range in list, and then the rest
  else {
    const [range, ...rest] = ranges
    return fixCommentIfInASelection(editor, range, selections)
      .then(() => fixComments(editor, rest, selections))
  }
}


/** Gets a range of info for a comment */
function getCommentInfo
  (doc: TextDocument, startLine: number, endLine: number
  ): CommentInfo
{
  const lang = getLanguage(doc)
      , multiLineMatch = lang.start 
          && doc.lineAt(startLine).text
              .match('^[ \\t]*' + lang.start + '[ \\t]*')
              
  let startAt = endLine + 1, endAt = startLine - 1
    , lineRegex, linePrefix
  // Only set if needed: if it's a multiline comment with text on the first line
  let firstLinePrefix
  
  // Multi-line comments
  if(multiLineMatch) {
    const isJavaDoc = multiLineMatch[0].trim() === '/**'
        , isCoffeeDoc = multiLineMatch[0].trim() === '###*'
    // Always allow for a single * or # in the middle line prefix (used in eg
    // javadoc or coffeescript doc). If this causes problems with other
    // languages, can check if it's that sort of comment first
    lineRegex = new RegExp('^[ \\t]*[#*]?[ \\t]*')
    
    for(var line = startLine; line <= endLine; line++) {
      const lineText = doc.lineAt(line).text
      
      if(containsActualText(lineText)) {
        startAt = Math.min(startAt, line)
        endAt = Math.max(endAt, line)
        
        if(line === startLine) {
          firstLinePrefix = multiLineMatch[0]
        }
      }
      
      // Get the template line prefix from the second line
      if(line > startLine && linePrefix === undefined) {
        linePrefix = lineText.match(lineRegex)[0]
        
        // If first line contains text, middle lines not allowed to be indented
        // more (it's otherwise treated as a code sample)
        if(firstLinePrefix) {
          linePrefix = linePrefix.substr(0, firstLinePrefix.length)
        }
      }
    }
    
    // If linePrefix wasn't set (because the comment is only 1 line), create
    // default linePrefix with the same indent as the first/only line.
    if(linePrefix === undefined) {
      linePrefix = multiLineMatch[0].match('^[ \\t]*')[0]
      if(isJavaDoc) {
        linePrefix += ' * ' + multiLineMatch[0].match('[ \\t]*$')[0]
      }
    }
  }
  
  // Line comments
  else {
    lineRegex = new RegExp('^[ \\t]*' + lang.line + '[ \\t]*')
    
    for(var line = startLine; line <= endLine; line++) {
      const lineText = doc.lineAt(line).text
      
      if(containsActualText(lineText)) {
        startAt = Math.min(startAt, line)
        endAt = Math.max(endAt, line)
      
        // Get the template line prefix
        if(linePrefix === undefined) {
          linePrefix = lineText.match(lineRegex)[0]
        }
      }
    }
    
    // If there was no text in the comment, linePrefix won't be set, but that
    // won't matter as we won't be processing the comment anyway
  }
  
  return {
    startAt: startAt,
    endAt: endAt,
    lineRegex: lineRegex,
    linePrefix: linePrefix,
    firstLinePrefix: firstLinePrefix,
  }
}

/** Gets the ranges of all comments in the document. These will be
 *  cross-referenced with the selections made in the editor so we know what to
 *  work on. */
function getDocumentCommentRanges(doc: TextDocument): Range[] {
  const text = doc.getText()
    , commentsRegex = getCommentsRegex(doc)
    , ranges = []
  let match

  while(match = commentsRegex.exec(text)) {
    const start = doc.positionAt(match.index)
      , end = doc.positionAt(match.index + match[0].length)
    ranges.push(new Range(start, end))
  }

  return ranges
}


/** Gets the text of a line after the prefix (eg '  //') */
function getTextAfterPrefix
  (lineText: string,  prefexRegex: RegExp, prefixMaxLength: number): string
{
  const prefixLength = lineText.match(prefexRegex)[0].length
  let textAfter = lineText.substr(Math.min(prefixLength, prefixMaxLength))
    
  // Allow an extra one-space indent
  if(prefixLength > prefixMaxLength && /^ \S/.test(textAfter)) {
    textAfter = textAfter.substr(1)
  }
  
  // Also trim end
  return trimEnd(textAfter)
}


/** Gets the wrapping column (eg 80) from the user's setting */
function getWrappingColumn() {
  const editorColumn =
        workspace.getConfiguration('editor').get<number>('wrappingColumn')
    , extensionColumn =
        workspace.getConfiguration('rewrap').get<number>('wrappingColumn')

  return extensionColumn
    || (0 < editorColumn && editorColumn <= 120) && editorColumn
    || 80
}


/** Trims whitespace from the end of a string */
function trimEnd(s: string) {
  return s.replace(/\s+$/, "")
}


interface CommentInfo {
  /** The line to start processing the comment at. */
  startAt: number
  /** The line to to end processing the comment on (inclusive). */
  endAt: number
  /** Regex to use to separate a line's prefix from the rest of the text */
  lineRegex: RegExp
  /** The standard template line prefix to when re-wrapping the comment */
  linePrefix: string
  /** Prefix for the first line of a multi-line comment. Is only given if
   *  startAt is not modified */
  firstLinePrefix: string
}