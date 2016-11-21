import * as assert from 'assert'
import makeTest from './makeTest'
import { TextDocument, TextEditor } from './mocks'

import { wrapSomething } from '../src/Main'


suite("Markdown", () => 
{
  suite("edit", () => {
    const wrapTest = makeTest(testWrap(6, 2))
    
    wrapTest("Blank document", "", "")
    
    wrapTest("1 short paragraph", "a", "a")
    
    wrapTest("1 long paragraph", "abc def", "abc\ndef")
    
    wrapTest("1 paragraph on 2 short lines", "a\nb", "a b")
    
    wrapTest("2 short paragraphs", "a\n\nb", "a\n\nb")
    
    wrapTest("Long heading ignored", "# abcdef", "# abcdef")
    
    wrapTest("Heading with paragraph", 
      "# abcdef\nabc def", "# abcdef\nabc\ndef")
    
    suite("Lists", () => 
    {
      wrapTest("1 list item short", "- a", "- a")
      
      wrapTest("1 list item long", "- abc def", "- abc\n  def")
      
      wrapTest("1 list item + 1 sub item short", "- a\n  - b", "- a\n  - b")
      
      wrapTest.skip(
        "1 list item long + 1 sub item long",
        "- abc def\n  - g h",
        "- abc\n  def\n  - g\n    h")
    })
    
    suite("Blockquotes", () => 
    {
      wrapTest("1 blockquote short", "> a", "> a")
      
      wrapTest("1 blockquote long", "> abc def", "> abc\n> def")
      
      wrapTest("1 blockquote on 2 lines", "> abc\n> def", "> abc\n> def")
      
      wrapTest("1 blockquote on 2 lines (2)", "> abc\ndef", "> abc\ndef")
      
      wrapTest(
        "1 blockquote on 3 lines", 
        "> abc\n> def\nghi", 
        "> abc\n> def\n> ghi" )
    })
  })
})


function testWrap(wrappingColumn: number, tabSize: number) 
{
  return (
    (input: string, expected: string) => () => 
    {
      const doc = new TextDocument(input, "a.md", "markdown")
          , editor = new TextEditor(doc)
      editor.options.tabSize = tabSize
      
      const options = { wrappingColumn, tabSize, doubleSentenceSpacing: false }
      return wrapSomething(editor, options)
        .then(() => {
          assert.equal(editor.document.getText(), expected)
         })
    }
  )
}