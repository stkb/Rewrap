module internal Parsers

open Prelude
open Line
open Parsing_
open Parsing_Internal

let ignoreAll : ContentParser =
  let rec parseLine line = pending line noWrapBlock (ThisLine << parseLine)
  fun _ctx -> parseLine

let markdown ctx = Parsers_Markdown.markdown ctx

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
