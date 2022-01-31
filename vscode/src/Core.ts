// This file serves as a wrapper to the core JS library. It does nothing more
// than a .d.ts file would do, except keeping this ugly import path to one place
import * as Main from '../../core/dist/dev/Main.js'


export interface CustomMarkers { line: string, block: [string, string] }

export interface DocState { filePath: string, version: number, selections: readonly Selection[] }

export interface DocType {
  path: string
  language: string
  getMarkers: () => CustomMarkers
}

export interface Edit {
  startLine: number
  endLine: number
  lines: readonly string[]
  selections: readonly Selection[]

  isEmpty: boolean
}

export interface Position { line: number, character: number }

export interface Selection { anchor: Position, active: Position }

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

export const maybeAutoWrap:
  (docType: DocType, settings: Settings,
   newText: string, pos: Position, docLine: (i:number) => string) => Edit =
  Main.maybeAutoWrap

export const maybeChangeWrappingColumn:
  (docState: DocState, columns: number[]) => number = Main.maybeChangeWrappingColumn

export const noCustomMarkers : CustomMarkers = Main.noCustomMarkers

export const rewrap:
  (docType: DocType, settings: Settings,
   selections: readonly Selection[], docLine: (i:number) => string) => Edit =
  Main.rewrap

export const saveDocState: (docState: DocState) => void = Main.saveDocState
