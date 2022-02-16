module internal Parsing.DocComments
// This module deals with special formatting inside comments that are used for
// documentation, eg javadoc, xmldoc, RDoc

open Prelude
open Rewrap
open Block
open Parsers
open Parsing_
open Parsing.Core
open Sgml
open System.Text.RegularExpressions

type private Lines = Nonempty<string>

let private markdown = Parsing.Markdown.markdown

/// Splits lines into sections which start with lines matching the given regex.
/// For each of those sections, the matchParser is applied to turn the lines
/// into blocks. If the first section doesn't start with a matching line, it is
/// processed with the noMatchParser.
let private splitBeforeTags
  : Regex -> (Match -> Settings -> string TotalParser) -> (Settings -> string TotalParser)
  -> Settings -> Lines -> Blocks =
  fun regex matchParser noMatchParser settings (Nonempty(outerHead, outerTail)) ->

  let rec prependRev (Nonempty(head, tail)) maybeRest =
    let nextRest = maybeRest |> maybe (singleton head) (Nonempty.cons head)
    Nonempty.fromList tail |> maybe nextRest (fun next -> prependRev next (Some nextRest))

  let rec loop (tagMatch: Match) buffer maybeOutput lines =
    let parser = if tagMatch.Success then matchParser tagMatch else noMatchParser
    let addBufferToOutput () = prependRev (parser settings (Nonempty.rev buffer)) maybeOutput
    match lines with
      | [] -> (addBufferToOutput ()) |> Nonempty.rev
      | headLine :: tailLines ->
          let m = regex.Match(headLine)
          let nextTagMatch, nextBuffer, nextOutput =
            if m.Success then m, singleton headLine, Some (addBufferToOutput ())
            else tagMatch, Nonempty.cons headLine buffer, maybeOutput
          loop nextTagMatch nextBuffer nextOutput tailLines

  loop (regex.Match(outerHead)) (Nonempty.singleton outerHead) None outerTail

let javadoc =
  let tagRegex = Regex(@"^\s*@(\w+)(.*)$")

  /// "Freezes" inline tags ({@tag }) so that they don't get broken up
  let inlineTagRegex = Regex(@"{@[a-z]+.*?[^\\]}", RegexOptions.IgnoreCase)
  let markdownWithInlineTags settings =
    let replaceSpace (m: Match) = m.Value.Replace(' ', '\000')
    map (fun s -> inlineTagRegex.Replace(s, replaceSpace)) >> markdown settings

  let matchParser (m: Match) =
    if Line.isBlank (m.Groups.Item(2).Value) then
      if m.Groups.Item(1).Value.ToLower() = "example" then
        (fun _ -> ignoreBlock >> singleton)
      else ignoreFirstLine markdownWithInlineTags
    else markdownWithInlineTags

  splitBeforeTags tagRegex matchParser markdownWithInlineTags


let psdoc =
  let tagRegex = Regex(@"^\s*\.([A-Z]+)")
  let codeLineRegex = Regex(@"^\s*PS C:\\>")

  let exampleSection settings lines =
    let trimmedExampleSection =
      ignoreFirstLine (splitBeforeTags codeLineRegex (fun _ -> ignoreFirstLine markdown) markdown)
    match Nonempty.span Line.isBlank lines with
      | Some (blankLines, None) -> Nonempty.singleton (ignoreBlock blankLines)
      | Some (blankLines, Some remaining) ->
          Nonempty.cons (ignoreBlock blankLines) (trimmedExampleSection settings remaining)
      | None -> trimmedExampleSection settings lines

  splitBeforeTags tagRegex
    (fun m ->
      if m.Groups.Item(1).Value = "EXAMPLE" then ignoreFirstLine exampleSection else
      ignoreFirstLine
        (fun settings ->
            Comments.extractWrappable "" false (fun _ -> "  ") settings
                >> Block.oldSplitUp (markdown settings)
        )
    )
    markdown


/// DDoc for D. Stub until it's implemented. https://dlang.org/spec/ddoc.html
let ddoc =
    markdown



let xmldoc =
    let blank = docOf ignoreAll
    let blockTags =
        [| "code"; "description"; "example"; "exception"; "include"
           "inheritdoc"; "list"; "listheader"; "item"; "para"; "param"
           "permission"; "remarks"; "seealso"; "summary"; "term"; "typeparam"
           "typeparamref"; "returns"; "value"
        |]
    sgml blank blank blockTags
