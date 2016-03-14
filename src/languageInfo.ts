import { TextDocument } from 'vscode'
import { extname } from 'path'

const leadingWhitespace = '^[ \\t]*'

const singleLine = (chars: string): string =>
  chars && leadingWhitespace + chars + '[^]+?$(?!\\r?\\n' + leadingWhitespace + chars +')'

const multiLine = ([start, end]: [string, string]): string =>
  start && end && leadingWhitespace + start + '[^]+?' + end

const regexp = (multi: [string, string], single: string) =>
  new RegExp( [multiLine(multi), singleLine(single)].filter(s => !!s).join('|')
            , 'mg'
            )

export interface LanguageInfo { start?: string, end?: string, line?: string }

export const plainText: LanguageInfo = { line: '(?=\\S)' }

/** Map of languages to comment start/end/line patterns. Probably not the best
 *  way to do this but it works. Mostly uses the file extension to get the
 *  language but in some cases (eg dockerfile) the languageId has to be used
 *  instead. */
const languages: { [key: string]: LanguageInfo } = 
  { '.bat.':
      { line: '(?:rem|::)' }
  , '.c.cpp.cs.css.go.groovy.hpp.h.java.js.jsx.less.m.sass.shader.swift.ts.tsx.': 
      { start: '\\/\\*\\*?', end: '\\*\\/', line: '\\/{2,3}' }
  , '.coffee.':
      { start: '###\\*?', end: '###', line: '#' }
  , '.dockerfile.makefile.perl.r.shellscript.yaml.':
      // These all seem not to have standard multi-line comments
      { line: '#' }
  , '.elm.hs.purs.':
      { start: '{-', end: '-}', line: '--' }
  , '.fs.':
      { start: '\\(\\*', end: '\\*\\)', line: '\\/\\/' }
  , '.html.xml.xsl.':
      { start: '<!--', end: '-->' }
  , '.ini.':
      { line: ';' }
  , '.jade.':
      // Jade block comments are a bit different and might need some more thought
      { line: '\\/\\/' }
  , '.lua.':
      { start: '--\\[\\[', end: '\\]\\]', line: '--' }
  , '.p6.perl6.rb.':
      { start: '^=begin', end: '^=end', line: '#' }
  , '.php.':
      { start: '\\/\\*', end: '\\*\\/', line: '(?:\\/\\/|#)' }
  , '.powershell.ps1.':
      { start: '<#', end: '#>', line: '#' }
  , '.py.python.': 
      { start: "'''", end: "'''", line: '#' }
  , '.rust.': 
      { line: '\\/{2}(?:\\/|\\!)?' }
  , '.sql.':
      { start: '\\/\\*', end: '\\*\\/', line: '--' }
  , 'vb.':
      { line: "'" }
  }

export function docLanguage(doc: TextDocument): LanguageInfo
{
  for(let langs of Object.keys(languages)) {
    if(langs.includes(extname(doc.fileName) + '.')
      || langs.includes('.' + doc.languageId + '.')
    ) {
      return languages[langs]
    }
  }
  
  return plainText
}


export function rangesRegex(lang: LanguageInfo) {
  return regexp([lang.start, lang.end], lang.line)
}