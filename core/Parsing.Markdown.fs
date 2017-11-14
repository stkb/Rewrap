module private rec Parsing.Markdown

open System.Text.RegularExpressions
open Extensions
open Nonempty
open Rewrap
open Block
open Parsing.Core


let rec markdown (settings: Settings): TotalParser =

    let shrinkIndentTo n (lines: Lines) : Lines =
        let minIndent =
            Nonempty.toList lines
                |> List.map (fun s -> s.Length - s.TrimStart().Length)
                |> List.min

        lines |> Nonempty.map (String.dropStart (minIndent - n))


    /// Ignores fenced code blocks (``` ... ```)
    ///
    /// The ending marker:
    /// - Must be on a separate line
    /// - Must be no more than 3 spaces indented
    /// - May be more than 3 `'s or ~'s
    let fencedCodeBlock (Nonempty(headLine: string, tailLines: List<string>) as lines) =

        let takeLinesTillEndMarker marker (startLineIndent: int) : Lines * Option<Lines> =
                
            let contentLines, otherLines =
                List.span (not << lineStartsWith marker) tailLines
            let maybeEndLine, maybeRemainingLines =
                match otherLines with
                    | [] -> ([], None)
                    | x :: xs -> ([x], Nonempty.fromList xs)

            let outputLines = 
                if settings.reformat then
                    let contentIndentShift: int = 
                        contentLines
                            |> List.map (fun l -> l.Length)
                            |> List.min
                            |> min startLineIndent
                    Nonempty 
                        ( String.trimStart headLine
                        , List.map (String.dropStart contentIndentShift) contentLines
                            @ (List.map String.trimStart maybeEndLine)
                        )
                else
                    Nonempty (headLine, contentLines @ maybeEndLine)
            
            (outputLines, maybeRemainingLines)


        let prefix, remainder = Line.split (mdMarker "(`{3,}|~{3,})") headLine
        let hasStartMarker = String.length prefix > 0

        if hasStartMarker then
            let marker = String.trimStart prefix
            // If another marker char is found later in the string, it's not a
            // valid fenced code block
            let markerChar = marker.Chars(0)
            if remainder.IndexOf(markerChar) >= 0 then
                None
            else
                takeLinesTillEndMarker marker (prefix.Length - marker.Length)
                    |> Tuple.mapFirst (NoWrap >> Nonempty.singleton)
                    |> Some
        else
            None
        

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
        let takeLines =
            Nonempty.span (Line.contains (Regex("^(\\s{4}|\\t)")))
        let toBlocks =
            if settings.reformat then shrinkIndentTo 4 else id
                >> NoWrap >> Nonempty.singleton

        optionParser takeLines toBlocks

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
  
    let paragraph =
        splitIntoChunks (afterRegex (Regex(@"(\\|\s{2})$")))
            >> Nonempty.map (firstLineIndentParagraphBlock settings.reformat)

    let paragraphTerminator =
        tryMany [
            blankLines
            fencedCodeBlock
            nonText
            listItem
            blockQuote
        ]

    let allParsers lines =
        tryMany [
            blankLines
            fencedCodeBlock
            table
            nonText
            atxHeading
            indentedCodeBlock
            listItem
            blockQuote
        ] lines
            |> Option.defaultWith 
                (fun () -> takeUntil paragraphTerminator paragraph lines)
      

    Nonempty.map (Line.tabsToSpaces settings.tabWidth)
        >> (repeatToEnd allParsers)



        


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
