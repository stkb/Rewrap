Markdown documents can be re-wrapped too, and the contents of comments in other types of files is parsed as markdown too.

> language: "markdown"

Long lines are split at whitespace and wrapped.

    A long line.      ->      A long  ¦
            ¦                 line.   ¦

Short lines are concatenated and then wrapped as appropriate.

    Three.        ¦      ->      Three. Short. ¦
    Short.        ¦              Lines.        ¦
    Lines.        ¦

A line break can be forced by ending a line in a single `\` or two spaces.

    Line break\       ¦
    Line break··      ¦
    More text         ¦

## Features ##

Any line without text (used for decoration) is left alone

    ###############      ->      ###############
    ***************              ***************
    ===============              ===============
    Text text¦text               Text text¦
    ---------------              text     ¦
             ¦                   ---------------

ATX headings. The text must be on a single line, so these are left alone too.

    ### Heading style 1 ¦
    ### With trailing #'s ###

However setext headings (with underlines) can be wrapped. But the underline remains unchanged (this is maybe something that could be changed).

    Heading with underline      ->      Heading with         ¦
    ---------------------               underline            ¦
    With double                         ---------------------¦
    underline                           With double underline¦
    ==================   ¦              ==================   ¦

Bullet lists. They can immediately follow a paragraph.

    List items:    ¦              ->      List items:
    - With dash    ¦                      - With dash
    * With star    ¦                      * With star
    + With plus text wrapped              + With plus
                                            text wrapped

Numbered lists

    1) Item 1
    2. Item 2
    999999999) Item 999999999 ¦

Indented code blocks

    ····a = 1;
    ····b = 2;

Fenced code blocks. Can come directly after a paragraph.

    Text       ¦
    ``` c      ¦
    a = 1;     ¦
    b = 2;     ¦
    ```        ¦
    ··~~~ javascript
    a = 1;     ¦
    b = 2;     ¦
    ·~~~       ¦

## Indentation ##

Rewrap has two settings for indentation:
- tidyUpIndents: false (default): The indentation for each paragraph is preserved.
- tidyUpIndents: true: Each paragraph's indent is "fixed" or cleaned up

> language: "markdown", tidyUpIndents: false

Paragraph indents are all left as-is. (Paragraphs can be indented up to 3 spaces. More than that is a code block)

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

(A small exception, because of how Rewrap works, is paragraphs in a list item where lines after the first have are indented less than the first line. These will be "corrected" to the same indent.)

    *·Line one.      ¦      ->      * Line one. Line ¦
    Line two.        ¦                two.           ¦
                     ¦                               ¦
    ··Line one.      ¦              ··Line one. Line ¦
    ·Line two.       ¦                two.           ¦


> language: "markdown", tidyUpIndents: true

With tidyUpIndents: true, indents are tidied-up and reduced as much as possible.

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

With list items specifically, the whitespace between the bullet marker and the item content is reduced to 1 space.
The bullet markers for sub-items are lined-up with this content.

    ·*··Item       ¦      ->      *·Item
    ······* Subitem¦              ··* Subitem

However this can cause undesired alignment in the case of numbered items (this could
maybe be fixed in the future).

    9) Item 9   ¦      ->      9)·Item 9
    10) Item 10 ¦              10) Item 10


