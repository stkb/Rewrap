module Block

open Prelude
open Parsing_


module Wrappable =
  let inline mapPrefixes f x = Tuple.mapFirst f x
  let inline mapLines f x = Tuple.mapSecond f x
  let inline fromLines prefixes lines = (prefixes, lines)
  let toLines ((pHead: string, pTail: string), lines) =
    lines |> Nonempty.mapHead ((+) pHead) |> Nonempty.mapTail ((+) pTail)

/// Takes a function (string -> string), parser (Lines -> Blocks) and Prefixes &
/// Lines tuple. Uses the parser on the lines to make blocks. For each of these
/// blocks, prepends the prefix from Prefixes for the corresponding line. Where
/// wrapping produces new lines in a block, uses the function to transform the
/// prefix for the first line of that block to something for new lines.
let splitUp : (string -> string) -> (Nonempty<string> -> Blocks) -> (Nonempty<string> * Nonempty<string>) -> Blocks =
  let concatPrefixes (h1, t1) (h2, t2) = h1 + h2, t1 + t2

  let prependPrefixTrimEndOfBlankLine (p: string) (s: string) : string =
    if Line.isBlank s then p.TrimEnd() else p + s

  fun makeDefPrefix ->
  /// Takes the remaining list of prefixes and the block that needs prefixes
  /// prepending. Removes prefixes from the list equal to the current size of
  /// the block. Returns that list, plus the prefixes to prepend to the head &
  /// tail lines of the block.
  let takePrefixes : Nonempty<string> -> Block -> (string * string * Nonempty<string>) =
    fun prefixes block ->
    let (Nonempty(p1, pBlockRest)), maybePRest = Nonempty.splitAt (size block) prefixes
    let pRest = maybePRest |? (singleton (List.tryLast pBlockRest |? p1))
    p1, List.tryHead pBlockRest |? makeDefPrefix p1, pRest

  let prependPrefixes (prefixes, Nonempty(block, nextBlocks)) =
    let pre1, pre2, preNext = takePrefixes prefixes block
    let block' =
      match block with
      | Block.Comment _ -> // A comment in a comment (probably) won't happen :)
          block
      | Block.Wrap wrappable ->
          Block.Wrap (Wrappable.mapPrefixes (concatPrefixes (pre1, pre2)) wrappable)
      | Block.NoWrap ls ->
          ls
            |> Nonempty.mapHead (prependPrefixTrimEndOfBlankLine pre1)
            |> Nonempty.mapTail (prependPrefixTrimEndOfBlankLine pre2)
            |> Block.NoWrap
      | NBlock _ -> raise (System.Exception("splitUp on new block"))
    block', tuple preNext <<|> Nonempty.fromList nextBlocks

  fun parser (prefixes, lines) ->
  Nonempty.unfold prependPrefixes (prefixes, parser lines)

/// Old version, for compatibility with old code
let oldSplitUp : (Nonempty<string> -> Blocks) -> Wrappable -> Blocks =
  fun parser ((pre1, pre2), lines) ->
  splitUp (always pre2) parser (Nonempty(pre1, [pre2]), lines)


// Constructors

let commentBlock : (Nonempty<string> -> Blocks) -> Wrappable -> Block =
  fun parser wrappable -> Comment (oldSplitUp parser wrappable)
let textBlock : Wrappable -> Block = Block.Wrap
let ignoreBlock : Nonempty<string> -> Block = Block.NoWrap
