module private Parsing.Html

open Prelude
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

    let embeddedScript (markers: Regex * Regex) contentParser =
        let afterFirstLine _ lines =
            let (Nonempty(lastLine, initLinesRev)) = Nonempty.rev lines
            if (snd markers).IsMatch(lastLine) then
                match Nonempty.fromList (List.rev initLinesRev) with
                    | Some middleLines ->
                        Nonempty.snoc
                            (Block.ignore (Nonempty.singleton (Nonempty.last lines)))
                            (contentParser settings middleLines)
                    | None ->
                        Nonempty.singleton <| Block.ignore (Nonempty.singleton (Nonempty.last lines))
            else contentParser settings lines

        optionParser
            (takeLinesBetweenMarkers markers)
            (ignoreFirstLine afterFirstLine settings)

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
