> language: "yaml"

In yaml (like markdown), the content is treated as wrappable text rather than
code. This allows multiline text content to be wrapped if desired.

    short values:   ¦         ->      short values:   ¦
        a: 10       ¦                     a: 10       ¦
        b: 20       ¦                     b: 20       ¦
    long line: >    ¦                 long line: >    ¦
        aaa«» bbb ccc ddd                 aaa bbb ccc ¦
    short lines: >  ¦                     ddd         ¦
        aaa«»       ¦                 short lines: >  ¦
        bbb         ¦                     aaa bbb ccc ¦
        ccc         ¦                     ddd         ¦
        ddd         ¦                                 ¦

Paragraphs are only determined by blank lines or significant differences (>=2)
in indent. Therefore care must be taken to select only the desired paragraphs,
because with normal key: value lines undesirable wrapping will happen. 

    short values: ¦         ->      short values: ¦
        a: 10     ¦                     a: 10 b:  ¦
        b: 20     ¦                     20        ¦

Full support for yaml files (like markdown, latex) may be added in the future.
