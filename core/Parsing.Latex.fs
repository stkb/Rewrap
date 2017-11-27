module private Parsing.Latex

open Extensions
open Nonempty
open Rewrap
open Block
open Parsing.Core
open System.Text.RegularExpressions


let private commandRegex: Regex =
    Regex(@"^\s*\\([a-z]+|\S)")

let private newlineRegex: Regex =
    Regex(@"\\(\\\*?|hline|newline|break|linebreak)(\[.*?\])?(\{.*?\})?\s*$")

let private emptyCommands = 
    [| "begin"; "documentclass"; "section"; "subsection"; "end"; |]

let private inlineCommands =
    [| "cite"; "dots"; "emph"; "href"; "latex"; "latexe"; "ref"; "verb" |]


/// Like Core.takeUntil, but checks from the 2nd line.
let private takeFrom2ndLineUntil otherParser parser (Nonempty(headLine, tailLines)) =

    let bufferToBlocks =
        Nonempty.rev >> parser

    let rec loopFrom2ndLine buffer lines =
        match Nonempty.fromList lines with
            | None ->
                ( bufferToBlocks buffer, None)

            | Some (Nonempty(head, tail) as neLines) ->
                match otherParser neLines with
                    | None ->
                        loopFrom2ndLine (Nonempty.cons head buffer) tail

                    | Some result ->
                        result |> Tuple.mapFirst (Nonempty.append (bufferToBlocks buffer))

    loopFrom2ndLine (Nonempty.singleton headLine) tailLines


let latex (settings: Settings) : TotalParser =
    
    let startsWithCommand (line: string): Option<string> =
        let m = commandRegex.Match(line)
        if m.Success then Some(m.Groups.Item(1).Value) else None

    let paragraphBlocks: Lines -> Blocks =
        splitIntoChunks (afterRegex newlineRegex)
            >> Nonempty.map
                (firstLineIndentParagraphBlock settings.reformat)
    
    let emptyCommand (Nonempty(headLine, tailLines)) =
        startsWithCommand headLine
            |> Option.filter (fun c -> Array.contains c emptyCommands)
            |> Option.map
                (fun _ ->
                    ( Nonempty.singleton headLine |> paragraphBlocks
                    , Nonempty.fromList tailLines
                    )
                )

    let rec blockCommand lines =
        startsWithCommand (Nonempty.head lines)
            |> Option.filter (fun c -> not (Array.contains c inlineCommands))
            |> Option.map (fun _ -> takeFrom2ndLineUntil otherParsers paragraphBlocks lines)

    and otherParsers =
        tryMany [
            blankLines
            Comments.lineComment Markdown.markdown "%" settings
            emptyCommand
            blockCommand
        ]

    takeUntil otherParsers paragraphBlocks |> repeatToEnd