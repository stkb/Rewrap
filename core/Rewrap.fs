namespace Rewrap

/// Settings passed in from the editor
type Settings = {
    column : int
    tabWidth : int
    doubleSentenceSpacing : bool
    tidyUpIndents : bool
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
    active : Position
    anchor : Position 
}
