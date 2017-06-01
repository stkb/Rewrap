## Plain text

Tabs in the indent are preserved.

As long as tab stops represent more that 1 space in the editor, a  single tab character will separate paragraphs by indent.

> language: "plaintext", tabWidth: 2

    -→a b c       ->      -→a b¦
    -→d  ¦                -→c d¦
    -→-→e f               -→-→e¦
         ¦                -→-→f¦

## Comments ##

Tabs can be used as part of the "prefix" (whitespace around the comment marker).
The width of the line is calculated using the current tab width setting in the
editor, making the result look the same as if spaces had been used.

> language: "c", tabWidth: 2

    ····// text text text      ->      ····// text text   ¦
        // text        ¦                   // text text   ¦
                       ¦                                  ¦
    -→-→// text text text      ->      -→-→// text text   ¦
    -→-→// text        ¦               -→-→// text text   ¦

> language: "c", tabWidth: 4

    ········// text text text      ->      ········// text text   ¦
            // text        ¦                       // text text   ¦
                           ¦                                      ¦
    ---→---→// text text text      ->      ---→---→// text text   ¦
    ---→---→// text        ¦               ---→---→// text text   ¦

Tabs between comment marker and text:

    ---→//-→text text text       ->      ---→//-→text text
    ---→//-→text        ¦                ---→//-→text text


### Odd cases ###

It also works with a mix of tabs and spaces on one line

    ··-→·// text text text      ->      ··-→·// text text   ¦
                        ¦               ··-→·// text        ¦

Tabs in the comment content can make things tricky, and therefore all tabs
that appear after the column where text begins are converted to spaces when wrapping.

    ---→//·text text text      ->      ---→//·text text   ¦
    ---→//-→text      ¦                ---→//··text text   ¦

### Markdown ###

Because tab characters in the middle of a line could produce weird results when
wrapping, all tabs are replaced with the correct number of spaces when wrapping.
This only happens on lines that are wrapped.