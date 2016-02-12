import { TextDocument } from 'vscode'

const singleLine = (chars: string): string =>
  chars && '^[ \\t]*' + chars + '[^]+?$(?!\\r?\\n[ \\t]*' + chars +')'

const multiLine = ([start, end]: [string, string]): string =>
  start && end && '^[ \\t]*' + start + '[^]+?' + end

const regexp = (multi: [string, string], single: string) =>
  new RegExp( [multiLine(multi), singleLine(single)].filter(s => !!s).join('|')
            , 'mg'
            )


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
      return regexp(['\\/\\*','\\*\\/'], '\\/\\/')
    case 'html':
    case 'xml':
    case 'xsl':
      return regexp(['<!--', '-->'], null)
    case 'ruby':
      return regexp(['^=begin', '^=end'], '#')
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