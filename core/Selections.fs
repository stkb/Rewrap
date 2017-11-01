module internal Selections

open Nonempty
open Extensions
open Rewrap
open Block

// Private type used to represent line ranges, both of selections and blocks.
[<Struct>]
type private LineRange private (s: int, e: int) =
    member x.startLine = s
    member x.endLine = e

    member x.length =
        max (x.endLine - x.startLine + 1) 0
    member x.isEmpty =
        x.endLine < x.startLine

    static member fromStartEnd startLine endLine = 
        LineRange(startLine, endLine)

    static member fromStartLength startLine length = 
        LineRange(startLine, startLine + length - 1)

    static member fromSelection s =
        let startLine =
            min s.active.line s.anchor.line
        let endLine =
            max s.active.line s.anchor.line
        let isEmpty =
            startLine = endLine && s.anchor.character = s.active.character

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
        if x.endLine > x.startLine then
            Some (LineRange(x.startLine + 1, x.endLine))
        else
            None
    member x.shiftEndUp =
        if x.endLine > x.startLine then
            Some (LineRange(x.startLine, x.endLine - 1))
        else
            None


let rec private intersects (r1: LineRange) (r2: LineRange) =
    if r2.isEmpty then
        intersects r2 r1
    else if r1.isEmpty then
        r1.startLine >= r2.startLine && r1.startLine <= r2.endLine
    else
        max r1.startLine r2.startLine <= min r1.endLine r2.endLine


let private normalizeRanges =
    let rec loop (output: List<LineRange>) (input: List<LineRange>) =
        match input with
            | [] ->
                output

            | first :: [] ->
                 first :: output

            | first :: second :: rest ->
                if first.endLine = second.startLine then
                    if first.isEmpty && second.isEmpty then
                        loop output (second :: rest)

                    else if first.isEmpty then
                        match second.shiftStartDown with
                            | None ->
                                loop (first :: output) rest

                            | Some second_ ->
                                loop (first :: output) (second_ :: rest)

                    else if second.isEmpty then
                        match first.shiftEndUp with
                            | None ->
                                loop output (second :: rest)

                            | Some first_ ->
                                loop (first_ :: output) (second :: rest)
                    else
                        loop 
                            output 
                            (LineRange.fromStartEnd first.startLine second.endLine 
                                :: rest
                            )
                else
                    loop (first :: output) (second :: rest)

    loop [] >> List.rev

 
let private splitWrappable length ((pHead, pTail), lines) : Wrappable * Option<Wrappable> =
    
    let maybeTopLines =
        lines |> Nonempty.toList |> List.truncate length |> Nonempty.fromList

    let maybeBottomLines =
        lines |> Nonempty.toList |> List.safeSkip length |> Nonempty.fromList

    match maybeTopLines with
        | Some topLines ->
            ( ((pHead, pTail), topLines)
            , maybeBottomLines |> Option.map (fun ls -> ((pTail, pTail), ls))
            )

        | None ->
            raise (System.ArgumentException("Length < 1"))



let private ignoreBlock block =
    Block.Ignore (Block.length block)



let private withRange offset block =
    (block, LineRange.fromStartLength offset (Block.length block))


let private addRanges baseOffset blocks =
    Nonempty.unfold
        (fun (offset, Nonempty(head, tail)) ->
            ( withRange offset head
            , Nonempty.fromList tail
                |> Option.map (fun list -> (offset + Block.length head, list))
            )
        )
        (baseOffset, blocks)



// If wholeComments is true, if there's an empty selection, the
// section is passed through. If there's no empty selection, it's split up and each
// part is put through this function
let rec private applySelectionsToBlock 
    (selections: List<LineRange>)
    (wholeComment: bool)
    ((block, blockRange): Block * LineRange)
    : Nonempty<Block> =

    let selectionsTouching: List<LineRange> =
        selections |> List.filter (intersects blockRange)

    let hasEmptySelection =
        List.exists (fun (s: LineRange) -> s.isEmpty) selectionsTouching

    match List.tryHead selectionsTouching with

        // No selections touching: ignore whole block
        | None ->
            Nonempty.singleton (ignoreBlock block)

        | Some firstSelection ->
            match block with
                | Wrap(Comment p, wrappable) ->
                    if wholeComment && hasEmptySelection then
                        Nonempty.singleton block
                    else
                        Block.splitUp p wrappable
                            |> addRanges blockRange.startLine
                            |> Nonempty.collect
                                (applySelectionsToBlock selections wholeComment)

                | Wrap(prio, wrappable) ->
                    if hasEmptySelection then
                        Nonempty.singleton block
                    else
                        let selectionStartOffset =
                            firstSelection.startLine - blockRange.startLine

                        let selectionEndOffset =
                            firstSelection.endLine - blockRange.startLine + 1
                        
                        let ( splitAt, mapper ) =
                            // If selection start is within the block, we split at 
                            // that line; lines before are ignored
                            if selectionStartOffset > 0 then
                                ( selectionStartOffset, fun (_, ls) -> Block.ignore ls )
                            // Else we assume selection end is within the block
                            // and take lines up to there
                            else
                                ( selectionEndOffset, fun w -> Wrap(prio, w) )

                        let ( firstPart, maybeSecondPart ) =
                            splitWrappable splitAt wrappable
                                |> Tuple.mapFirst mapper
                    
                        match maybeSecondPart with
                            | Some secondPart ->
                                Nonempty.cons
                                    firstPart
                                    (applySelectionsToBlock
                                        selectionsTouching
                                        wholeComment
                                        (Wrap(prio, secondPart)
                                            |> withRange 
                                                (blockRange.startLine + splitAt)
                                        )
                                    )

                            | None ->
                                Nonempty.singleton firstPart

                | _ ->
                    Nonempty.singleton block




let applyToBlocks (selections: seq<Selection>) (settings: Settings) (blocks: Blocks) =
    
    let selectionRanges = 
        List.ofSeq selections 
            |> List.map LineRange.fromSelection
            |> normalizeRanges

    // Create list of blocks together with absolute start/end line. This
    // is done for ease of calculating intersections with selections later.
    let blocksWithRanges : Nonempty<Block * LineRange> =
        addRanges 0 blocks

    // For each block, check if there are selections that touch it,
    // otherwise ignore it. For non-comments only check non-comment
    // selections.
    let unselectedBlocksExcluded =
        blocksWithRanges
            |> Nonempty.map
                (fun (block, blockRange) ->
                    if
                        selectionRanges
                            |> List.filter (intersects blockRange)
                            |> (not << List.isEmpty)
                    then
                        ( block, blockRange )
                    else
                        ( ignoreBlock block, blockRange )
                )

    unselectedBlocksExcluded
        |> Nonempty.collect
            (fun (block, range) ->
                applySelectionsToBlock
                    selectionRanges
                    settings.wholeComment
                    (block, range)
            )







