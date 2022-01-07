# reStructuredText

reStructuredText (RST or reST) is used for .rst files and docstrings in Python.

> language: reStructuredText

These wrapping behaviors were created from [the official RST spec](
https://docutils.sourceforge.io/docs/ref/rst/restructuredtext.html) and the Online RST
Editor at http://rst.ninjs.org

When source text doesn't follow the spec correctly, RST processors can sometimes emit
warnings (where output is produced anyway) and errors (where the text is malformed and no
output is produced). For the purposes of wrapping, warnings are simply ignored and where
there would be an error, the source text is left as-is and not wrapped.


## Paragraphs

Paragraphs are separated by blank lines.

    paragraph one                             paragraph ¦
              ¦                        ->     one       ¦
    para      ¦                                         ¦
    two       ¦                               para two  ¦

In almost all cases lines in a paragraph must all have the same indent. A difference in
indent denotes a new block in some way.

reStructuredText has no indented code block. Any indent denotes a block quote, and further
indent a new block quote, although there must be a blank line before.

    ····block          ¦                      ····block quote    ¦
    ····quote text     ¦                      ····text           ¦
                       ¦               ->                        ¦
    ········further indented                  ········further    ¦
    ········block      ¦                      ········indented   ¦
    ········quote      ¦                      ········block quote¦

If a line in a paragraph is indented less than previous ones, RST considers
it a new paragraph (though would give a warning).

    ····Para text       ¦                     ····Para text       ¦
    ··Less indented     ¦              ->     ··Less indented text¦
    ··text              ¦                                         ¦


## Section titles

([details](transitions-section-titles.md))

Section titles are made up of one line of text with an underline and possible overline of
punctuation characters.

All text in the title must be on the same line. Because of this, section titles are never
wrapped. Where there are more than two lines, it will be treated as a normal paragraph,
with the underline characters as part of the text.

    a long section title               ->     a long section title
    ====================                      ====================
                   ¦                                         ¦
    not a          ¦                          not a section  ¦
    section        ¦                   ->     title =======  ¦
    title          ¦                                         ¦
    =======        ¦                                         ¦


## Transitions

([details](transitions-section-titles.md))

Transitions are horizontal separators created by 4 or more punctuation characters. There
must be no indent and there must be a blank line before and after do avoid confusion with
section titles.

    Transition markers are                    Transition markers  ¦
    left untouched      ¦                     are left untouched  ¦
                        ¦                                         ¦
    --------------------------         ->     --------------------------
                        ¦                                         ¦
    """"""""""""""""""""""""""                """"""""""""""""""""""""""
                        ¦                                         ¦
    **************************                **************************


## Bullet lists

([details](bullet-lists.md))

Bullet lists are created with one of the markers `* + - • ‣ ⁃`.

A single line will wrap so text is at the same indent

    • bullet item text                 ->     • bullet item ¦
                  ¦                             text        ¦


## Enumerated lists

([details](enumerated-lists.md))

Enumerated lists have either numerals, letters, or roman numerals with the form `X.`, `X)`
or `(X)`

    1. item one text                          1. item one ¦
    2. item     ¦                      ->        text     ¦
       two      ¦                             2. item two ¦

    (a) item one text                          (a) item one ¦
    (b) item     ¦                      ->         text     ¦
        two      ¦                             (b) item two ¦


## Field lists

([details](field-lists.md))

Field list items have the form `:<field name>: <field body>`. The line break after each
item is preserved and the body content is wrapped. Extra lines of the body content must
all be indented by the same amount, though the indent can be as little as 1 space.

    :Date: 2001-08-16          ¦              :Date: 2001-08-16          ¦
    :Version: 1                ¦              :Version: 1                ¦
    :Authors: - Me             ¦              :Authors: - Me             ¦
              - Myself         ¦       ->               - Myself         ¦
              - I              ¦                        - I              ¦
    :Abstract: Lorem ipsum lorem              :Abstract: Lorem ipsum     ¦
     ipsum lorem ipsum.        ¦               lorem ipsum lorem ipsum.  ¦


## Option lists

Not supported yet


## Literal blocks

([details](literal-blocks.md))

Literal blocks are used for code samples or anything that mustn't be wrapped. A literal
block follows any line that ends with `::`.

    ::            ¦                           ::            ¦
                  ¦                                         ¦
        code      ¦                               code      ¦
        block     ¦                               block     ¦
                  ¦                                         ¦
        not wrapped                    ->         not wrapped
                  ¦                                         ¦
    normal paragraph                          normal        ¦
                  ¦                           paragraph     ¦

    Example:: ¦                               Example:: ¦
              ¦                                         ¦
        code block                     ->         code block
              ¦                                         ¦
    normal paragraph                          normal    ¦
              ¦                               paragraph ¦


## Line blocks

Line blocks are a way of preserving line breaks. When a line begins with "|" + whitespace,
the line-break before it will be preserved, though the rest of the line can be wrapped.

    | Line one     ¦                   ->     | Line one     ¦
    | Line two     ¦                          | Line two     ¦

A long line is wrapped with the text indent of created lines matching that of the first.

    | A too-long line                         | A too-long    ¦
    |   Another long line              ->       line          ¦
                    ¦                         |   Another long¦
                    ¦                             line        ¦

Line blocks cannot contain other elements such as bullet lists or literal blocks.


## Doctest blocks

([details](doctest-blocks.md))

Doctest blocks are intended for interactive Python sessions cut-and-pasted into
documentation. They start with `>>> ` and all following lines in the block are not
wrapped.

    This is an          ¦                     This is an ordinary ¦
    ordinary paragraph. ¦                     paragraph.          ¦
                        ¦              ->                         ¦
    >>> print 'this is a Doctest block'       >>> print 'this is a Doctest block'
    this is a Doctest block                   this is a Doctest block


## Tables

([details](tables.md))

No wrapping is attemped on tables. Tables can take two forms: "grid tables"

    +---+---+---+            ¦                +---+---+---+            ¦
    |   | T | F |            ¦                |   | T | F |            ¦
    +===+===+===+            ¦                +===+===+===+            ¦
    | T | F | T |            ¦         ->     | T | F | T |            ¦
    +---+---+---+            ¦                +---+---+---+            ¦
    | F | T | F |            ¦                | F | T | F |            ¦
    +---+---+---+            ¦                +---+---+---+            ¦

And "simple tables".

    === === ===             ¦                 === === ===             ¦
         T   F              ¦                      T   F              ¦
    === === ===             ¦                 === === ===             ¦
     T   F   T              ¦         ->       T   F   T              ¦
     F   T   F              ¦                  F   T   F              ¦
    === === ===             ¦                 === === ===             ¦


## Explicit markup blocks

Explicit markup blocks cover footnotes, citations, hyperlink targets, directives,
substitution definitions, and comments. They are denoted by a block that starts with `..`
followed by whitespace.


### Footnotes and citations

These can contain body content

The footnote content (body elements) must be consistently indented and left-aligned. The
first body element within a footnote may often begin on the same line as the footnote
label. However, if the first element fits on one line and the indentation of the remaining
elements differ, the first element must begin on the line after the footnote label.
Otherwise, the difference in indentation will not be detected.

    .. [*] This is a footnote          ->     .. [*] This is a   ¦
                       ¦                          footnote       ¦

    .. [#] - A footnote       ¦               .. [#] - A footnote       ¦
           - With bullets     ¦                      - With bullets     ¦
    .. [#note.1] A            ¦        ->     .. [#note.1] A named      ¦
      named footnote          ¦                 footnote                ¦
    .. [GVR-2001] This is     ¦               .. [GVR-2001] This is a   ¦
                  a citation  ¦                             citation    ¦


### Hyperlink targets

These aren't wrapped for now.

Explicit and implicit

    .. link_1: https://longer.url             .. link_1: https://longer.url
    .. __: a.com             ¦         ->     .. __: a.com             ¦
    __ https://c.d           ¦                __ https://c.d           ¦


### Directives

These aren't wrapped for now.

    .. meta::           ¦                     .. meta::           ¦
     :keywords: key words              ->      :keywords: key words
     :description: description                 :description: description
        text            ¦                         text            ¦


### Substitution definitions

These aren't wrapped for now

    .. |pic| image:: pic.png           ->     .. |pic| image:: pic.png
                         ¦                                         ¦


### Comments

These aren't wrapped for now

    .. Anything else that's written           .. Anything else that's written
       in a block           ¦          ->        in a block           ¦
       after a ".."         ¦                    after a ".."         ¦
