import * as assert from 'assert'
import makeTest from './makeTest'
import { TextDocument } from './mocks'
import BasicLanguage from '../src/BasicLanguage'
import Markdown from '../src/Markdown'

import wrappingHandler from '../src/documentTypes'

suite("Document Types", () => {

  // All vscode-supported languages need to be tested, but that should be done
  // as part of the language integration tests
  suite("Return javascript processor for `javascript` language id", () => {

    const expected = new BasicLanguage(
            { start: '\\/\\*\\*?', end: '\\*\\/', line: '\\/{2,3}' })

    test("With no extension", () => {
      const doc = new TextDocument("", 'untitled', 'javascript')
          , handler = wrappingHandler(doc)
      
      assert.deepEqual(handler, expected)
    })

    test("With .md extension", () => {
      const doc = new TextDocument("", 'test.md', 'javascript')
          , handler = wrappingHandler(doc)
      
      assert.deepEqual(handler, expected)    
    })
  })

  test("Return Haskell processor for `plaintext` language id and 'hs' extension", () => {

    const expected = new BasicLanguage({ start: '{-', end: '-}', line: '--' })
        , doc = new TextDocument("", 'test.hs', 'plaintext')
        , handler = wrappingHandler(doc)

    assert.deepEqual(handler, expected)
  })

  test("Return Markdown processor for unknown language and extension", () => {

    const doc = new TextDocument("", 'test.abc', 'plaintext')
        , handler = wrappingHandler(doc)

    assert.ok(handler instanceof Markdown)
  })

})