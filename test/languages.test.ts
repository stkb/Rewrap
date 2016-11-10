import fileTest from './fileTest'
import makeTest from './makeTest'

/**
 * The language files are listed here instead of iterating though a directory,
 * to enable skipping/singling-out of each language test.
 */
suite("Languages", async () => 
{
  const languageTest = makeTest(() => testExtension)

  languageTest('abc')
  languageTest('ahk')
  languageTest.skip('c')
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


async function testExtension()
{
  this.test.timeout(5000)
  const ext = this.test.title
  return fileTest('languages/data.' + ext, 'languages/expected.80.' + ext)
}