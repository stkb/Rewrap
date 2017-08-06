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
                    (fun p -> String.replicate p.Length " " |> String.dropStart prefixLength)
                >> fun (pre, rest) -> pre + rest

        let newPrefix =
            if settings.reformat then (reformatPrefix prefix) else prefix

        Block.wrappable (newPrefix, newPrefix) (Nonempty.map stripLine lines)


/// Creates a line comment parser, given a content parser and marker.
let lineComment 
    (contentParser: Settings -> TotalParser)
    (marker: string)
    (settings: Settings) 
    : OptionParser =

    optionParser
        (Nonempty.span (Line.startsWith marker))
        (extractWrappable marker (fun p -> p.TrimEnd() + " ") settings
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
        
        let addHeadLine w =
            let (_, tail) = w.prefixes
            wrappable (newHeadPrefix, tail) (Nonempty.cons headRemainder w.lines)

        Nonempty.fromList tailLines
            |> Option.map
                (extractWrappable tailMarker (fun _ -> Line.leadingWhitespace headLine + defaultTailMarker) settings
                    >> addHeadLine
                )
            |> Option.defaultValue
                ( wrappable 
                    (newHeadPrefix, (Line.leadingWhitespace headLine + defaultTailMarker))
                    (Nonempty.singleton headRemainder)
                )
            |> (Block.comment (contentParser settings) >> Nonempty.singleton)
 

    optionParser
        (takeLinesBetweenMarkers (startRegex, (Regex(endMarker))))
        toComment
