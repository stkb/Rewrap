import * as assert from 'assert'
import * as Tests from './Tests'

import { WrappingOptions } from '../src/DocumentProcessor'
import MarkdownProcessor from '../src/Parsers/Markdown'
import { getEditsAndSelections } from '../src/Main'


suite("Markdown:", () => 
{
  const test = 
          Tests.makeTestFunction
            ( new MarkdownProcessor()
            , { wrappingColumn: 6, tabSize: 2, doubleSentenceSpacing: false }
            )
      , _ = null as string

  suite("Basics:", () => 
  {
    test
      ( "" , ""
      )
      
    test
      ( "a" , "a"
      )

    test
      ( "abc def" , "abc"
      ,  _        , "def"
      )

    test
      ( "a" , "a b"
      , "b" ,  _
      )

    test
      ( "a" , "a"
      , ""  , ""
      , "b" , "b"
      )
  })

  suite("Headings:", () =>
  {
    // In Markdown there must be whitespace between the '#' and the text. They
    // must be on one line, so long headings should be ignored.
    test
      ( "# abc def" , "# abc def"
      )

    test
      ( "# abc def" , "# abc def"
      , "abc def"   , "abc"
      ,  _          , "def"
      )
  })
    
  suite("Lists:", () => 
  {
    test
      ( "- a" , "- a"
      )
    
    test
      ( "- abc def" , "- abc"
      ,  _          , "  def"
      )
    
    test
      ( "- a"   , "- a"
      , "  - b" , "  - b"
      )
    
    test
      ( "- abc def" , "- abc"
      , "  - g h"   , "  def"
      ,  _          , "  - g"
      ,  _          , "    h"
      )
  })
  
  suite("Blockquotes:", () => 
  {
    // Blockquotes can be written with a '>' on either just tehe first line or
    // on all lines.

    test
      ("> a" , "> a"
      )
    
    test
      ( "> abc def" , "> abc"
      ,  _          , "> def")
    
    test
      ( "> abc" , "> abc"
      , "> def" , "> def"
      )
    
    // Don't remember why the expected results from the next two tests are as
    // they are. I think for the first one, it assumes you only want the '>' on
    // the first line; and for the second, since the first two lines have a '>',
    // it assumes all the lines should have one.

    test
      ( "> abc" , "> abc"
      , "def"   , "def"
      )
    
    test
      ( "> abc" , "> abc"
      , "> def" , "> def"
      , "ghi"   , "> ghi"
      )
  })
})