module Rewrap.Core

open System
open Extensions
open Wrapping

let mutable private lastDocState : DocState = 
    { filePath = ""; version = 0; selections = [||] }

let private docWrappingColumns =
    new System.Collections.Generic.Dictionary<string, int>()

let getWrappingColumn filePath rulers =
    if not (docWrappingColumns.ContainsKey(filePath)) then
        docWrappingColumns.[filePath] <-
            Array.tryHead rulers |> Option.defaultValue 80
    docWrappingColumns.[filePath]

let maybeChangeWrappingColumn (docState: DocState) (rulers: int[]) : int =
    let filePath = docState.filePath
    if not (docWrappingColumns.ContainsKey(filePath)) then
        getWrappingColumn filePath rulers
    else
        if docState = lastDocState then
            let nextRulerIndex =
                rulers
                    |> Array.tryFindIndex ((=) docWrappingColumns.[filePath])
                    |> Option.map (fun i -> (i + 1) % rulers.Length)
                    |> Option.defaultValue 0

            docWrappingColumns.[filePath] <- rulers.[nextRulerIndex]

        docWrappingColumns.[filePath]

let saveDocState docState =
    lastDocState <- docState

let findLanguage name filePath : string =
    Parsing.Documents.findLanguage name filePath
        |> Option.map (fun l -> l.name)
        |> Option.defaultValue null

let languages : string[] =
    Parsing.Documents.languages
        |> Array.map (fun l -> l.name)

/// The main rewrap function, to be called by clients
let rewrap file settings selections (getLine: Func<int, string>) =
    let parser = Parsing.Documents.select file.language file.path
    let linesList =
        Seq.unfold
            (fun i -> Option.ofObj (getLine.Invoke(i)) |> Option.map (fun l -> (l,i+1)))
            0
            |> List.ofSeq |> Nonempty.fromListUnsafe
    linesList
        |> parser settings
        |> Selections.wrapSelected linesList selections settings

/// Gets the visual width of a string, taking tabs into account
let strWidth usTabSize (str: string) =
    let tabSize = max usTabSize 1
    let rec loop acc i =
        if i >= str.Length then acc
        else loop (acc + charWidthEx tabSize i ((uint16) str.[i])) (i + 1)
    loop 0 0

/// The autowrap function, to be called by clients. Checks conditions and does
/// an autowrap if all pass.
///
/// The client must supply the new text that was inserted in the edit, as well
/// as the position where it was inserted
let maybeAutoWrap file settings newText (pos: Position) (getLine: Func<int, string>) =
    let noEdit = { startLine=0; endLine=0; lines = [||]; selections = [||] }
    
    if String.IsNullOrEmpty(newText) then noEdit
    elif not (String.IsNullOrWhiteSpace(newText)) then noEdit else

    let enterPressed, indent =
        match newText.[0] with
        | '\r' -> true, newText.Substring(2) 
        | '\n' -> true, newText.Substring(1)
        | _ -> false, ""
    if not enterPressed && newText.Length > 1 then noEdit else

    let line, char =
        pos.line, pos.character + (if enterPressed then 0 else newText.Length)
    let lineText = getLine.Invoke(line)
    let visualWidth = strWidth settings.tabWidth (String.takeStart char lineText)
    if visualWidth <= settings.column then noEdit else

    let fakeSelection = {
        anchor = { line = line; character = 0 }
        active = { line = line; character = lineText.Length }
    }
    let wrappedGetLine =
        Func<int, string>(fun i -> if i > line then null else getLine.Invoke(i))
    rewrap file settings ([|fakeSelection|]) wrappedGetLine
        |> fun edit -> 
            let afterPos = 
                if enterPressed then { line = line + 1; character = indent.Length }
                else { line = line; character = char }
            { edit with selections = [| { anchor=afterPos; active=afterPos} |] }
