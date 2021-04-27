module internal Selections

open Prelude
open Rewrap
open Block
open Wrapping
open System


// Private type used to represent line ranges, both of selections and blocks.
[<Struct>]
type private LineRange private (s: int, e: int) =
  member _.startLine = s
  member _.endLine = e
  member x.length =  max (x.endLine - x.startLine + 1) 0
  member x.isEmpty = x.endLine < x.startLine

  static member fromStartEnd startLine endLine = LineRange(startLine, endLine)

  static member fromStartLength startLine length = LineRange(startLine, startLine + length - 1)
  
  static member toInfinity startLine = LineRange(startLine, Int32.MaxValue - 1)

  static member fromSelection s =
    let startLine = min s.active.line s.anchor.line
    let endLine = max s.active.line s.anchor.line
    let isEmpty = startLine = endLine && s.anchor.character = s.active.character

    if isEmpty then
      LineRange(startLine, startLine - 1)
    // If active is below anchor and at column 0, don't count last line
    else if s.active.line > s.anchor.line && s.active.character = 0 then
        LineRange(s.anchor.line, s.active.line - 1)
    // Same but for anchor
    else if s.anchor.line > s.active.line && s.anchor.character = 0 then
        LineRange(s.active.line, s.anchor.line - 1)
    else
        LineRange(startLine, endLine)

  member x.shiftStartDown =
    if x.endLine > x.startLine then Some (LineRange(x.startLine + 1, x.endLine))
    else None
  member x.shiftEndUp =
    if x.endLine > x.startLine then Some (LineRange(x.startLine, x.endLine - 1))
    else None


let rec private intersects (r1: LineRange) (r2: LineRange) =
  if r2.isEmpty then intersects r2 r1
  elif r1.isEmpty then r1.startLine >= r2.startLine && r1.startLine <= r2.endLine
  else max r1.startLine r2.startLine <= min r1.endLine r2.endLine


/// Where consecutive ranges end and start on the same line, combines them. In
/// the case of a nonempty and an empty range, the empty takes precedence on
/// that line and we pretend that the nonempty range ended/started on the line
/// before/after the empty one. If the nonempty range was only 1 line then it's
/// removed. This function requires the ranges to already be in order from the
/// top of the document, but can cope with "invalid" ie overlapping nonempty
/// ranges.
let private normalizeRanges : LineRange seq -> LineRange list =
  let step (mCur: LineRange option, output: LineRange list) (next: LineRange) =
    match mCur with
    | None -> (Some next, output)
    | Some cur ->
      if cur.endLine >= next.startLine then
        if cur.isEmpty && next.isEmpty then (Some next, output)

        elif cur.isEmpty then
          match next.shiftStartDown with
          | None -> (Some cur, output)
          | Some shifted -> (Some shifted, cur :: output)

        elif next.isEmpty then
          match cur.shiftEndUp with
          | None -> (Some next, output)
          | Some shifted -> (Some next, shifted :: output)

        else
          (Some <| LineRange.fromStartEnd cur.startLine (max cur.endLine next.endLine), output)
      else
        (Some next, cur :: output)

  Seq.fold step (None, []) >> (uncurry List.maybeCons) >> List.rev


type ParseResult = {startLine : int; originalLines : Lines; blocks : Blocks}

/// Process blocks into lines, given the given selections. This might be called
/// on the subblocks of a comment.
let rec private processBlocks : OutputBuffer -> Settings -> LineRange list -> ParseResult -> unit =
  fun output settings selections parseResult ->

  // Splits a block into 2 parts at n lines
  let splitBlock n block : (Block * Option<Block>) =
    match block with
    | Wrap ((pHead, pTail), lines) ->
        Nonempty.splitAt n lines |> bimap
          (fun ls -> Wrap ((pHead, pTail), ls))
          (map (fun ls -> Wrap ((pTail, pTail), ls)))
    | NoWrap lines -> Nonempty.splitAt n lines |> bimap NoWrap (map NoWrap)
    | Comment _ -> raise (System.Exception("Not going to split a comment"))

  // Processes a block as if it were completely selected
  let rec processWholeBlock length origLines block : int * string Nonempty option =
    match block with
    | Wrap ((f, s), lines) -> output.wrap (f .@ [s], lines)
    | NoWrap lines -> output.noWrap lines
    | Comment subBlocks ->
        subBlocks |> Seq.iter (processWholeBlock 1 origLines >> ignore)
    length, origLines |> Nonempty.splitAt length |> snd

  let skipLines count lines : int * string Nonempty Option =
    count, lines |> Nonempty.splitAt count |> lmap output.skip |> snd

  let rec loop (sels: List<LineRange>) start (Nonempty(block, otherBlocks)) origLines =
    let blockLength = size block
    let selsTouching = sels |> List.filter (fun s -> s.startLine < (start + blockLength))
    let hasEmptySelection = selsTouching |> List.exists (fun s -> s.isEmpty)

    let (consumedLineCount, nextOrigLines), maybeRemainingPartialBlock =
      match List.tryHead selsTouching with
      | None -> skipLines blockLength origLines, None
      | Some sel ->
          match block with
          | Wrap _ | NoWrap _ ->
            if hasEmptySelection then processWholeBlock blockLength origLines block, None
            elif sel.startLine <= start then // First lines of block selected
              let splitAt = min (sel.endLine - start + 1) blockLength
              splitBlock splitAt block |> lmap (processWholeBlock splitAt origLines)
            else // First lines of block not selected
              let splitAt = sel.startLine - start
              splitBlock splitAt block |> lmap (fun _ -> skipLines splitAt origLines)
          | Comment subBlocks ->
              if hasEmptySelection && settings.wholeComment then
                processWholeBlock blockLength origLines block, None
              else
                let pr = {startLine = start; originalLines = origLines; blocks = subBlocks}
                processBlocks output settings sels pr
                (blockLength, origLines |> Nonempty.splitAt blockLength |> snd), None

    let nextBlocks = maybe otherBlocks (fun b -> b :: otherBlocks) maybeRemainingPartialBlock
    match Nonempty.fromList nextBlocks with
    | Some neNextBlocks ->
        let remaining = LineRange.toInfinity (start + consumedLineCount)
        let nextSels = sels |> List.skipWhile (not << intersects remaining)
        if nextSels.IsEmpty then ()
        else loop nextSels remaining.startLine neNextBlocks (Option.get nextOrigLines)
    | None -> ()

  loop selections parseResult.startLine parseResult.blocks parseResult.originalLines


// let private trimEdit (originalLines: Lines) (edit : Edit) : Edit =

//     let editNotZeroLength x =
//         edit.endLine - edit.startLine > x && edit.lines.Length > x

//     let originalLinesArray = originalLines |> Nonempty.toList |> List.toArray

//     let mutable s = 0
//     while editNotZeroLength s
//         && originalLinesArray.[edit.startLine + s] = edit.lines.[s]
//         do s <- s + 1

//     let mutable e = 0
//     while editNotZeroLength (s + e)
//         && originalLinesArray.[edit.endLine - e] = edit.lines.[edit.lines.Length - e - 1]
//         do e <- e + 1

//     { edit with
//         startLine = edit.startLine + s
//         endLine = edit.endLine - e
//         lines = edit.lines |> Array.skip s |> Array.truncate (edit.lines.Length - s - e)
//     }


let wrapSelected : Lines -> Selection seq -> Settings -> Blocks -> Edit =
  fun originalLines selections settings blocks ->

  let selectionRanges = selections |> Seq.map LineRange.fromSelection |> normalizeRanges
  let parseResult = {startLine = 0; originalLines = originalLines; blocks = blocks}
  let outputBuffer = OutputBuffer(settings)

  processBlocks outputBuffer settings selectionRanges parseResult
  {(outputBuffer.toEdit ()) with selections = Array.ofSeq selections}

    // trimEdit originalLines <|
    //     { startLine = 0
    //       endLine = size originalLines - 1
    //       lines = newLines
    //       selections = selections
    //     }
