const Path = require('path')
const FS = require('fs')
const Assert = require('assert')
const Core = require('./fable/Main')

const specsDir = Path.join(__dirname, 'specs')
const defaultSettings = 
    { language: 'plaintext'
    , tabWidth: 4
    , doubleSentenceSpacing: false
    , tidyUpIndents: false
    , wholeComment: true
    }

const cmdlineFileNames =
    process.argv.length > 2 ? process.argv.slice(2) : null

const fileNames =
    cmdlineFileNames || getSpecFiles()

const tests =
    fileNames
        .map(readFileLines)
        .map(readSamplesInFile)
        .reduce((xs, x) => [...xs, ...x], [])
        .map(({settings, lines}) => 
            Object.assign(
                { settings }, readTestLines(lines)
            )
        )

const failures = runTests(tests)

if(failures.length) {
    failures
        .reduce((xs, x) => [...xs, '', ...x], [])
        .forEach(s => console.log(s))

    console.log()
    console.log(`${tests.length} ${testOrTests(tests.length)} run`)
    console.log(`${failures.length} ${testOrTests(failures.length)} failed.`)
    process.exitCode = -1
}
else {
    console.log(`${tests.length} ${testOrTests(tests.length)} run`)
    console.log(`All ${testOrTests(tests.length)} passed.`)
}

function testOrTests(n)
{
    return n == 1 ? "test" : "tests"
}



function getSpecFiles(dir)
{
    dir = dir || specsDir
    const children = FS.readdirSync(dir).map(n => Path.join(dir, n))

    const specFiles = children.filter(n => n.match(/\.md$/))
    const subDirs = children.filter(n => FS.statSync(n).isDirectory())
    return subDirs.map(getSpecFiles).reduce((xs, x) => [...xs, ...x], specFiles)
}


function readFileLines(fileName) {
    return (
        FS.readFileSync(fileName, { encoding: 'utf8' })
            .split(/\r?\n/)
    )
}


function readSamplesInFile(lines) 
{
    return loop([], defaultSettings, [], lines)

    function loop(output, settings, sampleLines, remainingLines)
    {
        const [line, ...nextRemainingLines] = remainingLines

        if(line == null) {
            if(sampleLines.length > 0) {
                return [...output, {settings, lines: sampleLines} ]
            }
            else {
                return output
            }
        }
        else {
            // Sample line: add it to buffer
            if(line.startsWith("    ")) {
                return loop
                    ( output
                    , settings
                    , [...sampleLines, line]
                    , nextRemainingLines
                    )
            }
            // End of sample: add it to output
            else if(sampleLines.length > 0) {
                return loop
                    ( [...output, {settings, lines: sampleLines} ]
                    , settings
                    , []
                    , remainingLines
                    )
            }
            // Settings line
            else if(line.startsWith("> ")) {
                return loop
                    ( output
                    , readSettings(line)
                    , []
                    , nextRemainingLines
                    )
            }
            // Any other line
            else {
                return loop(output, settings, sampleLines, nextRemainingLines)
            }
        }
    }
}


function readSettings(line) 
{
    const settings = eval("({" + line.substr(1) + "})")
    return Object.assign({}, defaultSettings, settings)
}


function readTestLines(lines) 
{
    let inputLines, outputLines

    const splitPoint = 
        Math.max(...lines.map(l => l.indexOf('->')))

    if(splitPoint == -1) {
        outputLines = inputLines = removeIndent(lines)
    }
    else {
        [inputLines, outputLines] =
            lines
                .map(line => splitAt(splitPoint + 2, line))
                // Convert list of tuples to tuple of lists
                .reduce
                    ( ([xs, ys], [x, y]) =>
                        [[...xs, x], [...ys, y]]
                    , [[], []]
                    )
                .map(removeIndent)
    }


    const wrappingColumn =
        getWrappingColumn(inputLines)

    return {
        err: wrappingColumn == -1 ? "Wrapping column" : undefined,
        input: cleanUp(inputLines),
        expected: cleanUp(outputLines),
        wrappingColumn,
        selections: getSelections(inputLines)
    }

    /** Splits a string at a given points and returns a string tuple */
    function splitAt(p, s) 
    {
        return [s.substr(0, p), s.substr(p)]
    }

    /** Removes any indent whitespace that is common to all lines */
    function removeIndent(lines) 
    {
        const indents = 
           lines
                .filter(l => l.match(/\S/))
                .map(l => l.match(/\s*/)[0].length)
        const minIndent =
            Math.min(...indents)

        return lines.map(l => l.substr(minIndent))
    }

    function getWrappingColumn(lines)
    {
        const positions =
            lines
                .map(l => l.indexOf('¦'))
                .filter(x => x > 0)

        if(positions.length > 0 
            && positions.every(p => p == positions[0]))
        {
            return positions[0]
        }
        else {
            return -1
        }
    }

    function getSelections(lines) 
    {
        // Make copy for mutation
        lines = [ ...lines ]
        const selections = []
        let selection = {}

        for(let i = 0; i < lines.length; i++) {
            const match = lines[i].match(/[«»]/)
            if(match) {
                pos = { line : i, character: match.index }
                if(match[0] == '«') selection.anchor = pos
                else selection.active = pos

                lines[i] = 
                    lines[i].substr(0, match.index) +
                    lines[i].substr(match.index + 1)
                i--
            }

            if(selection.anchor && selection.active) {
                selections.push(selection)
                selection = {}
            }
        }

        if(selections.length > 0) {
            return selections
        }
        else {
            return [ {
                anchor: { line: 0, character: 0 },
                active: { line: lines.length, character: 0 },
            } ]
        }
    }


    /** Removes special characters and trailing whitespace */
    function cleanUp(lines) 
    {
        return lines
            .map(line =>
                line
                    .replace(/->(?=\s*$)/, '  ')
                    .replace(/¦/g, ' ')
                    .replace(/[«»]/g, '')
                    .replace(/\s+$/, '')
                    .replace(/·/g, ' ')
                    .replace(/-*→/g, '\t')
            )
            // Trim empty lines off end
            .reduceRight
                ( (ls, l) => l || ls.length ? [l, ...ls] : []
                , []
                )
        
    }
}

/**
 * Runs the given tests and returns any failures
 */
function runTests(tests)
{
    const failures =
        tests.map(runTest).filter(f => !!f)

    return failures

    /** Runs a test and returns a failure object if it fails, otherwise null */
    function runTest({input, expected, settings, selections, wrappingColumn}) 
    {
        const edit =
            Core.rewrap
                ( settings.language
                , ''
                , selections
                , Object.assign(settings, { column: wrappingColumn })
                , input
                )

        const actual = applyEdit(edit, input)
        try {
            Assert.deepEqual(actual, expected)
            return null
        }
        catch(err) {
            return printTest
                ( input
                , expected
                , actual
                , wrappingColumn
                , settings.tabWidth
                )
        }
    }
}

function applyEdit(edit, lines)
{
    const copy = Array.from(lines)
    const length = edit.endLine - edit.startLine + 1 
    copy.splice(edit.startLine, length, ...edit.lines)
    return copy
}


function printTest(input, expected, actual, width, tabWidth) 
{
    const output = []
    const columns = [input, expected, actual]
    const columnLengths = columns.map(c => c.length)
    const lineCount = Math.max(...columnLengths)

    if(width == -1) {
        output.push("Error: no wrapping column set (with '¦')")
        output.push(...input)
        return output
    }

    const headers = 
        ["Input", "Expected", "Actual"]
            .map((s, i) => s + " (" + columnLengths[i] + ")")
    print(headers)

    print(['-', '-', '-'].map(s => s.repeat(width)))

    for(let i = 0; i < lineCount; i++) {
        const parts = columns
            .map(c => c[i] || '')
            .map(s => s.replace(/ /g, '·'))
            .map(showTabs)
        print(parts)
    }

    return output

    function print(parts) {
        const line = parts
            .map(s => s || '')
            .map(padRight)
            .join(' | ')
        output.push(' ' + line + ' ')
    }

    function padRight(s) {
        s = s.substr(0, width)
        return s + " ".repeat(width - s.length)
    }

    function showTabs(str)
    {
        const symbol =
            '-'.repeat(tabWidth - 1) + '→'
        const parts = 
            str.split('\t')
        return parts
            .map((x, i) =>
                i < parts.length - 1
                     ? x + '-'.repeat(tabWidth - x.length % tabWidth - 1) + '→'
                     : x
            )
            .join('')
    }
}