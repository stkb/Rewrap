/// Can remove underscore from name after we remove all the old Parsing.* modules
module rec Parsing_

open Prelude
open Rewrap
open Line
open Wrapping

// A parser is at the most basic level a function that takes a Line object and
// may return a result, *for that line*. The result contains information such
// as: whether the line starts or ends a new "block", and what type of block it is
// (wrappable or not wrappable); as well a parser function to be used to parse
// the next line.
//
// For consistency there are two types of line result: those that come only from the
// first line of a new block (`FirstLineRes`) and those that come from
// subsequent lines (`NextLineRes`). The reason is that a `NextLineRes` can
// state that the block already finished on the previous line, but this would be
// invalid for first line results (we already know that the block finished on
// the previous line).
//
// There's no concept of a parser of a parser "error" here, only that a parser
// may or may not match. There are conceptually three types of parser:
// 1. Those that only return a result if they match
// 2. Those that are guaranteed to match (and return a result), and:
//    a) Can at some point terminate (return a result with no continuation parser)
//    b) Don't terminate (will consume all lines given)
//
// However, for simplicity in the type system, there's currently no typed
// distinction between 2a and 2b; they both fall under the type `DocParser`;
// named so because they usually parse whole documents. The type for type 1 is
// `TryNewParser`.
//
// Because there's currently no guarantee that a `DocParser` won't terminate,
// the calling code has to account for this (which is usually just calling the
// same parser again on the next line)

/// Stores information while parsing
type Context(settings: Settings) =
  let mutable blocks: Block list = []
  let mutable lines: Line list = []
  member val output = OutputBuffer (settings)
  member _.settings = settings
  member _.addBlock : Block -> Unit = fun block -> blocks <- block :: blocks
  //member _.addWrap : (string -> string) Option -> Nonempty<Line> -> Unit =
  //  fun prefixFn lines -> blocks <- NBlock (wrapBlock' prefixFn, lines) :: blocks
  //member _.addNoWrap : Nonempty<Line> -> Unit =
  //  fun lines -> blocks <- NBlock (noWrapBlock, lines) :: blocks
  member _.getBlocks () = Nonempty.fromSeqUnsafe (List.rev blocks)


/// The BlockType is so that we know what to do w.r.t. selections. Comments can be wholly
/// wrapped if there's an empty selection in them, and take precedence over NoWrap. NoWrap
/// only gets wrapped if there are no comments in the selection.
type BlockType = Comment | Wrap | NoWrap | Embedded

[<AbstractClass>]
type NewBlock (bType) =
  abstract member output : Context -> Nonempty<Line> -> unit
  member _.bType : BlockType = bType
  member x.isComment = x.bType = BlockType.Comment
  member x.isContainer = x.isComment || x.bType = BlockType.Embedded


type Block =
  | Comment of Blocks | Wrap of Wrappable | NoWrap of Nonempty<string>
  //| NewComment of ((Context -> Nonempty<Line> -> unit) * Nonempty<Line>)
  //| NewWrap of (Option<string -> string> * Nonempty<Line>) | NewNoWrap of Nonempty<Line>
  | NBlock of (NewBlock * Nonempty<Line>)
  with
  static member size (HasSize, b: Block) =
    match b with
      | Comment subBlocks -> subBlocks |> Seq.sumBy (fun b -> Block.size(HasSize, b))
      | NoWrap lines -> Nonempty.size (HasSize, lines)
      | Wrap (_, lines) -> Nonempty.size (HasSize, lines)
      //| NewComment (_, lines) -> size lines
      //| NewWrap (_, lines) -> Nonempty.size (HasSize, lines)
      //| NewNoWrap lines -> Nonempty.size (HasSize, lines)
      | NBlock (_, lines) -> size lines

type Blocks = Nonempty<Block>


/// A function that takes a (string) prefix and returns a prefix. It's used when a block
/// to be wrapped only has one line, but the 2nd line must have a different prefix from
/// the first; eg a list item bullet point.
type PrefixTransformer = string -> string

/// For a block that will be wrapped if selected
type WrapBlock (prefixFn_) =
  inherit NewBlock (BlockType.Wrap)
  member _.prefixFn : PrefixTransformer = prefixFn_
  override x.output ctx lines : unit = ctx.output.wrap (Some x.prefixFn, lines)


/// Creates a WrapBlock with no prefix function
let wrapBlock = WrapBlock (id) :> NewBlock

/// Creates a NewWrap block with the given prefix function
let wrapBlock' (prefixFn: PrefixTransformer) = WrapBlock (prefixFn) :> NewBlock

/// For a block that normally won't be wrapped
type NoWrapBlock () =
  inherit NewBlock (BlockType.NoWrap)
  override x.output ctx lines : unit = ctx.output.noWrap lines

/// Creates a NoNewWrap block
let noWrapBlock = NoWrapBlock () :> NewBlock



/// Result from parsing a line. Takes as a parameter a parser to parse the next line
type LineRes<'p>(line: Line, blockType: NewBlock, isDefault: bool, nextParser: 'p) =
  member _.line = line
  member _.blockType = blockType
  // Whether the block is the "default" type; ie a normal paragraph. This is
  // ugly but needed until we come up with a better way.
  member _.isDefault = isDefault
  member _.nextParser = nextParser


/// Result from parsing a line when the block hasn't finished. The parameter is
/// a parser for the next line
type PendingRes = LineRes<NextLineParser>

/// Parses lines of a block after the first
and NextLineParser = Line -> NextLineRes

/// Result from parsing a line where the block has finished. Optionally provides
/// a parser for the next line.
and FinishedRes = LineRes<Option<FirstLineParser>>

/// Parses the first line of a new block
and FirstLineParser = Line -> FirstLineRes

/// Result from the first line of a block
and FirstLineRes = Pending of PendingRes | Finished of FinishedRes

/// Result from subsequent lines of a block
and NextLineRes = ThisLine of FirstLineRes | FinishedOnPrev of Option<FirstLineRes>


/// Function that takes a Context and Line and is certain to produce a result
/// from it.
type ContentParser = Context -> Line -> FirstLineRes


/// Something that can process a whole document, given a context and sequence of lines
type DocumentProcessor = Context -> seq<Line> -> unit


/// State for processContent
type private PLState =
  | NewParser of Option<Line -> FirstLineRes>
  | InParser of List<Line> * PendingRes

/// For the shim to the old code
type private SpecialLineRes(line: Line, nextParser: Line -> NextLineRes, fn: unit -> unit) =
  inherit LineRes<Line -> NextLineRes>(line, noWrapBlock, false, nextParser)
  member _.fn = fn

/// Called from outside this module. Takes a DocParser and feeds a sequence of
/// lines into it, collecting the results
let processContent : ContentParser -> Context -> seq<Line> -> unit =
  fun docParser ctx ->
  let initParser = docParser ctx
  let inline addBlock lines (res: LineRes<'p>) =
    ctx.addBlock (NBlock (res.blockType, (Nonempty.rev (res.line .@ lines))))
  let doFirstLineRes lines : FirstLineRes -> PLState = function
    | Pending r -> InParser (lines, r)
    | Finished r -> addBlock lines r; NewParser r.nextParser
  let step state line : PLState =
    match state with
    | NewParser p -> doFirstLineRes [] ((p |? initParser) line)
    | InParser (ls, rPrev) ->
        match rPrev.nextParser line with
        | ThisLine x -> doFirstLineRes (rPrev.line :: ls) x
        | FinishedOnPrev (Some x) -> addBlock ls rPrev; doFirstLineRes [] x
        | FinishedOnPrev None -> addBlock ls rPrev; doFirstLineRes [] (initParser line)

  fun lines ->
  let init = doFirstLineRes [] (initParser (Seq.head lines))
  match Seq.fold step init (Seq.tail lines) with
  | NewParser _ -> ()
  | InParser (ls, rPrev) ->
      match rPrev with
      | :? SpecialLineRes as rPrevS -> rPrevS.fn ()
      | _ -> addBlock ls rPrev


/// Converts an old parser to a new ContentParser
let toNewContent : (Settings -> Nonempty<string> -> Blocks) -> Context -> Line -> FirstLineRes =
  fun oldParser ctx firstLine ->
  let mutable lines = singleton firstLine.content
  let runOldParser () =
    oldParser ctx.settings (Nonempty.rev lines) |> Seq.iter ctx.addBlock
  let rec parseLine (line: Line) : NextLineRes =
    lines <- Nonempty.cons line.content lines
    ThisLine (Pending (SpecialLineRes (line, parseLine, runOldParser)))
  Pending (SpecialLineRes (Line("", ""), parseLine, runOldParser))


/// Converts an old parser to a new DocumentProcessor
let toNewDocProcessor : (Settings -> Nonempty<string> -> Blocks) -> DocumentProcessor =
  fun oldParser ctx seqLines ->
  let lines = Nonempty.fromSeqUnsafe (seqLines |> Seq.map (fun l -> l.content))
  oldParser ctx.settings lines |> Seq.iter ctx.addBlock


/// Converts a new ContentParser to an old parser
let toOldParser : ContentParser -> Settings -> Nonempty<string> -> Blocks =
  fun parser settings lines ->
  let ctx = Context(settings)
  processContent parser ctx (lines |> Seq.map (fun s -> Line("", s)))
  ctx.getBlocks()


/// Creates a DocumentProcessor in place of a ContentParser
let docOf = processContent
