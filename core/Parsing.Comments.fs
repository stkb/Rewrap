module private Parsing.Comments

open Nonempty
open System.Text.RegularExpressions
open Extensions
open Rewrap
open Parsing.Core



let private markerRegex marker =
    Regex(@"^\s*" + marker + @"\s*")


/// Creates a line comment parser, given a content parser and marker.
let lineComment 
    (contentParser: Settings -> TotalParser)
    (marker: string)
    (settings: Settings) 
    : OptionParser =

    optionParser
        (Nonempty.span (Line.startsWith marker))
        (fun lines ->
            let regex = 
                markerRegex marker

            let prefix =
                lines
                    |> Nonempty.tryFind Line.containsText
                    |> Option.defaultValue (Nonempty.head lines)
                    |> Line.tryMatch regex
                    |> Option.defaultValue marker
                
            let prefixLength =
                Line.tabsToSpaces settings.tabWidth prefix |> String.length

            let stripLine =
                Line.tabsToSpaces settings.tabWidth
                    >> Line.split regex
                    >> Tuple.mapFirst (fun p -> String.replicate p.Length " ")
                    >> Tuple.mapFirst (String.dropStart prefixLength)
                    >> fun (pre, rest) -> pre + rest

            let newPrefix =
                if settings.reformat then 
                    prefix.TrimEnd() + " "
                else
                    prefix

            lines
                |> Nonempty.map stripLine
                |> Block.wrappable (Block.prefixes newPrefix newPrefix)
                |> (Block.comment (contentParser settings) >> Nonempty.singleton)
        )


/// Creates a multiline comment parser, given a content parser and markers.
let multiComment 
    (contentParser: Settings -> TotalParser) 
    (tailMarker: string, defaultTailMarker: string)
    (startMarker: string, endMarker: string)
    (settings: Settings)
    : OptionParser =

    let tailRegex =
        markerRegex tailMarker

    let startRegex =
        markerRegex startMarker
    
    let toComment (Nonempty(headLine, tailLines)) =
        let originalHeaadPrefix, headRemainder =
            Line.split startRegex headLine

        let newHeadPrefix =
            if settings.reformat then
                originalHeaadPrefix.TrimEnd() + " "
            else
                originalHeaadPrefix

        let originalTailPrefix =
            List.tryFind Line.containsText tailLines
                |> Option.orElse (List.tryHead tailLines)
                |> Option.map
                    (Line.tryMatch tailRegex >> Option.defaultValue "")
                |> Option.defaultValue
                    (Line.leadingWhitespace headLine + defaultTailMarker)

        let prefixLength =
            Line.tabsToSpaces settings.tabWidth originalTailPrefix |> String.length

        let newTailPrefix = 
            if settings.reformat then
                Line.leadingWhitespace headLine + defaultTailMarker
            else
                originalTailPrefix

        let stripLine line =
            let spacedLine = Line.tabsToSpaces settings.tabWidth line
            let linePrefixLength = 
                spacedLine
                    |> (Line.tryMatch tailRegex >> Option.defaultValue "")
                    |> (String.length >> min prefixLength)
            String.dropStart linePrefixLength spacedLine

        Nonempty(headRemainder, List.map stripLine tailLines)
            |> Block.wrappable (Block.prefixes newHeadPrefix newTailPrefix)
            |> (Block.comment (contentParser settings) >> Nonempty.singleton)


    optionParser
        (takeLinesBetweenMarkers (startRegex, (Regex(endMarker))))
        toComment

