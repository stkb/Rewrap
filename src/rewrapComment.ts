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
  
export default function rewrapComment(editor: TextEditor) {
  
  /** A reference to the document we're working on */
  const doc = editor.document
    
  /** Gets the ranges of all comments in the document. These will be 
   * cross-referenced with the selections made in the editor so we know what to
   * work on.
   */ 
  const getDocumentCommentRanges = () : Range[] => {
    const text = doc.getText()
      , cCommentsRegex = 
          /^[ \t]*\/\*[\s\S]*?\*\/|^[ \t]*\/\/[\s\S]+?$(?!\r?\n[ \t]*\/\/)/mg
      , ranges = []
    let match
    
    while(match = cCommentsRegex.exec(text)) {
      const start = doc.positionAt(match.index)
        , end = doc.positionAt(match.index + match[0].length)
      ranges.push(new Range(start, end))
    }
    
    return ranges
  }
  
  /** Rewraps comments with given ranges & selections */
  const fixComments
    = (ranges: Range[], selections: Selection[])
    : Thenable<void> => 
  {
    // If no comments or selections, do nothing
    if(ranges.length === 0 || selections.length === 0) {
       return
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
  const fixCommentIfInASelection
    = (range: Range, selections: Selection[])
    : Thenable<boolean> =>
  {
    const selection = selections.find(s => !!s.intersection(range))
    return selection ? editComment(range, selection) : Promise.resolve(true)
  }
  
  interface LineInfo {
     prefix: string
     text: string
  }
  
  const getMiddleLinePrefix = (firstLinePrefix: string): string => {
    // This is gonna be hacky
    const leadingWhitespace = firstLinePrefix.match(/\s*/)[0]
    if(firstLinePrefix.includes("/**")) return leadingWhitespace + " * "
    if(firstLinePrefix.includes("//")) return leadingWhitespace + "// "
    return leadingWhitespace  
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
  const editComment 
    = (commentRange: Range, selection: Range)
    : Thenable<boolean> => 
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
      lines.push({ prefix: getMiddleLinePrefix(lines[0].prefix), text: ''})
    }
    
    // Process the raw text to get the processed text
    const rawText = lines.map(li => li.text).join(' ')
      , wrappedWidth = getWrappingColumn() - lines[0].prefix.length
      , processedText = 
          wrap(rawText, { width: wrappedWidth })
          .split('\n')
          .map((text, i) => lines[Math.min(i, lines.length - 1)].prefix + text)
          .join('\n') // vscode takes care of converting to \r\n if necessary

    // Do the edit
    return editor.edit(
      editBuilder => editBuilder.replace(textRange, processedText)
    )
  }

  // Start the work 
  fixComments(getDocumentCommentRanges(), editor.selections)
}
