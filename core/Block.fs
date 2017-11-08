module internal rec Block

open Nonempty
open Extensions


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

let comment parser wrappable: Block =
    Comment (Block.splitUp parser wrappable)

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

let splitUp (parser: Lines -> Blocks) ((pHead, pTail), lines) =

    let concatPrefixes (head1, tail1) (head2, tail2) =
        (head1 + head2, tail1 + tail2)

    let prependPrefixes p block =
        match block with
            | Comment subBlocks ->
                // A comment in a comment (probably) won't happen :)
                block
            | Wrap wrappable ->
                Wrap (Wrappable.mapPrefixes (concatPrefixes p) wrappable)

            | NoWrap ls ->
                ls 
                    |> Nonempty.mapHead ((+) (fst p))
                    |> Nonempty.mapTail ((+) (snd p))
                    |> NoWrap
    
    parser lines
        |> Nonempty.mapHead (prependPrefixes (pHead, pTail))
        |> Nonempty.mapTail (prependPrefixes (pTail, pTail))
