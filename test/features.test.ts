import fileTest from './fileTest'
import makeTest from './makeTest'


suite("Features", () => {
  
  const featureTest = makeTest(() => testFeature)
  
  featureTest('basic_single.js')
  featureTest('basic_multi.js')
  featureTest('blank_lines_single.js')
  featureTest('blank_lines_multi.js')
  featureTest('text_indent_single.js')
  featureTest('text_indent_multi.js')
  featureTest('xmldoc.cs')
})

function testFeature() 
{
  const name = this.test.title
      , input = `features/${name.replace('.', '.input.')}`
      , expected = `features/${name.replace('.', '.expected.')}`
  
  return fileTest(input, expected)
}