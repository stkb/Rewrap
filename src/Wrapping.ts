import wrap = require('greedy-wrap')
import { containsActualText, trimInsignificantEnd } from './Strings'

export { LineType, lineType, wrapText, wrapLinesDetectingTypes }
export default exports


/** The main function that takes text and wraps it. */
function wrapLinesDetectingTypes(wrappingWidth: number, lines: string[])
  : string[]
{
  return (
    lines
      .map(text => ({ text, type: lineType(text) }))
      .apply(groupLinesWithTypes)
      .flatMap(({text, wrap}) =>  wrap ? wrapText(wrappingWidth, text) : [text])
  )
}


/** Gets the LineType of a line */
function lineType(text: string): LineType 
{
  if(
    // No text on line
    !containsActualText(text) ||
    
    // After implementing trimInsignificantStart, make this 2 spaces
    // Don't forget ^ priority
    /^[ \t]/.test(text) ||
      
    // Whole line is a single xml tag
    /^<[^!][^>]*>$/.test(text)
  ) {
    return new LineType.NoWrap()
  }
  
  else {
    let breakBefore = false, breakAfter = false
    
    // Start and end xml tag on same line
    if(/^<[^>]+>[^<]*<\/[^>+]>$/.test(text)) {
      [breakBefore, breakAfter] = [true, true]
    }
    
    else {
      // Starts with xml or @ tag
      if(/^[@<]/.test(text)) breakBefore = true
      
      // Ends with (at least) 2 spaces
      if(/  $/.test(text)) breakAfter = true
    }
    
    return new LineType.Wrap(breakBefore, breakAfter)
  }
}


function wrapText(wrappingWidth: number, text: string): string[]
{
  return (
    wrap(text, {width: wrappingWidth})
      .split('\n')
      .map(trimInsignificantEnd) // trim off extra whitespace left by greedy-wrap
  )
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