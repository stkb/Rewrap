import { TextDocument } from 'vscode'
import { extname } from 'path'

const singleLine = (chars: string): string =>
  chars && '^[ \\t]*' + chars + '[^]+?$(?!\\r?\\n[ \\t]*' + chars +')'

const multiLine = ([start, end]: [string, string]): string =>
  start && end && '^[ \\t]*' + start + '[^]+?' + end
  
const noMultiLine: [string, string] = [null, null]

const regexp = (multi: [string, string], single: string) =>
  new RegExp( [multiLine(multi), singleLine(single)].filter(s => !!s).join('|')
            , 'mg'
            )


export function getCommentsRegex(doc: TextDocument) {
  
  switch(extname(doc.fileName)) {
    case '.hs':
    case '.elm':
    case '.purs':
      return regexp(['{-', '-}'], '--')
  }
  
  switch(doc.languageId) {
    case 'bat':
      return regexp([null, null], '(?:rem|::)')
    case 'c':
    case 'cpp':
    case 'csharp':
    case 'css':
    case 'go':
    case 'groovy':
    case 'java':
    case 'javascript':
    case 'javascriptreact':
    case 'less':
    case 'objective-c':
    case 'sass':
    case 'shaderlab':
    case 'swift':
    case 'typescript':
    case 'typescriptreact':
      // Swift can have nested multiline, but we don't support these
      return regexp(['\\/\\*','\\*\\/'], '\\/\\/')
    case 'coffeescript':
      return regexp(['###', '###'], '#')
    case 'dockerfile':
    case 'makefile':
    case 'perl':
    case 'r':
    case 'shellscript':
    case 'yaml':
      // These all seem not to have standard multi-line comments
      return regexp(noMultiLine, '#')
    case 'fsharp':
      return regexp(['\\(\\*', '\\*\\)'], '\\/\\/')
    case 'html':
    case 'xml':
    case 'xsl':
      return regexp(['<!--', '-->'], null)
    case 'ini':
      return regexp(noMultiLine, ';')
    case 'jade':
      // Jade block comments are a bit different and might need some more thought
      return regexp(noMultiLine, '\\/\\/')
    case 'lua':
      return regexp(['--\\[\\[', '\\]\\]'], '--')
    case 'perl6':
    case 'ruby':
      return regexp(['^=begin', '^=end'], '#')
    case 'php':
      return regexp(['\\/\\*', '\\*\\/'], ('?:\\/\\/|#'))
    case 'powershell':
      return regexp(['<#','#>'], '#')
    case 'python':
      return regexp(["'''", "'''"], '#')
    case 'rust':
      // Rust has only single-line '//' and doc '///|//!'
      return regexp(noMultiLine, '//')
    case 'sql':
      return regexp(['\\/\\*', '\\*\\/'], '--')
    case 'vb':
      return regexp(noMultiLine, "'")
  }
  console.log(`Rewrap: No support for ${doc.languageId}`)
}

export function getMiddleLinePrefix(doc: TextDocument, prefix: string): string
{
  const singleLine = ['///', '//!', '//', '#', '--', "'''", "'", ';']
  const customPrefixes = 
    { '/**': ' * '
    }

  const [_, leadingWhitespace, chars, trailingWhiteSpace] = 
        prefix.match(/(\s*)(\S*)(\s*)/)
        
  if(singleLine.indexOf(chars) > -1) return prefix

  else {
    for(let pre of Object.keys(customPrefixes)) {
      if(pre === chars) {
        return leadingWhitespace + customPrefixes[pre] + trailingWhiteSpace
      }
    }
    return leadingWhitespace
  }
}