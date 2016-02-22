/** A jsdoc-style ^comment on a single line.^ When this gets wrapped, new lines will have a '*' prefix added, that lines up with the first '*' of the first line, and the text will be indented the same amount the first line. */

  /**
   * Represents a text document^^, such as a source file. Text documents have
   * [lines](#TextLine) and knowledge about an underlying resource like a file.
   */

 /**
  * We need to be able to rewrap jsdoc comments^^; lines that start with documentation tags, eg @readonly, need to be wrapped separately. 
  * Tags that appear in the middle of a line, like that last one, should be wrapped within the paragraph as normal.
  * @param {string} title - The title of the book.
  * @param {string} author - The author of the book.
  */