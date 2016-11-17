import * as assert from 'assert'
import makeTest from './makeTest'

import { LineType, lineType, wrapText, wrapLinesDetectingTypes } from '../src/Wrapping'

suite("Wrapping", () => 
{
  suite("wrapLinesDetectingTypes", () => 
  {
    const test = makeTest((input: string[], expected: string[]) => () => {
      assert.deepEqual(wrapLinesDetectingTypes(4, false, input), expected)
    })
      
    test("1 short line", ["a"], ["a"])
    test("2 short lines", ["a", "b"], ["a b"])
    test("1 long line", ["abc def"], ["abc", "def"])
    test("trim 1 space", ["a "], ["a"])
    test("leave 2 spaces", ["a  "], ["a  "])
    test("leave 3 spaces", ["a   "], ["a   "])
    test("Code", ["a", "  bcde"], ["a", "  bcde"])

    suite("Double spacing", () => 
    {
      const test = makeTest((input: string[], expected: string[]) => () => {
        assert.deepEqual(wrapLinesDetectingTypes(6, true, input), expected)
      })
      
      test("Ends with .", ["a.", "b."], ["a.  b."])
      test("Ends with .-space", ["a. ", "b."], ["a.  b."])
      test("Ends with .-2spaces", ["a.  ", "b."], ["a.  ", "b."])
      test("Ends with ?", ["a?", "b."], ["a?  b."])
      test("Ends with !", ["a!", "b."], ["a!  b."])
    })
  })

  suite("lineType", () => 
  {
    const test = makeTest((expected: LineType) => function() { 
      assert.deepEqual(lineType(this.test.title), expected)
    })
    
    test("Plain", new LineType.Wrap(false, false))
    test("  Code", new LineType.NoWrap)
    test("", new LineType.NoWrap())
    test("<xml-min/>", new LineType.NoWrap())
    test('<xml with="attribute">', new LineType.NoWrap())
    test("<s>StartWithTag</s>", new LineType.Wrap(true, true))
    test("@startsWith Tag", new LineType.Wrap(true, false))
    test("2 spaces at end  ", new LineType.Wrap(false, true))
  })

  suite("wrapLines", () => 
  {
    const test = makeTest((input: string, expected: string[]) => () => {
      assert.deepEqual(wrapText(4, input), expected)
    })
      
    test("1 short line", "a", ["a"])
    test("1 long line", "abc def", ["abc", "def"])
    test("trim 1 space", "a ", ["a"])
    test("leave 2 spaces", "a  ", ["a  "])
    test("leave 3 spaces", "a   ", ["a   "])
  })
})