> language: "markdown"

Footnotes are not to be confused with [link reference definitions](linkrefdefs.md).

Footnotes are not in the CommonMark spec, but are supported by a variety of other flavors
of markdown, including GFM, (PHP) Markdown Extra, MultiMarkdown and Pandoc. These all have
slight differences, so until Rewrap offers support for multiple markdown specs, it tries
to cover everything as best it can.

A footnote is a section beginning with `[^<label>]:`, where `<label>` is any sequence of
non-whitespace characters, excluding `]`

`[^fn1]: Footnote`

Since following paragraphs in the footnote must be indented by at least 4 spaces (see below), a
when a single-line footnote is wrapped, created lines are also indented 4 spaces.

    [^1]: foot note                    ->     [^1]: foot ¦
               ¦                              ····note   ¦

Does a footnote interrupt a paragraph? Pandoc says no but GFM, PHP Markdown, and
MultiMarkdown say yes So we allow it.

    text       ¦                              text [^1]  ¦
    [^1] text  ¦                       ->     text       ¦
    [^1]: foot note                           [^1]: foot ¦
               ¦                                  note   ¦

Similarly footnotes can be on consecutive lines

    text        ¦                             text [^1]   ¦
    [^1] [^2]   ¦                             [^2]        ¦
    [^1]: 1     ¦                      ->     [^1]: 1     ¦
    [^2]: 2     ¦                             [^2]: 2     ¦

Subsequent paragraphs within the footnote must be indented 4-7 spaces. 8 spaces or more
becomes an indented code block

    [^fn1]: text text                         [^fn1]: text ¦
                 ¦                                text     ¦
    ····text     ¦                     ->                  ¦
    ····text     ¦                            ····text text¦
                 ¦                                         ¦
    ········code block                        ········code block

Unlike [list items](lists.md), there can be any amount of whitespace between the footnote
label and its content; the indent isn't significant. Therefore it's not possible to start
an indented code block on the first line of a footnote.

    [^fn1]:         > block quote      ->     [^fn1]:         > block ¦
                            ¦                     > quote             ¦
