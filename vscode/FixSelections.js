"use strict";
// When we replace lines of text in the editor with new text that's been
// wrapped, the original cursor position/selections can get messed up; ie. the
// cursor isn't by the word it used to be.
//
// This module does a diff of the old and new text (using the js module
// fast-diff), and uses it to calculate where the selections should be after
// wrapping, so they can be re-applied in the editor after the edit(s) have been
// done.
const vscode = require("vscode");
const fd = require('fast-diff');

/** Given lines of original text, a set of selections and a set of edits,
 *  returns the positions of the selections for after the edits have been
 *  applied. */
module.exports = function (oldLines, selections, edit)
{
    if(!edit || !edit.lines.length) return selections

    selections = selections.map(s =>
        new vscode.Selection(s.anchor.line, s.anchor.character, s.active.line, s.active.character)
    )

    let runningLineGrowth = 0;
    const { startLine, endLine } = edit
         , newStartLine = startLine + runningLineGrowth
         , oldLineCount = endLine - startLine + 1
         , diff = fd(oldLines.join('\n'), edit.lines.join('\n'))
         , newLineCount = edit.lines.length
         , rangeLineGrowth = newLineCount - oldLineCount;
    selections = selections.map(s => {
        const points = [s.anchor, s.active]
            .map(p => {
            // For selection points in the edit range, adjust from the diff
            if (p.line >= newStartLine && p.line <= endLine + runningLineGrowth) {
                const oldOffset = offsetAt(oldLines, p.translate(-newStartLine)), newOffset = newOffsetFromOld(oldOffset, diff);
                p = positionAt(edit.lines, newOffset).translate(newStartLine);
            }
            else if (p.line > endLine + runningLineGrowth) {
                p = p.translate(rangeLineGrowth);
            }
            return p;
        });
        return new vscode.Selection(points[0], points[1]);
    });
    runningLineGrowth += rangeLineGrowth;

    return selections;
}

/** Gets the new offset of a position, given and old offset and a diff between
 *  old and new text. */
function newOffsetFromOld(offset, diff) {
    // Count up chars from parts of the diff until we get to the original offset.
    // Keep count of the delta between old & new text from added & removed chars.
    let runningOffset = 0, delta = 0;
    for (let [operation, text] of diff) {
        if (operation !== 1) {
            if (runningOffset + text.length > offset)
                break;
            runningOffset += text.length;
        }
        delta += operation * text.length;
    }
    return offset + delta;
}

function offsetAt(lines, position) {
    return (lines
        .slice(0, position.line)
        .reduce((sum, s) => sum + s.length + 1, 0)
        + position.character);
}


function positionAt(lines, offset) {
    for (let i = 0; i < lines.length; i++) {
        const lineLength = lines[i].length + 1;
        if (offset < lineLength)
            return new vscode.Position(i, offset);
        else
            offset -= lineLength;
    }
    throw new Error("Offset greater than text length.");
}
