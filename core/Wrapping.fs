module internal Wrapping

open Nonempty
open Rewrap
open Block
open Extensions
open System.Text.RegularExpressions
open System


let charWidthEx tabSize index charCode =
    match charCode with
        | 0x0009us -> tabSize - (index % tabSize)
        | 0x0000us -> 1 // We use this as a placeholder for non breaking spaces
        | x when x < 0x0020us -> 0
        | x when x < 0x2E80us -> 1
        | x when x >= 0x2E80us && x <= 0xD7AFus -> 2
        | x when x >= 0xF900us && x <= 0xFAFFus -> 2
        | x when x >= 0xFF01us && x <= 0xFF5Eus -> 2
        | _ -> 1

let private charWidth =
    charWidthEx 1 0

let private isWhitespace cc =
    // \0 is a special placeholder we use ourselves for non-breaking space
    cc <> 0x0000us && cc <= 0x0020us || cc = 0x3000us

let isCJK charCode = 
    (charCode >= 0x3040us && charCode <= 0x30FFus)
        || (charCode >= 0x3400us && charCode <= 0x4DBFus)
        || (charCode >= 0x4E00us && charCode <= 0x9FFFus)

type CanBreak = Always | Sometimes | Never

let private specialChars = 
    [| (Never, Sometimes), "})]?,;¢°′″‰℃"
       (Never, Always)
       , "、。｡､￠，．：；？！％・･ゝゞヽヾーァィゥェォッャュョヮヵヶぁ"
            + "ぃぅぇぉっゃゅょゎゕゖㇰㇱㇲㇳㇴㇵㇶㇷㇸㇹㇺㇻㇼㇽㇾㇿ々〻ｧｨｩｪｫｬｭｮｯｰ”"
            + "〉》」』】〕）］｝｣"
       (Sometimes, Never), "([{"
       (Always, Never), "‘“〈《「『【〔（［｛｢£¥＄￡￥＋"
    |] 
        |> Array.map 
            (Tuple.mapSecond (fun s -> s.ToCharArray() |> Array.map (uint16)))

let canBreak charCode =
    if isWhitespace charCode then (Always, Always)
    else
        match Array.tryFind (snd >> Array.contains charCode) specialChars with
            | Some (res, _) -> res
            | None ->
                if isCJK charCode then (Always, Always) else (Sometimes, Sometimes)

let private canBreakBefore = fst << canBreak
let private canBreakAfter = snd << canBreak

let canBreakBetweenChars c1 c2 =
    match (canBreakAfter c1, canBreakBefore c2) with
        | Sometimes, Sometimes -> false
        | Never, _ -> false
        | _, Never -> false
        | _ -> true

// Wraps a string without newlines and returns a Lines with all lines but
// the last trimmed at the end. Takes a tuple of line widths for the first
// and rest of the lines.
let private wrapLines (headWidth, tailWidth) lines : Lines =
    let addEolSpacesWhereNeeded (ls: List<string>) (l: string) =
        let compare (h: string) =
            match (canBreakAfter (uint16 h.[h.Length-1]), canBreakBefore (uint16 l.[0])) with
                | Sometimes, Sometimes -> l :: " " :: ls
                | _ -> l :: ls
        match ls with | [] -> [l] | h :: _ -> compare h

    let str = 
        List.fold addEolSpacesWhereNeeded [] (Nonempty.toList lines)
            |> List.rev |> String.concat ""

    let rec findBreakPos min p =
        if p = min then min
        elif canBreakBetweenChars (uint16 str.[p-1]) (uint16 str.[p]) then p
        else findBreakPos min (p - 1)

    let rec loop acc mw s p w =
        if p >= str.Length then Nonempty(str.Substring(s), acc) else
        let cc = uint16 str.[p]
        if p = s && isWhitespace cc then loop acc mw (s+1) (p+1) w else
        let w' = w + charWidth cc //todo: change to charWidthEx
        if w' <= mw then loop acc mw s (p+1) w' else
        let bP = findBreakPos s p
        if bP = s then loop acc mw s (p+1) w' else
        let line = str.Substring(s, bP - s).TrimEnd()
        loop (line :: acc) tailWidth bP bP 0

    loop [] headWidth 0 0 0 |> Nonempty.rev

let private inlineTagRegex =
    Regex(@"{@[a-z]+.*?[^\\]}", RegexOptions.IgnoreCase)

let private addPrefixes prefixes =
    Nonempty.mapHead ((+) (fst prefixes))
        >> Nonempty.mapTail ((+) (snd prefixes))

/// Wraps a Wrappable, creating lines prefixed with its Prefixes
let wrap settings (prefixes, lines) : Lines =

    /// If the setting is set, adds an extra space to lines ending with .?!
    let addDoubleSpaces =
        Nonempty.mapInit 
            (fun (s: string) ->
                let t = s.TrimEnd()
                if settings.doubleSentenceSpacing
                    && Array.exists (fun (c: string) -> t.EndsWith(c)) [| ".";"?";"!" |]
                then t + "  "
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
        |> Nonempty.map freezeInlineTags
        |> wrapLines lineWidths 
        |> Nonempty.map unfreezeInlineTags
        |> addPrefixes prefixes
