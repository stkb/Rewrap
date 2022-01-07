module internal rec Parsers_Markdown

// This is a rec module because there's lots of recursive references among parsers. It's
// important to heed the FS0040 warnings, otherwise there can be runtime errors where
// functions are not yet set (though it does run fine in Fable). For this reason some
// functions here have had points added where they could normally be written point-free.
// To fix the warnings you usually have to make sure the containing function takes at
// least one arg before inner functions are defined.
//
// For markdown reference see https://spec.commonmark.org/

open Prelude
open Line
open Parsing_
open Parsing_Internal
open System
open System.Text.RegularExpressions


/// Creates a regex that allows up to 3 spaces before given pattern. Also ignores case.
let private mdMarker marker = regex (@"^ {0,3}" + marker)


//---------- Leaf blocks ---------------------------------------------------------------//


/// The default paragraph type (fallback for if no other parser matches)
let private defaultPara (_ctx: Context) : FirstLineParser =
  let lineBreakEnd = regex @"(\\|\s{2}|<br/?>)$"
  let rec parseLine (line: Line) : FirstLineRes =
    // We assume a >3 space indent on the first line would have already been
    // picked up by another parser, so it's ok to just trim all indent
    let line = Line.trimUpTo Int32.MaxValue line
    if isMatch lineBreakEnd line then Finished (LineRes(line, wrapBlock, true, None))
    else Pending (LineRes(line, wrapBlock, true, parseLine >> ThisLine))
  parseLine


/// Eg: ## Heading
let private atxHeading : TryNewParser =
  fun _ctx line ->
  if isMatch (mdMarker "#{1,6} ") line then Some (finished_ line noWrapBlock) else None


let private setextUnderline : TryNewParser = fun _ctx ->
  let rx = mdMarker @"(?:=+|-+)\s*$"
  tryMatch' rx >> map ^| fun (_, line) -> (finished_ line noWrapBlock)


/// Code block that begins and ends with ``` or ~~~
let private fencedCode : TryNewParser =
  fun _ctx ->
  let rec parseLine markerLength rxEnd line =
    match tryMatch rxEnd line with
    | Some m when m.[1].Length >= markerLength -> ThisLine (finished_  line noWrapBlock)
    | _ -> ThisLine (pending line noWrapBlock (parseLine markerLength rxEnd))
  let tryStart rxStart rxEnd line =
    tryMatch rxStart line |> map (fun m ->
      pending line noWrapBlock (parseLine m.[1].Length rxEnd)
    )
  let tildes = tryStart (mdMarker "(~{3,})") (mdMarker "(~{3,})\s*$")
  let backticks = tryStart (mdMarker "(`{3,})[^`]*$") (mdMarker "(`{3,})\s*$")
  tildes <|> backticks


/// Certain types of HTML blocks. See the commonmark spec
let private htmlType1To6 : TryNewParser =
  fun ctx ->
  let rec matchEnd (rx: Regex) fromPos (line: Line) : FirstLineRes =
    if rx.Match(line.content, fromPos).Success then finished_ line noWrapBlock
    else pending line noWrapBlock (matchEnd rx 0 >> ThisLine)

  let mkParser (patStart, patEnd) =
    let rxStart, rxEnd = mdMarker patStart, regex patEnd
    fun _ctx (line: Line) ->
    let m = rxStart.Match(line.content)
    if not m.Success then None else Some <| matchEnd rxEnd m.Length line

  let types : List<TryNewParser> =
    [ "<(script|pre|style)( |>|$)", "</(script|pre|style)>"
      "<!--", "-->"
      "<\\?", "\\?>"
      "<![A-Z]", ">"
      "<!\\[CDATA\\[", "]]>"
      "</?(address|article|aside|base|basefont|blockquote"
        + "|body|caption|center|col|colgroup|dd|details"
        + "|dialog|dir|div|dl|dt|fieldset|figcaption|figure"
        + "|footer|form|frame|frameset|h1|h2|h3|h4|h5|h6"
        + "|head|header|hr|html|iframe|legend|li|link|main"
        + "|menu|menuitem|meta|nav|noframes|ol|optgroup"
        + "|option|p|param|section|source|summary|table"
        + "|tbody|td|tfoot|th|thead|title|tr|track|ul)"
        + "(\\s|/?>|$)", "^\\s*$" // terminates on a blank line (works)
    ] |> map mkParser

  (tryMany types) ctx


/// Code block marked by an at-least-4-space indent
let private indentedCode : TryNewParser =
  fun _ctx ->
  let rec parseLine line =
    if not (isBlankLine line) && indentLength line < 4 then FinishedOnPrev None
    else ThisLine (pending line noWrapBlock parseLine)
  fun line ->
  if isBlankLine line then None
  else match parseLine line with | ThisLine r -> Some r | FinishedOnPrev _ -> None

let private table : TryNewParser =
  fun _ctx ->
  let rxCellsRow = mdMarker @"\S.*?[^\\]\|\s*\S"
  let rxSeparatorRow = mdMarker @"[|:-][ |:-]+$"
  let isSeparatorRow =
    Line.onContent ^| fun c -> rxSeparatorRow.IsMatch c && c.Contains "|" && c.Contains "-"
  let rec parseNext has2Lines hasSeparator (line: Line) =
    if not (isMatch rxCellsRow line) then
      if has2Lines && hasSeparator then FinishedOnPrev None
      else                FinishedOnPrev None
    else
    let hasSeparator = hasSeparator || isSeparatorRow line
    let block = if hasSeparator then noWrapBlock else wrapBlock
    let next = parseNext true hasSeparator
    ThisLine ^| Pending ^| LineRes (line, block, not hasSeparator, next)
  tryMatch' rxCellsRow >> map ^| fun (_, line) ->
    Pending ^| LineRes (line, wrapBlock, true, parseNext false (isSeparatorRow line))


let private thematicBreak : TryNewParser = fun _ctx ->
  let rx = mdMarker @"(?:\*\s*\*\s*(?:\*\s*)+|-\s*-\s*(?:-\s*)+|_\s*_\s*(?:_\s*)+)$"
  tryMatch' rx >> map ^| fun (_, line) -> finished_ line noWrapBlock


//---------- Container blocks ----------------------------------------------------------//


/// Makes a container. Takes a prefix-modifying function, and a function to test each line
/// to check we're still in the container, before passing the line to the inner parser.
let container : (string -> string) -> (Line -> Option<Line>) -> Context -> FirstLineParser =
  fun prefixFn lineTest ctx ->

  let rec wrapFLR : FirstLineRes -> FirstLineRes = function
  | Pending r -> Pending (wrapResultParser (nlpWrapper r.isDefault) r)
  | Finished r -> Finished (wrapResultParser (fun p -> Some (flpWrapper r.isDefault p)) r)

  and flpWrapper wasPara maybeInnerParser line : FirstLineRes =
    match lineTest line with
    | Some line -> (maybeInnerParser |? getNewBlock ctx) line |> wrapFLR
    | None ->
        match maybeInnerParser, wasPara with
        | Some p, true -> wrapFLR (p line)
        | _ -> (maybeInnerParser |? getNewBlock ctx) line

  and nlpWrapper wasPara innerParser line : NextLineRes =
    match lineTest line with
    | Some line -> innerParser line |> function
      | ThisLine flr -> ThisLine (wrapFLR flr)
      | FinishedOnPrev maybeThisLineRes ->
          let tlr = maybeThisLineRes ||? fun () -> getNewBlock ctx line
          FinishedOnPrev <| Some (wrapFLR tlr)
    | None when not wasPara -> FinishedOnPrev None
    | None -> innerParser line |> function
      | ThisLine x -> ThisLine (wrapFLR x)
      // This should only occur if the paragraph parser found a
      // (non-paragraph) interruption. So we're out (don't wrap result)
      | FinishedOnPrev x -> FinishedOnPrev x

  getNewBlock ctx >> wrapFLR >> wrapPrefixFn prefixFn


/// Block quote container. Reference: https://spec.commonmark.org/0.29/#block-quotes
let private blockquote : TryNewParser =
  fun ctx ->
  let findMarker line : Option<Line> =
    tryMatch (mdMarker "> ?") line |> map (fun c -> Line.adjustSplit (c.[0].Length) line)

  fun line -> findMarker line |> map (container id findMarker ctx)


/// Footnote container (not in commonmark)
let private footnote : TryNewParser =
  fun ctx ->
  let testLineIndent (line: Line) : Option<Line> =
    let minIndent = 4
    if not (isBlankLine line) && indentLength line < minIndent then None
    else Some (Line.adjustSplit minIndent line)

  fun line -> option {
    let! m = tryMatch (mdMarker "(\[\^\S+?\]:)( +)") line
    let prefixFn = blankOut' line.split m.[0].Length 0 "    "
    let line = Line.adjustSplit m.[0].Length line
    return container prefixFn (testLineIndent) ctx line
  }


/// Link reference definition. "Wraps" a single paragraph
let linkRefDef : TryNewParser =
  fun ctx ->
  let rxLabel = mdMarker @"\[\s*\S.*?\]:\s*"

  let rec wrapFLR : FirstLineRes -> FirstLineRes = function
  | Pending r -> Pending (wrapResultParser nlpWrapper r)
  | Finished r -> Finished (wrapResultParser (fun _ -> Some flpWrapper) r)

  and flpWrapper line : FirstLineRes =
    match tryMatch rxLabel line with
    | Some m -> onMatch line m
    | None ->
        match tryParaInterrupters ctx line with
        | Some flr -> flr
        | None -> defaultPara ctx line |> wrapFLR

  and nlpWrapper paraParser line : NextLineRes =
    match tryMatch rxLabel line with
    | Some m -> FinishedOnPrev <| Some (onMatch line m )
    | None ->
        match tryParaInterrupters ctx line with
        | None ->
            match paraParser line with
            | ThisLine flr -> ThisLine (wrapFLR flr)
            | FinishedOnPrev r -> FinishedOnPrev r
        | someParser -> FinishedOnPrev someParser

  and onMatch line (m: string[]) : FirstLineRes =
    let prefixFn = blankOut' line.split (indentLength line) 0 "    "
    defaultPara ctx (trimWhitespace line) |> wrapFLR |> wrapPrefixFn prefixFn

  fun line -> tryMatch rxLabel line <|>> onMatch line


/// List item container
let private listItem : TryNewParser =
  fun ctx ->
  let rx = mdMarker "([-+*]|[0-9]{1,9}[.)])( +)"

  let testLineIndent childIndent line =
    if not (isBlankLine line) && indentLength line < childIndent then None
    else Some (Line.adjustSplit childIndent line)

  fun line -> option {
    let! m = tryMatch rx line
    let childIndent = // If spaces after marker > 4 we trim it down to 1
      if m.[2].Length <= 4 then m.[0].Length else m.[0].Length - m.[2].Length + 1
    let prefixFn = blankOut line childIndent
    let line = Line.adjustSplit childIndent line
    return container prefixFn (testLineIndent childIndent) ctx line
  }


//---------- Putting it together -------------------------------------------------------//


/// List of blocks that can interrupt a paragraph
let private tryParaInterrupters : TryNewParser =
  tryMany [| blankLine; atxHeading; setextUnderline; thematicBreak;
             blockquote; footnote; listItem; fencedCode; htmlType1To6 |]


/// list of all blocks except setext underline and default paragraph
let private tryContentBlocks : TryNewParser =
    tryMany [| blankLine; atxHeading; thematicBreak; blockquote; footnote; listItem;
               fencedCode; htmlType1To6; linkRefDef; indentedCode; table |]


/// Finds a new block, checking all options and falling back on default paragraph in
/// necessary.
let private getNewBlock : Context -> Line -> FirstLineRes =
  fun ctx ->
  // Wraps a result from a paragraph to add linebreak marker handling
  let rec paragraphFallback ctx = defaultPara ctx >> function
    | Pending r -> Pending (LineRes(r.line, wrapBlock, true, parseOtherLine))
    | Finished r -> Finished (LineRes(r.line, wrapBlock, true, Some parseLineAfterLineBreak))
  and parseLineAfterLineBreak (line: Line) : FirstLineRes =
    (tryContentBlocks |? paragraphFallback) ctx line
  and parseOtherLine (line: Line) : NextLineRes =
    match tryParaInterrupters ctx line with
    | None -> ThisLine (paragraphFallback ctx line)
    | other -> FinishedOnPrev other
  parseLineAfterLineBreak


/// The same as markdown but with a header starting & ending with `---` lines
/// https://dotnet.github.io/docfx/spec/docfx_flavored_markdown.html
let markdown : ContentParser =
  fun ctx ->
  // This "container" makes sure we always return a next parser from markdown. Otherwise
  // the markdown function will be called again and we might erraneously detect a header
  // section in the middle of a document.
  let restOfContent : ContentParser = container id Some
  let yamlHeader =
    let marker = mdMarker "(---)\s*$"
    let rec parseLine line =
      match tryMatch marker line with
      | Some _ -> ThisLine (finished line noWrapBlock (restOfContent ctx))
      | None -> ThisLine (pending line noWrapBlock parseLine)
    fun line ->
      tryMatch marker line |> map (fun _ -> pending line noWrapBlock parseLine)

  fun line -> yamlHeader line ||? fun () -> restOfContent ctx line
