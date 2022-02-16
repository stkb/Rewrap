module internal rec Parsers

open Prelude
open Line
open Parsing_
open Parsing_Internal


/// DartDoc has just a few special tags. We keep lines beginning with these unwrapped.
let dartdoc : ContentParser -> ContentParser =
  fun content ctx ->

  let tryMatchTag : Line -> Option<FirstLineRes> =
    tryMatch' (regex @"^\s*(@nodoc|{@template|{@endtemplate|{@macro)") <>>> fun (_, line) ->
      finished_ line noWrapBlock

  let rec wrapFLR : FirstLineRes -> FirstLineRes = function
  | Pending r -> Pending ^| wrapResultParser nlpWrapper r
  | Finished r -> Finished ^| wrapResultParser (fun p -> Some (flpWrapper p)) r

  and flpWrapper maybeInnerParser : FirstLineParser =
    (tryMatchTag |? (maybeInnerParser |? content ctx)) >> wrapFLR

  and nlpWrapper innerParser : NextLineParser =
    fun line ->
      tryMatchTag line
        |> map (FinishedOnPrev << Some)
        |> Option.defaultWith ^| fun _ ->
          match innerParser line with
          | ThisLine r -> ThisLine (wrapFLR r)
          | FinishedOnPrev maybeR ->
              FinishedOnPrev (wrapFLR <<|> (maybeR <|> (fun _ -> Some (content ctx line))))

  content ctx >> wrapFLR

let dartdoc_markdown ctx = dartdoc markdown_noHeader ctx

let ignoreAll ctx = Parsing_Internal.ignoreAll ctx

let godoc : ContentParser = fun _ctx ->
  let rec parseNext (line: Line) =
    if isBlankLine line || line.content[0] = ' ' || line.content[0] = '\t' then
      FinishedOnPrev ^| Some ^| finished_ line noWrapBlock
    else ThisLine ^| pending line wrapBlock parseNext
  fun line ->
    if isBlankLine line || line.content[0] = ' ' || line.content[0] = '\t' then
      finished_ line noWrapBlock
    else pending line wrapBlock parseNext

let private jsdoc : ContentParser -> ContentParser =
  fun content ctx ->

  /// "Freezes" inline tags ({@tag }) so that they don't get broken up
  let freezeInlineTags : Line -> Line =
    let rx = regex @"{@[a-z]+.*?[^\\]}"
    Line.mapContent (fun s -> rx.Replace(s, fun m -> m.Value.Replace(' ', '\000')))

  let tryMatchTag : Line -> Option<FirstLineRes> =
    tryMatch' (regex @"^\s*@(\w+)(.*)$") <>>> fun (m, line) ->
      if isBlank m[2] then
        if m[1].ToLower() = "example" then ignoreAll ctx line else finished_ line noWrapBlock
      else content ctx line

  let rec wrapFLR : FirstLineRes -> FirstLineRes = function
  | Pending r -> Pending ^| wrapResultParser nlpWrapper r
  | Finished r -> Finished ^| wrapResultParser (fun p -> Some (flpWrapper p)) r

  and flpWrapper maybeInnerParser : FirstLineParser =
    freezeInlineTags >> (tryMatchTag |? (maybeInnerParser |? content ctx)) >> wrapFLR

  and nlpWrapper innerParser : NextLineParser =
    freezeInlineTags >> fun line ->
      tryMatchTag line
        |> map (FinishedOnPrev << Some)
        |> Option.defaultWith ^| fun _ ->
          match innerParser line with
          | ThisLine r -> ThisLine (wrapFLR r)
          | FinishedOnPrev maybeR ->
              FinishedOnPrev (wrapFLR <<|> (maybeR <|> (fun _ -> Some (content ctx line))))

  content ctx >> wrapFLR

let jsdoc_markdown ctx = jsdoc markdown_noHeader ctx

let markdown ctx = Parsers_Markdown.markdown ctx

let markdown_noHeader ctx = Parsers_Markdown.markdown_noHeader ctx

let plainText : ContentParser = fun _ctx ->
  let rec parseNext line =
    let line = trimWhitespace line
    if isBlankLine line then FinishedOnPrev ^| Some ^| finished_ line noWrapBlock
    else ThisLine ^| pending line wrapBlock parseNext
  trimWhitespace >> fun line ->
    if isBlankLine line then finished_ line noWrapBlock else pending line wrapBlock parseNext

let plainText_indentSeparated : ContentParser = fun _ctx ->
  let rec parseNext prevIndent line =
    let indent, line = trimIndent _ctx line
    if isBlankLine line then FinishedOnPrev ^| Some ^| finished_ line noWrapBlock else
    let res = pending line wrapBlock (parseNext indent)
    if indent <> prevIndent then FinishedOnPrev ^| Some res else ThisLine res
  trimIndent _ctx >> fun (indent, line) ->
    if isBlankLine line then finished_ line noWrapBlock else pending line wrapBlock (parseNext indent)

let rst ctx = Parsers_RST.rst ctx
