import { TextDocument } from 'vscode'

export function getCommentsRegex(doc: TextDocument) {
  switch(doc.languageId) {
    case 'c':
    case 'cpp':
    case 'csharp':
    case 'css':
    case 'go':
    case 'java':
    case 'javascript':
    case 'javascriptreact':
    case 'typescript':
    case 'typescriptreact':
      // Single line: //... and multi-line: /*(*)...*/
      return /^[ \t]*\/\*[^]*?\*\/|^[ \t]*\/\/[^]+?$(?!\r?\n[ \t]*\/\/)/mg
    case 'html':
    case 'xml':
    case 'xsl':
      // Only multi-line: <!-- ... -->
      return /^[ \t]*<!--[^]+?-->/mg
    case 'ruby':
      // Single line: #... and multi-line: ^=begin ... ^=end
      return /^=begin[^]+^=end|^[ \t]*#[^]+?$(?!\r?\n[ \t]*#)/mg
  }
}

export function getMiddleLinePrefix(doc: TextDocument, prefix: string): string
{
  const singleLine = ['///', '//', '#']
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