module internal Parsing.Sgml

open Prelude
open Rewrap
open Parsing.Core
open System.Text.RegularExpressions


let inline private regex str =
    Regex(str, RegexOptions.IgnoreCase)

let private scriptMarkers =
    (regex "<script", regex "</script>")

let private cssMarkers =
    (regex "<style", regex "</style>")

let sgml
    (scriptParser: Settings -> TotalParser)
    (cssParser: Settings -> TotalParser)
    (blockTags: seq<string>)
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

    // Checks if a regex contains a block tag name as its first captured group
    let isBlockTag (pattern: string) (line: string) =
        let m = (regex pattern).Match(line)
        m.Success && (Seq.isEmpty blockTags ||
                      Seq.contains (m.Groups.[1].Value.ToLower()) blockTags)

    let beginsWithBlockStartTag = isBlockTag @"^\s*<([\w.-]+)"
    let justBlockEndTag = isBlockTag @"^\s*</([\w.-]+)\s*>"
    let endsWithBlockTag = isBlockTag @"([\w.-]+)>\s*$"

    let breakBefore line = justBlockEndTag line || beginsWithBlockStartTag line
    let breakAfter line =
        Line.contains (regex @"([""\s]>\s*|  )$") line || endsWithBlockTag line

    let paragraphBlocks =
        splitIntoChunks (splitBefore breakBefore)
            >> Nonempty.collect (splitIntoChunks (Nonempty.splitAfter breakAfter))
            >> Nonempty.map (indentSeparatedParagraphBlock Block.text)

    takeUntil otherParsers paragraphBlocks |> repeatToEnd
