module internal rec Block

open Prelude


///////////////////////////////////////////////////////////////////////////////
// TYPES
///////////////////////////////////////////////////////////////////////////////

type Blocks = Nonempty<Block>

type Block =
    | Comment of Blocks
    | Wrap of Wrappable
    | NoWrap of Lines

type Wrappable =
    Prefixes * Lines

module Wrappable =
    let mapPrefixes = Tuple.mapFirst
    let mapLines = Tuple.mapSecond
    let fromLines prefixes lines =
        (prefixes, lines)
    let toLines ((pHead: string, pTail: string), lines) =
        lines |> Nonempty.mapHead ((+) pHead) |> Nonempty.mapTail ((+) pTail)

/// A tuple of two strings. The first represents the prefix used for the first
/// line of a block of lines; the second the prefix for the rest. Some blocks,
/// eg a list item or a block comment, will have a different prefix for the
/// first line than for the rest. Others have the same for both.
type Prefixes =
    string * string

type Lines =
    Nonempty<string>


///////////////////////////////////////////////////////////////////////////////
// CONSTRUCTORS
///////////////////////////////////////////////////////////////////////////////

let comment parser wrappable : Block =
    Comment (oldSplitUp parser wrappable)
    
let text wrappable: Block =
    Wrap wrappable

let ignore lines: Block =
    NoWrap lines


///////////////////////////////////////////////////////////////////////////////
// GETTING INFO FROM BLOCKS
///////////////////////////////////////////////////////////////////////////////

/// Gets the length of a block
let length block =
    match block with
        | Comment subBlocks ->
            Nonempty.toList subBlocks |> List.sumBy length

        | Wrap (_, lines) ->
            Nonempty.length lines

        | NoWrap lines ->
            Nonempty.length lines


///////////////////////////////////////////////////////////////////////////////
// MODIFYING BLOCKS
///////////////////////////////////////////////////////////////////////////////

/// Splits a Lines up into Blocks with the given parser, then prepends the given
/// prefixes to those child blocks
let splitUp : (Lines -> Blocks) -> (Nonempty<string> * Lines) -> Blocks =
    let concatPrefixes (h1, t1) (h2, t2) = h1 + h2, t1 + t2

    let prependPrefixTrimEndOfBlankLine (p: string) (s: string) : string =
        if Line.isBlank s then p.TrimEnd() else p + s

    let takePrefixes : Nonempty<string> -> Block -> (string * string * Nonempty<string>) =
        fun (Nonempty(pre1, preRest) as prefixes) block ->
        // Drops up to n elements from a nonempt list, always leaving 1 remaining
        let rec dropUpTo n (Nonempty(_, t) as neList) =
            match Nonempty.fromList t with
            | Some next when n > 0 -> dropUpTo (n - 1) next | _ -> neList
        pre1, fromMaybe pre1 (List.tryHead preRest), dropUpTo (length block) prefixes

    let prependPrefixes (prefixes, Nonempty(block, nextBlocks)) =
        let pre1, pre2, preNext = takePrefixes prefixes block
        let block' =
            match block with
            | Comment subBlocks -> // A comment in a comment (probably) won't happen :)
                block
            | Wrap wrappable ->
                Wrap (Wrappable.mapPrefixes (concatPrefixes (pre1, pre2)) wrappable)
            | NoWrap ls ->
                ls
                    |> Nonempty.mapHead (prependPrefixTrimEndOfBlankLine pre1)
                    |> Nonempty.mapTail (prependPrefixTrimEndOfBlankLine pre2)
                    |> NoWrap
        block', tuple preNext <<|> Nonempty.fromList nextBlocks

    fun parser (prefixes, lines) ->
    Nonempty.unfold prependPrefixes (prefixes, parser lines)

let oldSplitUp : (Lines -> Blocks) -> Wrappable -> Blocks =
    fun parser ((pre1, pre2), lines) -> splitUp parser (Nonempty(pre1, [pre2]), lines)
