import { makeTestFunction } from './Tests'
import { fromLanguage } from '../src/documentTypes'

suite("Selections:", () =>
{
  const test = 
          makeTestFunction
          ( fromLanguage('c')
          , { wrappingColumn: 4, tabSize: 2, doubleSentenceSpacing: false }
          )
      , __ = null as string

  test
    ( "«a b c d»" , "«a b"
    ,  __         , "c d»"
    )

  test
    ( "a b« »c d" , "a b«"
    ,  __         , "»c d"
    )

  test
    ( "a« b c »d" , "a« b"
    ,  __         , "c »d"
    )

  // Active before anchor
  test
    ( "»a b c d«" , "»a b"
    ,  __         , "c d«"
    )

  test
    ( "a b »c d«" , "a b"
    ,  __         , "»c d«"
    )

  // In comments
  test
    ( "// a" , "// a"
    , "//«»" , "//«»"
    , "// b" , "// b"
    )
  
  test
    ( "// a b"   , "// a b"
    , "// «c d»" , "// «c"
    ,  __        , "// d»"
    )
})