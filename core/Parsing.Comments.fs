module private rec Parsing.Comments

open Nonempty
open System.Text.RegularExpressions
open Extensions
open Rewrap
open Parsing.Core


/// Creates a line comment parser, given a content parser and marker.
let lineComment 
    (contentParser: Settings -> TotalParser) (settings: Settings) (marker: string) 
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
                    >> Tuple.mapFirst (fun s -> s.Substring(prefixLength))
                    >> fun (pre, rest) -> pre + rest

            let newPrefix =
                if settings.tidyUpIndents then 
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
    (settings: Settings)
    (tailMarker: string, defaultTailMarker: string)
    (startMarker: string, endMarker: string)
    : OptionParser =

    let tailRegex =
        markerRegex tailMarker

    let startRegex =
        markerRegex startMarker
    
    let toComment (Nonempty(headLine, tailLines)) =
        let headPrefix, headRemainder =
            Line.split startRegex headLine

        let tailPrefix =
            List.tryFind Line.containsText tailLines
                |> Option.orElse (List.tryHead tailLines)
                |> Option.map
                    (Line.tryMatch tailRegex >> Option.defaultValue "")
                |> Option.defaultValue
                    (Line.leadingWhitespace headLine + defaultTailMarker)

        let prefixLength =
            Line.tabsToSpaces settings.tabWidth tailPrefix |> String.length

        let stripLine line =
            let spacedLine = Line.tabsToSpaces settings.tabWidth line
            let linePrefixLength = 
                spacedLine
                    |> (Line.tryMatch tailRegex >> Option.defaultValue "")
                    |> (String.length >> min prefixLength)
            spacedLine.Substring(linePrefixLength)

        Nonempty(headRemainder, List.map stripLine tailLines)
            |> Block.wrappable (Block.prefixes headPrefix tailPrefix)
            |> (Block.comment (contentParser settings) >> Nonempty.singleton)


    optionParser
        (takeLinesBetweenMarkers (startRegex, (Regex(endMarker))))
        toComment


let private markerRegex marker =
    Regex(@"^\s*" + marker + @"\s*")