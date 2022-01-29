module internal Parsing.Latex

open Prelude
open Nonempty
open Rewrap
open Block
open Parsing_
open Parsing.Core
open System.Text.RegularExpressions


let private markdown = Parsing.Markdown.markdown

let private newlineRegex: Regex =
    Regex
        ( @"(\\(\\\*?|hline|newline|break|linebreak)(\[.*?\])?(\{.*?\})?\s*$)"
        + @"|([^\\]%)"
        )

/// Commands that, when starting a line, should always preserve the line break
/// before them, even if text comes right after. For other commands, the rule
/// that a command being alone on a line preserves breaks before and after is
/// enough.
let private blockCommands =
    [| "["; "begin"; "item" |]

let private preserveEnvironments =
    [| "align"; "alltt"; "displaymath"; "equation"; "gather"; "listing";
       "lstlisting"; "math"; "multline"; "verbatim"
    |]
        |> Array.collect (fun x -> [| x; x + "*" |])

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
/// before them.) The command name and first argument contained in {} are
/// captured as groups 1 and 2 respectively. Following that any number of [] or
/// {} args are allowed.
///
/// This approach may have some false positives and doesn't for example allow
/// nested [] or {}. A proper parser would be better.
let private commandRegex: Regex =
    Regex(@"^"                   // Must be at beginning of string
        + @"\\(\[|[a-z]+)\*?\s*" // Command name with optional '*', + whitespace
        + @"(?:(?:"              // Either
        + @"\[.*?\]"             // Anything in []
        + @"|"                   // Or
        + @"\{(.*?)\}"           // Anything in {} (& capture the contents)
        + @")\s*)*"              // Can have whitespace after & occur many times
        )


let private findPreserveSection beginMarker : Nonempty<string> -> Blocks * Option<Nonempty<string>> =

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


let latex (settings: Settings) : TotalParser<string> =

    /// Checks the first line of a block of lines to see what sort of command it
    /// starts with, and outputs the corresponding block.
    let rec command (Nonempty(headLine, tailLines) as lines) : Option<Blocks * Option<Nonempty<string>>> =
        let trimmedLine = headLine |> String.trim
        let cmdMatch = commandRegex.Match(trimmedLine)
        let cmdName, cmdArg, isWholeLine =
            if cmdMatch.Success then
                ( cmdMatch.Groups.Item(1).Value
                , cmdMatch.Groups.Item(2).Value
                , cmdMatch.Length = trimmedLine.Length
                )
            else
                ("", "", false)

        // Check for preserved section
        if Array.contains trimmedLine preserveShortcuts then
            Some (findPreserveSection trimmedLine lines)
        else if cmdName = "begin" && Array.contains cmdArg preserveEnvironments then
            Some (findPreserveSection cmdArg lines)

        // Whole line is command: preserve break before & after
        else if isWholeLine then
            Some
                ( Nonempty.singleton (NoWrap (Nonempty(headLine, [])))
                , Nonempty.fromList tailLines
                )

        // For some 'block' commands, keep line break before
        else if trimmedLine.StartsWith("$$")
            || Array.contains cmdName blockCommands
        then
            Some (takeFrom2ndLineUntil otherParsers plainText lines)

        // Else we don't match: don't start or end the paragraph
        else
            None

    /// Plain paragraph parser for paragraphs that don't start with commands
    and plainText: Nonempty<string> -> Blocks =
        let freezeAnyEOLComment (str: string) =
            let m = Regex(@"[^\\]%").Match(str)
            if not m.Success then str else
            let p = m.Index + 2
            str.Substring(0, p) + str.Substring(p).Replace(' ', '\000')
        let processChunk =
            (Nonempty.mapTail freezeAnyEOLComment >> firstLineIndentParagraphBlock settings.reformat)

        splitIntoChunks (afterRegex newlineRegex) >> map processChunk

    /// Combination of other parsers
    and otherParsers =
        tryMany [
            blankLines
            Comments.lineComment markdown "%" settings
            command
        ]

    takeUntil otherParsers plainText |> repeatToEnd
