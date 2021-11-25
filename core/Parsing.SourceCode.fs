module internal Parsing.SourceCode

open Rewrap
open Block
open Core
open Sgml
open Parsing_

let private markdown = Parsing.Markdown.markdown

/// Creates a parser for source code files, given a list of comment parsers
let oldSourceCode : List<Settings -> OptionParser<string,string>> -> DocumentProcessor =
  fun commentParsers ->

  toNewDocProcessor <| fun settings ->
    let commentParsers = tryMany (List.map (fun cp -> cp settings) commentParsers)
    let codeParser = (ignoreBlock >> Nonempty.singleton)
    takeUntil commentParsers codeParser |> repeatToEnd


/// Line comment parser that takes a custom content parser
let customLine =
    Comments.lineComment

/// A standard line comment parser with the given pattern
let oldLine : string -> Settings -> OptionParser<string,string> =
    customLine markdown

/// Block comment parser that takes a custom content parser and middle line prefixes
let customBlock =
    Comments.blockComment

/// A standard block comment parser with the given start and end patterns
let oldBlock : (string * string) -> Settings -> OptionParser<string,string> =
    customBlock markdown ("", "")


/// C-Style line comment parser (//)
let cLine =
    oldLine "//"

/// C-Style block comment parser (/* ... */)
let cBlock =
    customBlock markdown ("*", "") (@"/\*", @"\*/")

/// Markers for javadoc
let javadocMarkers =
    (@"/\*[*!]", @"\*/")


/// Parser for java/javascript (also used in html)
let java : DocumentProcessor =
    oldSourceCode
        [ customBlock DocComments.javadoc ( "*", " * " ) javadocMarkers
          cBlock
          customLine DocComments.javadoc "//[/!]"
          cLine
        ]

/// Parser for css (also used in html)
let css : DocumentProcessor =
    oldSourceCode
        [ customBlock DocComments.javadoc ( "*", " * " ) javadocMarkers
          cBlock
        ]

/// Parser for html
let html : DocumentProcessor =
    toNewDocProcessor <| sgml java css [||]
