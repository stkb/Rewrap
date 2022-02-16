module internal rec Parsers_RST

open Prelude
open Line
open Parsing_
open Parsing_Internal
open System


//---------- Helper functions ----------------------------------------------------------//


let private prefixWidth : Context -> Line -> int =
  fun ctx line -> strWidth ctx.settings.tabWidth line.prefix

let private contentWidth : Context -> Line -> int =
  fun ctx line -> strWidth' (prefixWidth ctx line) ctx.settings.tabWidth (line.content.TrimEnd ())

/// Gets the end column (right edge) of the line, not including whitespace
let private endCol (ctx: Context) (line: Line) : int =
  let tabWidth = ctx.settings.tabWidth
  let p = strWidth tabWidth line.prefix in p + strWidth' p tabWidth (line.content.TrimEnd ())

let private indentWidth (ctx: Context) (line: Line) : int =
  let tabWidth = ctx.settings.tabWidth
  let ws = line.content.Substring (0, line.content.Length - line.content.TrimStart().Length)
  strWidth' (strWidth tabWidth line.prefix) tabWidth ws

let private compareIndents ctx line1 line2 = prefixWidth ctx line2 - prefixWidth ctx line1

let private finishedOnPrev : FirstLineRes ->  NextLineRes = FinishedOnPrev << Some
let private finishedOnPrev_ = FinishedOnPrev None

/// If the given string starts with a punctiation character, returns it
let private startsWithPunc (s: string) : Option<Char> =
  if s.Length = 0 then None else
  let c = int s[0]
  if (c > 32 && c < 48) || (c > 57 && c < 65) || (c > 90 && c < 97) || (c > 122 && c < 127)
    then Some s[0] else None

/// Match a line of punctiation characters. Returns the length of indent & chars
let private matchesPuncLine (line: Line) : Option<int * int> =
  let str = line.content.TrimStart()
  let wsLength = line.content.Length - str.Length
  let str = str.TrimEnd()
  startsWithPunc str
    |> Option.filter (fun s -> Seq.forall ((=) s) str) |> voidRight (wsLength, str.Length)


//---------- Leaf blocks ---------------------------------------------------------------//


let private anonymousHyperlink : TryNewParser = fun ctx ->
  tryMatch' (regex "^__ \S") >> map ^| fun (_, line) -> finished_ line noWrapBlock


let private bulletItem : TryNewParser = fun ctx ->
  fun line -> option {
    // Don't capture spaces after the marker. Let the inner parser do it. This is how a
    // transition/title knows it's not at the base indent
    let! m = tryMatch (regex "^\s*[-+*•‣⁃](?=\s|$)") line
    let prefixFn = blankOut line m[0].Length
    return bodyElements false ctx (Line.adjustSplit m[0].Length line) |> wrapPrefixFn prefixFn
  }


let private docTestBlock : TryNewParser = fun ctx ->
  let rx = regex "^\s*>>>(?:\s|$)"
  let rec nextLine indent = trimWhitespace >> fun line ->
    if line.content = "" || prefixWidth ctx line < indent then finishedOnPrev_
    else ThisLine ^| pending line noWrapBlock (nextLine indent)
  let onMatch line = pending line noWrapBlock (nextLine (prefixWidth ctx line))
  tryMatch' rx >> map ^| (snd >> trimWhitespace >> onMatch)


let private lineBlock : TryNewParser = fun ctx ->
  let rx = regex "^(\s*)\|\s+"
  // Only text allowed in tail. No :: either
  let rec parseTail paraIndent (line: Line) : NextLineRes =
    match (blankLine <|> tryAnotherLineBlock paraIndent) ctx line with
    | None -> ThisLine <| pending (trimWhitespace line) wrapBlock (parseTail paraIndent)
    | someRes -> FinishedOnPrev someRes
  and tryAnotherLineBlock paraIndent _ (line: Line) =
    match tryMatch rx line with
    | Some m -> if m[1].Length > paraIndent then None else Some <| onMatch (m, line)
    | None -> None
  and onMatch ((m: string[]), line) : FirstLineRes =
    let prefixFn = blankOut line m[0].Length
    let line = Line.adjustSplit m[0].Length line
    pending line (wrapBlock' prefixFn) (parseTail m[1].Length)
  tryMatch' rx >> map onMatch


let private literalIndented (minIndent: int) : TryNewParser = fun ctx ->
  let rec parseTail line =
    if isBlankLine line || indentWidth ctx line > minIndent then
      ThisLine <| pending line noWrapBlock parseTail
    else finishedOnPrev_
  fun line ->
  if indentWidth ctx line > minIndent then Some <| pending line noWrapBlock parseTail
  else None


let private literalQuoted (indent: int): TryNewParser = fun ctx ->
  let rec parseTail (prefix: char) (line: Line) : NextLineRes =
    if not (line.content.Length > 0 && line.content[0] = prefix) then finishedOnPrev_
    else ThisLine <| pending line noWrapBlock (parseTail prefix)
  trimIndent ctx >> fun (lineIndent, line) ->
    if lineIndent <> indent then None
    else startsWithPunc line.content <|>> fun c -> pending line noWrapBlock (parseTail c)


let private numberedItem : TryNewParser = fun ctx ->
  let parseRoman : string -> Option<int> =
    let vals = [|
      [|"";"i";"ii";"iii";"iv";"v";"vi";"vii";"viii";"ix"|]
      [|"";"x";"xx";"xxx";"xl";"l";"lx";"lxx";"lxxx";"xc"|]
      [|"";"c";"cc";"ccc";"cd";"d";"dc";"dcc";"dccc";"cm"|]
      [|"";"m";"mm";"mmm";"mmmm" |]
      |]
    let rec loop (fn: string -> string) acc exp i (str: string) =
      if exp < 0 then (if str.Length = 0 then Some acc else None) else
      let pat = vals[exp][i]
      if not (str.StartsWith (fn pat)) then loop fn acc exp (i-1) str
      else loop fn (acc + pown 10 exp * i) (exp-1) 9 (str.Substring pat.Length)
    fun (str: string) ->
    if str.Length = 0 then None else
    let fn = if Char.IsUpper str[0] then (fun (s: string) -> s.ToUpper()) else id
    loop fn 0 3 4 str
  let rx = regex @"^\s*(\(?(?:(#)|([0-9]+)|([a-z])|([mdclxvi]+))[.)])(?=\s|$)"
  fun line -> option {
    let! m = tryMatch rx line
    if m[1].StartsWith "(" && m[1].EndsWith "." then return! None else
    let! _value = // value not used yet
      if m[2] = "#" then Some 1
      elif m[3] <> "" then Some (int m[3])
      elif m[4] <> "" then Some (int (m[4].ToUpper()[0]) - 64)
      else parseRoman m[5]
    let prefixFn = blankOut line m[0].Length
    return bodyElements false ctx (Line.adjustSplit m[0].Length line) |> wrapPrefixFn prefixFn
  }


/// Default block type. Only to be used if all others fail
let private paragraph (line2DeterminesIndent: bool): ContentParser = fun ctx ->

  let isSameIndent lineX lineY = prefixWidth ctx lineX = prefixWidth ctx lineY
  let isMoreIndented lineX lineY = prefixWidth ctx lineX > prefixWidth ctx lineY

  let rec blankLinesBeforeLiteral indent (line: Line) : NextLineRes =
    if isBlankLine line then
      ThisLine <| pending line noWrapBlock (blankLinesBeforeLiteral indent)
    else FinishedOnPrev <| (literalIndented indent <|> literalQuoted indent) ctx line

  let checkBlankAndLiteral prevLine (line: Line) : Option<NextLineRes> =
    if line.content <> "" then None
    elif isMatch (regex "::\s*$") prevLine then
      Some <| finishedOnPrev (pending line noWrapBlock (blankLinesBeforeLiteral (prefixWidth ctx prevLine)))
    else Some <| finishedOnPrev (finished_ line noWrapBlock)

  let rec checkIndentDifference prevLine line =
    if not (isSameIndent line prevLine) then finishedOnPrev_
    else ThisLine <| pending line wrapBlock (restOfParagraph line)

  and restOfParagraph prevLine : NextLineParser =
    trimWhitespace >> (checkBlankAndLiteral |? checkIndentDifference) prevLine

  // Only used on 2nd line. Line must be already trimmed
  let checkUnderline  prevLine (line: Line) : Option<NextLineRes> =
    if not line2DeterminesIndent && not (isSameIndent line prevLine) then None else
    match matchesPuncLine line with
    | Some (_, l) when l > 3 || l >= contentWidth ctx prevLine ->
        Some ^| ThisLine ^| finished_ line noWrapBlock
    | _ -> None

  // 3-line section title not possible here
  let otherLine2 prevLine =
    let altDef _ line = ThisLine <| pending line wrapBlock (restOfParagraph line)
    let def = if line2DeterminesIndent then altDef else checkIndentDifference
    trimWhitespace >> (checkBlankAndLiteral <|> checkUnderline |? def) prevLine

  let maybeTitleLine3 headLine prevLine : NextLineParser =
    let altFn _ (line: Line) =
      let sameIndentAsHeadLine = isSameIndent line headLine
      if sameIndentAsHeadLine && line.content.TrimEnd() = headLine.content.TrimEnd() then
        ThisLine <| finished_ line noWrapBlock
      elif isSameIndent line prevLine then
        if sameIndentAsHeadLine then ThisLine <| pending line wrapBlock (restOfParagraph line)
        else otherLine2 prevLine line
      else finishedOnPrev_

    trimWhitespace >> (checkBlankAndLiteral |? altFn) prevLine

  // only called when line1 not indented
  let maybeTitleLine2 prevLine : NextLineParser =
    let altFn _ (line: Line) =
      match isMoreIndented line prevLine, endCol ctx line > endCol ctx prevLine with
      | false, false -> ThisLine <| pending line wrapBlock (maybeTitleLine3 prevLine line)
      | false, true -> ThisLine <| pending line wrapBlock (restOfParagraph line)
      | true, false -> finishedOnPrev (pending line wrapBlock (maybeTitleLine3 prevLine line))
      | true, true -> finishedOnPrev_

    trimWhitespace >> (checkBlankAndLiteral <|> checkUnderline |? altFn) prevLine

  fun line ->
  let hasOverline = matchesPuncLine line |> Option.exists (fun (w,_) -> w = 0)
  let line = trimWhitespace line
  let next = if hasOverline then maybeTitleLine2 line else otherLine2 line
  pending line wrapBlock next


let private tableGrid : TryNewParser = fun ctx ->
  let rx = regex "^\+-{3}[-+]*\+\s*$"
  let rec nextLine prevLine = trimWhitespace >> fun line ->
    if line.content = "" then finishedOnPrev ^| finished_ line noWrapBlock else
    let otherIndent = compareIndents ctx prevLine line <> 0
    if otherIndent || (line.content[0] <> '|' && line.content[0] <> '+') then finishedOnPrev_
    else ThisLine ^| pending line noWrapBlock (nextLine line)
  trimWhitespace >> tryMatch' rx >> map ^| fun (_, line) -> pending line noWrapBlock (nextLine line)


let private tableSimple : TryNewParser = fun ctx ->
  let rx = regex "^=+(?:\s+=+)+\s*$"
  let rec afterHeader indent = trimWhitespace >> fun line ->
    if line.content = "" then ThisLine ^| pending line noWrapBlock (afterHeader indent) else
    let d = prefixWidth ctx line - indent
    if d < 0 then finishedOnPrev_
    elif d = 0 && isMatch rx line then ThisLine ^| finished_ line noWrapBlock
    else ThisLine ^| pending line noWrapBlock (afterHeader indent)
  let rec afterFirst indent = trimWhitespace >> fun line ->
    if line.content = "" then ThisLine ^| pending line noWrapBlock (afterFirst indent) else
    let d = prefixWidth ctx line - indent
    if d < 0 then finishedOnPrev_
    elif isMatch rx line then ThisLine ^| pending line noWrapBlock (afterHeader indent)
    else ThisLine ^| pending line noWrapBlock (afterFirst indent)
  trimWhitespace >> tryMatch' rx >> map ^| fun (_, line) ->
    pending line noWrapBlock (afterFirst (prefixWidth ctx line))


let private transitionOrTitle : TryNewParser = fun ctx ->
  let thirdLine line = ThisLine (finished_ line noWrapBlock)
  let secondLine = fun line ->
    if isBlankLine line then ThisLine (finished_ line noWrapBlock) else
    match matchesPuncLine line with
    | Some (ws, _) when ws = 0 -> ThisLine (finished_ line noWrapBlock)
    | _ -> ThisLine (pending line noWrapBlock thirdLine)
  fun line -> option {
    let! ws, l = matchesPuncLine line
    if l < 4 then return! None
    elif ws > 0 then return finished_ line noWrapBlock
    else return pending line noWrapBlock secondLine
  }


//---------- Container blocks ----------------------------------------------------------//


/// All other explicit markup blocks are not wrapped, while we work out the details
let private explicitOther : TryNewParser =
  fun ctx ->
  let rx = regex @"^(\s*)(?:\.\.(?=\s|$)|__\s\S)"
  let testLine indent line =
    if prefixWidth ctx (trimWhitespace line) > indent then Some line else None
  let rec literalLine test = test >> function
    | Some line -> ThisLine ^| pending line noWrapBlock (literalLine test)
    | None -> finishedOnPrev_
  let onMatch (m: string[], line) =
    let trimmedLine = Line.adjustSplit m[1].Length line
    let test = testLine (prefixWidth ctx trimmedLine)
    pending line noWrapBlock (literalLine test)
  tryMatch' rx >> map onMatch


/// Footnotes & citations can contain ant type of content
let private footnoteCitation : TryNewParser =
  fun ctx ->
  let rx = regex @"^(\s*)\.\. \[(?:\*|#(?:[-_.a-z0-9]+)?|[-_.a-z0-9]+)\](?=\s|$)"
  let testLine indent line =
    if prefixWidth ctx (trimWhitespace line) > indent then Some line else None
  let onMatch (m: string[], line) =
    let trimmedLine = Line.adjustSplit m[1].Length line
    let test = testLine (prefixWidth ctx trimmedLine)
    let prefixFn = blankOut' line.split m[0].Length 3 ""
    container (bodyElements true) prefixFn test ctx (Line.adjustSplit m[0].Length line)
  tryMatch' rx >> map onMatch


let private fieldItem : TryNewParser =
  fun ctx ->
  let rx = regex @"^(\s*):(.*?[^\\]):(?=\s|$)"
  let testLine indent line =
    if prefixWidth ctx (trimWhitespace line) > indent then Some line else None
  let rec literalLine test = test >> function
    | Some line -> ThisLine ^| pending line noWrapBlock (literalLine test)
    | None -> finishedOnPrev_
  let onMatch (m: string[], line) =
    let trimmedLine = Line.adjustSplit m[1].Length line
    let test = testLine (prefixWidth ctx trimmedLine)
    let prefixFn = blankOut' line.split m[0].Length 3 ""
    match m[2] with
    | "Address" -> pending line noWrapBlock (literalLine test)
    | _ -> container (bodyElements true) prefixFn test ctx (Line.adjustSplit m[0].Length line)
  tryMatch' rx >> map onMatch


//---------- Putting it together -------------------------------------------------------//


let private bodyElements : bool -> ContentParser =
  fun inFieldItem ->
    tryMany [| blankLine; anonymousHyperlink; footnoteCitation; explicitOther;
               transitionOrTitle; lineBlock; tableGrid; tableSimple; bulletItem;
               numberedItem; fieldItem; docTestBlock |]
      |? paragraph inFieldItem


let rst : ContentParser =
  fun ctx ->
  let rec wrapFLR : FirstLineRes -> FirstLineRes = function
    | Pending r -> Pending (wrapResultParser nlpWrapper r)
    | Finished r -> Finished (wrapResultParser (fun p -> Some (flpWrapper p)) r)
  and flpWrapper : Option<FirstLineParser> -> FirstLineParser = function
    | Some inner -> inner >> wrapFLR
    | None -> bodyElements false ctx
  and nlpWrapper inner line : NextLineRes = inner line |> function
    | ThisLine flr -> ThisLine (wrapFLR flr)
    | FinishedOnPrev maybeThisLineRes ->
        let tlr = maybeThisLineRes ||? fun () -> flpWrapper None line
        FinishedOnPrev <| Some (wrapFLR tlr)

  flpWrapper None
