module internal Parsing.Language

open Rewrap
open Parsing.Core
open System

type Language =
    private Language of string * array<string> * array<string> * (Settings -> TotalParser)

module Language =

    let create (name: string) (aliases: string) (exts: string) parser : Language =
        let split (s: string) = s.ToLower().Split([|'|'|], StringSplitOptions.RemoveEmptyEntries)
        Language(name, Array.append [|name.ToLower()|]  (split aliases), split exts, parser)

    let name (Language(n,_,_,_)) = n

    let parser (Language(_,_,_,p)) = p

    let matchesFileLanguage (fileLang: string) (Language(_,ids,_,_)) =
        Seq.contains (fileLang.ToLower()) ids

    let matchesFilePath (path: string) (Language(_,_,exts,_)) =
        let extOrName =
            match (path.ToLower().Split('\\', '/') |> Array.last).Split('.') with
                | [| noExt |] -> noExt
                | arr -> "." + Array.last arr
        Seq.contains extOrName exts
