import * as assert from 'assert'
import { TextDocument } from './mocks'
import BasicLanguage from '../src/BasicLanguage'
import wrappingHandler from '../src/documentTypes'


// These two tests were added to help fix an error caused by comment lines with
// no characters after the '//' (issue #11). Might as well leave them in.
suite("BasicLanguage", () => {

  test("// \n ", function() {
    const doc = new TextDocument(this.test.title, "test", "javascript")
        , handler = wrappingHandler(doc)
        , sections = handler.findSections(doc, 4)

    assert.equal(sections.secondary.length, 0)
    assert.equal(sections.primary.length, 1)
    const section = sections.primary[0]
    assert.equal(section.startAt, 0)
    assert.equal(section.endAt, 0)
  })

  test("  //\n  a", function() {
    const doc = new TextDocument(this.test.title, "test", "javascript")
        , handler = wrappingHandler(doc)
        , sections = handler.findSections(doc, 4)

    assert.equal(sections.secondary.length, 1)
    assert.equal(sections.primary.length, 1)
    const section = sections.primary[0]
    assert.equal(section.startAt, 0)
    assert.equal(section.endAt, 0)
  })
})