module private Parsing.Javadoc

open Nonempty
open Parsing.Core
open System.Text.RegularExpressions


let javadoc settings =
    splitIntoChunks (beforeRegex (Regex("^\\s*@")))
        >> Nonempty.collect
            (fun (Nonempty(firstLine, _) as lines) ->
                if Line.contains (Regex("^\\s*@example")) firstLine then
                    Block.ignore lines |> Nonempty.singleton
                else
                    // Split after lines that contain only a tag
                    lines
                        |> (splitIntoChunks
                                (afterRegex (Regex("^\\s*@\\w+\\s*$")))
                            )
                        |> Nonempty.collect (Markdown.markdown settings)
            )