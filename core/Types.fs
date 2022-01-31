namespace Rewrap

open System

// Set of comment markers provided by the editor to create a custom language. To
// be valid, either `line` must be a nonempty string or `block` two nonempty
// strings.
type CustomMarkers = { line: string; block : string * string }


/// File language and path. Used to select a parser.
type File = { language: string; path: string; getMarkers: Func<CustomMarkers> }

/// Settings passed in from the editor
type Settings = {
  column : int
  tabWidth : int
  doubleSentenceSpacing : bool
  reformat : bool
  wholeComment : bool
}

type Position = { line: int; character: int }

type Selection = { anchor: Position; active: Position }

/// Edit object to be passed out to the editor. If endLine < startLine and lines is empty,
/// then it's a no-op edit.
[<Fable.Core.AttachMembers>]
type Edit (startLine_, endLine_, lines_, selections_) =
  member _.startLine : int = startLine_
  member _.endLine : int = endLine_
  member _.lines : array<string> = lines_
  /// In a standard wrap this is the same as the selections passed in to wrapping. For an
  /// auto-wrap this is the normal selection position for after the just-done edit (before
  /// wrapping). These selections still need adjusting to be in the expected places after
  /// a wrap (both clients do this themselves). In the future we might do this in Core
  /// instead.
  member _.selections : array<Selection> = selections_

  member _.isEmpty = endLine_ < startLine_ && lines_.Length = 0
  static member empty = Edit (0, -1, [||], [||])
  member _.withSelections newSels = Edit (startLine_, endLine_, lines_, newSels)

type DocState = { filePath: string; version: int; selections: Selection[] }
