module Rewrap.Core

let rewrap 
    (language: string)
    (extension: string)
    (selections: seq<Selection>)
    (settings: Settings) 
    (lines: seq<string>) =

    let parser = 
        Parsing.Documents.select language extension

    let originalLines =
        List.ofSeq lines |> Nonempty.fromListUnsafe

    originalLines
        |> parser settings
        |> Selections.applyToBlocks selections settings
        |> Wrapping.wrapBlocks settings originalLines
