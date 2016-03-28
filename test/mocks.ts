import { Position, Range, Selection, TextLine, Uri } from 'vscode'
import * as vscode from 'vscode'

export { TextDocument, TextEditor }


const maxRange = new Range(0, 0, Number.MAX_VALUE, Number.MAX_VALUE)

type Location = Position | Range | Selection

class TextDocument implements vscode.TextDocument
{
  constructor(
    contents: string, fileName: string, public languageId: string = "plaintext") 
  {
    this.uri = Uri.file(fileName)
    this.eol = contents.match(/\r\n/) ? '\r\n' : '\n'
    this._lines = contents.split(/\r?\n/g)
  }

  eol: string

  get fileName(): string {
    return this.uri.fsPath
  }

  isDirty: boolean;

  isUntitled: boolean;
  
  get lineCount() {
    return this._lines.length
  }
  
  uri: Uri

  version: number;

  save(): Thenable<boolean> {
    throw Error("Not implemented")
  }

  lineAt(lineOrPosition: number | Position): TextLine 
  {
    const line = 
      lineOrPosition instanceof Position ? lineOrPosition.line : lineOrPosition

    if (line < 0 || line >= this.lineCount) {
      throw new Error('Illegal value ' + line + ' for `line`');
    }

    let result = this._textLines[line];
    if (!result || result.lineNumber !== line || result.text !== this._lines[line]) {

      const text = this._lines[line];
      const firstNonWhitespaceCharacterIndex = /^(\s*)/.exec(text)[1].length;
      const range = new Range(line, 0, line, text.length);
      const rangeIncludingLineBreak = new Range(line, 0, line + 1, 0);

      result = Object.freeze({
        lineNumber: line,
        range,
        rangeIncludingLineBreak,
        text,
        firstNonWhitespaceCharacterIndex,
        isEmptyOrWhitespace: firstNonWhitespaceCharacterIndex === text.length
      });

      this._textLines[line] = result;
    }

    return result;
  }

  offsetAt(position: vscode.Position): number 
  {
    position = this.validatePosition(position);
    
    return this._lines.reduce((sum, lineText, line) => {
      return line < position.line ? sum + lineText.length + this.eol.length
           : line === position.line ? sum + position.character
           : sum
    }, 0)
  }

  positionAt(offset: number): vscode.Position 
  {
    // Adjust offset to integer >= 0
    offset = Math.max(0, Math.floor(offset));

    for(let line = 0; line < this._lines.length; line++) {
      const lineLength = this._lines[line].length + this.eol.length
      
      if(offset >= lineLength) offset -= lineLength
      else {
        return new Position(line, offset)
      }
    }
    
    // If we got here then offset > the text length. Return the max valid pos.
    return this.validatePosition(maxRange.end)
  }

  getText(range?: Range): string 
  {
    range = this.validateRange(range || maxRange)
    
    if(range.isSingleLine) {
      return this._lines[range.start.line]
        .substring(range.start.character, range.end.character)
    }
    
    else {
      const textLines = [] as string[]
      
      for(let line = range.start.line; line <= range.end.line; line++) {
        const lineText = this._lines[line]
        if(line === range.start.line) {
          textLines.push(lineText.substr(range.start.character))
        }
        else if(line === range.end.line) {
          textLines.push(lineText.substr(0, range.end.character))
        }
        else {
          textLines.push(lineText)
        }
      }
      
      return textLines.join(this.eol)
    }
  }

  getWordRangeAtPosition(position: Position): Range {
    throw Error("Not implemented")
  }

  validateRange(range: Range): Range 
  {
    return new Range(
      this.validatePosition(range.start),
      this.validatePosition(range.end)
    )
  }

  validatePosition(position: Position): Position 
  {
    const line: number = Math.min(position.line, this._lines.length - 1)
    const column = Math.min(position.character, this._lines[line].length)
    return new Position(line, column)
  }
  
  private _lines: string[]
  private _textLines: TextLine[] = []
}

class TextEditor 
{
  constructor(public document: TextDocument) { }
  
  edit(callback: (editBuilder: vscode.TextEditorEdit) => void): Thenable<boolean>
  {
    // Functions as insert, delete or replace
    const doEdit = (location: Location, value: string = "") =>
    {
        const rangeToReplace = location instanceof Position ?
            new Range(location, location) : location
        , rangeBefore = new Range(maxRange.start, rangeToReplace.start)
        , rangeAfter = new Range(rangeToReplace.end, maxRange.end)
        , textBefore = this.document.getText(rangeBefore)
        , textAfter = this.document.getText(rangeAfter)
        , textToInsert = value.split(/\r?\n/g).join(this.document.eol)
        , newDocumentText = textBefore + textToInsert + textAfter
        
      this.document = new TextDocument(
        newDocumentText, this.document.fileName, this.document.languageId
      )
    }

    callback(new TextEditorEdit(doEdit, doEdit, doEdit))
    
    return Promise.resolve(true)
  }
  
  options: vscode.TextEditorOptions = {
    tabSize: 4, insertSpaces: true
  }
  
  selections = [new Selection(maxRange.start, maxRange.end)]
}

class TextEditorEdit 
{
  constructor
    ( public replace: (location: Location, value: string) => void
    , public insert: (location: Position, value: string) => void
    // `delete` variable name not allowed here
    , public _delete: (location: Range | Selection) => void
    ) { }
  
  public get delete() {
    return this._delete;
  }
}