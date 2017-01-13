import { TextDocument } from 'vscode'
import { extname } from 'path'
import DocumentProcessor from './DocumentProcessor'
import Standard from './Parsers/Standard'
import Markdown from './Parsers/Markdown'
import Xml from './Parsers/Xml'


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
      return new Standard({ start: '\\/\\*', end: '\\*\\/', line: ';' })

    case 'bat':
      return new Standard({ line: '(?:rem|::)' })

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
      return new Standard({ start: '\\/\\*\\*?', end: '\\*\\/', line: '\\/{2,3}' })

    case 'clojure':
      // todo
      return null

    case 'coffeescript':
      return new Standard({ start: '###\\*?', end: '###', line: '#' })

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
      return new Standard({ line: '#' })

    // These not provided by vscode
    case 'elm':
    case 'haskell':
    case 'purescript':
      return new Standard({ start: '{-', end: '-}', line: '--' })

    case 'fsharp':
      return new Standard({ start: '\\(\\*', end: '\\*\\)', line: '\\/\\/' })

    case 'git-commit':
    case 'git-rebase':
      // These are plain text
      return null


    case 'handlebars':
      // Todo: handlebars template comments:  
      // {{!-- --}} and {{! }}
      return new Xml()

    case 'html':
    case 'xml':
    case 'xsl':
      return new Xml()

    case 'ini':
      return new Standard({ line: '[#;]' })

    case 'jade':
      // Jade block comments are a bit different and might need some more thought
      return new Standard({ line: '\\/\\/' })

    case 'lua':
      return new Standard({ start: '--\\[\\[', end: '\\]\\]', line: '--' })

    case 'markdown':
      return new Markdown()
    
    case 'perl6':
    case 'ruby':
      // Todo: multi-line comments in Perl 6
      // https://docs.perl6.org/language/syntax#Comments
      return new Standard({ start: '^=begin', end: '^=end', line: '#' })

    case 'php':
      return new Standard({ start: '\\/\\*', end: '\\*\\/', line: '(?:\\/\\/|#)' })

    case 'powershell':
      return new Standard({ start: '<#', end: '#>', line: '#' })

    case 'python':
      return new Standard({ start: "('''|\"\"\")", end: "('''|\"\"\")", line: '#' })

    case 'razor':
      // todo
      return null

    case 'rust':
      return new Standard({ line: '\\/{2}(?:\\/|\\!)?' })

    case 'sql':
      return new Standard({ start: '\\/\\*', end: '\\*\\/', line: '--' })

    case 'vb':
      return new Standard({ line: "'" })

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