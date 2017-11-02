# Specs #

The files in these folders use examples to document Rewrap's capabilities and
specify expected results.

The examples also serve as tests.

#### Test layout ####

Each test, set into an indented code block, consists of 2 (or 3) columns, with
the the input in the 1st column separated from the expected output in the 2nd
column by the marker `->`. Some tests have a 3rd column, with the 2nd & 3rd
separated by the marker `-or-`. In these cases, the 2nd column is for the
expected output with the setting `reformat` set to `false`, and the 3rd with it
set to `true`.

#### Wrapping column ####

The wrapping column in each sample is represented by `¦` characters. This must
appear on at least one line but doesn't need to be on all lines. The column
should appear on both input and output sections for clarity.

#### Selections ####

Most samples are treated as if they're wholly selected, though where selections
are explicitly needed, they're marked with `«»` characters, with `«` as the
anchor point and `»` as the active point. These are removed from the input.

#### Whitespace ####

Tabs are represented by `-→`. Where explicitly needed, spaces are represented by
a `·` (U+00B7). All lines are assumed to have no trailing whitespace unless
these characters are used.

#### Settings ####

Settings for a test are given in a blockquote, on one line, separated by commas
and colons. They apply to all subsequent tests until a new settings block is
given. Eg:

> tabSize: 2, doubleSentenceSpacing: true

When absent, the default values are used:
- tabSize: 4
- doubleSentenceSpacing: false
- language: "plaintext"
- wrapWholeComment: true
- reformat: false

A new settings block completely replaces the previous one; any absent values are
reset back to defaults.