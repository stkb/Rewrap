module Rewrap.Core.Test

open System
open System.Text.RegularExpressions
open Rewrap

/// Rewrap settings that are applied to each test
type TestSettings =
  { language: string; column: int; tabWidth: int
    doubleSentenceSpacing: bool; reformat: bool; wholeComment: bool }

/// Default settings that are applied unless modifications are specified in the
/// spec file.
let defaultSettings: TestSettings =
  { language = "plaintext"; column = 0; tabWidth = 4
    doubleSentenceSpacing = false; reformat = false; wholeComment = true }

type Lines = string array

/// Data for a test that will be run
type Test =
  { fileName: string; settings: Settings; language: string; only: bool
    input: Lines; expected: Lines; selections: Selection array }

type TestErrorType = NoOutput | InvalidWrappingColumn | InvalidSelection
/// For when a test specification has an error. File name, input lines and error
/// type.
type TestError = string * Lines * TestErrorType

type SelPosType = Anchor | Active

/// Functions that are native to the test platform (.Net or JS)
module Native =
#if FABLE_COMPILER
  open Fable.Core.JsInterop
  let files : Lines = importMember "./Native.js"
  let readFile : string -> Lines = importMember "./Native.js"
#else
  open System.IO
  let files =
    let rec readSpecs dir =
      if not (Directory.Exists (dir + "/docs")) then readSpecs (dir + "/..")
      else Directory.GetFiles(dir + "/docs/specs", "*.md", SearchOption.AllDirectories)
    readSpecs (Directory.GetCurrentDirectory())
  let readFile p = File.ReadAllLines(p)
#endif

let maybe def f = Option.map f >> Option.defaultValue def

let testOrTests n = if n = 1 then "test" else "tests"


/// Makes a test object from test lines
let readTestLines fileName (settings: TestSettings) lines : Result<Test * Option<Test>,TestError> =
  let error errType = Error (fileName, lines, errType)

  /// Splits a string at a given column and returns a string tuple
  let splitAtWidth (col: int) (s: string) : string * string =
    let rec loop i width =
      if i > s.Length || width > col then i-1
      elif i >= s.Length then i
      else loop (i+1) (width + Core.strWidth 1 (s.Substring (i,1)))
    let p = loop 0 0 in s.Substring (0, p), s.Substring p

  /// Gets the width of the substring before the given marker
  let strWidthBefore (marker: string) (s: string) : int =
    let p = s.IndexOf(marker)
    if p < 0 then -1 else Core.strWidth 1 (s.Substring (0, p))

  /// Removes special characters and trailing whitespace
  let cleanUp : Lines -> Lines =
    let cleanUps = [|
      Regex(" ->(?=\s*$)"), "   "; Regex("-or-"), "    "; Regex("¦"), " "
      Regex("[«»]"), ""; Regex("\s+$"), ""; Regex("·"), " "; Regex("-*→"), "\t"
    |]
    let cleanLine (l: string) =
      cleanUps |> Seq.fold (fun s (reg, rep) -> reg.Replace(s, rep)) l
    Array.map cleanLine >> Array.rev >> Array.skipWhile String.IsNullOrEmpty >> Array.rev

  /// Gets the wrapping column (denoted by `¦` from the given lines)
  let getWrappingColumn lines =
    let ps = lines |> Seq.map (strWidthBefore "¦") |> Seq.filter (fun x -> x >= 0) |> Seq.toArray
    if ps.Length > 0 && Seq.forall (fun p -> p = ps.[0]) ps then Some ps.[0] else None

  /// Removes all common whitespace indent from each of a set of lines
  let removeIndent (lines: Lines) =
    let indentLength (l: string) =
      let t = l.TrimStart() in if t.Length > 0 then Some (l.Length - t.Length) else None
    let indents = lines |> Seq.choose indentLength
    let minIndent = if Seq.isEmpty indents then 0 else Seq.min indents
    lines |> Array.map (fun l -> l.Substring(Math.Min(l.Length, minIndent)))

  /// Splits a group of lines with the given marker. The marker can be on any line
  let splitLines marker (lines: Lines) =
    let splitPoint = lines |> Seq.map (strWidthBefore marker) |> Seq.max
    if splitPoint < 0 then removeIndent lines, None else
    Array.map (splitAtWidth (splitPoint + marker.Length)) lines
      |> Array.unzip
      |> fun (l, r) -> removeIndent l, Some (removeIndent r)

  /// Makes a selections array from markers in the given input lines
  let getSelections (lines: Lines) : Result<Selection array,TestErrorType> =
    let sel l1 c1 l2 c2 = {anchor={line=l1; character=c1}; active={line=l2; character=c2}}
    let regex = Regex("[«»]")
    let enum = (lines :> string seq).GetEnumerator()

    let rec loop sels pending i line : Result<Selection array,TestErrorType> =
      let m = regex.Match(line)
      if m.Success then
        let c, found = m.Index, if m.Value = "«" then Anchor else Active
        match pending, found with
          | None, x ->
              loop sels (Some (x,i,c)) i (line.Remove(c, 1))
          | Some (Anchor,lp,cp), Active ->
              loop (sel lp cp i c :: sels) None i (line.Remove(c, 1))
          | Some (Active,lp,cp), Anchor ->
              loop (sel i c lp cp :: sels) None i (line.Remove(c, 1))
          | _ ->
              Error InvalidSelection
      elif enum.MoveNext() then loop sels pending (i+1) (enum.Current)
      else
        match pending, sels with
          | Some _, _ -> Error InvalidSelection
          | None, [] -> Ok ([||])
          | None, _ -> Ok (List.rev sels |> List.toArray)

    enum.MoveNext() |> ignore
    loop [] None 0 enum.Current

  // Search for "<only>" at the end of a line to filter this test
  let lines, only =
    let keyword = "<only>"
    let findOnly b (l: string) =
      if l.EndsWith(keyword) then l.Replace(keyword, ""), true else l, b
    lines |> Array.mapFold findOnly false
  let inputLines, maybeOutputLines = splitLines " -> " lines
  match maybeOutputLines with
  | None -> error NoOutput
  | Some outputLines ->
  let expectedLines, maybeReformatLines = splitLines "-or-" outputLines
  let maybeWrappingColumn =
    Option.toList maybeReformatLines @ [inputLines; expectedLines]
      |> Seq.map getWrappingColumn
      |> Seq.reduce (fun x y -> if x = y then x else None)

  match maybeWrappingColumn with
  | None -> error InvalidWrappingColumn
  | Some wrappingColumn ->
  match getSelections inputLines with
  | Error err -> error err
  | Ok sels ->
      let mkTest forceReformat expectedLines =
        { fileName = fileName; language = settings.language; only = only
          settings =
            { column = wrappingColumn; tabWidth = settings.tabWidth
              doubleSentenceSpacing = settings.doubleSentenceSpacing
              reformat = forceReformat || settings.reformat
              wholeComment = settings.wholeComment
            }
          input = cleanUp inputLines; selections = sels; expected = cleanUp expectedLines
        }

      Ok (mkTest false expectedLines, Option.map (mkTest true) maybeReformatLines)

/// Reads all the test samples (set of indented lines) in the given file.
let readSamplesInFile (fileName: string) : Result<Test * Option<Test>,TestError> list =

  let readSettings (line: string) : TestSettings =
    let pairs = line.Substring(1).Split(',')
    let splitPair (s: string) =
      let p = s.Split(':') |> Array.map (fun s -> s.Trim()) in p.[0], p.[1]
    let map = pairs |> Seq.map splitPair |> Map.ofSeq
    let pick k fn def = Map.tryFind k map |> maybe def fn
    {
      language = pick "language" (fun (s: string) -> s.Trim('"')) defaultSettings.language
      column = defaultSettings.column
      tabWidth = pick "tabWidth" Int32.Parse defaultSettings.tabWidth
      doubleSentenceSpacing = pick "doubleSentenceSpacing" bool.Parse defaultSettings.doubleSentenceSpacing
      reformat = pick "reformat" bool.Parse defaultSettings.reformat
      wholeComment = pick "wholeComment" bool.Parse defaultSettings.wholeComment
    }

  // Used when we reach EOF; adds the last test if we were in one.
  let maybeFinishSample newSample (tests, settings, sampleLines) =
    match sampleLines with
      | None | Some [] -> tests, settings, newSample
      | Some ls ->
        let newTest = readTestLines fileName settings (List.toArray (List.rev ls))
        newTest :: tests, settings, newSample

  let readLine (tests, settings, sampleLines) (line: string) =
    // Possible sample line: add it to buffer if was following a blank line
    if (line.StartsWith "    ") then
      match sampleLines with
        | None -> tests, settings, None
        | Some ls -> tests, settings, Some (line :: ls)
    // Settings line
    elif (line.StartsWith "> ") then tests, readSettings line, None
    // All other lines. If line is empty then start new sample
    else
      let newSample = if line.Length = 0 then Some [] else None
      maybeFinishSample newSample (tests, settings, sampleLines)

  Native.readFile fileName
    |> Seq.fold readLine ([], defaultSettings, Some [])
    |> maybeFinishSample None
    |> fun (tests, _, _) -> List.rev tests

/// Applies an edit returned from wrapping to the given (input) lines.
let applyEdit (edit: Edit) (oldLines: Lines) : Lines =
  Array.concat [
    oldLines.[0..(edit.startLine-1)]
    edit.lines
    oldLines.[(edit.endLine+1)..]
  ]

let prettyFileName (fileName: string) = fileName.Substring(fileName.IndexOf("/docs/") + 6)

/// Prints details of a test failure with input, expected and actual columns
let printFailure (test: Test) (actual: Lines) =
  let settings = [
    test.language
    if test.settings.tabWidth <> 4 then "tabWidth: " + test.settings.tabWidth.ToString() else ""
    if test.settings.doubleSentenceSpacing then "doubleSentenceSpacing: true " else ""
    if test.settings.reformat then "reformat: true " else ""
    if not test.settings.wholeComment then "wholeComment: false" else ""
  ]

  eprintfn "\nFailed: %s %s\n" (prettyFileName test.fileName)
    (String.Join (' ', List.filter ((<>) "") settings))
  let width = test.settings.column
  let colWidth = width + 10
  let print (cols: string[]) =
    let cols = cols |> Array.map (fun c -> c.PadRight(colWidth).Substring(0, colWidth))
    eprintfn "%s" (String.Join(" | ", cols))

  let columns = [| test.input; test.expected; actual |]

  let showTabs (str: string) =
    let symbol = String('-', test.settings.tabWidth - 1) + "→"
    let parts = str.Split('\t')
    let addTab i (s: string) : string =
      if i = parts.Length - 1 then s else
      s + symbol.Substring(s.Length % test.settings.tabWidth)
    String.Join("", parts |> Seq.mapi addTab)

  print [| "Input"; "Expected"; "Actual" |]
  [|'-';'-';'-'|]
    |> Array.map (fun c -> String(c, width) + "¦" + String(c, colWidth - width - 1))
    |> print

  let lineCount = columns |> Seq.map (fun c -> c.Length) |> Seq.max
  for i = 0 to lineCount - 1 do
    columns
      |> Array.map (fun a -> if i >= a.Length then "" else a.[i])
      |> Array.map (fun s -> s.Replace(' ', '·'))
      |> Array.map showTabs
      |> print

/// Runs a test
let runTest (test: Test) =
  let getLine = Func<int,string>(fun i -> if i < test.input.Length then test.input.[i] else null)
  let file = {language = test.language; path = ""; getMarkers = Func<CustomMarkers>(fun () -> Core.noCustomMarkers)}
  let edit = Core.rewrap file test.settings test.selections getLine
  let actual = applyEdit edit test.input
  let arrayEqual (a: 'a[]) (b: 'a[]) = (a.Length = b.Length && not (Seq.exists2 (fun x y -> x <> y) a b))
  if arrayEqual actual test.expected then true else printFailure test actual; false

/// Results accumulator type for tests
type Results = {passes:int; failures:int; errors:int}

[<EntryPoint>]
let main argv =
  let norm (s: String) = s.ToLower().Replace('\\', '/')
  let argv = Array.map norm argv
  let inArgv (f: string) = argv |> Array.exists (fun a -> f.EndsWith (a))
  let filesToTest =
    Native.files |> Seq.map norm |> if Array.isEmpty argv then id else Seq.filter inArgv

  let processTest (acc: Results) = function
    | Ok (test, maybeReformatTest) ->
        let run (acc: Results) (t: Test) =
          if t.settings.reformat then acc // Skip reformat tests
          elif runTest t then { acc with passes = acc.passes + 1 }
          else { acc with failures = acc.failures + 1 }

        Option.fold run (run acc test) maybeReformatTest

    | Error (fileName, lines, err) ->
        eprintfn "\nError: %A, %s" err (prettyFileName fileName)
        eprintfn "==%s==" "Input"
        Seq.iter (eprintfn "%s") lines
        { acc with errors = acc.errors + 1 }

  let init = {passes = 0; failures = 0; errors = 0}
  let allTests = filesToTest |> Seq.collect readSamplesInFile |> Seq.toList
  let filtered = allTests |> List.filter (function | Ok (t, _) when t.only = true -> true | _ -> false)
  let testsToRun = if filtered.Length > 0 then filtered else allTests
  let results = testsToRun |> Seq.fold processTest init
  eprintfn "Passed: %i; Failed: %i; Errored: %i" results.passes results.failures results.errors
  results.failures + results.errors
