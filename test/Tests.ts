export { makeTestFunction }

import * as assert from 'assert'
import { Selection } from 'vscode'

import DocumentProcessor, { Edit, WrappingOptions } from '../src/DocumentProcessor'
import { getEditsAndSelections } from '../src/Main'

type TestData = 
  { input:
      { lines: string[]
      , selections: Selection[]
      }
  , expected:
      { lines: string[]
      , selections: Selection[]
      }
  }


function makeTestFunction
  ( documentProcessor: DocumentProcessor, options: WrappingOptions 
  ) : (...lines: string[]) => void
{
  return function(...data: string[]) {
    const title = data.filter((s, i) => i % 2 == 0 && s != null).join('\\n')
    test(title, function() {
      testWrapping(documentProcessor, options, data)
    })
  }
}


function testWrapping
  ( processor: DocumentProcessor, options: WrappingOptions, data: string[]
  ) : void
{
  const { input, expected } = extractTestData(data)
  
  const [edits, selections] = 
        getEditsAndSelections(processor, input.lines, input.selections, options)

  const output = { lines: applyEdits(edits, input.lines), selections }

  assert.deepEqual(output.lines, expected.lines)
  assert.deepEqual(output.selections, expected.selections)
}


function extractTestData
  ( data: string[]
  ) : TestData
{
  if(data.length % 2 !== 0) {
    throw "Invalid test data: there must be an even number of lines"
  }

  // Get input lines from even lines and expected lines from odd
  const testData: TestData = 
    { input: extractSelections(data.filter((s, i) => i % 2 == 0 && s != null))
    , expected: extractSelections(data.filter((s, i) => i % 2 == 1 && s != null))
    }

  return testData
}


function applyEdits
  ( edits: Edit[], inputLines: string[]
  ) : string[]
{
  const outputLines = Array.from(inputLines) 
  edits.reduceRight
    ( (_, e) => 
        outputLines.splice(e.startLine, e.endLine - e.startLine + 1, ...e.lines)
    , null
    )
  return outputLines
}


function extractSelections
  ( lines: string[] 
  ) : { lines: string[], selections: Selection[] }
{
  return {
    lines,
    selections: 
      [ new Selection
        ( 0, 0, lines.length - 1, lines[lines.length - 1].length
        ) 
      ]
  }
}