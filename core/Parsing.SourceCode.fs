module internal Parsing.SourceCode

open Rewrap
open Block
open Core
open Sgml


/// Creates a parser for source code files, given a list of comment parsers
let sourceCode
    (commentParsers: List<Settings -> OptionParser<string,string>>)
    (settings: Settings)
    : TotalParser<string> =

    let commentParsers =
        tryMany (List.map (fun cp -> cp settings) commentParsers)

    let codeParser =
        (ignoreBlock >> Nonempty.singleton)

    takeUntil commentParsers codeParser |> repeatToEnd


/// Line comment parser that takes a custom content parser
let customLine =
    Comments.lineComment

/// A standard line comment parser with the given pattern
let line : string -> Settings -> OptionParser<string,string> =
    customLine Markdown.markdown

/// Block comment parser that takes a custom content parser and middle line prefixes
let customBlock =
    Comments.blockComment

/// A standard block comment parser with the given start and end patterns
let block : (string * string) -> Settings -> OptionParser<string,string> =
    customBlock Markdown.markdown ("", "")


/// C-Style line comment parser (//)
let cLine =
    line "//"

/// C-Style block comment parser (/* ... */)
let cBlock =
    customBlock Markdown.markdown ("*", "") (@"/\*", @"\*/")

/// Markers for javadoc
let javadocMarkers =
    (@"/\*[*!]", @"\*/")


/// Parser for java/javascript (also used in html)
let java : Settings -> TotalParser<string> =
    sourceCode
        [ customBlock DocComments.javadoc ( "*", " * " ) javadocMarkers
          cBlock
          customLine DocComments.javadoc "//[/!]"
          cLine
        ]

/// Parser for css (also used in html)
let css : Settings -> TotalParser<string> =
    sourceCode
        [ customBlock DocComments.javadoc ( "*", " * " ) javadocMarkers
          cBlock
        ]

/// Parser for html
let html : Settings -> TotalParser<string> =
    sgml java css [||]
