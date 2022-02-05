# Specs

The files in these folders use examples to document Rewrap's capabilities and specify
expected results. They are all in markdown format.

The examples also serve as tests.


## Test layout

    An example test                   ->      An example ¦
               ¦                              test       ¦

Each test, set into an indented code block, consists of 2 sections, or columns, with the input
on the left and and the expected output after the `->` marker.

### Wrapping column

The wrapping column in each sample is represented by `¦` character. It's required to appear
on at least one line in the input and output sections (though for asthetics should appear
on all lines where it can). It's checked that all `¦` markers are at the same column.

### Selections

Most samples are treated as if they're wholly selected, though where selections are
explicitly needed, they're marked with `«»` characters, with `«` as the anchor point and
`»` as the active point. These are removed from the input.

### Whitespace

Tabs are represented by `-→`. Where explicitly needed, spaces are represented by a `·`
(U+00B7). All lines are assumed to have no trailing whitespace unless these characters are
used.

## Settings

Settings for a test are given in a blockquote, on one line, separated by commas and
colons. They apply to all subsequent tests until a new settings block is given. Eg:

> language: javascript, tabSize: 2, doubleSentenceSpacing: true

When absent, the default values are used:
- tabSize: 4
- doubleSentenceSpacing: false
- language: plaintext
- wrapWholeComment: true
- reformat: false

A new settings block completely replaces the previous one; any absent values are reset
back to defaults.


## Writing tests

If writing tests in VS Code, the first thing that's highly recommended is the [Overtype
](https://marketplace.visualstudio.com/items?itemName=DrMerfy.overtype) extension, which
gives a toggle to type in overtype mode with the 'ins' key.

This project folder also has extra guideline rulers at columns 38 & 46 in markdown files,
as well as a couple of code snipets.

Steps:

1. Press ctrl+space and select the `test` snippet. This will insert lines of spaces and
   put the cursor at the correct starting position.
2. Using overtype mode, add the input text. Use the cursor keys to move down a line rather
   than enter.
3. Add `¦` characters on all lines to indicate the wrapping column (you can copy this char
   from another test or use the `wm` snippet). You can do this quicker by first using
   ctrl+alt+down to make an insertion point on each line.
4. Select all the input as a block by using ctrl+alt+up/down to select vertically, and
   shift+left/right to select horizontally. Copy, press End to move the insertion points
   to the end of each line (should be on col 47, at the 2nd ruler) and paste.
5. Adjust input & output text as needed.
6. On one of the lines, add a `->`  marker (dash + greater than) at col 39 (the first
   ruler). This can be on any line but for asthetics should be on/near the middle line of
   the block

### Additional notes/tips

- Though not outright banned, tests with identical input and output are discouraged, since
  something not working at all will produce a false pass. In tests where a non-wrapping
  block is expected, try to include another block that should wrap (eg. normal paragraph
  text).
- Tests are easier to see (and edit) if input and output contain the same amount of lines.
  When adding wrapped sections, try to add lines that will be split and lines that will be
  joined, to keep the number of lines after wrapping (almost) the same.
- When running tests, examples can be singled-out by adding the tag `<only>` at the end of
  any of their lines.
