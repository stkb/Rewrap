module Wrapping

open Prelude
open Rewrap
open Line
open System


let private isWhitespace cc =
  // \0 is a special placeholder we use ourselves for non-breaking space
  cc <> 0x0000us && cc <= 0x0020us || cc = 0x3000us


/// If a char is Chinese or Japanese. These chars can generally be broken between and
/// don't have spaces added between them if lines are concatenated.
let isCJ charCode =
  (charCode >= 0x3040us && charCode <= 0x30FFus)
  || (charCode >= 0x3400us && charCode <= 0x4DBFus)
  || (charCode >= 0x4E00us && charCode <= 0x9FFFus)


// Chars that under CJ rules, can't start or end a line, unless the is
// whitespace before/after them
let cjNoStart =
  ( "})]?,;¢°′″‰℃、。｡､￠，．：；？！％・･ゝゞヽヾーァィゥェォッャュョヮヵヶぁ"
  + "ぃぅぇぉっゃゅょゎゕゖㇰㇱㇲㇳㇴㇵㇶㇷㇸㇹㇺㇻㇼㇽㇾㇿ々〻ｧｨｩｪｫｬｭｮｯｰ”〉》」』】〕）］｝｣"
  ).ToCharArray() |> map (uint16)
let cjNoEnd = "([{‘“〈《「『【〔（［｛｢£¥＄￡￥＋".ToCharArray() |> map (uint16)

/// Returns whether we're allowed to break between the two given chars
let canBreakBetweenChars c1 c2 =
  if isWhitespace c1 || isWhitespace c2 then true
  elif Array.contains c1 cjNoEnd || Array.contains c2 cjNoStart then false
  elif isCJ c1 || isCJ c2 then true
  else false


/// When concatenating lines before breaking up again, whether to add a space
/// between chars
let addSpaceBetweenChars c1 c2 = if c1 = 10us || isCJ c1 || isCJ c2 then false else true


/// Concatenates lines into a single string, adding the right amount of
/// whitespace inbetween where necessary
let concatLines doubleSentenceSpacing lines =
  let stops = [| 0x2Eus; 0x3Fus; 0x21us |]
  let addLine (acc: string) (line: string) =
    // Shouldn't be getting empty strings in this function but just in case...
    if line = String.Empty || acc = String.Empty then acc else
    let acc = if acc.EndsWith("  ") then acc + "\n" else acc.TrimEnd()
    let accEnd = uint16 acc.[acc.Length-1]
    let space = if doubleSentenceSpacing && Array.contains accEnd stops then "  " else " "
    if addSpaceBetweenChars accEnd (uint16 line.[0]) then acc + space + line else acc + line
  List.reduce addLine (Nonempty.toList lines)


/// Breaks a string up into lines using the given prefixes and max width
let breakUpString addLine tabWidth maxWidth (str: string) =
  let popOrPeek (Nonempty(first, rest) as list) =
    first, Nonempty.fromList rest |? list

  /// Gets the width of the first prefix in the given list
  let prefixWidth prefixes = (strWidth tabWidth (Nonempty.head prefixes))

  /// Tries to find a break position, searching back from the given position as
  /// far as the given min position (exclusive). If none is found, returns the
  /// min position.
  let rec findBreakPos min p =
    if p = min then min
    elif canBreakBetweenChars (uint16 str.[p-1]) (uint16 str.[p]) then p
    else findBreakPos min (p - 1)

  // If pEnd <= pStart it's ignored and we take the rest of the string, and
  // don't trim the end
  let outputLine prefixes pStart pEnd =
    let prefix, nextPrefixes = popOrPeek prefixes
    let content =
      if pEnd > pStart then
        if str[pEnd-1] = '\n' then str.Substring(pStart, pEnd - pStart - 1)
        else str.Substring(pStart, pEnd - pStart).TrimEnd()
      else str.Substring(pStart)
    addLine (prefix + content.Replace('\000', ' '))
    nextPrefixes

  /// Iterate through chars of string
  let rec loop prefixes lineStart curWidth pStr =
    // If we're at the end
    if pStr >= str.Length then outputLine prefixes lineStart 0 |> ignore
    else
    let charCode = uint16 str.[pStr]
    // If we come across a LF char then start a new line
    if charCode = 10us then
      let nextPrefixes = outputLine prefixes lineStart (pStr+1)
      loop nextPrefixes (pStr+1) (prefixWidth nextPrefixes) (pStr+1)
    else
    let newWidth = curWidth + Line.charWidth tabWidth curWidth charCode
    // If current char is whitespace we don't need to wrap yet. Wait until we come across
    // a non-whitespace char
    if newWidth <= maxWidth || isWhitespace charCode then loop prefixes lineStart newWidth (pStr+1)
    else
    // We're past the wrapping column. Try to find a break position before current
    // position. If we don't find one keep going.
    let breakPos = findBreakPos lineStart pStr
    if breakPos <= lineStart then loop prefixes lineStart newWidth (pStr+1) else
    // We found a break pos. Output the line & start the next one
    let nextPrefixes = outputLine prefixes lineStart breakPos
    loop nextPrefixes breakPos (prefixWidth nextPrefixes) breakPos

  fun prefixes ->
    loop prefixes 0 (prefixWidth prefixes) 0


/// OutputBuffer came from an experiment in outputting all content using a
/// StringBuilder-type setup instead of a list of strings. That turned out not
/// to be faster in JS, but this still functions as a single object that holds
/// the settings and keeps track of where the edit starts and ends, and the new
/// content lines. Its future is uncertain. It may be turned into a monad.
type OutputBuffer(settings : Settings) =
  let mutable startLine = 0
  let mutable linesConsumed = 0
  let mutable outputLines = []

  member private _.IsEmpty = outputLines.IsEmpty

  member this.skip (lines: string Nonempty) =
    if this.IsEmpty then startLine <- startLine + size lines
    else this.noWrap lines

  member _.noWrap (lines: Line Nonempty) =
    let consumed, newOutputLines =
      lines |> Seq.fold (fun (c, ls) l -> (c + 1, (l.prefix + l.content) :: ls)) (0, outputLines)
    linesConsumed <- linesConsumed + consumed
    outputLines <- newOutputLines

  member _.noWrap (lines: string Nonempty) =
    let consumed, newOutputLines =
      lines |> Seq.fold (fun (c, ls) l -> (c + 1, l :: ls)) (0, outputLines)
    linesConsumed <- linesConsumed + consumed
    outputLines <- newOutputLines

  member this.wrap (mbPrefixFn: (string -> string) option, lines: Line Nonempty) =
    let prefixes = lines |> map (fun l -> l.prefix)
    let prefixes =
      match mbPrefixFn, prefixes with
      | Some fn, Nonempty (h, []) -> h .@ [fn h]
      | _ -> prefixes
    let contents = lines |> map (fun l -> l.content)
    this.wrap (prefixes, contents)

  member _.wrap (prefixes: string Nonempty, contents: string Nonempty) =
    let addLine line = outputLines <- line :: outputLines
    let str = concatLines settings.doubleSentenceSpacing contents
    let column = if settings.column > 0 then settings.column else Int32.MaxValue
    breakUpString addLine settings.tabWidth column str prefixes
    linesConsumed <- linesConsumed + size contents

  member _.toEdit () =
    Edit (startLine, startLine + linesConsumed - 1, List.toArray (List.rev outputLines), [||])
