# Markdown: Headings

> language: markdown


## Setext headings

As little as one `=` or `-` is required. A paragraph can come straight after.

    heading      ¦                            heading text ¦
    text one     ¦                            one          ¦
    =            ¦                    ->      =            ¦
    some         ¦                            some para    ¦
    para text    ¦                            text         ¦

    heading      ¦                            heading text ¦
    text two     ¦                            two          ¦
    -            ¦                    ->      -            ¦
    some         ¦                            some para    ¦
    para text    ¦                            text         ¦


The underline can be indented up to 3 spaces.

    heading      ¦                    ->      heading      ¦
    ···=         ¦                            ···=         ¦

Whitespace after the underline is allowed.

    heading      ¦                    ->      heading      ¦
    =·           ¦                            =·           ¦

Whitespace inbetween underline characters is not allowed.

    heading      ¦                    ->      heading = =  ¦
    = =          ¦                                         ¦

No other characters are allowed.

    heading      ¦                    ->      heading =-=-=¦
    =-=-=        ¦                                         ¦

    heading      ¦                    ->      heading ---  ¦
    --- jkl      ¦                            jkl          ¦


A setext heading underline can't some after a different block, so in this case it must be
treated as normal paragraph text.

    ····indented code block                   ····indented code block
    ===          ¦                    ->      === text     ¦
    text         ¦                                         ¦
