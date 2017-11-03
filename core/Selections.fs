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


type ParseResult = {
    startLine : int
    originalLines : Lines
    blocks : Blocks
}

 
let private splitWrappable n ((pHead, pTail), lines) =
    Nonempty.splitAt n lines
        |> Tuple.mapFirst (Wrappable.fromLines (pHead, pTail))
        |> Tuple.mapSecond (Option.map (Wrappable.fromLines (pTail, pTail)))


let rec private processBlocks
    (inComment: bool)
    (settings: Settings) 
    (selections: List<LineRange>) 
    (parseResult: ParseResult)
    : Lines =

        let wrap =
            Wrapping.wrap settings

        let reindent =
            Wrappable.toLines >> Nonempty.map String.trimEnd

        let rec loop output (sels: List<LineRange>) start (Nonempty(block, otherBlocks)) origLines =

            let blockLength =
                Block.length block
            let selsTouching =
                sels |> List.filter (fun s -> s.startLine < (start + blockLength))
            let hasEmptySelection =
                selsTouching |> List.exists (fun s -> s.isEmpty)

            let maybeNewLines, maybePartialBlock =
                match List.tryHead selsTouching with
                    | None ->
                        (None, None)

                    | Some sel ->
                        let applyToWrappable wrappable textType mapper =
                            if hasEmptySelection then
                                (Some (wrap wrappable), None)
                            else
                                let splitAt, mapFirst =
                                    if sel.startLine > start then 
                                        (sel.startLine - start, (fun _ -> None))
                                    else
                                        (sel.endLine - start + 1, mapper)
                                
                                splitWrappable splitAt wrappable
                                    |> Tuple.mapFirst mapFirst
                                    |> Tuple.mapSecond (Option.map (fun w -> Wrap (textType, w)))

                        match block with
                            
                            | NoWrap _ ->
                                (None, None)

                            | Wrap (Comment linesToBlocks, wrappable) ->
                                let commentSelections =
                                    if hasEmptySelection && settings.wholeComment then
                                        [ LineRange.fromStartLength start blockLength ]
                                    else
                                        sels
                                let commentParseResult =
                                    { startLine = start
                                    ; originalLines = origLines
                                    ; blocks = Block.splitUp linesToBlocks wrappable 
                                    }


                                ( Some (processBlocks true settings commentSelections commentParseResult)
                                , None
                                )


                            | Wrap (Text, wrappable) ->
                                applyToWrappable wrappable Text (Some << wrap)

                            | Wrap (Code, wrappable) ->
                                applyToWrappable wrappable Code (Some << reindent)

            let consumedLineCount, nextBlocks =
                match maybePartialBlock with
                    | Some partialBlock ->
                        (blockLength - Block.length partialBlock, partialBlock :: otherBlocks)
                    | None ->
                        (blockLength, otherBlocks)
            let newLines, maybeNextOrigLines =
                Nonempty.splitAt consumedLineCount origLines
                    |> Tuple.mapFirst (fun oL -> Option.defaultValue oL maybeNewLines)
            let nextOutput =
                Nonempty.rev newLines |> (fun (Nonempty(head, tail)) -> Nonempty(head, tail @ output))
            
            match Nonempty.fromList nextBlocks with
                | Some neNextBlocks ->
                    let nextStart =
                        start + consumedLineCount
                    let nextSels =
                        sels |> List.skipWhile 
                            (fun s -> 
                                s.endLine < nextStart && not (s.isEmpty && s.startLine >= nextStart)
                            )
                    
                    loop (Nonempty.toList nextOutput) nextSels nextStart neNextBlocks (Option.get maybeNextOrigLines)
                | None ->
                    Nonempty.rev nextOutput 
        
        loop [] selections parseResult.startLine parseResult.blocks parseResult.originalLines


let wrapSelected
    (originalLines: Lines)
    (selections: List<Selection>) 
    (settings: Settings) 
    (blocks: Blocks) 
    : Edit =

    let selectionRanges = 
        List.ofSeq selections 
            |> List.map LineRange.fromSelection
            |> normalizeRanges
    let parseResult =
        { startLine = 0
        ; originalLines = originalLines
        ; blocks = blocks
        }
    
    let newLines =
        processBlocks false settings selectionRanges parseResult
            |> Nonempty.toList
            |> List.toArray
    
    { startLine = 0; endLine = Nonempty.length originalLines - 1; lines = newLines }