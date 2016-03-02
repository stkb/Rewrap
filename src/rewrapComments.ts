import wrap = require('greedy-wrap')

import { languages, Position, Range, Selection, TextDocument, TextEditor, workspace } from 'vscode'

import { LanguageInfo, docLanguage, plainText, rangesRegex } from './languageInfo'


/** Function called by the rewrap.rewrapComment command */
export default function rewrapComments(editor: TextEditor): Thenable<void> 
{
  const doc = editor.document
      , docLang = docLanguage(doc)

  let selections: Range[] = editor.selections
    , commentRanges = getDocumentRanges(doc, rangesRegex(docLang))
    , plainTextRangesRaw = getDocumentRanges(doc, rangesRegex(plainText))
    , plainTextRanges = []
  
  // plainTextRangesRaw consists of a crude list of document ranges, separated
  // only by blank lines. We need to adjust them to make sure they don't
  // intersect with the comment ranges.
  while(plainTextRangesRaw.length) {
    const ptRange = plainTextRangesRaw.pop()
    const cRangeIn = commentRanges.find(cr => !!cr.intersection(ptRange))
    if(cRangeIn) {
      if(cRangeIn.start.line > ptRange.start.line) {
        plainTextRanges.push(new Range(ptRange.start.line, 0, cRangeIn.start.line - 1, Number.MAX_VALUE))
      }
      if(cRangeIn.end.line < ptRange.end.line) {
        plainTextRangesRaw.push(new Range(cRangeIn.end.line + 1, 0, ptRange.end.line, Number.MAX_VALUE))
      }
    }
    else {
      plainTextRanges.push(ptRange)
    }
  }

  // All ranges need to be sorted into reverse document order, so that edits
  // lower in the document are made first, so that they don't invalidate the
  // other ranges.
  selections = sortRangesInReverseOrder(selections)
  commentRanges = sortRangesInReverseOrder(commentRanges)
  plainTextRanges = sortRangesInReverseOrder(plainTextRanges)
  
  return fixCommentsAndPlainText(
    editor, commentRanges, plainTextRanges, selections)
}



/** Checks whether a line of text contains actual words etc, not just symbols */
function containsActualText(lineText: string): boolean {
  const text = lineText.trim()
  // This exception needed for Ruby
  return text !== '=begin' && text !== '=end' && /\w/.test(text)
}


/** Edits the comment to rewrap the selected lines */
function editTextRange
  ( editor: TextEditor, lang: LanguageInfo, textRange: Range, selection: Range
  ): Thenable<boolean>
{
  
  // Treat empty selection as for the whole comment/paragraph
  if(selection.isEmpty) {
    selection = textRange
  }
  // In the case of plain text, treat the paragraph as just the selected lines
  else if(lang === plainText && textRange.contains(selection)) {
    textRange = selection
  }
  

  const doc = editor.document
      , commentInfo = getCommentInfo(doc, lang, textRange)
      , { lineRegex, linePrefix, firstLinePrefix } = commentInfo
      , startAt = Math.max(selection.start.line, commentInfo.startAt)
      , endAt = Math.min(selection.end.line, commentInfo.endAt)

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


/** Rewraps comments with given ranges & selections */
function fixCommentsAndPlainText
  ( editor: TextEditor
  , commentRanges: Range[]
  , plainTextRanges: Range[]
  , selections: Range[]
  ): Thenable<void>
{
  if(selections.length === 0) {
      return Promise.resolve()
  }
  
  // Process first selection in list, and then the rest
  else {
    const [selection, ...rest] = selections
        , docLang = docLanguage(editor.document)
    
    const commentsRangesInSelection =
            commentRanges.filter(c => !!c.intersection(selection))
        , plainTextRangesInSelection = 
            plainTextRanges.filter(p => !!p.intersection(selection))
      
    // If selection is in a comment, fix the comment. Otherwise fix as plain
    // text
    const promise = commentsRangesInSelection.length 
            ? fixTextRangesForSelection(
                editor, docLang, selection, commentsRangesInSelection
              ) 
            : fixTextRangesForSelection(
                editor, plainText, selection, plainTextRangesInSelection
              )

    return promise.then(() => 
      fixCommentsAndPlainText(editor, commentRanges, plainTextRanges, rest))
  }
}

function fixTextRangesForSelection
  ( editor: TextEditor
  , lang: LanguageInfo
  , selection: Range
  , textRanges: Range[]
  ): Thenable<{}> 
{
  if(textRanges.length) {
    const [comment, ...rest] = textRanges
    return editTextRange(editor, lang, comment, selection)
      .then(() => fixTextRangesForSelection(editor, lang, selection, rest))
  }
  else {
    return Promise.resolve(null)
  }
}


/** Gets a range of info for a comment */
function getCommentInfo
  ( doc: TextDocument, lang: LanguageInfo, textRange: Range
  ): CommentInfo
{
  const startLine = textRange.start.line
      , endLine = textRange.end.line
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
function getDocumentRanges(doc: TextDocument, regex: RegExp): Range[] 
{
  const text = doc.getText()
    , ranges = []
  let match

  while(match = regex.exec(text)) {
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

/** Sorts an array of ranges into reverse document order */
function sortRangesInReverseOrder(ranges: Range[]) {
  return ranges.sort((s1, s2) => s1.start.isAfter(s2.start) ? -1 : 1)
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