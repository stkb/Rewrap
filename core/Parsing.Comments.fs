module private Parsing.Comments

open Nonempty
open System.Text.RegularExpressions
open Extensions
open Rewrap
open Block
open Parsing.Core


let private markerRegex marker =
    Regex(@"^\s*" + marker + @"\s*")

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
            Nonempty.tryFind Line.containsText lines
                |> Option.orElseWith 
                    (fun () -> Nonempty.tryFind (Line.contains (Regex(@"\S"))) lines)
                |> Option.defaultValue (Nonempty.head lines)
                |> Line.tryMatch regex |> Option.defaultValue marker
                
        let prefixLength =
            prefix |> Line.tabsToSpaces settings.tabWidth |> String.length

        let stripLine =
            Line.tabsToSpaces settings.tabWidth
                >> Line.split regex
                >> Tuple.mapFirst 
                    (fun pre -> 
                        if eraseIndentedMarker then
                            String.replicate pre.Length " " 
                                |> String.dropStart prefixLength
                        else String.dropStart prefixLength pre
                    )
                >> fun (pre, rest) -> pre + rest

        let newPrefix =
            if settings.reformat then (reformatPrefix prefix) else prefix

        ((newPrefix, newPrefix), Nonempty.map stripLine lines)


/// Creates a line comment parser, given a content parser and marker.
let lineComment 
    (contentParser: Settings -> TotalParser)
    (marker: string)
    (settings: Settings) 
    : OptionParser =

    optionParser
        (Nonempty.span (Line.startsWith marker))
        (extractWrappable marker true (fun p -> p.TrimEnd() + " ") settings
            >> (Block.comment (contentParser settings) >> Nonempty.singleton)
        )


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
