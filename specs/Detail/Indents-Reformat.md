# Indents & Reformat #

## Markdown ##

Rewrap has two settings for indentation:
- reformat: false (default): The indentation for each paragraph is
  preserved.
- reformat: true: Each paragraph's indent is "fixed" or reformatted.

> language: "markdown", reformat: false

Paragraph indents are all left as-is. (Paragraphs can be indented up to 3
spaces. More than that is a code block)

    ·First          ¦      ->      ·First paragraph ¦
     paragraph      ¦                               ¦
                    ¦              ···Second        ¦
    ···Second paragraph            ···paragraph     ¦
                    ¦                               ¦
    ·*  Item 1      ¦              ·*  Item 1       ¦
    ···* Item 2     ¦              ···* Item 2      ¦
    ······* Subitem 2.1            ······* Subitem  ¦
                    ¦                      2.1      ¦
            ·Para 2 ¦                               ¦
                    ¦                      ·Para 2  ¦

(A small exception, because of how Rewrap works, is paragraphs in a list item,
where lines after the first are "unindented". These will be "corrected" to the
same indent.)

    *·Line one.      ¦      ->      * Line one. Line ¦
    Line two.        ¦                two.           ¦
                     ¦                               ¦
    ··Line one.      ¦              ··Line one. Line ¦
    ·Line two.       ¦                two.           ¦


> language: "markdown", reformat: true

With reformat: true, indents are tidied-up and reduced as much as possible.

    ·First          ¦      ->      First paragraph  ¦
     paragraph      ¦                               ¦
                    ¦              Second paragraph ¦
    ···Second paragraph                             ¦
                    ¦              * Item 1         ¦
    ·*··Item 1      ¦              * Item 2         ¦
    ···* Item 2     ¦              ··* Subitem 2.1  ¦
    ······* Subitem 2.1                             ¦
                    ¦                  Para 2       ¦
            ·Para 2 ¦                               ¦

With list items specifically, the whitespace between the bullet marker and the
item content is reduced to 1 space. The bullet markers for sub-items are
lined-up with this content.

    ·*··Item       ¦      ->      *·Item
    ······* Subitem¦              ··* Subitem

However this can cause undesired alignment in the case of numbered items (this
could maybe be fixed in the future).

    9) Item 9   ¦      ->      9)·Item 9
    10) Item 10 ¦              10) Item 10


## Comments ##

This is still under consideration. With `reformat` on, the basic indent
after all comment markers will be reduced to 1 space.

> language: "csharp", reformat: true

    //   a ¦     ->      // a   ¦

### Block comments ###

With block comments, the defaults for reformatting are 1 space after the marker
on the first line, and no indent for following lines.

> language: "xml", reformat: false

    <!--a b  ¦       ->      <!--a b c¦
        c d -->                  d -->¦

> language: "xml", reformat: true

    <!--a b  ¦       ->      <!-- a b ¦
        c d -->              c d -->  ¦