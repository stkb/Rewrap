module Parsing.SourceCode

open Rewrap
open Core
open Comments


/// Creates a parser for source code files
let customSourceCode 
    (commentParsers: List<Settings -> OptionParser>)
    (settings: Settings)
    : TotalParser =

    let otherParsers =
        tryMany
            (blankLines :: (List.map (fun cp -> cp settings) commentParsers))

    let codeParser =
        takeLinesUntil
            otherParsers
            (splitIntoChunks (onIndent settings.tabWidth)
                >> Nonempty.map (indentSeparatedParagraphBlock Block.code)
            )

    repeatUntilEnd otherParsers codeParser


let stdLineComment =
    lineComment Markdown.markdown

let stdMultiComment =
    multiComment Markdown.markdown ("", "")


/// Parser for standard source code files
let sourceCode 
    (maybeSingleMarker: Option<string>)
    (maybeMultiMarkers: Option<string * string>)
    : Settings -> TotalParser =

    customSourceCode
        ( List.choose id
            [ maybeSingleMarker |> Option.map stdLineComment
              maybeMultiMarkers |> Option.map stdMultiComment
            ]
        )


let cMultiMarkers =
    (@"/\*", @"\*/")


let javadocMarkers =
    (@"/\*\*", @"\*/")


/// Parser for java/javascript (also used in html)
let java =
    customSourceCode
        [ multiComment DocComments.javadoc ( "\\*", " * " ) javadocMarkers
          stdMultiComment cMultiMarkers
          stdLineComment "//"
        ]


/// Parser for css (also used in html)
let css =
    sourceCode None (Some cMultiMarkers)


/// Parser for html (also used in dotNet)
let html = 
    Html.html java css