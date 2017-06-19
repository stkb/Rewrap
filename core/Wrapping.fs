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

    // Wraps a string without newlines and returns a Lines with all lines but the last trimmed at the end
    let wrapString (width: int) (str: string) : Lines =
        let rec loop output (line: string) words =
            match words with
                | [] ->
                    Nonempty (line, output)

                | word :: nextWords ->
                    if line = "" then
                        loop output word nextWords
                    else if line.Length + 1 + word.Length > width then
                        loop (line :: output) word nextWords
                    else
                        loop output (line + " " + word) nextWords                    
        
        loop [] "" (List.ofArray (str.Split([|' '|])))
            |> Nonempty.mapTail (fun s -> s.TrimEnd())
            |> Nonempty.rev
    

    // Wraps a Wrappable, creating lines prefixed with its Prefixes
    let wrapWrappable (w: Wrappable) : Lines =

        let spacedHeadPrefix =
            Line.tabsToSpaces settings.tabWidth w.prefixes.head
        let tailPrefixLength =
            (Line.tabsToSpaces settings.tabWidth w.prefixes.tail).Length
        let headPrefixIndent =
            spacedHeadPrefix.Length - tailPrefixLength
        let wrapWidth =
            settings.column - tailPrefixLength

        let freezeInlineTags str =
            inlineTagRegex.Replace
                ( str
                , (fun (m: Match) -> m.Value.Replace(" ", "\0"))
                )

        let unfreezeInlineTags (str: string) =
            str.Replace("\0", " ")

        let concatenatedText =
            w.lines 
                |> Nonempty.mapInit 
                    (fun s ->
                        let t = s.TrimEnd()
                        if settings.doubleSentenceSpacing
                            && Array.exists (fun c -> t.EndsWith(c)) [| ".";"?";"!"|]
                        then t + " "
                        else t
                    )
                |> (Nonempty.toList >> String.concat " ")
                |> freezeInlineTags
        
        if headPrefixIndent > 0 then
            concatenatedText
                |> (+) (String.replicate headPrefixIndent "+")
                |> wrapString wrapWidth
                |> Nonempty.map unfreezeInlineTags
                |> Nonempty.mapHead
                    (String.dropStart headPrefixIndent >> (+) w.prefixes.head)
                |> Nonempty.mapTail ((+) w.prefixes.tail)
        else if headPrefixIndent < 0 then
            concatenatedText
                |> String.dropStart -headPrefixIndent
                |> wrapString wrapWidth
                |> Nonempty.map unfreezeInlineTags
                |> Nonempty.mapHead
                    ((+) (String.takeStart -headPrefixIndent concatenatedText)
                        >> (+) w.prefixes.head
                    )
                |> Nonempty.mapTail ((+) w.prefixes.tail)
        else
            concatenatedText
                |> wrapString wrapWidth
                |> Nonempty.map unfreezeInlineTags
                |> Nonempty.mapHead ((+) w.prefixes.head)
                |> Nonempty.mapTail ((+) w.prefixes.tail)

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
                    (List.safeSkip (Nonempty.length w.lines) remainingOriginalLines)
                    nextRemainingBlocks

            | Ignore n :: nextRemainingBlocks ->
                let blockLines, restLines =
                    List.splitAt n remainingOriginalLines
                loop (outputLines @ blockLines) restLines nextRemainingBlocks

    let newLines = 
        loop [] (Nonempty.toList originalLines |> List.safeSkip startLine) blocksToWrap
            |> Array.ofList

    { startLine = startLine; endLine = endLine; lines = newLines }
        



