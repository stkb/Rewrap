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
