import * as assert from 'assert'
import * as Tests from './Tests'

import Xml from '../src/Parsers/Xml'


suite("Html:", () => 
{
  const test = 
          Tests.makeTestFunction
            ( new Xml(true)
            , { wrappingColumn: 16, tabSize: 2, doubleSentenceSpacing: false }
            )
      , _ = null as string

  suite("Embedded JS:", () =>
  {
    test
      ( "<script>"  , "<script>"
      , "  // a"    , "  // a b"
      , "  // b"    , "</script>"
      , "</script>" , _
      )
    test
      ( "<notscript>"  , "<notscript>"
      , "  // a"       , "  // a // b"
      , "  // b"       , "</notscript>"
      , "</notscript>" , _
      )
  })
  suite("Embedded CSS:", () =>
  {
    test
      ( "<style>"   , "<style>"
      , "  /* a"    , "  /* a b */"
      , "     b */" , "</style>"
      , "</style>"  , _
      )
  })
})