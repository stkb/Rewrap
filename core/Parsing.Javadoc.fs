module private Parsing.Javadoc

open Nonempty
open Parsing.Core
open Markdown
open System.Text.RegularExpressions


let private tagRegex =
    Regex(@"^\s*@(\w+)(.*)$")


let javadoc settings =

    let isStandaloneTag (m: Match) =
        m.Success && System.String.IsNullOrEmpty(m.Groups.Item(2).Value)

    let splitTaggedSection (Nonempty(headLine, tailLines) as lines) =
        let m = tagRegex.Match(headLine)
        if isStandaloneTag m then
            if m.Groups.Item(1).Value.ToLower() = "example" then
                Nonempty.singleton (Block.ignore lines)
            else
                let tagBlock = Block.Ignore 1
                match Nonempty.fromList tailLines with
                    | Some restLines ->
                        Nonempty.cons tagBlock (markdown settings restLines)
                    | None ->
                        Nonempty.singleton tagBlock
        else
            markdown settings lines

    splitIntoChunks (beforeRegex tagRegex)
        >> Nonempty.collect splitTaggedSection
