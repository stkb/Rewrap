module internal Parsing.Core

open System.Text.RegularExpressions
open Prelude
open Block


/// A parser that when given lines, may consume some of them. If it does, it
/// returns blocks created from the consumed lines, and lines remaining.
type OptionParser =
    Lines -> Option<Blocks * Option<Lines>>

/// A parser that consumes at least one of the lines given
type PartialParser =
    Lines -> Blocks * Option<Lines>

/// A parser that consumes all lines given and returns the block created
type TotalParser =
    Lines -> Blocks

type SplitFunction =
    Lines -> Lines * Option<Lines>

type OptionSplitFunction =
    Lines -> Option<Lines * Option<Lines>>


//-----------------------------------------------------------------------------
// CREATING PARSERS
//-----------------------------------------------------------------------------


/// Creates an OptionParser, taking a split function and a function to parse the
/// lines into blocks
let optionParser (splitter: OptionSplitFunction) (parser: TotalParser): OptionParser =
    splitter >> Option.map (Tuple.mapFirst parser)


/// Creates an OptionParser that will ignore the matched lines
let ignoreParser (splitter: OptionSplitFunction): OptionParser =
    splitter >> Option.map (Tuple.mapFirst (Block.ignore >> Nonempty.singleton))


//-----------------------------------------------------------------------------
// COMBINING PARSERS
//-----------------------------------------------------------------------------


let rec tryMany (parsers: list<OptionParser>) (lines: Lines): Option<Blocks * Option<Lines>> =
    match parsers with
        | [] ->
            None
        | p :: ps ->
            match p lines with
                | None ->
                    tryMany ps lines
                | result ->
                    result


/// Searches lines until an OptionParser matches. Parses those lines with the
/// given TotalParser. Returns blocks from both parsers.
let takeUntil
    (otherParser: OptionParser)
    (totalParser: TotalParser)
    : PartialParser =

    let rec loop buffer (Nonempty(headLine, tailLines) as lines) =
        match otherParser lines with
            | Some (blocks, remainingLines) ->
                match Nonempty.fromList (List.rev buffer) with
                    | Some bufferLines ->
                        (Nonempty.append (totalParser bufferLines) blocks, remainingLines)
                    | None ->
                        (blocks, remainingLines)
            | None ->
                match Nonempty.fromList tailLines with
                    | Some neLines ->
                        loop (headLine :: buffer) neLines
                    | None ->
                        ( Nonempty(headLine, buffer) |> Nonempty.rev |> totalParser
                        , None
                        )
    loop []


// Repeats a PartialParser until all lines are consumed
let repeatToEnd partialParser : TotalParser =
    let rec loop blocks lines =
        match partialParser lines with
            | (newBlocks, Some remainingLines) ->
                loop (blocks @ Nonempty.toList newBlocks) remainingLines
            | (newBlocks, None) ->
                Nonempty.appendToList blocks newBlocks
    loop []


//-----------------------------------------------------------------------------
// WORKING WITH LINES AND BLOCKS
//-----------------------------------------------------------------------------


/// Takes a split function, and splits Lines into chunks of Lines
let splitIntoChunks (splitFn: SplitFunction) : (Lines -> Nonempty<Lines>) =
    Nonempty.unfold splitFn

let splitBefore (predicate: string -> bool) (Nonempty(head, tail) as lines) =
    match Nonempty.span (not << predicate) lines with
        | Some res -> res
        | None ->
            List.span (not << predicate) tail
                |> Tuple.mapFirst (fun t -> Nonempty(head, t))
                |> Tuple.mapSecond Nonempty.fromList

/// Creates a SplitFunction that splits before a line matches the given regex
let beforeRegex (regex: Regex) : SplitFunction =
    splitBefore (Line.contains regex)

/// Creates a SplitFunction that splits after a line matches the given regex
let afterRegex regex : SplitFunction =
    Nonempty.splitAfter (Line.contains regex)


/// Creates a SplitFunction that splits on indent differences > 2
let onIndent tabWidth (Nonempty(firstLine, otherLines)): Lines * Option<Lines> =

    let indentSize =
        Line.leadingWhitespace >> Line.tabsToSpaces tabWidth >> String.length

    let firstLineIndentSize =
        indentSize firstLine

    otherLines
        |> List.span
            (fun line -> abs (indentSize line - firstLineIndentSize) < 2)
        |> Tuple.mapFirst (fun tail -> Nonempty(firstLine, tail))
        |> Tuple.mapSecond Nonempty.fromList


/// Convert paragraph lines into a Block. The indent of the first line may be
/// different from the rest. If reformat is True, indents are removed from all
/// lines.
let firstLineIndentParagraphBlock reformat (Nonempty(headLine, tailLines) as lines) =
    let prefixes =
        if reformat then
            ("", "")
        else
            ( Line.leadingWhitespace headLine
            , List.tryHead tailLines
                |> Option.defaultValue headLine
                |> Line.leadingWhitespace
            )

    Block.text (prefixes, lines |> map String.trimStart)


/// Ignores the first line and parses the rest with the given parser
let ignoreFirstLine otherParser settings (Nonempty(headLine, tailLines)) : Blocks =
    let headBlock =
        Block.ignore (Nonempty.singleton headLine)

    Nonempty.fromList tailLines
        |> Option.map (Nonempty.cons headBlock << otherParser settings)
        |> Option.defaultValue (Nonempty.singleton headBlock)


/// Convert paragraph lines into a Block, in a document where paragraphs can be
/// separated by difference in indent. There is only one indent for the whole
/// paragraph, determined from the first line.
let indentSeparatedParagraphBlock
    (textType: Wrappable -> Block) (lines: Lines) : Block =

    let prefix =
        Line.leadingWhitespace (Nonempty.head lines)

    textType ((prefix, prefix), lines |> map String.trimStart)


/// Creates an OptionSplitFunction that will take all lines between a start and
/// end marker (inclusive). If the start marker is found but end marker isn't,
/// then all lines (including and) after the start marker are returned.
let takeLinesBetweenMarkers
    (startRegex: Regex, endRegex: Regex)
    (Nonempty(headLine, _) as lines)
    : Option<Lines * Option<Lines>> =

    let takeUntilEndMarker (prefix: string) =
        lines
            |> Nonempty.mapHead (String.dropStart prefix.Length)
            |> afterRegex endRegex
            |> Tuple.mapFirst (Nonempty.replaceHead headLine)

    headLine
        |> Line.tryMatch startRegex
        |> Option.map takeUntilEndMarker


//-----------------------------------------------------------------------------
// COMMON PARSERS
//-----------------------------------------------------------------------------


/// Ignores blank lines
let blankLines: OptionParser =
    ignoreParser (Nonempty.span Line.isBlank)
