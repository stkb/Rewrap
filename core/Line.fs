module Line

// This file is a mixture of convenience functions for strings (older code) and
// the new Line type.

open System.Text.RegularExpressions
open Prelude


/// Returns true if the line is empty or just whitespace
let isBlank : string -> bool = fun l -> System.String.IsNullOrWhiteSpace(l)

/// Returns true if the line matches the given regex
let contains : Regex -> string -> bool = fun regex line -> regex.IsMatch(line)

/// Returns true if the line starts with the given string, after optional whitespace
let startsWith : string -> string -> bool =
  fun marker line -> Regex(@"^\s*" + marker).IsMatch(line)

/// Tries to match a regex against a line. If a match is found, returns all
/// characters up to and including the end of the match.
let tryMatch : Regex -> string -> string Option =
  fun regex line ->
  let m = regex.Match(line)
  if m.Success then Some (String.takeStart (m.Index + m.Length) line) else None

let private leadingWhitespaceRegex: Regex = Regex(@"^\s*")
/// Gets the leading whitespace for the line
let leadingWhitespace : string -> string =
  fun line -> leadingWhitespaceRegex.Match(line).Value

/// Returns if a line contains text
let containsText : string -> bool =
  fun line ->
  // Don't use \w because it contains underscore. Using almost the whole
  // Unicode range but it's probably good enough. Little hack for Ruby.
  contains (Regex("[A-Za-z0-9\u00C0-\uFFFF]")) line
    && not (contains (Regex(@"^=(begin|end)\s*$")) line)

/// Splits a line into prefix and remainder. If the prefix pattern is not found
/// the prefix is an empty string and the remainder is the whole line.
let split : Regex -> string -> string * string =
  fun regex line ->
  let prefix = tryMatch regex line |? "" in prefix, String.dropStart prefix.Length line

// Converts all tabs in the line to spaces
let tabsToSpaces (tabSize: int) (str: string) =
  match str.Split([|'\t'|]) |> List.ofArray |> List.rev with
    // This case isn't possible since String.split never returns an empty array
    | [] -> str
    // We add padding to all but the last string
    | x :: xs ->
      xs
        |> List.map
          (fun s -> s.PadRight((s.Length / tabSize + 1) * tabSize))
        |> (fun tail -> List.Cons (x, tail))
        |> List.rev
        |> String.concat ""

/// Returns the width of the given char. To give the correct width of tab
/// characters, also takes a tab size and column the character is at. This
/// column should be calculated with the strWidth function, and not just the
/// index of the char in the string, to account for tabs or double-width chars
/// before it.
let charWidth tabSize column charCode =
  match charCode with
    | 0x0009us -> tabSize - (column % tabSize)
    | 0x0000us -> 1 // We use this as a placeholder for non breaking spaces
    | x when x < 0x0020us -> 0
    | x when x < 0x2E80us -> 1
    | x when x >= 0x2E80us && x <= 0xD7AFus -> 2
    | x when x >= 0xF900us && x <= 0xFAFFus -> 2
    | x when x >= 0xFF01us && x <= 0xFF5Eus -> 2
    | _ -> 1

/// Gets the visual width of a string, taking tabs into account. Takes an offset
/// for if the string is positioned at a different column than 0. Returns the
/// width of just the string given.
let strWidth' : int -> int -> string -> int =
  fun offset tabWidth str ->
  let tabWidth = max tabWidth 1
  let rec loop acc i =
    if i >= str.Length then acc - offset
    else loop (acc + charWidth tabWidth acc ((uint16) str.[i])) (i + 1)
  loop offset 0

/// strWidth' but from column 0
let strWidth : int -> string -> int = strWidth' 0


/// New Line type. An object that represents a line, divided into prefix and
/// content. This is context-sensitive; eg first comment markers may be parsed
/// and put into the prefix, and then later markdown blockquote markers may by
/// added to the prefix, and the rest of the content parsed.
type Line (p: string, c: string) =

  member _.prefix = p
  member _.content = c

  // Constructors
  new(line: Line) = Line(line.prefix, line.content)
  /// If splitAt is greater than the string's length then it's taken as equal
  new(str: string, splitAt: int) =
    let splitAt = min splitAt str.Length
    Line(str.Substring(0, splitAt), str.Substring(splitAt))
  private new(line: Line, splitAt: int) =
    Line(line.prefix + line.content, splitAt)

  /// The number of chars before the split
  member _.split = p.Length



  /// Increases the split position by the given number of chars
  static member adjustSplit : int -> Line -> Line = fun d line ->
    if line.content = "" then line else Line(line, line.prefix.Length + d)

  /// Returns a new line with the prefix length adjusted so that whitespace is
  /// trimmed up to the given indent, where possible. Indent is absolute for
  /// the line, not relative to the current prefix position
  static member trimUpTo : int -> Line -> Line =
    fun indent line ->
    let trimmed = line.content.TrimStart()
    let maxIndent = line.prefix.Length + line.content.Length - trimmed.Length
    Line(line, min indent maxIndent)

  static member mapContent : (string -> string) -> Line -> Line =
    fun fn line -> Line(line.prefix, fn line.content)
  static member mapPrefix : (string -> string) -> Line -> Line =
    fun fn line -> Line(fn line.prefix, line.content)

  override this.ToString() = this.prefix + this.content

  static member toString : Line -> string = fun line -> line.ToString()

  // These for compatibility with old new code

  /// Applies a function to the content part of the line and returns the
  /// result of that function
  static member onContent fn (line: Line) = fn (line.content)
