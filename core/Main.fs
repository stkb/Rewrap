module Rewrap.Core

open Extensions


let languageFromFileName (fileName: string) : string =
    Parsing.Documents.languageFromFileName fileName |> Option.defaultValue null


let rewrap 
    (language: string)
    (filePath: string)
    (selections: seq<Selection>)
    (settings: Settings) 
    (lines: seq<string>) =

    let parser = 
        Parsing.Documents.select language filePath

    let originalLines =
        List.ofSeq lines |> Nonempty.fromListUnsafe

    originalLines
        |> parser settings
        |> Selections.applyToBlocks selections settings
        |> Wrapping.wrapBlocks settings originalLines
