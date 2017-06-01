module Rewrap

open OtherTypes

let rewrap 
    (language: string)
    (extension: string)
    (selections: seq<Selection>)
    (options: Options) 
    (lines: seq<string>) =

    let parser = 
        Parsing.Documents.select language extension

    let originalLines =
        List.ofSeq lines |> Nonempty.fromListUnsafe

    originalLines
        |> parser options
        |> Selections.applyToBlocks selections options
        |> Wrapping.wrapBlocks options originalLines
