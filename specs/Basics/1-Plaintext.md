# Basics

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

Or by a significant difference in indent (2 characters or more). This only
applies to the default plain text type.

    Foo bar baz.      ->      Foo bar   ¦
      Foo     ¦               baz.      ¦
      bar.    ¦                 Foo bar.¦

Also works with tab characters.

## Odd cases ##

Tab characters in the middle of a paragraph could cause improper wrapping.