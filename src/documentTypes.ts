import { TextDocument } from 'vscode'
import { extname } from 'path'
import DocumentProcessor from './DocumentProcessor'
import BasicLanguage from './BasicLanguage'
import Markdown from './Markdown'
import Sgml from './Sgml'


export { fromDocument, fromLanguage, fromExtension }


/** Gets a DocumentProcessor for a document, taken from its file type */
function fromDocument(doc: TextDocument): DocumentProcessor
{
  return fromLanguage(doc.languageId)
    || fromExtension(extname(doc.fileName))
    || new Markdown()
}

/** Gets a DocumentProcessor for a given language id. Returns null if the id is
 *  not known */
function fromLanguage(id: string): DocumentProcessor
{
  switch(id) 
  {
    case 'ahk':
      return new BasicLanguage({ start: '\\/\\*', end: '\\*\\/', line: ';' })

    case 'bat':
      return new BasicLanguage({ line: '(?:rem|::)' })

    // There can be slight differences in all of these but they're all basically
    // the same
    case 'c':
    case 'csharp':
    case 'cpp':
    case 'css':
    case 'go':
    case 'groovy':
    case 'java':
    case 'javascript':
    case 'javascriptreact':
    case 'json':
    case 'less':
    case 'objective-c':
    case 'scss':
    case 'shaderlab':
    case 'swift':
    case 'typescript':
    case 'typescriptreact':
      return new BasicLanguage({ start: '\\/\\*\\*?', end: '\\*\\/', line: '\\/{2,3}' })

    case 'clojure':
      // todo
      return null

    case 'coffeescript':
      return new BasicLanguage({ start: '###\\*?', end: '###', line: '#' })

    case 'diff':
      // Not sure what this is
      return null

    case 'dockerfile':
    case 'makefile':
    case 'perl':
    case 'r':
    case 'shellscript':
    case 'yaml':
      // These all seem not to have standard multi-line comments
      return new BasicLanguage({ line: '#' })

    // These not provided by vscode
    case 'elm':
    case 'haskell':
    case 'purescript':
      return new BasicLanguage({ start: '{-', end: '-}', line: '--' })

    case 'fsharp':
      return new BasicLanguage({ start: '\\(\\*', end: '\\*\\)', line: '\\/\\/' })

    case 'git-commit':
    case 'git-rebase':
      // These are plain text
      return null


    case 'handlebars':
      // Todo: handlebars template comments:  
      // {{!-- --}} and {{! }}
      return new Sgml()

    case 'html':
    case 'xml':
    case 'xsl':
      return new Sgml()

    case 'ini':
      return new BasicLanguage({ line: '[#;]' })

    case 'jade':
      // Jade block comments are a bit different and might need some more thought
      return new BasicLanguage({ line: '\\/\\/' })

    case 'lua':
      return new BasicLanguage({ start: '--\\[\\[', end: '\\]\\]', line: '--' })

    case 'markdown':
      return new Markdown()
    
    case 'perl6':
    case 'ruby':
      // Todo: multi-line comments in Perl 6
      // https://docs.perl6.org/language/syntax#Comments
      return new BasicLanguage({ start: '^=begin', end: '^=end', line: '#' })

    case 'php':
      return new BasicLanguage({ start: '\\/\\*', end: '\\*\\/', line: '(?:\\/\\/|#)' })

    case 'powershell':
      return new BasicLanguage({ start: '<#', end: '#>', line: '#' })

    case 'python':
      return new BasicLanguage({ start: "('''|\"\"\")", end: "('''|\"\"\")", line: '#' })

    case 'razor':
      // todo
      return null

    case 'rust':
      return new BasicLanguage({ line: '\\/{2}(?:\\/|\\!)?' })

    case 'sql':
      return new BasicLanguage({ start: '\\/\\*', end: '\\*\\/', line: '--' })

    case 'vb':
      return new BasicLanguage({ line: "'" })

    default:
      return null;
  }
}


/** Gets a DocumentProcessor for a given file extension (with period). Return
 *  null if the extension is not known. */
function fromExtension(extension: string): DocumentProcessor
{
  switch(extension) 
  {
    case '.ahk':
      return fromLanguage('ahk')

    case '.cs':
      return fromLanguage('csharp')

    case '.elm':
      return fromLanguage('elm')
    case '.purs':
      return fromLanguage('purescript')
    case '.hs':
      return fromLanguage('haskell')

    case '.sass':
      // Pretend .sass comments are the same as .scss for basic support.
      // Actually they're slightly different.
      // http://sass-lang.com/documentation/file.INDENTED_SYNTAX.html
      return fromLanguage('scss')

    default:
      return null
  }
}