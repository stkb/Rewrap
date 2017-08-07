module internal Wrapping

open System
open Nonempty
open Rewrap
open Block
open Extensions
open System.Text.RegularExpressions

let private inlineTagRegex =
    Regex(@"{@[a-z]+.*?[^\\]}", RegexOptions.IgnoreCase)

let wrapBlocks (settings: Settings) (originalLines: Lines) (blocks: Blocks) : Edit =

    // Wraps a string without newlines and returns a Lines with all lines but
    // the last trimmed at the end. Takes a tuple of line widths for the first
    // and rest of the lines.
    let wrapString (headWidth, tailWidth) (str: string) : Lines =

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
            |> Nonempty.mapTail (fun s -> s.TrimEnd())
            |> Nonempty.rev

    /// Wraps a Wrappable, creating lines prefixed with its Prefixes
    let wrapWrappable (prefixes, lines) : Lines =

        /// If the setting is set, adds an extra space to lines ending with .?!
        let addDoubleSpaces =
            Nonempty.mapInit 
                (fun (s: string) ->
                    let t = s.TrimEnd()
                    if settings.doubleSentenceSpacing
                        && Array.exists (fun c -> t.EndsWith(c)) [| ".";"?";"!"|]
                    then t + " "
                    else t
                )
        
        /// "Freezes" inline tags ({@tag }) so that they don't get broken up
        let freezeInlineTags str =
            inlineTagRegex.Replace
                ( str
                , (fun (m: Match) -> m.Value.Replace(" ", "\0"))
                )

        /// "Unfreezes" inline tags
        let unfreezeInlineTags (str: string) =
            str.Replace("\0", " ")

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
            |> Nonempty.mapHead ((+) (fst prefixes))
            |> Nonempty.mapTail ((+) (snd prefixes))
        

    let startLine =
        List.takeWhile Block.isIgnore (Nonempty.toList blocks)
            |> List.sumBy Block.length
    let blocksToWrap =
        List.skipWhile Block.isIgnore (Nonempty.toList blocks)
            |> (List.rev >> List.skipWhile Block.isIgnore >> List.rev)
    let endLine = 
        startLine + (List.sumBy Block.length blocksToWrap) - 1

    let rec loop outputLines remainingOriginalLines remainingBlocks =
        match remainingBlocks with
            | [] ->
                outputLines

            | Wrap (Comment p, w) :: nextRemainingBlocks ->
                loop
                    outputLines
                    remainingOriginalLines
                    (Block.splitUp p w
                        |> Nonempty.toList
                        |> (fun bs -> bs @ nextRemainingBlocks)
                    )

            | Wrap (_, w) :: nextRemainingBlocks ->
                loop
                    (outputLines @ (Nonempty.toList (wrapWrappable w)))
                    (List.safeSkip (Nonempty.length (snd w)) remainingOriginalLines)
                    nextRemainingBlocks

            | Ignore n :: nextRemainingBlocks ->
                let blockLines, restLines =
                    List.splitAt n remainingOriginalLines
                loop (outputLines @ blockLines) restLines nextRemainingBlocks

    let newLines = 
        loop [] (Nonempty.toList originalLines |> List.safeSkip startLine) blocksToWrap
            |> Array.ofList

    { startLine = startLine; endLine = endLine; lines = newLines }
        



