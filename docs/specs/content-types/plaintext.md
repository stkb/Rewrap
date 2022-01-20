# PlainText

`plaintext` is the most basic content type. Paragraphs are only separated by blank lines.
This documents also details basic wrapping behavior.

> language: "plaintext"

Long lines are split at whitespace and wrapped.

    A long line.      ->      A long  ¦
            ¦                 line.   ¦

Short lines are concatenated and then wrapped as appropriate.

    Three.        ¦      ->      Three. Short. ¦
    Short.        ¦              Lines.        ¦
    Lines.        ¦

Words that are longer than the wrapping width (like URLs) will be put on a new
line but won't be broken up.

    Go to www.example.com      ->      Go to    ¦
             ¦                         www.example.com

Paragraphs are separated by blank lines.

    Foo bar baz.      ->      Foo bar  ¦
             ¦                baz.     ¦
    Foo      ¦                         ¦
    bar.     ¦                Foo bar. ¦

The indent of all lines is kept as is was:

    one two three                             one two     ¦
      four five six                   ->        three four¦
     seven eight¦                              five six   ¦
                ¦                              seven eight¦

## PlainText-IndentSeparated

With the `plaintext-indentseparated` processor, a difference in line indent *does* start a
new paragraph.

> language: plaintext-indentseparated

    Foo bar baz.                              Foo bar   ¦
     Foo      ¦                       ->      baz.      ¦
     bar.     ¦                                Foo bar. ¦

    Paragraph     ¦                           Paragraph one.¦
    one.          ¦                            Paragraph    ¦
     Paragraph two.                   ->       two.         ¦
    Paragraph three.                          Paragraph     ¦
                  ¦                           three.        ¦

Also works with tab characters. In the case of mixed spaces and tabs, it's the *indent
width* that counts, not the actual characters.

> language: plaintext-indentseparated, tabWidth: 2

    Paragraph one.  ¦                         Paragraph one.  ¦
    -→Paragraph     ¦                 ->      -→Paragraph two.¦
    ··two.          ¦                         ·Paragraph      ¦
    ·Paragraph three.                         ·three.         ¦
