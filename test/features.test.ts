import fileTest from './fileTest'
import makeTest from './makeTest'


suite("Features", () => {
  
  const featureTest = makeTest(() => testFeature)
  
  featureTest('basic_single.js')
  featureTest('basic_multi.js')
  featureTest('blank_lines_single.js')
  featureTest('blank_lines_multi.js')
  featureTest('alignment_single.js')
  featureTest('alignment_multi.js')
  featureTest('jsdoc.js')
  featureTest('xmldoc.cs')
  featureTest('normalText.html')
})

function testFeature() 
{
  this.timeout(5000)
  const name = this.test.title
      , input = `features/${name.replace('.', '.input.')}`
      , expected = `features/${name.replace('.', '.expected.')}`
  
  return fileTest(input, expected)
}