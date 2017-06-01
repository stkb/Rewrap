module private Parsing.Html

open Extensions
open OtherTypes
open Parsing.Core
open System.Text.RegularExpressions


let private regex str =
    Regex(str, RegexOptions.IgnoreCase)

let private scriptMarkers = 
    (regex "<script", regex "</script>")

let private cssMarkers =
    (regex "<style", regex "</style>")
    


let html 
    (scriptParser: Options -> TotalParser)
    (cssParser: Options -> TotalParser)
    (options: Options) 
    : TotalParser =

    let embeddedScript markers contentParser =
        optionParser
            (takeLinesBetweenMarkers markers)
            (fun lines ->
                List.tryInit (Nonempty.tail lines)
                    |> Option.bind Nonempty.fromList
                    |> Option.map
                        (contentParser options
                            >> Nonempty.snoc (Block.Ignore 1)
                            >> Nonempty.cons (Block.Ignore 1)
                        )
                    |> Option.defaultValue
                        (Nonempty.singleton (Block.ignore lines))
            )

    let otherParsers =
        tryMany
            [ blankLines
              Comments.multiComment 
                Markdown.markdown options ( "", "" ) ( "<!--", "-->" )
              embeddedScript scriptMarkers scriptParser
              embeddedScript cssMarkers cssParser
            ]

    let paragraphBlocks =
        splitIntoChunks (beforeRegex (regex "^\\s*<"))
            >> Nonempty.collect
                (splitIntoChunks (afterRegex (regex "\\>\\s*$")))
            >> Nonempty.map (indentSeparatedParagraphBlock Block.text)

    let paragraphs =
        takeLinesUntil otherParsers paragraphBlocks
    
    repeatUntilEnd otherParsers paragraphs

