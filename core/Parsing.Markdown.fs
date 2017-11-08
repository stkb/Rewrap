module private rec Parsing.Markdown

open System.Text.RegularExpressions
open Extensions
open Nonempty
open Rewrap
open Block
open Parsing.Core


let rec markdown (settings: Settings): TotalParser =

    /// Ignores fenced code blocks (``` ... ```) 
    let fencedCodeBlock (Nonempty(headLine, tailLines) as lines) =
        Line.tryMatch (mdMarker "(`{3,}|~{3,})") headLine
            |> Option.map String.trimStart
            |> Option.map
                (fun marker ->
                    tailLines
                        |> Nonempty.fromList
                        |> Option.map
                            (Nonempty.splitAfter (lineStartsWith marker)
                                >> Tuple.mapFirst
                                    (Nonempty.cons headLine
                                        >> (Block.ignore >> Nonempty.singleton)
                                    )
                            )
                        |> Option.defaultValue
                            ( Block.ignore lines |> Nonempty.singleton, None )
                )

    let htmlType1to6 =
        [ 
            takeLinesBetweenMarkers 
                ( mdMarker "<(script|pre|style)( |>|$)"
                , Regex("</(script|pre|style)>", RegexOptions.IgnoreCase)
                )
            takeLinesBetweenMarkers (mdMarker "<!--", Regex("-->"))
            takeLinesBetweenMarkers (mdMarker "<?", Regex("\\?>"))
            takeLinesBetweenMarkers (mdMarker "<![A-Z]", Regex(">"))
            takeLinesBetweenMarkers (mdMarker "<!\\[CDATA\\[", Regex("]]>"))
            takeLinesBetweenMarkers
                ( mdMarker
                    ("</?(address|article|aside|base|basefont|blockquote"
                        + "|body|caption|center|col|colgroup|dd|details"
                        + "|dialog|dir|div|dl|dt|fieldset|figcaption|figure"
                        + "|footer|form|frame|frameset|h1|h2|h3|h4|h5|h6"
                        + "|head|header|hr|html|iframe|legend|li|link|main"
                        + "|menu|menuitem|meta|nav|noframes|ol|optgroup"
                        + "|option|p|param|section|source|summary|table"
                        + "|tbody|td|tfoot|th|thead|title|tr|track|ul)"
                        + "(\\s|/?>|$)"
                    )
                , Regex("^\\s*$")
                )
        ]
            |> List.map ignoreParser
            |> tryMany

    /// Ignores tables 
    let table =
        let cellsRowRegex =
                Regex("\\S\\s*\\|\\s*\\S")

        let dividerRowRegex =
                Regex(":?-+:?\\s*\\|\\s*:?-+:?")

        let splitter lines =
            match Nonempty.toList lines with
                | firstLine :: secondLine :: rest ->
                    if
                        Line.contains cellsRowRegex firstLine
                            && Line.contains dividerRowRegex secondLine
                    then
                        List.span (Line.contains cellsRowRegex) rest
                            |> Tuple.mapFirst
                                (fun rows -> Nonempty(firstLine, secondLine :: rows))
                            |> Tuple.mapSecond (Nonempty.fromList)
                            |> Some
                    else
                        None

                | _ ->
                    None

        ignoreParser splitter

    /// Ignores any non-blank line that doesn't contain any text.
    let nonText =
        ignoreParser
            (Nonempty.span (fun s -> not (Line.containsText s || Line.isBlank s)))

    /// Ignores ATX headings (### Heading ###)
    let atxHeading =
        ignoreParser (Nonempty.span (lineStartsWith "#{1,6} "))

    /// BlockQuote (> ...)
    let blockQuote =
        let splitter lines =
            Some lines
                |> Option.filter (Nonempty.head >> lineStartsWith ">")
                |> Option.bind (Nonempty.span (fun s -> not (Line.isBlank s)))

        let mapper lines =
            let Nonempty(firstTuple, otherTuples) as tuples =
                // We already know the first line contains a `>`
                Nonempty.map (Line.split (Regex(" {0,3}>? ?"))) lines

            let prefixes =
                if settings.reformat then
                   ("> ", "> ")
                else
                   ( fst firstTuple
                   , fst
                        (List.tryHead otherTuples
                            |> Option.defaultValue firstTuple
                        )
                   )

            (prefixes, tuples |> Nonempty.map snd)
                |> Block.splitUp (markdown settings)

        optionParser splitter mapper

    /// Ignores code block indented 4 spaces (or 1 tab)
    let indentedCodeBlock =
        ignoreParser (Nonempty.span (Line.contains (Regex("^(\\s{4}|\\t)"))))

    let listItem (Nonempty(firstLine, otherLines)) =
        let doStuff listItemPrefix =
            let strippedFirstLine =
                String.dropStart (String.length listItemPrefix) firstLine

            let prefixWithSpace =
                if listItemPrefix.EndsWith(" ") then
                    listItemPrefix
                else
                    listItemPrefix + " "

            let indent =
                String.length prefixWithSpace

            let tailLines, remainingLines =
                if strippedFirstLine = "" then
                    findListItemEnd indent NonParagraph otherLines
                else
                    findListItemEnd indent Paragraph otherLines

            let tailRegex =
                Regex("^ {0," + indent.ToString() + "}")

            let headPrefix =
                (if settings.reformat then
                    String.trim prefixWithSpace + " "
                    else
                    prefixWithSpace
                )
                
            ( ( (headPrefix, (String.replicate (String.length headPrefix) " "))
              , Nonempty
                    ( strippedFirstLine
                    , List.map (Line.split tailRegex >> snd) tailLines
                    )
              )
                    |> Block.splitUp (markdown settings)
            , remainingLines
            )
        
        Line.tryMatch listItemRegex firstLine |> Option.map doStuff
  
    let paragraphBlocks =
        splitIntoChunks (afterRegex (Regex(@"(\\|\s{2})$")))
            >> Nonempty.map (firstLineIndentParagraphBlock settings.reformat)

    let paragraphTerminatingParsers =
        tryMany [
            blankLines
            fencedCodeBlock
            nonText
            listItem
            blockQuote
        ]

    let paragraphs =
        takeLinesUntil paragraphTerminatingParsers paragraphBlocks

    let allParsers =
        tryMany [
            blankLines
            fencedCodeBlock
            table
            nonText
            atxHeading
            indentedCodeBlock
            listItem
            blockQuote
        ]

    Nonempty.map (Line.tabsToSpaces settings.tabWidth)
        >> repeatUntilEnd allParsers paragraphs


        


let private mdMarker marker =
    Regex(@"^ {0,3}" + marker, RegexOptions.IgnoreCase)

let private listItemRegex =
        mdMarker @"([-+*]|[0-9]+[.)])(\s+|$)"

let private blockQuoteRegex =
        mdMarker ">"

let private lineStartsWith =
        Line.contains << mdMarker

type private MarkdownState =
    | FencedCodeBlock
    | Paragraph
    | NonParagraph

let private findListItemEnd indent: MarkdownState -> List<string> -> List<string> * Option<Lines> =

    let combine output remaining =
            ( List.rev output, Nonempty.fromList remaining )

    let modifyState state line =
            // Need to fix fencedCodeBlock start/end markers
            match state with
                | FencedCodeBlock ->
                    if lineStartsWith "(```|~~~)" line then
                        NonParagraph
                    else
                        FencedCodeBlock

                | Paragraph ->
                    if lineStartsWith "(```|~~~)" line then
                        FencedCodeBlock
                    else if not (Line.containsText line) || lineStartsWith "#{1,6} " line then
                        NonParagraph
                    else
                        Paragraph

                | NonParagraph ->
                    if lineStartsWith "(```|~~~)" line then
                        FencedCodeBlock
                    else if not (Line.containsText line) || lineStartsWith "#{1,6} " line then
                        NonParagraph
                    else if Line.contains (Regex("^ {4,}")) line then
                        NonParagraph
                    else
                        Paragraph

    
    let rec loop (output: List<string>) (state: MarkdownState) (lines: List<string>) =
        match lines with
            | line :: otherLines ->
                if (line |> Line.leadingWhitespace |> String.length) < indent then
                    if
                        Line.contains blockQuoteRegex line
                            || Line.contains listItemRegex line
                    then
                        combine output lines
                    else
                        match state with
                            | Paragraph ->
                                loop (line :: output) (modifyState state line) otherLines

                            | _ ->
                                combine output lines
                else
                    loop (line :: output) (modifyState state line) otherLines

            | [] ->
                combine output lines

    loop []
