import { TextDocument } from 'vscode'
import { extname } from 'path'
import DocumentProcessor from './DocumentProcessor'
import BasicLanguage from './BasicLanguage'
import Markdown from './Markdown'
import Sgml from './Sgml'


/** Gets a DocumentProcessor for a document, taken from its file type */
export default function wrappingHandler(doc: TextDocument): DocumentProcessor
{
  const extPattern = extname(doc.fileName) ? extname(doc.fileName) + '.' : null
      , langIdPattern = '.' + doc.languageId + '.'
      
  for(let langs of Object.keys(languages)) {
    if(langs.includes(extPattern) || langs.includes(langIdPattern)) {
      return languages[langs]
    }
  }
  
  return new Markdown()
}


/** Map of languages to comment start/end/line patterns. Mostly uses the file
 *  extension to get the language but in some cases (eg dockerfile) the
 *  languageId has to be used instead. */
const languages: { [key: string]: DocumentProcessor } = 

  { '.bat.':
      new BasicLanguage({ line: '(?:rem|::)' })
      
  , '.c.cpp.cs.css.go.groovy.hpp.h.java.js.jsx.less.m.sass.shader.swift.ts.tsx.': 
      new BasicLanguage({ start: '\\/\\*\\*?', end: '\\*\\/', line: '\\/{2,3}' })
      
  , '.coffee.':
      new BasicLanguage({ start: '###\\*?', end: '###', line: '#' })
      
  , '.dockerfile.makefile.perl.r.shellscript.yaml.':
      // These all seem not to have standard multi-line comments
      new BasicLanguage({ line: '#' })
      
  , '.elm.hs.purs.':
      new BasicLanguage({ start: '{-', end: '-}', line: '--' })
      
  , '.fs.':
      new BasicLanguage({ start: '\\(\\*', end: '\\*\\)', line: '\\/\\/' })
      
  , '.html.xml.xsl.':
      new Sgml()
      
  , '.ini.':
      new BasicLanguage({ line: ';' })
      
  , '.jade.':
      // Jade block comments are a bit different and might need some more thought
      new BasicLanguage({ line: '\\/\\/' })
      
  , '.lua.':
      new BasicLanguage({ start: '--\\[\\[', end: '\\]\\]', line: '--' })
      
  , '.p6.perl6.rb.':
      new BasicLanguage({ start: '^=begin', end: '^=end', line: '#' })
      
  , '.php.':
      new BasicLanguage({ start: '\\/\\*', end: '\\*\\/', line: '(?:\\/\\/|#)' })
      
  , '.powershell.ps1.':
      new BasicLanguage({ start: '<#', end: '#>', line: '#' })
      
  , '.py.python.': 
      new BasicLanguage({ start: "'''", end: "'''", line: '#' })
      
  , '.rust.': 
      new BasicLanguage({ line: '\\/{2}(?:\\/|\\!)?' })
      
  , '.sql.':
      new BasicLanguage({ start: '\\/\\*', end: '\\*\\/', line: '--' })
      
  , '.vb.':
      new BasicLanguage({ line: "'" })
      
  }