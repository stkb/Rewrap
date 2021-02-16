const Types = require('./core/Types')
const Main = require('./core/Main')


export interface CustomMarkers { line: string, block: [string, string] }
export interface CustomMarkersStatic {
  new(line: string, block: string[]): CustomMarkers
}

export interface DocState { filePath: string, version: number, selections: Selection[] }

export interface DocType {
  path: string
  language: string
  getMarkers: () => CustomMarkers
}

export interface Edit {
  startLine: number
  endLine: number
  lines: string[]
  selections: Selection[]
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

export const CustomMarkers: CustomMarkersStatic = Types.CustomMarkers

/** Gets the current wrapping column used for the given document */
export const getWrappingColumn: (path: string, columns: number[]) => number =
  Main.getWrappingColumn

export const maybeAutoWrap:
  (docType: DocType, settings: Settings,
   newText: string, pos: Position, docLine: (i:number) => string) => Edit =
  Main.maybeAutoWrap

export const maybeChangeWrappingColumn:
  (docState: DocState, columns: number[]) => number = Main.maybeChangeWrappingColumn

export const rewrap:
  (docType: DocType, settings: Settings,
   selections: Selection[], docLine: (i:number) => string) => Edit =
  Main.rewrap

export const saveDocState: (docState: DocState) => void = Main.saveDocState
