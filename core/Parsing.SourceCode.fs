module Parsing.SourceCode

open Rewrap
open Core


/// Creates a parser for source code files, given a list of comment parsers
let sourceCode 
    (commentParsers: List<Settings -> OptionParser>)
    (settings: Settings)
    : TotalParser =

    let commentParsers =
        tryMany (List.map (fun cp -> cp settings) commentParsers)

    let codeParser =
        (Block.ignore >> Nonempty.singleton)
        
    takeUntil commentParsers codeParser |> repeatToEnd


/// Line comment parser that takes a custom content parser
let customLine =
    Comments.lineComment

/// A standard line comment parser with the given pattern
let line : string -> Settings -> OptionParser =
    customLine Markdown.markdown

/// Block comment parser that takes a custom content parser and middle line prefixes
let customBlock =
    Comments.blockComment

/// A standard block comment parser with the given start and end patterns
let block : (string * string) -> Settings -> OptionParser =
    customBlock Markdown.markdown ("", "")


/// C-Style line comment parser (//)
let cLine =
    line "//"

/// C-Style block comment parser (/* ... */)
let cBlock =
    customBlock Markdown.markdown (@"\*?", "") (@"/\*", @"\*/")

/// Markers for javadoc
let javadocMarkers =
    (@"/\*[*!]", @"\*/")


/// Parser for java/javascript (also used in html)
let java =
    sourceCode
        [ customBlock DocComments.javadoc ( "\\*?", " * " ) javadocMarkers
          cBlock
          customLine DocComments.javadoc "//[/!]"
          cLine
        ]

/// Parser for css (also used in html)
let css =
    sourceCode 
        [ customBlock DocComments.javadoc ( "\\*?", " * " ) javadocMarkers
          cBlock
        ]

/// Parser for html (also used in dotNet)
let html = 
    Html.html java css