module internal rec Block

open Nonempty


///////////////////////////////////////////////////////////////////////////////
// TYPES
///////////////////////////////////////////////////////////////////////////////

type Blocks = Nonempty<Block>

type Block =
    | Wrap of TextType * Wrappable
    | Ignore of int

type TextType =
    | Comment of (Lines -> Blocks)
    | Text
    | Code

type Wrappable = {
    prefixes : Prefixes
    lines : Lines
}

type Prefixes = {
    head : string
    tail : string
}

type Lines = 
    Nonempty<string>


///////////////////////////////////////////////////////////////////////////////
// CONSTRUCTORS
///////////////////////////////////////////////////////////////////////////////

let prefixes head tail =
    { head = head; tail = tail }

let wrappable prefixes lines =
    { prefixes = prefixes; lines = lines }


let comment parser wrappable: Block =
    Wrap(Comment parser, wrappable)

let text wrappable: Block =
    Wrap(Text, wrappable)

let code wrappable: Block =
    Wrap(Code, wrappable)

let ignore lines: Block =
    Ignore(Nonempty.length lines)

///////////////////////////////////////////////////////////////////////////////
// GETTING INFO FROM BLOCKS
///////////////////////////////////////////////////////////////////////////////


/// Gets the length of a block
let length block =
    match block with
        | Wrap (_, w) ->
            Nonempty.length w.lines

        | Ignore n ->
            n

// Returns whether the block is an ignore block
let isIgnore block =
    match block with
        | Ignore _ ->
            true

        | _ ->
            false


///////////////////////////////////////////////////////////////////////////////
// MODIFYING BLOCKS
///////////////////////////////////////////////////////////////////////////////

let splitUp (mapper: Lines -> Blocks) wrappable =

    let concatPrefixes first second =
        { head = (first.head + second.head); tail = (first.tail + second.tail) }

    let middlePrefixes =
        { head = wrappable.prefixes.tail; tail = wrappable.prefixes.tail }

    let prependPrefixes p block =
        match block with
            | Wrap (t, w) ->
                Wrap (t, { w with prefixes = concatPrefixes p w.prefixes })

            | Ignore _ ->
                block
    
    mapper wrappable.lines
        |> Nonempty.mapHead (prependPrefixes wrappable.prefixes)
        |> Nonempty.mapTail (prependPrefixes middlePrefixes)
