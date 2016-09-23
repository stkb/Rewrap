import fileTest from './fileTest'
import makeTest from './makeTest'


suite("Languages", () => 
{
  const languageTest = makeTest(() => testExtension)
  
  languageTest('ahk')
  languageTest('abc')
  languageTest('c')
  languageTest('coffee')
  languageTest('cs')
  languageTest('elm')
  languageTest('go')
  languageTest('ini')
  languageTest('js')
  languageTest('md')
  languageTest('rb')
  languageTest('sh')
  languageTest('xml')
})


function testExtension() 
{
  const ext = this.test.title
  
  return fileTest('languages/data.' + ext, 'languages/expected.80.' + ext)
}