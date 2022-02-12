/// A source code document is one that has source code and comments. Source code is only
/// wrapped if selected expressly.
module internal Parsing_SourceCode

// The main function is `sourceCode`. That, `lineComment` and `blockComment` are the
// public functions.
//
// Lots of code from Parsing.Comments has been copy-pasted just to make it work.
// Refactoring will come later

open Prelude
open Line
open Parsing_
open Parsing_Internal
open System
open System.Text.RegularExpressions


/// Decoration lines are not wrapped and their prefix is not modified. With normal lines
/// the prefix may be adjusted if reformat is on, and the content is passed to the content
/// parser.
type DecorationLine(basedOn: Line) = inherit Line (basedOn.prefix, basedOn.content)


/// Convert all tabs in a line's content to spaces, to make things easier. (Markdown, the
/// content processor used in most cases, doesn't allow them anyway.)
let private tabsToSpacesContent : int -> Line -> Line =
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


let private splitAtWidth : int -> int -> int -> Line -> Line =
  fun tabWidth leftWidth extraWidth line ->
  // This will turn a double-width char into spaces if split within it
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

/// Regex that captures whitespace at the beginning of a string
let private wsRegex = regex @"^\s*"

type CommentFormat =
  /// Line comment (//). Takes the comment lines and the column that's immediately after
  /// the comment markers.
  | LineFmt of Line Nonempty * int
  /// Block comment (/* ... */) that has multiple lines. Takes the first line with type; a
  /// list of "body" lines; the last line separately, if it only contains the end comment
  /// marker (without text preceeding it); and a string of characters that are allowed in
  /// each line prefix (eg "*")
  | MultiLineBlockFmt of Line * Line List * Line Option * string
  /// Block comment, but one that's only on a single line. Takes the prefix to use if new
  /// lines are added.
  | SingleLineBlockFmt of Line * (string -> string)


/// Wrapper that makes sure DecorationLines aren't wrapped
let withDecorations : ContentParser -> PrefixTransformer -> ContentParser =
  fun contentParser prefixFn ctx ->

  let rec wrapFLR = function
    | Pending r -> Pending (wrapResultParser nlpWrapper r)
    | Finished r -> Finished (wrapResultParser (fun p -> Some (flpWrapper p)) r)

  and flpWrapper maybeFLP : FirstLineParser = function
    | :? DecorationLine as line -> finished line noWrapBlock (flpWrapper None)
    | _ as line -> (maybeFLP |? contentParser ctx) line |> wrapFLR

  and nlpWrapper nlp : NextLineParser = function
    | :? DecorationLine as line -> FinishedOnPrev <| Some (flpWrapper None line)
    | _ as line -> nlp line |> function
        | ThisLine r -> ThisLine (wrapFLR r)
        | FinishedOnPrev maybeR ->
            FinishedOnPrev <| Some (maybe' (fun _ -> flpWrapper None line) wrapFLR maybeR)

  contentParser ctx >> wrapFLR >> wrapPrefixFn prefixFn


/// Takes comment lines from either a line or block comment and parses into blocks.
let private inspectAndProcessContent :
   ContentParser -> CommentFormat -> Context -> unit =
  fun contentParser fmt ctx ->

  let tabWidth = ctx.settings.tabWidth

  // Depending on the type of comment block, get the lines we want to look at, prefix
  // regex and initial indent (indent up to inc comment marker)
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

  // 1st pass: Examine each line's content after the prefix. Non-blank, non-text lines are
  // all marked as DecorationLines here. Otherwise, if the line is not blank, note the
  // indent of the content. The minimum indent of all normal lines will be taken as the
  // content indent for the whole block. We get back lines with decoration lines marked,
  // and the amount we need to increase prefix for all lines by.
  let lines, indentIncrease =
    let mapping minIndent (line: Line) =
      let m = prefixRegex.Match(line.content)
      if line.content.Length = m.Length then line, minIndent
      else if (containsText line.content) then line, min minIndent (strWidth m.Value)
      else DecorationLine(line) :> Line, minIndent
    lines |> Seq.mapFold mapping Int32.MaxValue

  // 2nd pass: examine existing decoration lines, and if their content is indented the
  // same as or further than the body indent, then convert them back to normal content
  // lines. (In the case of a block comment, this is not done to the first or last line.)
  // For each normal line, increment its prefix so it matches that of the body indent,
  // then convert post-prefix tabs to spaces. The bodyPrefix is the prefix to use for
  // newly-lines created after the first
  let lines, maybeBodyPrefix =
    let rec adjust : Line -> Line * Option<string> = function
      | :? DecorationLine as line ->
          let m = prefixRegex.Match(line.content)
          if strWidth m.Value >= indentIncrease then adjust (Line line) else upcast line, None
      | _ as line ->
          let line = splitAtWidth tabWidth initialIndent indentIncrease line
                      |> tabsToSpacesContent tabWidth
          line, if Line.isBlank line.content then None else Some line.prefix
    let mapping maybePrefix = adjust >> rmap (fun mlp -> maybePrefix <|> mlp)
    lines |> Seq.mapFold mapping None

  // In the case of block comments — combining the adjusted body lines with the first (and
  // possibly last) lines of the block.
  let lines, prefixFn =
    match fmt with
      | LineFmt _ -> Nonempty.fromSeqUnsafe lines, id
      | MultiLineBlockFmt (line, _, mbLastLine, _) ->
        let last: seq<Line> =
          maybe Seq.empty (fun l -> Seq.singleton (upcast DecorationLine(l))) mbLastLine
        line .@ List.ofSeq (Seq.append lines last), id
      | SingleLineBlockFmt (typ_line, prefixFn) ->
          singleton typ_line, prefixFn

  processContent (withDecorations contentParser prefixFn) ctx lines


type private LineCommentBlock (contentParser: ContentParser) =
  inherit NewBlock (BlockType.Comment)
  // To start with this just adds blocks to the container We'll need a prefixFn if we
  // support end-of-line comments
  override _.output ctx lines =
    let fmt = LineFmt (lines, strWidth ctx.settings.tabWidth (Nonempty.head lines).prefix)
    inspectAndProcessContent contentParser fmt ctx


let lineComment : string -> ContentParser -> TryNewParser =
  fun marker contentParser ->
  let rx = regex (@"^(\s*)" + marker)
  let tryMatchLine line = tryMatch rx line <|>> fun m -> Line.adjustSplit m.[0].Length line
  let comment = LineCommentBlock (contentParser)

  fun ctx ->
  let strWidth = strWidth' 0 ctx.settings.tabWidth

  let rec readRestOfBlock blockPrefixWidth =
    let rec readLine line =
      match tryMatchLine line with
      | Some line ->
         let linePrefixWidth = strWidth line.prefix
         if linePrefixWidth = blockPrefixWidth
           then ThisLine <| pending line comment readLine
           else FinishedOnPrev <| Some (pending line comment (readRestOfBlock linePrefixWidth))
      | None -> FinishedOnPrev None
    readLine

  tryMatchLine >> map (fun l -> pending l comment (readRestOfBlock (strWidth l.prefix)))


/// Takes the contents of the last line so that we later know when we've hit it
type private BlockCommentBlock
  (contentParser: ContentParser, bodyMarkers: string, defaultBodyMarker: string, prefixFn: string -> string, mEndIndex: int) =
  inherit NewBlock (BlockType.Comment)
  override _.output ctx (Nonempty(hLine, tLines)) =

    let inline hasTextUpTo p (str: string) = Line.containsText (str.Substring(0, min str.Length p))

    let mkFirstLine p : Line =
      if not (hasTextUpTo p hLine.content) then upcast (DecorationLine hLine)
      else tabsToSpacesContent ctx.settings.tabWidth hLine

    let fmt =
      match Nonempty.fromList tLines with
      | None ->
          SingleLineBlockFmt (mkFirstLine mEndIndex, prefixFn)
      | Some tail ->
          let bodyLines, nonTextLastLine =
            let (Nonempty(lastLine, bodyRev)) = Nonempty.rev tail
            if hasTextUpTo mEndIndex lastLine.content then tLines, None
            else List.rev bodyRev, Some lastLine
          let fl = mkFirstLine hLine.content.Length
          MultiLineBlockFmt (fl, bodyLines, nonTextLastLine, bodyMarkers)

    inspectAndProcessContent contentParser fmt ctx

let private mkPrefixFn start len rep (pre: string) : string =
  pre.Substring(0, start) + rep + pre.Substring(start + len)


/// Block comment parser
let blockComment :
  (string * string) -> (string * string) -> ContentParser -> TryNewParser =

  fun (bodyMarkers, defaultBodyMarker) (startMarker, endMarker) contentParser ->
  let startRegex = regex (@"^\s*" + startMarker + @"\s*")
  let comment prefixFn endMatchIndex =
    BlockCommentBlock(contentParser, bodyMarkers, defaultBodyMarker, prefixFn, endMatchIndex)

  let onFindStart (startLine: Line) (m: string[]) : FirstLineRes =
    // Make prefix replacement fn
    let prefixFn =
      let pre =
        if m.Length = 1 || m.[1] = "" then m.[0]
        else m.[0].Replace(m.[1], String.replicate m.[1].Length " ")
      let rep = leadingWhitespace pre + defaultBodyMarker
      mkPrefixFn startLine.prefix.Length m.[0].Length rep

    let defComment = comment prefixFn Int32.MaxValue
    let endPattern =
      let step (i:int, r:string) s = i + 1, r.Replace("$" + i.ToString(), s)
      Array.fold step (0, endMarker) m |> snd
    let endRegex = regex endPattern

    let rec testForEnd (line: Line) : FirstLineRes =
      let m = endRegex.Match(line.content)
      if m.Success then finished_ line (comment prefixFn m.Index)
      else pending line defComment (ThisLine << testForEnd)



    let startLine = Line.adjustSplit m.[0].Length startLine
    testForEnd startLine

  fun _ctx line -> tryMatch startRegex line <|>> onFindStart line


let sourceCode : List<TryNewParser> -> DocumentProcessor =
  fun commentParsers ->
  let contentParser = (tryMany commentParsers |? fun _ line -> finished_ line noWrapBlock)
  docOf contentParser
