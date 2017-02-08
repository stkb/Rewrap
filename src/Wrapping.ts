import wrap = require('word-wrap')
import { containsActualText, trimInsignificantEnd } from './Strings'

export { LineType, wrapText, wrapLinesDetectingTypes }
export default exports


/** The main function that takes text and wraps it. */
function wrapLinesDetectingTypes
  ( wrappingWidth: number
  , lineType: (line: string) => LineType
  , doubleSentenceSpacing: boolean
  , lines: string[]
  ) : string[]
{
  return (
    lines
      .map(text => ({ text, type: lineType(text) }))
      .apply(ls => addSpaceToLinesEndingASentence(doubleSentenceSpacing, ls))
      .apply(groupLinesWithTypes)
      .flatMap(({text, wrap}) => wrap ? wrapText(wrappingWidth, text) : [text])
  )
}


/** Iterate though a list of lines. If a line ends in . ? or !, add an extra
 *  space on the end. This will then become a double space between sentences
 *  when the text is wrapped. This isn't done to the last line since it's not
 *  needed.
 *  @param doubleSentenceSpacing If this is false, this function is a no-op.
 */
function addSpaceToLinesEndingASentence
  ( doubleSentenceSpacing: boolean
  , lines: { text: string, type: LineType }[]
  ) : { text: string, type: LineType }[]
{
  lines =
    lines
      .map(({text, type}, i) => {
          if(doubleSentenceSpacing && i < lines.length - 1 && /[.?!]$/.test(text)) {
            text += ' '
          } 
          return { text, type }
        })
  return lines
}


/** Calls an external js library to wrap a text string. Then splits the string
 *  into lines. */
function wrapText(wrappingWidth: number, text: string): string[]
{
  return wrap(text, { width: wrappingWidth, indent: '' }).split(/\s*\n/)
}


/** Groups lines with the same LineType, to be wrapped together */
function groupLinesWithTypes
  ( lines: { text: string, type: LineType }[]
  ): { text: string, wrap: boolean }[]
{
  const groups = [] as { text: string, wrap: boolean }[]
  
  while(lines.length > 0) {
    let i = 0
    for(; i < lines.length; i++) {
      const { type } =  lines[i]
      
      if(type.breakBefore && i > 0) {
        break
      }
      else if(type.breakAfter) {
        i = i + 1
        break
      }
    }
    
    // lines becomes the tail
    const doWrap = lines[i - 1].type instanceof LineType.Wrap
        , sectionLines = lines.splice(0, i).map(({text}) => text)
        , group = { text: sectionLines.join(' '), wrap: doWrap }
    
    groups.push(group)
  }
  
  return groups
}


/** Types that represent how individual lines within a comment or paragraph
 *  should be handled */
abstract class LineType 
{
  /** The line requires a line break before it */
  public get breakBefore() {
    return this._breakBefore
  }
  protected _breakBefore: boolean
  
  /** The line requires a line break after it */
  public get breakAfter() {
    return this._breakAfter
  }
  protected _breakAfter: boolean
  
  
  /** Represents a line that should be wrapped */
  static Wrap = class extends LineType {
    constructor(breakBefore: boolean, breakAfter: boolean) {
      super()
      this._breakBefore = breakBefore
      this._breakAfter = breakAfter
    }
  }
  
  /** Represents a line that sould not be wrapped */
  static NoWrap = class extends LineType {
    constructor() {
      super()
      this._breakBefore = true
      this._breakAfter = true
    }
  }
}