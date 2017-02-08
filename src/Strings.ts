/** Various string-related functions */
export { 
  containsActualText, prefixSize, textAfterPrefix, 
  trimEnd, trimInsignificantEnd 
}


/** Checks whether a line of text contains actual words etc, not just symbols */
function containsActualText(lineText: string): boolean {
  const text = lineText.trim()
  // This exception needed for Ruby
  return text !== '=begin' && text !== '=end' && /\w/.test(text)
}


/** Gets the display size of a string prefix, taking in to account the render of
 *  tabs for the editor */
function prefixSize(tabSize: number, prefix: string) 
{
  let size = 0;
  for(let i = 0; i < prefix.length; i++) {
    if(prefix.charAt(i) === '\t') {
      size += tabSize - (size % tabSize)
    }
    else {
      size++
    }
  }
  return size
}


/** Gets the text of a line after the prefix (eg '  //') */
function textAfterPrefix
  ( lineText: string
  , prefixRegex: RegExp
  , prefixMaxLength: number = Number.MAX_VALUE
  ): string
{
  const prefixLength = lineText.match(prefixRegex)[0].length
  let textAfter = lineText.substr(Math.min(prefixLength, prefixMaxLength))
    
  // Allow an extra one-space indent
  if(prefixLength > prefixMaxLength && /^ \S/.test(textAfter)) {
    textAfter = textAfter.substr(1)
  }
  
  // Also trim end
  return trimInsignificantEnd(textAfter)
}


/** Trims all whitespace from just the end of the string */
function trimEnd(s: string): string 
{
  return s.replace(/\s+$/, "")
}


/** Trims non-significant whitespace from the end of a string. Non-significant
 *  whitespace is defined as:
 *    No more than 1 space, or
 *    Any whitespace if the string is completely whitespace.
 */
function trimInsignificantEnd(s: string): string
{
  s = s.replace(/\r$/, "")
  
  if(/\S {2,}$/.test(s)) {
    return s;
  }
  else {
    return trimEnd(s)
  }
}