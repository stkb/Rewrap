namespace Rewrap

/// Settings passed in from the editor
type Settings = {
    column : int // To be removed
    columns: int[]
    tabWidth : int
    doubleSentenceSpacing : bool
    reformat : bool
    wholeComment : bool
}


/// Edit object to be passed out to the editor
type Edit = {
    startLine : int
    endLine : int
    lines : array<string>
}


type Position = {
    line : int
    character : int
}
    

type Selection = { 
    anchor : Position 
    active : Position
}


type DocState = 
    { filePath: string
    ; language : string
    ; version: int
    ; selections: Selection[]
    }