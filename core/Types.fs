namespace Rewrap

/// File language and path. Used to select a parser.
type File = { language: string; path: string }

/// Settings passed in from the editor
type Settings = {
    column : int
    tabWidth : int
    doubleSentenceSpacing : bool
    reformat : bool
    wholeComment : bool
}

type Position = {
    line : int
    character : int
}

type Selection = {
    anchor : Position
    active : Position
}

/// Edit object to be passed out to the editor
type Edit = {
    startLine : int
    endLine : int
    lines : array<string>
    /// In a standard wrap this is the same as the selections passed in to
    /// wrapping. For an auto-wrap this is the normal selection position for
    /// after the just-done edit (before wrapping). These selections still need
    /// adjusting to be in the expected places after a wrap (both clients do
    /// this themselves). In the future we might do this in Core instead.
    selections: array<Selection>
}

type DocState = { filePath: string; version: int; selections: Selection[] }
