﻿module internal Parsing.Language

open System
open Parsing_

type Language =
    private Language of string * array<string> * array<string> * DocumentProcessor

module Language =

    // Takes 4 args to create a Language:
    //  1. display name (used only in VS)
    //  2. string of aliases (language IDs used by the client. Not needed if
    //     they only differ from display name by casing)
    //  3. string of file extensions (including `.`). Used to give support to
    //     files that are not known by the client.
    //  4. document processor
    //
    // Aliases and extensions are separated by `|`
    let create (name: string) (aliases: string) (exts: string) parser : Language =
        let split (s: string) = s.ToLower().Split([|'|'|], StringSplitOptions.RemoveEmptyEntries)
        Language(name, Array.append [|name.ToLower()|]  (split aliases), split exts, parser)

    let name (Language(n,_,_,_)) = n

    let parser (Language(_,_,_,p)) = p

    let matchesFileLanguage (fileLang: string) (Language(_,ids,_,_)) =
        Seq.contains (fileLang.ToLower()) ids

    let matchesFilePath (path: string) (Language(_,_,exts,_)) =
        let fileName = path.ToLower().Split('\\', '/') |> Array.last
        let tryMatch : string -> bool = function
            | ext when ext.StartsWith(".") -> fileName.EndsWith(ext)
            | fullname -> fileName.Equals(fullname)

        Seq.exists tryMatch exts
