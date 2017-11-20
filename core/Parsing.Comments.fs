module private Parsing.Comments

open Nonempty
open System.Text.RegularExpressions
open Extensions
open Rewrap
open Block
open Parsing.Core


let private markerRegex marker =
    Regex(@"^\s*" + marker + @"\s*")

let private extractPrefix prefixRegex lines : string =
    Nonempty.tryFind Line.containsText lines
        |> Option.defaultValue (Nonempty.head lines)
        |> (Line.split prefixRegex >> fst)

let private stripLines prefixRegex prefix tabWidth eraseIndentedMarker : Lines -> Lines =
    let prefixLength = 
        prefix |> Line.tabsToSpaces tabWidth |> String.length

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


let extractWrappable 
    (marker: string)
    (eraseIndentedMarker: bool)
    (reformatPrefix: string -> string)
    (settings: Settings)
    (lines: Lines)
    : Wrappable =

        let regex = 
            markerRegex marker

        let prefix =
            extractPrefix regex lines
                
        let prefixLength =
            prefix |> Line.tabsToSpaces settings.tabWidth |> String.length


        let newPrefix =
            if settings.reformat then (reformatPrefix prefix) else prefix

        ( (newPrefix, newPrefix)
        , stripLines regex prefix settings.tabWidth eraseIndentedMarker lines
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
        let prefix =
            extractPrefix prefixRegex lines

        let isDecorationLine line : bool =
            let prefix, rest =
                Line.split prefixRegex line
            not (rest = "")
                && prefix = String.trimEnd prefix
                && not (Line.containsText rest)

        let decorationLinesParser =
            let dlPrefix =
                prefix.TrimEnd()
            optionParser
                (Nonempty.span isDecorationLine)
                (Nonempty.map (fun s -> dlPrefix + (snd (Line.split prefixRegex s)))
                    >> (NoWrap >> Nonempty.singleton)
                )

        let otherLinesParser =
            let newPrefix =
                if settings.reformat then prefix.TrimEnd() + " " else prefix

            Wrappable.fromLines (newPrefix, newPrefix)
                >> Block.splitUp
                    (stripLines prefixRegex prefix settings.tabWidth true
                        >> contentParser settings
                    )

        lines
            |> (takeUntil decorationLinesParser otherLinesParser |> repeatToEnd)
            |> (Comment >> Nonempty.singleton)
            
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
    
    let toComment (Nonempty(headLine, tailLines)) =
        let headPrefix, headRemainder =
            Line.split startRegex headLine

        let newHeadPrefix =
            if settings.reformat then
                headPrefix.TrimEnd() + " "
            else
                headPrefix
        
        let addHeadLine =
            Wrappable.mapPrefixes (Tuple.replaceFirst newHeadPrefix)
            >> Wrappable.mapLines (Nonempty.cons headRemainder)
        
        let tailLinesArray =
            List.toArray tailLines

        Nonempty.fromList tailLines
            |> Option.map
                (fun neTailLines ->
                    let endLine, middleLinesRev =
                        match Nonempty.rev neTailLines with
                            | Nonempty(h, t) -> (h, t)
                    
                    let contentLinesAndEndBlock = 
                        if Line.startsWith endMarker endLine then
                            These.maybeThis 
                                (List.rev middleLinesRev |> Nonempty.fromList)
                                (NoWrap (Nonempty.singleton endLine))
                        else
                            This neTailLines
                    
                    let contentBlocksAndEndBlock = 
                        These.mapThis 
                            (extractWrappable 
                                tailMarker 
                                false
                                (fun _ -> Line.leadingWhitespace headLine + defaultTailMarker)
                                settings
                                >> addHeadLine
                                >> Block.splitUp (contentParser settings)
                            )
                            contentLinesAndEndBlock

                    match contentBlocksAndEndBlock with
                        | This contentBlocks -> 
                            contentBlocks
                        | That endBlock -> 
                            Nonempty.singleton endBlock
                        | These (contentBlocks, endBlock) ->
                            contentBlocks |> Nonempty.snoc endBlock
                    
                )
            |> Option.defaultValue
                (Block.splitUp (contentParser settings)
                    ( (newHeadPrefix, (Line.leadingWhitespace headLine + defaultTailMarker))
                    , Nonempty.singleton headRemainder
                    )
                )
            |> (Block.Comment >> Nonempty.singleton)
 

    optionParser
        (takeLinesBetweenMarkers (startRegex, (Regex(endMarker))))
        toComment