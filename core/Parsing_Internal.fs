/// Helper functions and types for the parsers
module internal Parsing_Internal

open System
open System.Text.RegularExpressions
open Prelude
open Line
open Parsing_


//---------- Operations On Lines -------------------------------------------------------//


/// Creates a regex ignores case & is ecmascript compatible
let regex pat = Regex(pat, RegexOptions.IgnoreCase ||| RegexOptions.ECMAScript)


let tryMatch' : Regex -> Line -> Option<string[] * Line> =
  fun rx line ->
  let m = Line.onContent(fun s -> rx.Match(s)) line
  if not m.Success then None else
  Some (Array.ofSeq <| seq { for g in m.Groups -> g.Value }, line)

/// Try matching the given regex on the line's content. If no match returns
/// None. If there is a match, returns a string array where [0] is the whole
/// match, [1] the first group etc.
let tryMatch : Regex -> Line -> string array option =
  fun rx -> tryMatch' rx >> map fst

/// Like tryMatch, but just returns the match's success
let isMatch : Regex -> Line -> bool = fun rx -> tryMatch' rx >> Option.isSome


/// If the line (after the prefix) is just whitespace
let isBlankLine : Line -> bool = Line.onContent String.IsNullOrWhiteSpace

// Gets the number of initial blank spaces on the line, from the current prefix
// position
let indentLength : Line -> int = Line.onContent (fun s -> s.Length - s.TrimStart().Length)

// Adjusts the line splitpoint so that all whitespace is trimmed from the content
let trimWhitespace : Line -> Line =
  Line.trimUpTo Int32.MaxValue


//---------- Line Results --------------------------------------------------------------//


// Constructs a pending result
let pending line blockType nextParser =
  Pending (LineRes(line, blockType, false, nextParser))

/// Constructs a Finished result with a next parser
let finished line blockType nextParser =
  Finished (LineRes(line, blockType, false, Some nextParser))

/// Constructs a Finished result without a next parser
let finished_ line blockType =
  Finished (LineRes(line, blockType, false, None))


/// Takes a line result and "wraps" its parser in a function that takes the inner
/// parser and returns a new one. This is for when we want to either modify the
/// line before it's passed to the inner parser, or modify the result coming out
/// of it.
let wrapResultParser : ('a -> 'b) -> LineRes<'a> -> LineRes<'b> =
  fun p r -> LineRes<'b>(r.line, r.blockType, r.isDefault, p r.nextParser)


/// Creates a PrefixTransformer that starts at the start point, removes the given number
/// of chars, and inserts the given number of spaces + the extra string
let blankOut' start remove spaces extra : PrefixTransformer =
  let rep = String (' ', spaces)
  fun pre -> pre.Substring(0, start) + rep + extra + pre.Substring(start + remove)

/// Creates a PrefixTransformer from a given Line and length, that blanks out `length`
/// characters, starting at the line's split position.
let blankOut (line: Line) (length: int) : PrefixTransformer =
  blankOut' line.split length length ""


/// Adds a default prefix function to a FirstLineRes
let wrapPrefixFn prefixFn =
  let inline f (r: LineRes<'p>) =
    let blockType =
      match r.blockType with
      | :? WrapBlock as b -> wrapBlock' (prefixFn << b.prefixFn)
      | _ -> r.blockType
    LineRes(r.line, blockType, r.isDefault, r.nextParser)
  function | Pending r -> Pending (f r) | Finished r -> Finished (f r)


let voidParseLine = fun _ -> ()


//---------- Common Parsers ------------------------------------------------------------//


/// Function that takes a Context and Line and tries a new parser on it
type TryNewParser = Context -> Line -> Option<FirstLineRes>


/// Line content is empty or whitespace. nonText would cover this as well but this
/// separate case is added for performance
let blankLine : TryNewParser =
  fun _ctx line -> if isBlankLine line then Some (finished_ line noWrapBlock) else None


/// A line that doesn't have any text on it. Always stops after 1 line since there may be
/// other blocks that begin with non-text lines (eg "<!--"). Also for that reason, this
/// parser should only be tried after all others.
let nonText : TryNewParser = fun _ctx ->
  let rx = Regex "[A-Za-z0-9\u00C0-\uFFFF]"
  fun line -> if isMatch rx line then None else Some (finished_ line noWrapBlock)


//---------- Other ---------------------------------------------------------------------//


/// Same as Parsing.Core splitIntoChunks (afterRegex regex) but with our type of
/// lines.
let splitAfter : Regex -> Nonempty<Line> -> Nonempty<Nonempty<Line>> =
  Nonempty.unfold << Nonempty.splitAfter << isMatch
