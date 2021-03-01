# Indents & Reformat #

## Markdown ##

Rewrap has two settings for indentation:
- reformat: false (default): The indentation for each paragraph is
  preserved.
- reformat: true: Each paragraph's indent is "fixed" or reformatted.

> language: "markdown"

This example compares reformat off (left) and on (right)

    ·First          ¦      ->      ·First paragraph¦    -or-    First paragraph ¦
     paragraph      ¦                              ¦                            ¦
                    ¦              ···Second       ¦            Second paragraph¦
    ···Second paragraph            ···paragraph    ¦                            ¦
                    ¦                              ¦            * Item 1        ¦
    ·*  Item 1      ¦              ·*  Item 1      ¦            * Item 2        ¦
    ···* Item 2     ¦              ···* Item 2     ¦            ··* Subitem 2.1 ¦
    ······* Subitem 2.1            ······* Subitem ¦                            ¦
                    ¦                      2.1     ¦                Para 2      ¦
            ·Para 2 ¦                              ¦                            ¦
                    ¦                      ·Para 2 ¦                            ¦

Indented code blocks, where indented more than 4 spaces, are reduced to a
4-space indent.

    ······Indented code    ->      ······Indented code   -or-    ····Indented code
    ·······block    ¦              ·······block    ¦             ·····block      ¦

With fenced code blocks, like normal paragraphs, any indent is removed from the
lines with ```. Relative indents of the content are preserved.

    ··```         ¦      ->      ··```         ¦     -or-     ```           ¦
       Fenced     ¦                 Fenced     ¦               Fenced       ¦
        code block¦                  code block¦                code block  ¦
    ···```        ¦              ···```        ¦              ```           ¦

    ··```         ¦      ->      ··```         ¦     -or-     ```           ¦

With list items, the whitespace between the bullet marker and the
item content is reduced to 1 space. The bullet markers for sub-items are
lined-up with this content.

> language: "markdown", reformat: true

    ·*··Item       ¦      ->      *·Item
    ······* Subitem¦              ··* Subitem

However this can cause undesired alignment in the case of numbered items (this
could maybe be fixed in the future).

    9) Item 9   ¦      ->      9)·Item 9
    10) Item 10 ¦              10) Item 10

Also one thing to bear in mind, is that some things are always corrected even when
reformat is off. One example is list items where the second line is indented
less than the first. These will always be moved up to the same indent.

> language: "markdown", reformat: false

    *·Line one.      ¦      ->      * Line one. Line ¦
    Line two.        ¦                two.           ¦
                     ¦                               ¦
    ··Line one.      ¦              ··Line one. Line ¦
    ·Line two.       ¦                two.           ¦


## Comments ##

This is still under consideration. With `reformat` on, the basic indent after
all comment markers will be reduced to 1 space.

> language: "csharp", reformat: true

    //   a ¦     ->      // a   ¦

### Block comments ###

With block comments, the defaults for reformatting are 1 space after the marker
on the first line, and no indent for following lines.

> language: "xml"

    <!--a b  ¦       ->      <!--a b c¦    -or-    <!-- a b ¦
        c d -->                  d -->¦            c d -->  ¦
