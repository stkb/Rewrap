module private Parsing.Comments

open Nonempty
open System.Text.RegularExpressions
open Extensions
open Rewrap
open Block
open Parsing.Core


let private markerRegex marker =
    Regex(@"^\s*" + marker + @"\s*")


let private extractPrefix prefixRegex defaultPrefix tabWidth lines : string * int =
    List.tryFind Line.containsText lines
        |> Option.orElse (List.tryHead lines)
        |> Option.map (Line.split prefixRegex >> fst)
        |> Option.defaultValue defaultPrefix
        |> (fun p -> (p, (Line.tabsToSpaces tabWidth p).Length))

let private stripLines prefixRegex prefixLength tabWidth eraseIndentedMarker : Lines -> Lines =
    let stripLine = 
        Line.tabsToSpaces tabWidth
            >> Line.split prefixRegex
            >> Tuple.mapFirst 
                (fun pre -> 
                    if eraseIndentedMarker then
                        String.replicate pre.Length " " 
                            |> String.dropStart prefixLength
                    else String.dropStart prefixLength pre
                )
            >> fun (pre, rest) -> pre + rest
    Nonempty.map stripLine

let private maybeReformat settings (prefix: string) : string =
    if prefix <> "" && settings.reformat then prefix.TrimEnd() + " " else prefix

let extractWrappable 
    (marker: string)
    (eraseIndentedMarker: bool)
    (reformatPrefix: string -> string)
    (settings: Settings)
    (lines: Lines)
    : Wrappable =

        let regex = 
            markerRegex marker

        let prefix, prefixLength =
            extractPrefix regex "" settings.tabWidth (Nonempty.toList lines)

        let newPrefix =
            if settings.reformat then (reformatPrefix prefix) else prefix

        ( (newPrefix, newPrefix)
        , stripLines regex prefixLength settings.tabWidth eraseIndentedMarker lines
        )


let decorationLinesParser (fn: string -> Option<string>) lines 
    : Option<Blocks * Option<Lines>> =
    let rec loop output (Nonempty(headLine, tailLines)) =
        match fn headLine with
            | Some newLine -> 
                match Nonempty.fromList tailLines with
                    | Some nextLines ->
                        loop (newLine :: output) nextLines
                    | None ->
                        (newLine :: output)
            | None ->
                output
            
    Nonempty.fromList (loop [] lines)
        |> Option.map
            (fun newLinesRev ->
                ( Nonempty.singleton (NoWrap (Nonempty.rev newLinesRev))
                , Nonempty.fromList 
                    (List.safeSkip (Nonempty.length newLinesRev) (Nonempty.toList lines))
                )
            )


/// Creates a line comment parser, given a content parser and marker.
let lineComment
    (contentParser: Settings -> TotalParser)
    (marker: string)
    (settings: Settings)
    : OptionParser =

    let prefixRegex =
        markerRegex marker
    
    let linesToComment lines : Nonempty<Block> =
        let prefix, prefixLength =
            (extractPrefix prefixRegex "" settings.tabWidth (Nonempty.toList lines))
        
        let maybeMakeDecLine line = 
            let pre, rest = 
                Line.split prefixRegex line
            if pre = pre.TrimEnd() && rest <> "" && not (Line.containsText rest) then
                Some (prefix.TrimEnd() + rest)
            else
                None

        let otherLinesParser =
            let newPrefix = 
                maybeReformat settings prefix
            Wrappable.fromLines (newPrefix, newPrefix)
                >> Block.splitUp
                    (stripLines prefixRegex prefixLength settings.tabWidth true
                        >> contentParser settings
                    )

        let combinedParser =
            otherLinesParser
                |> takeUntil (decorationLinesParser maybeMakeDecLine) 
                |> repeatToEnd

        combinedParser lines |> (Comment >> Nonempty.singleton)
            
    optionParser (Nonempty.span (Line.contains prefixRegex)) linesToComment


/// Creates a block comment parser, given a content parser and markers.
let blockComment 
    (contentParser: Settings -> TotalParser) 
    (tailMarker: string, defaultTailMarker: string)
    (startMarker: string, endMarker: string)
    (settings: Settings)
    : OptionParser =

    let startRegex =
        markerRegex startMarker
    let endRegex =
        Regex(endMarker)

    let linesToComment lines : Nonempty<Block> =

        let headPrefix, headRemainder =
            Line.split startRegex (Nonempty.head lines)
    
        let prefixRegex =
            markerRegex tailMarker
        let defaultPrefix = 
            (Line.leadingWhitespace headPrefix + defaultTailMarker)
        let prefix, prefixLength =
            Nonempty.tail lines
                |> extractPrefix prefixRegex defaultPrefix settings.tabWidth
                    
        let newPrefix = 
            if settings.reformat then defaultPrefix else prefix

        let maybeMakeHeadDecLine line =
            if 
                line = Nonempty.head lines
                    && headPrefix = headPrefix.TrimEnd() 
                    && not (Line.containsText headRemainder) 
            then
                Some line
            else
                None
        
        let maybeMakeDecLine line =
            let pre, _ =
                Line.split prefixRegex line
            let leadingWhitespace =
                Line.leadingWhitespace pre
            let indent = 
                (Line.tabsToSpaces settings.tabWidth leadingWhitespace).Length
            let noMarkerWithSpaceAfter =
                pre = leadingWhitespace || pre = pre.TrimEnd()
            if 
                (not (Line.containsText line) && line.Trim().Length > 1)
                    && (noMarkerWithSpaceAfter && indent < prefixLength)
            then 
                Some (line) 
            else
                None

        let maybeMakeEndDecLine line =
            Line.tryMatch endRegex line
                |> Option.filter (not << Line.containsText)
                |> Option.map (fun _ -> line)

        let stripLine = 
            Line.tabsToSpaces settings.tabWidth
                >> Line.split prefixRegex
                >> (fun (p, r) -> String.dropStart prefixLength p + r)

        let stdDecLineParser =
            tryMany 
                [ decorationLinesParser maybeMakeEndDecLine
                ; decorationLinesParser maybeMakeDecLine
                ]

        let stdParser =
            let otherLinesParser =
                Nonempty.map stripLine
                    >> Wrappable.fromLines (newPrefix, newPrefix)
                    >> Block.splitUp (contentParser settings)

            otherLinesParser
                |> takeUntil stdDecLineParser
                |> repeatToEnd

        let beginParser lines = 
            let otherLinesParser =
                Nonempty.mapHead (fun _ -> headRemainder)
                    >> Nonempty.mapTail stripLine
                    >> Wrappable.fromLines 
                        (maybeReformat settings headPrefix, newPrefix)
                    >> Block.splitUp (contentParser settings)

            decorationLinesParser maybeMakeHeadDecLine lines
                |> Option.defaultWith 
                    (fun () -> takeUntil stdDecLineParser otherLinesParser lines)

        let blocks =
            match beginParser lines with
                | (beginBlocks, Some remainingLines) ->
                    beginBlocks + stdParser remainingLines
                | (beginBlocks, None) ->
                    beginBlocks
        Nonempty.singleton (Comment blocks)

    optionParser
        (takeLinesBetweenMarkers (startRegex, endRegex))
        linesToComment