module internal Parsing.Comments

open System
open System.Text.RegularExpressions
open Prelude
open Rewrap
open Line
open Parsing.Core
open Parsing_

type private Lines = Nonempty<string>

let inline regex pat = Regex(pat, RegexOptions.IgnoreCase ||| RegexOptions.ECMAScript)

let extractWrappable
  (marker: string)
  (eraseIndentedMarker: bool)
  (reformatPrefix: string -> string)
  (settings: Settings)
  (lines: Lines)
  : Wrappable =

  let extractPrefix prefixRegex defaultPrefix tabWidth lines : string * int =
    List.tryFind Line.containsText lines
      |> Option.orElse (List.tryHead lines)
      |> Option.map (Line.split prefixRegex >> fst)
      |> Option.defaultValue defaultPrefix
      |> (fun p -> (p, (Line.tabsToSpaces tabWidth p).Length))

  let stripLines prefixRegex prefixLength tabWidth eraseIndentedMarker (lines: Nonempty<string>) : Nonempty<string> =
    let stripLine =
      Line.tabsToSpaces tabWidth
        >> Line.split prefixRegex
        >> Tuple.mapFirst
          (fun pre ->
            if eraseIndentedMarker then
              String.replicate pre.Length " "
                |> String.dropStart prefixLength
            else String.dropStart prefixLength pre
          )
        >> fun (pre, rest) -> pre + rest
    lines |> map stripLine

  let rx = regex (@"^(\s*)" + marker + @"\s*")

  let prefix, prefixLength =
    extractPrefix rx "" settings.tabWidth (Nonempty.toList lines)

  let newPrefix = if settings.reformat then (reformatPrefix prefix) else prefix

  ( (newPrefix, newPrefix)
  , stripLines rx prefixLength settings.tabWidth eraseIndentedMarker lines
  )

let private maybeReformat settings (prefix: string) : string =
  if not settings.reformat then prefix
  else prefix.TrimEnd() |> fun p -> if p = "" then p else p + " "

/// Convert all tabs in a line's content to spaces, to make things easier.
/// (Markdown, the content processor used in most cases, doesn't allow them
/// anyway.)
let tabsToSpacesContent : int -> Line -> Line =
  fun tabSize ->
  let step (maybeAccStr, accWidth) s =
    let accWidth' = accWidth + strWidth tabSize s
    match maybeAccStr with
      | None -> Some s, accWidth'
      | Some accStr ->
        let spcCount = tabSize - (accWidth % tabSize)
        Some (accStr + String.replicate spcCount " " + s), accWidth' + spcCount
  let convert initWidth (str: string) =
    str.Split([|'\t'|]) |> Array.fold step (None, initWidth) |> fst |> Option.get

  fun line -> Line.mapContent (convert (strWidth tabSize line.prefix)) line

/// Decoration lines are not wrapped and their prefix is not modified. With
/// normal lines the prefix may be adjusted if reformat is on, and the content
/// is passed to the content parser.
type LineType = Decoration | Normal

// This will turn a double-width char into spaces if split within it
let private splitAtWidth : int -> int -> int -> Line -> Line =
  fun tabWidth leftWidth extraWidth line ->
  let spaces n = String.replicate n " "
  let rec loop accWidth p =
    if p >= line.content.Length then Line(line.prefix + line.content, "") else

    let cc = (uint16) line.content.[p]
    let ccWidth = charWidth tabWidth (leftWidth + accWidth) cc
    let diff = extraWidth - accWidth - ccWidth

    if diff = 0 then Line.adjustSplit (p+1) line
    elif diff > 0 then loop (accWidth + ccWidth) (p+1)
    else // This is designed for tabs going over the split point. Are replaced with spaces
      let line = Line.adjustSplit p line
      Line(line.prefix + spaces (diff + ccWidth), spaces -diff + line.content.Substring(1))
  (if extraWidth < 1 then line else loop 0 0) |> tabsToSpacesContent tabWidth

/// Processes lines where the comment that has at least 1 text line, using the
/// given content parser.
let private processCommentContent settings contentParser prefix =
  let decorationLine (Nonempty((typ, line), rest)) =
    match typ with
      | Decoration ->
          Some (singleton (NoWrap (singleton (Line.toString line))), Nonempty.fromList rest)
      | Normal -> None
  let normalLines (lines: Nonempty<Line>) : Blocks =
    let prefix = maybeReformat settings prefix
    Block.splitUp (always prefix) (contentParser settings)
      (lines |> map (fun l -> l.prefix), lines |> map (fun l -> l.content))
  takeUntil decorationLine (map snd >> normalLines) |> repeatToEnd


/// Regex that captures whitespace at the beginning of a string
let private wsRegex = regex @"^\s*"

type CommentFormat =
  /// Line comment (//). Takes the comment lines and the column that's
  /// immediately after the comment markers.
  | LineFmt of Line Nonempty * int
  /// Block comment (/* ... */) that has multiple lines. Takes the first line
  /// with type; a list of "body" lines; the last line separately, if it only
  /// contains the end comment marker (without text preceeding it); and a string of
  /// characters that are allowed in each line prefix (eg "*")
  | MultiLineBlockFmt of (LineType * Line) * Line List * Line Option * string
  /// Block comment, but one that's only on a single line. Takes the prefix to
  /// use if new lines are added.
  | SingleLineBlockFmt of (LineType * Line) * string

/// Takes comment lines from either a line or block comment and parses into
/// blocks.
let private inspectAndProcessContent :
   CommentFormat -> (Settings -> TotalParser<string>) -> Settings -> Blocks =
  fun fmt contentParser settings ->

  let tabWidth = settings.tabWidth

  // Depending on the type of comment block, get the lines we want to look at,
  // prefix regex and initial indent
  let (lines: Line seq), prefixRegex, initialIndent =
    match fmt with
    | LineFmt (lines, initialIndent) ->
        lines :> seq<Line>, wsRegex, initialIndent
    | MultiLineBlockFmt (_, tLines, _, bodyMarkers) ->
        let bm = if bodyMarkers <> "" then "[" + bodyMarkers + @"]?\s*" else ""
        tLines :> seq<Line>, regex (@"^\s*" + bm), 0
    | SingleLineBlockFmt _ ->
        Seq.empty, wsRegex, 0

  let strWidth = strWidth' initialIndent tabWidth

  // 1st pass: categorize lines into either Decoration or Normal by examining
  // content after the prefix. Then, if the line is Normal and not blank, note
  // the indent of the content. The minimum indent of all lines will be taken as
  // the content indent for the whole block.
  let categorize (line: Line) =
    let m = prefixRegex.Match(line.content)
    if line.content.Length = m.Length then Normal, Int32.MaxValue
    else if (containsText line.content) then Normal, strWidth m.Value
    else Decoration, Int32.MaxValue
  let lines, indentIncrease =
    let mapping minIndent line =
      let typ, indent = categorize line in (typ, line), min minIndent indent
    lines |> Seq.mapFold mapping Int32.MaxValue

  // 2nd pass: examine existing decoration lines, and if their content is
  // indented the same as or further than the body indent, then convert them to
  // normal content lines. (In the case of a block comment, this is not done to
  // the first or last line.) For each normal line, increment its prefix so it
  // matches that of the body indent, then convert post-prefix tabs to spaces:
  let lines, maybeBodyPrefix =
    let rec adjust (typ, line: Line) =
      match typ with
        | Decoration ->
          let m = prefixRegex.Match(line.content)
          if strWidth m.Value >= indentIncrease then adjust (Normal, line)
          else (typ, line), None
        | Normal ->
          let line = splitAtWidth tabWidth initialIndent indentIncrease line
          let line = line |> tabsToSpacesContent tabWidth |> Line.mapPrefix (maybeReformat settings)
          (typ, line), if String.IsNullOrWhiteSpace(line.content) then None else Some line.prefix
    let mapping (maybePrefix: string Option) (line: LineType * Line) =
      let typ_line, mlp = adjust line in typ_line, maybePrefix |> Option.orElse mlp
    lines |> Seq.mapFold mapping None

  // Finish by — in the case of block comments — combining the adjusted body
  // lines with the first (and possibly last) lines of the block.
  let lines, defaultPrefix =
    match fmt with
      | LineFmt _ -> Nonempty.fromSeqUnsafe lines, ""
      | MultiLineBlockFmt (typ_line, _, mbLastLine, _) ->
        let last = maybe Seq.empty (tuple Decoration >> Seq.singleton) mbLastLine
        typ_line .@ List.ofSeq (Seq.append lines last), ""
      | SingleLineBlockFmt (typ_line, defaultPrefix) ->
          singleton typ_line, defaultPrefix

  let bodyPrefix = maybeBodyPrefix |? defaultPrefix
  singleton <| Comment (processCommentContent settings contentParser bodyPrefix lines)


/// Creates a line comment parser, given a content parser and marker.
let lineComment : (Settings -> TotalParser<string>) -> string -> Settings -> OptionParser<string,string> =
  fun contentParser marker settings ->

  let prefixRegex = regex (@"^(\s*)" + marker)
  let strWidth = strWidth' 0 settings.tabWidth

  /// Tries to match the given line with our comment marker. If it matches,
  /// returns the with of the indent of the marker, and a Line object.
  let tryMatchPrefix str : Line Option =
    let m = prefixRegex.Match(str)
    if m.Success then Some (Line(str, m.Length)) else None

  // Test lines for comment markers
  fun (Nonempty(firstStr, restLines)) -> option {
    // Try matching on first line and if so find all next that match
    let! firstLine = tryMatchPrefix firstStr
    let indent = strWidth firstLine.prefix
    let matchesFirst = tryMatchPrefix >> Option.filter (fun l -> strWidth l.prefix = indent)
    let moreLines, linesAfter = List.spanMaybes matchesFirst restLines
    let fmt = LineFmt (firstLine .@ moreLines, indent)

    return inspectAndProcessContent fmt contentParser settings, Nonempty.fromList linesAfter
  }


/// Creates a block comment parser, given a content parser and markers.
// Tail markers are used for all but the first line. tailMarker is a regex
// to capture prefix chars on these lines (eg '*' in javadoc).
// defaultTailMarker is the prefix string to insert on new lines if the
// comment was only 1 line long but becomes multiple.
let blockComment :
  (Settings -> TotalParser<string>) -> (string * string) -> (string * string) -> Settings
  -> OptionParser<string,string> =
  fun contentParser (bodyMarkers, defaultBodyMarker) (startMarker, endMarker) settings ->

  let startRegex, endRegex = regex (@"^\s*" + startMarker + @"\s*"), regex (endMarker)
  let inline hasTextUpTo p (str: string) = Line.containsText (str.Substring(0, p))

  /// Given remaining lines, finds the comment end marker
  let rec findEnd acc = function
    | [] -> List.rev acc, None, []
    | str :: rest ->
      let inline toLine (s: string) = Line("", s)
      let m = endRegex.Match(str)
      if m.Success then
        let body, last =
          if hasTextUpTo m.Index str then (toLine str :: acc), None
          else acc, Some (toLine str)
        in List.rev body, last, rest
      else findEnd (toLine str :: acc) rest

  fun (Nonempty(hStr, tStrs)) -> option {
    let mStart = startRegex.Match(hStr)
    if not mStart.Success then return! None else

    let hLine = Line(hStr, mStart.Length)
    let mkFirstLine p =
      if not (hasTextUpTo p hLine.content) then Decoration, hLine
      else
        let hLine = hLine |> tabsToSpacesContent settings.tabWidth
        Normal, hLine |> Line.mapPrefix (maybeReformat settings)

    let fmt, linesAfter =
      let mEnd = endRegex.Match(hLine.content)
      if mEnd.Success then
        let addedLinesPrefix = leadingWhitespace hLine.prefix + defaultBodyMarker
        SingleLineBlockFmt (mkFirstLine mEnd.Index, addedLinesPrefix), tStrs
      else
        let bodyLines, nonTextLastLine, linesAfter = findEnd [] tStrs
        let fl = mkFirstLine hLine.content.Length
        MultiLineBlockFmt (fl, bodyLines, nonTextLastLine, bodyMarkers), linesAfter

    return inspectAndProcessContent fmt contentParser settings, Nonempty.fromList linesAfter
  }
