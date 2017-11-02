module internal Wrapping

open System
open Nonempty
open Rewrap
open Block
open Extensions
open System.Text.RegularExpressions

let private inlineTagRegex =
    Regex(@"{@[a-z]+.*?[^\\]}", RegexOptions.IgnoreCase)

let private addPrefixes prefixes =
    Nonempty.mapHead ((+) (fst prefixes))
        >> Nonempty.mapTail ((+) (snd prefixes))

// Wraps a string without newlines and returns a Lines with all lines but
// the last trimmed at the end. Takes a tuple of line widths for the first
// and rest of the lines.
let private wrapString (headWidth, tailWidth) (str: string) : Lines =

    let rec loop lineWidth output (line: string) words =
        match words with
            | [] ->
                Nonempty (line, output)

            | word :: nextWords ->
                if line = "" then
                    loop lineWidth output word nextWords
                else if line.Length + 1 + word.Length <= lineWidth then
                    loop lineWidth output (line + " " + word) nextWords                    
                else
                    loop tailWidth (line :: output) word nextWords
        
    loop headWidth [] "" (List.ofArray (str.Split([|' '|])))
        |> Nonempty.mapTail String.trimEnd
        |> Nonempty.rev


/// Wraps a Wrappable, creating lines prefixed with its Prefixes
let wrap settings (prefixes, lines) : Lines =

    /// If the setting is set, adds an extra space to lines ending with .?!
    let addDoubleSpaces =
        Nonempty.mapInit 
            (fun (s: string) ->
                let t = s.TrimEnd()
                if settings.doubleSentenceSpacing
                    && Array.exists (fun (c: string) -> t.EndsWith(c)) [| ".";"?";"!" |]
                then t + " "
                else t
            )
        
    /// "Freezes" inline tags ({@tag }) so that they don't get broken up
    let freezeInlineTags str =
        inlineTagRegex.Replace
            ( str
            , (fun (m: Match) -> m.Value.Replace(' ', '\000'))
            )

    /// "Unfreezes" inline tags
    let unfreezeInlineTags (str: string) =
        str.Replace('\000', ' ')

    /// Tuple of widths for the first and other lines
    let lineWidths =
        prefixes
            |> Tuple.map
                (Line.tabsToSpaces settings.tabWidth
                    >> (fun s -> settings.column - s.Length)
                )
        
    lines
        |> addDoubleSpaces
        |> (Nonempty.toList >> String.concat " ")
        |> freezeInlineTags
        |> wrapString lineWidths 
        |> Nonempty.map unfreezeInlineTags
        |> addPrefixes prefixes
