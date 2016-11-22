import { Position } from 'vscode'

export { offsetAt, positionAt }


function offsetAt(lines: string[], position: Position) : number
{
  return (
    lines
      .slice(0, position.line)
      .reduce((sum, s) => sum + s.length + 1, 0) 
      + position.character
  )
}


function positionAt(lines: string[], offset: number) : Position
{
  for(let i = 0; i < lines.length; i++) {
    const lineLength = lines[i].length + 1
    if(offset < lineLength) return new Position(i, offset)
    else offset -= lineLength
  }
  throw "Something went wrong with determining a position."
}