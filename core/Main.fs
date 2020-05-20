module Rewrap.Core

open System
open Extensions
open Extensions.Option
open Wrapping
open Parsing.Language
open Parsing.Documents
open Columns

// Re-exports from Columns
let getWrappingColumn filePath rulers = getWrappingColumn filePath rulers
let maybeChangeWrappingColumn docState rulers = maybeChangeWrappingColumn docState rulers
let saveDocState docState = saveDocState docState


let languageNameForFile (file: File) : string =
    option null Language.name (languageForFile file)

let languages : string[] =
    Seq.map Language.name Parsing.Documents.languages |> Seq.toArray

/// The main rewrap function, to be called by clients
let rewrap file settings selections (getLine: Func<int, string>) =
    let parser = Parsing.Documents.select file
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
    // If column < 1 we never wrap
    elif settings.column < 1 then noEdit
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
