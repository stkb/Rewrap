module internal Parsers

open Prelude
open Parsing_
open Parsing_Internal

let ignoreAll : ContentParser =
  let rec parseLine line = pending line noWrapBlock (ThisLine << parseLine)
  fun _ctx -> parseLine

let markdown ctx = Parsers_Markdown.markdown ctx

let rst ctx = Parsers_RST.rst ctx
