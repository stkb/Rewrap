module internal Columns

// Deals the [wrapping to rulers
// feature](https://github.com/stkb/Rewrap/wiki/Settings-VSCode#wrapping-to-rulers).
// The user can have multiple "rulers" (potential wrapping columns) set, either
// with the `editor.rulers` setting in VSCode or the Guidelines extension in VS.
//
// When a user opens a document the wrapping column is set at the first ruler in
// their settings. To change it to the next ruler, they have to do a wrap
// (alt+q) twice in a row without doing anything inbetween.
//
// If the user doesn't have any rulers set, we always treat it as if they have
// a list of one ruler (their set wrapping column).

open Rewrap
open Extensions.Option

// We store one DocState record for the state of the editor after every wrap
// operation, and then look at it again before the next. If it hasn't changed,
// it's counted as doing a wrap twice in a row, and we move the wrapping column
// to the next ruler (if it exists).
//
// The DocState stores the current filepath, document version number and
// selection position, to make sure the user hasn't switched file, made an
// edit, or even moved the cursor, respectively.
let mutable private lastDocState : DocState =
    { filePath = ""; version = 0; selections = [||] }

/// We remember the last wrapping column used for each document.
let private docWrappingColumns =
    new System.Collections.Generic.Dictionary<string, int>()


//vvvvvvvvvvvvvvvvvvvvvvvvvvvvvv PUBLIC MEMBERS vvvvvvvvvvvvvvvvvvvvvvvvvvvvvv//


// Gets the current wrapping column for the given document, given the supplied
// list of valid wrapping columns (rulers). The current wrapping column for the
// document is stored, but this is only returned if it's contained in the given
// list of rulers.
//
// If it isn't, then the user has changed these since the last wrap, so we just
// pick the first one in the list. The most likely scenario here is simply that
// the user changed the wrapping column in the settings, so we definitely want
// to respect the new setting.
//
// The list of rulers must not be empty.
let getWrappingColumn filePath rulers =
    let setAndReturn column = docWrappingColumns.[filePath] <- column; column
    let firstRuler = Option.defaultValue 80 <| Array.tryHead rulers
    if not (docWrappingColumns.ContainsKey(filePath)) then
         setAndReturn firstRuler
    else
        Array.tryFind ((=) docWrappingColumns.[filePath]) rulers
            |> Option.defaultValue firstRuler
            |> setAndReturn

// Takes a set of rulers and check if we already have a wrapping column for the
// given document.
// 1) If we don't yet have one, the first ruler is used and that value is saved.
// 2) If we do already have a wrapping column and it exists in the given rulers,
//     that value is returned.
// 3) If the value we have isn't found in the given rulers, then the rulers must
//     have changed since we last wrapped. Like 1) we save and return the first
//     ruler
let maybeChangeWrappingColumn (docState: DocState) (rulers: int[]) : int =
    let filePath = docState.filePath
    if not (docWrappingColumns.ContainsKey(filePath)) then
        getWrappingColumn filePath rulers
    else
        let shiftRulerIfDocStateUnchanged i =
            if docState = lastDocState then (i + 1) % rulers.Length else i
        let rulerIndex =
            Array.tryFindIndex ((=) docWrappingColumns.[filePath]) rulers
                |> option 0 shiftRulerIfDocStateUnchanged

        docWrappingColumns.[filePath] <- rulers.[rulerIndex]
        docWrappingColumns.[filePath]

/// Saves the DocState, to compare against next time.
let saveDocState docState =
    lastDocState <- docState
