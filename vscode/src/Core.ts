// This file serves as a wrapper to the core JS library. The first thing it does is keep
// this ugly import path to one place.
import * as Main from '../../core/dist/dev/Main.js'

// It also converts vscode Position/Selection objects to and from the equivalents used by
// the core code. If this is not done, errors may occur (eg when testing for equality).
import {Position as VscPosition, Selection as VscSelection} from 'vscode'
interface Position { line: number, character: number }
interface Selection { anchor: Position, active: Position }

// For data coming in
const pojoPosition = ({line, character}: Position) : Position => ({line, character})
const pojoSelections = (sels: readonly Selection[]) : readonly Selection[] =>
  sels.map(({anchor, active}) => ({anchor: pojoPosition(anchor), active: pojoPosition(active)}))
const pojoDocState = (vscDocState: DocState) =>
  ({...vscDocState, selections: pojoSelections(vscDocState.selections)})

// For data going out
const vscSelection = (s: Selection) : VscSelection =>
  new VscSelection(s.anchor.line, s.anchor.character, s.active.line, s.active.character)
const vscEdit = (edit) : Edit => {
  // Edit is a class instance so we can't just create a POJO. Instead we modify the
  // selections
  edit.selections_ = edit.selections.map(vscSelection)
  return edit
}


export interface CustomMarkers { line: string, block: [string, string] }

export interface DocState { filePath: string, version: number, selections: readonly VscSelection[] }

export interface DocType { path: string, language: string, getMarkers: () => CustomMarkers }

export interface Edit {
  startLine: number
  endLine: number
  lines: readonly string[]
  selections: readonly VscSelection[]

  isEmpty: boolean
}

export interface Settings {
  column: number
  tabWidth: number
  doubleSentenceSpacing: boolean
  reformat: boolean
  wholeComment: boolean
}


/** Gets the current wrapping column used for the given document */
export const getWrappingColumn: (path: string, columns: number[]) => number =
  Main.getWrappingColumn

/** Does an auto-wrap if space/enter is pressed after the wrapping column */
export const maybeAutoWrap =
    (docType: DocType, settings: Settings,
    newText: string, vscPos: VscPosition, docLine: (i:number) => string | null) : Edit =>
  vscEdit (Main.maybeAutoWrap (docType, settings, newText, pojoPosition (vscPos), docLine))


export const maybeChangeWrappingColumn = (vscDocState: DocState, columns: number[]) : number =>
  Main.maybeChangeWrappingColumn (pojoDocState(vscDocState), columns)

export const noCustomMarkers : CustomMarkers = Main.noCustomMarkers

export const rewrap =
    (docType: DocType, settings: Settings, unsafeSelections: readonly VscSelection[],
    docLine: (i:number) => string | null) : Edit =>
  vscEdit (Main.rewrap (docType, settings, pojoSelections(unsafeSelections), docLine))


export const saveDocState = (vscDocState: DocState) =>
  Main.saveDocState (pojoDocState(vscDocState))
