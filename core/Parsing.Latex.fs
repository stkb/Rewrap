module private Parsing.Latex

open Extensions
open Nonempty
open Rewrap
open Block
open Parsing.Core
open System.Text.RegularExpressions



let private newlineRegex: Regex =
    Regex(@"\\(\\\*?|hline|newline|break|linebreak)(\[.*?\])?(\{.*?\})?\s*$")

let private inlineCommands =
    [| "cite"; "dots"; "emph"; "href"; "latex"; "latexe"; "ref"; "verb" |]

let private preserveEnvironments =
    [| "align"; "alltt"; "displaymath"; "equation"; "gather"; "listing"; 
       "lstlisting"; "math"; "multline"; "verbatim"
    |]

let private preserveShortcuts = [| "\(" ; "\[" ; "$" ; "$$" |]


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


/// Regex that matches a command at the beginning of a line. Matches alphabetic
/// command names (with an optional *), as well as the shortcuts \[ and $$.
/// (These shortcuts are included because they must also preserve a line break
/// before them.) The alphabetic command name and first argument are captured as
/// groups 1 and 2.
///
/// This approach will have some false positives (anything can follow the
/// command name as long as the line ends with a '}'. A proper parser (that can
/// deal with nested '{}' would be better.
let private commandRegex: Regex =
    Regex(@"^" // Must be at beginning of string
        + @"(?:\$\$|\\\["       // Include $$ or \[
        + @"|\\(\[|[a-z]+)\*?"  // Or alphabetic command name with optional '*'
        + @")\s*"               // End of name group + whitespace
        + @"(?:\[[^\]]*\]\s*)?" // Optional [options] section, + whitespace
                                // Doesn't allow ']' (can this occur?)
        + @"(?:\{([a-z]+))?"    // Optional first {arg}, only leading letters are captured
        + @"(?:.*\})?"          // Capture the rest of the line if it ends with a '}'
        )


let private findPreserveSection beginMarker : Lines -> Blocks * Option<Lines> =
    
    let endMarker = 
        if beginMarker = "$" || beginMarker = "$$" then beginMarker
        else if beginMarker = "\(" then "\)"
        else if beginMarker = "\[" then "\]"
        else "\end{" + beginMarker + "}"

    let rec checkLine (line: string) (offset: int) : bool =
        let p = line.IndexOf(endMarker, offset)
        if p < 0 then false
        else if p = 0 || (line.Chars(p - 1) <> '\\') then true
        else checkLine line (p + 1) // IndexOf allows offset = length

    let rec loop acc (lines: List<string>): List<string> * List<string> =
        match lines with
            | [] ->
                (List.rev acc, [])
            | head :: tail ->
                if checkLine head 0 then (List.rev (head :: acc), tail)
                else loop (head :: acc) tail

    (fun (Nonempty(headLine, tailLines)) ->
        let sectionTail, remainingLines = loop [] tailLines

        ( Nonempty.singleton (NoWrap (Nonempty(headLine, sectionTail)))
        , Nonempty.fromList remainingLines
        )
    )


let latex (settings: Settings) : TotalParser =
    
    /// Checks the first line of a block of lines to see what sort of command it
    /// starts with, and outputs the corresponding block.
    let rec command (Nonempty(headLine, tailLines) as lines) : Option<Blocks * Option<Lines>> =
        let trimmedLine = headLine |> String.trim
        let cmdMatch = commandRegex.Match(trimmedLine)
        let cmdName, cmdArg = 
            if cmdMatch.Success then
                (cmdMatch.Groups.Item(1).Value, cmdMatch.Groups.Item(2).Value)
            else
                ("", "")

        // Check for preserved section
        if Array.contains trimmedLine preserveShortcuts then
            Some (findPreserveSection trimmedLine lines)
        else if cmdName = "begin" && Array.contains cmdArg preserveEnvironments then
            Some (findPreserveSection cmdArg lines)
            

        // Inline commands or normal text don't start or end a paragraph
        else if not cmdMatch.Success || Array.contains cmdName inlineCommands then
            None

        // Whole line is command: preserve break before & after
        else if cmdMatch.Length = trimmedLine.Length then
            Some 
                ( Nonempty.singleton (NoWrap (Nonempty(headLine, [])))
                , Nonempty.fromList tailLines
                )

        // Else start new paragraph and take lines until next
        else
            Some (takeFrom2ndLineUntil otherParsers plainText lines)

    /// Plain paragraph parser for paragraphs that don't start with commands
    and plainText: Lines -> Blocks =
        splitIntoChunks (afterRegex newlineRegex)
            >> Nonempty.map
                (firstLineIndentParagraphBlock settings.reformat)

    /// Combination of other parsers
    and otherParsers =
        tryMany [
            blankLines
            Comments.lineComment Markdown.markdown "%" settings
            command
        ]

    takeUntil otherParsers plainText |> repeatToEnd