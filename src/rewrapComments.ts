import { languages, Position, Range, Selection, TextDocument, TextEditor, workspace } from 'vscode'
import wrap = require('greedy-wrap')

const getWrappingColumn = () => {
  const editorColumn =
        workspace.getConfiguration('editor').get<number>('wrappingColumn')
    , extensionColumn =
        workspace.getConfiguration('rewrap').get<number>('wrappingColumn')

  return extensionColumn
    || (0 < editorColumn && editorColumn <= 120) && editorColumn
    || 80
}

const getCommentsRegex = (doc: TextDocument) => {
  switch(doc.languageId) {
    case 'c':
    case 'cpp':
    case 'csharp':
    case 'css':
    case 'go':
    case 'java':
    case 'javascript':
    case 'javascriptreact':
    case 'typescript':
    case 'typescriptreact':
      // Single line: //... and multi-line: /*(*)...*/
      return /^[ \t]*\/\*[^]*?\*\/|^[ \t]*\/\/[^]+?$(?!\r?\n[ \t]*\/\/)/mg
    case 'html':
    case 'xml':
    case 'xsl':
      // Only multi-line: <!-- ... -->
      return /^[ \t]*<!--[^]+?-->/mg
    case 'ruby':
      // Single line: #... and multi-line: ^=begin ... ^=end
      return /^=begin[^]+^=end|^[ \t]*#[^]+?$(?!\r?\n[ \t]*#)/mg
  }
}

const getMiddleLinePrefix = 
  (doc: TextDocument, prefix: string): 
  string => 
{
  const singleLine = ['///', '//', '#']
  const customPrefixes = 
    { '/**': ' * '
    }

  const [_, leadingWhitespace, chars, trailingWhiteSpace] = 
        prefix.match(/(\s*)(\S*)(\s*)/)
        
  if(singleLine.indexOf(chars) > -1) return prefix

  else {
    for(let pre of Object.keys(customPrefixes)) {
      if(pre === chars) {
        return leadingWhitespace + customPrefixes[pre] + trailingWhiteSpace
      }
    }
    return leadingWhitespace
  }
}

export default function rewrapComments(editor: TextEditor): Thenable<void> {

  /** A reference to the document we're working on */
  const doc = editor.document

  /** Gets the ranges of all comments in the document. These will be
   * cross-referenced with the selections made in the editor so we know what to
   * work on.
   */
  const getDocumentCommentRanges = () : Range[] => {
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

  /** Rewraps comments with given ranges & selections */
  const fixComments =
    (ranges: Range[], selections: Selection[]):
    Thenable<void> =>
  {
    // If no comments or selections, do nothing
    if(ranges.length === 0 || selections.length === 0) {
       return Promise.resolve()
    }
    // Make sure ranges and selections are in reverse order.
    // Am assuming this is needed to stop the edits messing up existing ranges.
    // TODO: Make sure this assumption is correct.
    else if(ranges.length > 1 && ranges[0].start.isBefore(ranges[1].start)) {
      return fixComments(ranges.reverse(), selections)
    }
    else if(selections.length > 1 && selections[0].start.isBefore(selections[1].start)) {
      return fixComments(ranges, selections.reverse())
    }
    // Process first range in list, and then the rest
    else {
      const [range, ...rest] = ranges
      return fixCommentIfInASelection(range, selections)
        .then(() => fixComments(rest, selections))
    }
  }

  /** If a selection is found that is in the given comment range, fix that
   * comment with regard to the selection */
  const fixCommentIfInASelection =
    (range: Range, selections: Selection[]):
    Thenable<boolean> =>
  {
    const selection = selections.find(s => !!s.intersection(range))
    return selection ? editComment(range, selection) : Promise.resolve(true)
  }

  interface LineInfo {
     prefix: string
     text: string
  }

  /** Used to find characters in a line of a comment that are actual text */
  const textCharRegex = /[\w$]/

  /** Trims a range of lines that don't contain any actual text.
   * Will throw an error of the whole range doesn't contain any text */
  const trimRange = (range: Range): Range => {
    if(!textCharRegex.test(doc.lineAt(range.start).text)) {
      return trimRange(range.with(range.start.translate(1)))
    }
    else if(!textCharRegex.test(doc.lineAt(range.end).text)){
      return trimRange(
        new Range(range.start, range.end.translate(-1, Number.MAX_VALUE))
      )
    }
    else {
      return range
    }
  }

  /** Edits the comment to rewrap the selected lines */
  const editComment =
    (commentRange: Range, selection: Range):
    Thenable<boolean> =>
  {
    // Treat empty selection as for the whole comment
    if(selection.isEmpty) {
      selection = commentRange
    }
    // Else expand selection to whole lines
    else {
      selection = new Range(
        selection.start.line, 0, selection.end.line, Number.MAX_VALUE
      )
      selection = selection.intersection(commentRange)
    }

    // Trim the range of lines that don't actually contain text.
    // We won't process those
    let textRange: Range
    try {
      textRange = doc.validateRange(trimRange(selection))
    }
    catch (e) {
      return Promise.resolve(false)
    }

    // Get prefix + text content for each line we're going to process
    
    const lines: LineInfo[] =
        doc.getText(textRange)
          .split('\n') // \r gets trimmed off later
          .map(line => {
            const splitPoint = line.match(textCharRegex).index
            return { prefix: line.substr(0, splitPoint)
                   , text: line.substr(splitPoint).trim()
                   }
          })

    // If the whole comment was only 1 line we need to create another
    // prefix for possible following lines
    if(lines.length === 1 && textRange.start.isEqual(commentRange.start)) {
      lines.push({ prefix: getMiddleLinePrefix(doc, lines[0].prefix), text: '' })
    }

    // Process the raw text to get the processed text
    const rawText = lines.map(li => li.text).join(' ').trim()
      , wrappedWidth = getWrappingColumn() - lines[0].prefix.length
      , processedText =
          wrap(rawText, { width: wrappedWidth })
          .split('\n')
          .map((text, i) => lines[Math.min(i, lines.length - 1)].prefix + text)
          // greedy-wrap doesn't trim spaces off the ends of lines after
          // processing, but we're going to, to make the results more
          // deterministic.
          .map(s => s.replace(/\s+$/, ""))
          .join('\n') // vscode takes care of converting to \r\n if necessary

    // Do the edit
    return editor.edit(eb => eb.replace(textRange, processedText))
  }


  // Start the work
  return fixComments(getDocumentCommentRanges(), editor.selections)
}
