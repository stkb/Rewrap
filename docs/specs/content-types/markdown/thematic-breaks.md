# Markdown: Thematic Breaks

> language: markdown

Thematic break lines are not be wrapped. They require at least 3 characters of `*` `-` or
`_`.

    ***               ¦                       ***               ¦
    ---               ¦               ->      ---               ¦
    ___               ¦                       ___               ¦

A paragraph can come immediately after.

    ***               ¦                       ***               ¦
    paragraph         ¦               ->      paragraph text    ¦
    text              ¦                                         ¦

Can be any length >= 3 and can contain spaces inbetween.

    *** *** *** *** ***                       *** *** *** *** ***
    paragraph         ¦               ->      paragraph text    ¦
    text              ¦                                         ¦

The marker 3 characters don't have to be consecutive. As long as there are at least 3.

    -  -  -           ¦                       -  -  -           ¦
    paragraph         ¦               ->      paragraph text    ¦
    text              ¦                                         ¦

Trailing spaces are allowed.

    ____·             ¦                       ____·             ¦
    paragraph         ¦               ->      paragraph text    ¦
    text              ¦                                         ¦

Up to 3 leading spaces are allowed (more and it's an indented code block instead, though
that makes no difference to wrapping).

    ···----           ¦                       ···----           ¦
    paragraph         ¦               ->      paragraph text    ¦
    text              ¦                                         ¦

Can interrupt a paragraph. If `---` is used, it would be a setext heading instead, though
that makes no difference to wrapping.

    some              ¦               ->      some paragraph    ¦
    paragraph text    ¦                       text              ¦
    ***               ¦                       ***               ¦
    and some          ¦                       and some more     ¦
    more              ¦                                         ¦


## Invalid cases

In these cases the break is invalid so will be wrapped into the paragraph instead:

    --                ¦                       -- too few        ¦
    too few           ¦               ->      characters        ¦
    characters        ¦                                         ¦

    *-*               ¦                       *-* characters not¦
    characters not    ¦               ->      the same          ¦
    the same          ¦                                         ¦

    ___ foo           ¦                       ___ foo extra     ¦
    extra characters  ¦               ->      characters after  ¦
    after             ¦                                         ¦

    ===               ¦                       === wrong         ¦
    wrong             ¦               ->      characters        ¦
    characters        ¦                                         ¦
