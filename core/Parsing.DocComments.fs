module internal Parsing.DocComments
// This module deals with special formatting inside comments that are used for
// documentation, eg javadoc, xmldoc, RDoc

open Prelude
open Block
open Parsing.Core
open Markdown
open Sgml
open System.Text.RegularExpressions


/// Splits lines into sections which start with lines matching the given regex.
/// For each of those sections, the sectionParser is applied to turn the lines
/// into blocks. If the first section doesn't start with a matching line, it is
/// processed with the normal markdown parser.
let private splitBeforeTags (regex: Regex) sectionParser settings (Nonempty(outerHead, outerTail)) =

    let rec prependRev (Nonempty(head, tail)) maybeRest =
        let nextRest =
            match maybeRest with
                | Some rest ->
                    Nonempty.cons head rest
                | None ->
                    Nonempty.singleton head

        match Nonempty.fromList tail with
            | Some next ->
                prependRev next (Some nextRest)
            | None ->
                nextRest

    let rec loop (tagMatch: Match) buffer maybeOutput lines =

        let parser =
            if tagMatch.Success then sectionParser tagMatch else markdown

        let addBufferToOutput () =
            prependRev (parser settings (Nonempty.rev buffer)) maybeOutput

        match lines with
            | headLine :: tailLines ->
                let m =
                    regex.Match(headLine)

                let nextTagMatch, nextBuffer, nextOutput =
                    if m.Success then
                        ( m
                        , Nonempty.singleton headLine
                        , Some (addBufferToOutput ())
                        )
                    else
                        (tagMatch, Nonempty.cons headLine buffer, maybeOutput)

                loop nextTagMatch nextBuffer nextOutput tailLines

            | [] ->
                (addBufferToOutput ()) |> Nonempty.rev


    loop (regex.Match(outerHead)) (Nonempty.singleton outerHead) None outerTail


let javadoc =
    let tagRegex =
        Regex(@"^\s*@(\w+)(.*)$")

    splitBeforeTags tagRegex
        (fun m ->
            if Line.isBlank (m.Groups.Item(2).Value) then
                if m.Groups.Item(1).Value.ToLower() = "example" then
                    (fun _ -> Block.ignore >> Nonempty.singleton)
                else
                    ignoreFirstLine markdown
            else
                markdown
        )


/// DartDoc has just a few special tags. We keep lines beginning with these
/// unwrapped.
let dartdoc =
    let tagRegex =
        Regex(@"^\s*(@nodoc|{@template|{@endtemplate|{@macro)")

    splitBeforeTags tagRegex (fun _ -> ignoreFirstLine markdown)


let psdoc =

    let tagRegex =
        Regex(@"^\s*\.([A-Z]+)")

    let codeLineRegex =
        Regex(@"^\s*PS C:\\>")

    let exampleSection settings lines =
        let trimmedExampleSection =
            ignoreFirstLine
                (splitBeforeTags codeLineRegex
                    (fun _ -> ignoreFirstLine markdown)
                )

        match Nonempty.span Line.isBlank lines with
            | Some (blankLines, None) ->
                Nonempty.singleton (ignore blankLines)
            | Some (blankLines, Some remaining) ->
                Nonempty.cons (ignore blankLines)
                    (trimmedExampleSection settings remaining)
            | None ->
                trimmedExampleSection settings lines

    splitBeforeTags tagRegex
        (fun m ->
            if m.Groups.Item(1).Value = "EXAMPLE" then
                ignoreFirstLine exampleSection
            else
                ignoreFirstLine
                    (fun settings ->
                        Comments.extractWrappable "" false (fun _ -> "  ") settings
                            >> Block.splitUp (markdown settings)
                    )
        )


/// DDoc for D. Stub until it's implemented. https://dlang.org/spec/ddoc.html
let ddoc =
    markdown


/// Godoc comments use a godoc compatible parser instead of markdown.
/// Here, any lines that are indented more than the "standard" aren't wrapped.
///
/// https://blog.golang.org/godoc-documenting-go-code
let godoc settings =
    let indentedLines =
        ignoreParser
            (Nonempty.span (fun line -> line.[0] = ' ' || line.[0] = '\t'))
    let textLines =
        splitIntoChunks (afterRegex (Regex("  $")))
            >> Nonempty.map (Wrap << Wrappable.fromLines ("", ""))

    textLines
        |> takeUntil (tryMany [blankLines; indentedLines])
        |> repeatToEnd

let xmldoc =
    let blank _ = Nonempty.singleton << ignore
    let blockTags =
        [| "code"; "description"; "example"; "exception"; "include"
           "inheritdoc"; "list"; "listheader"; "item"; "para"; "param"
           "permission"; "remarks"; "seealso"; "summary"; "term"; "typeparam"
           "typeparamref"; "returns"; "value"
        |]
    sgml blank blank blockTags
