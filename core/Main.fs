module Rewrap.Core

open System
open Extensions

let mutable private lastDocState : DocState = 
    { filePath = ""; language = ""; version = 0; selections = [||] }

let private docWrappingColumns =
    new System.Collections.Generic.Dictionary<string, int>()

let getDocWrappingColumn (docState: DocState) (rulers: int[]) : int =
    let filePath = docState.filePath

    if rulers.Length = 0 then 80
    else if rulers.Length = 1 then rulers.[0]
    else
        if not (docWrappingColumns.ContainsKey(filePath)) then
            docWrappingColumns.[filePath] <- rulers.[0]

        else if docState = lastDocState then
            let nextRulerIndex =
                rulers
                    |> Array.tryFindIndex ((=) docWrappingColumns.[filePath])
                    |> Option.map (fun i -> (i + 1) % rulers.Length)
                    |> Option.defaultValue 0

            docWrappingColumns.[filePath] <- rulers.[nextRulerIndex]

        docWrappingColumns.[filePath]

let saveDocState docState =
    lastDocState <- docState

let cursorBeforeWrappingColumn 
    (filePath: string)
    (tabSize: int)
    (line: string)
    (character: int)
    (getWrappingColumn: System.Func<int>) 
    =
    let wrappingColumn = 
        if not (docWrappingColumns.ContainsKey(filePath)) then
            docWrappingColumns.[filePath] <- getWrappingColumn.Invoke()
        docWrappingColumns.[filePath]
    let cursorColumn =
        line |> String.takeStart character |> Line.tabsToSpaces tabSize |> String.length
    cursorColumn <= wrappingColumn

let findLanguage name filePath : string =
    Parsing.Documents.findLanguage name filePath
        |> Option.map (fun l -> l.name)
        |> Option.defaultValue null

let languages : string[] =
    Parsing.Documents.languages
        |> Array.map (fun l -> l.name)

/// The main rewrap function, to be called by clients
let rewrap docState settings (getLine: Func<int, string>) =
    let parser = Parsing.Documents.select docState.language docState.filePath
    let linesList =
        Seq.unfold 
            (fun i -> Option.ofObj (getLine.Invoke(i)) |> Option.map (fun l -> (l,i+1)))
            0
            |> List.ofSeq |> Nonempty.fromListUnsafe
    linesList
        |> parser settings
        |> Selections.wrapSelected 
            linesList (List.ofSeq docState.selections) settings

/// The autowrap function, to be called by clients
let autoWrap docState settings (getLine: Func<int, string>) =
    let {line=line; character=col} = 
        docState.selections.[0].active
    let lineSelection = {
        anchor = { line = line; character = 0 }
        active = { line = line; character = col }
    }
    let getLine' = 
        Func<int, string>(fun i -> if i > line then null else getLine.Invoke(i))
    rewrap { docState with selections = [| lineSelection |] } settings getLine'
