module private Parsing.Html

open Extensions
open Rewrap
open Parsing.Core
open System.Text.RegularExpressions


let private regex str =
    Regex(str, RegexOptions.IgnoreCase)

let private scriptMarkers = 
    (regex "<script", regex "</script>")

let private cssMarkers =
    (regex "<style", regex "</style>")
    


let html 
    (scriptParser: Settings -> TotalParser)
    (cssParser: Settings -> TotalParser)
    (settings: Settings) 
    : TotalParser =

    let embeddedScript markers contentParser =
        optionParser
            (takeLinesBetweenMarkers markers)
            (fun lines ->
                List.tryInit (Nonempty.tail lines)
                    |> Option.bind Nonempty.fromList
                    |> Option.map
                        (contentParser settings
                            >> Nonempty.snoc (Block.ignore (Nonempty.singleton (Nonempty.last lines)))
                            >> Nonempty.cons (Block.ignore (Nonempty.singleton (Nonempty.head lines)))
                        )
                    |> Option.defaultValue
                        (Nonempty.singleton (Block.ignore lines))
            )

    let otherParsers =
        tryMany
            [ blankLines
              Comments.blockComment 
                Markdown.markdown ( "", "" ) ( "<!--", "-->" ) settings
              embeddedScript scriptMarkers scriptParser
              embeddedScript cssMarkers cssParser
            ]

    let paragraphBlocks =
        splitIntoChunks (beforeRegex (regex "^\\s*<"))
            >> Nonempty.collect
                (splitIntoChunks (afterRegex (regex "\\>\\s*$")))
            >> Nonempty.map (indentSeparatedParagraphBlock Block.text)

    takeUntil otherParsers paragraphBlocks |> repeatToEnd

