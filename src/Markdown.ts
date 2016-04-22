const parse = require("markdown-to-ast").parse;
import DocumentProcessor from './DocumentProcessor'
import { Position, Range, TextDocument } from 'vscode'
import Section, { SectionToEdit } from './Section'
import { wrapLinesDetectingTypes } from './Wrapping'
import { prefixSize, textAfterPrefix, trimInsignificantEnd } from './Strings'


export default class Markdown extends DocumentProcessor
{
  findSections(doc: TextDocument)
    : { primary: Section[], secondary: Section[] }
  {
    const text = doc.getText()
        , ast = parse(text) as AstNode
    
    const sections = 
      ast.children
        .flatMap(c => processNode(doc, c))

    return { 
      primary: sections.filter(s => !(s instanceof SecondarySection)),
      secondary: sections.filter(s => s instanceof SecondarySection),
    }
  }
}


function processNode(doc: TextDocument, node: AstNode): Section[] {
  switch(node.type) {
    case 'BlockQuote':
    case 'List':
    case 'ListItem':
      return node.children.flatMap(c => processNode(doc, c))
    case 'CodeBlock':
      return [codeBlock(doc, node)]
    case 'Paragraph':
      return [paragraph(doc, node)]
    default:
      return []
  }
}


class SecondarySection extends Section {}


function codeBlock(doc: TextDocument, node: AstNode): Section 
{
  return new SecondarySection(
    doc,
    node.loc.start.line - 1,
    node.loc.end.line - 1
  )
}

function paragraph(doc: TextDocument, node: AstNode): Section 
{
  return new Section(
    doc,
    node.loc.start.line - 1,
    node.loc.end.line - 1,
    /^[\t ]*(([-*+]|\d+[.)]|>)[\t ]+)*/,
    flp => flp.replace(/[^\t >]/g, " ")
  )
}


function range(node: AstNode): Range {
  const { start, end } = node.loc
  return new Range(start.line - 1, start.column, end.line - 1, end.column)
}


interface AstPosition { 
  line: number
  column: number 
}


interface AstNode {
  type: string
  loc: { 
    start: AstPosition
    end: AstPosition
  }
  raw: string
  children?: AstNode[]
}