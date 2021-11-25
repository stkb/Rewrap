# Markdown: Link reference definitions

> language: "markdown"

This doesn't precisely follow the [link reference definition
specification](https://spec.commonmark.org/current/#link-reference-definitions), but
should be a "good enough" implementation

A link reference definition, like all markdown paragraphs, can be indented up to 3 spaces.
It has the pattern `[<label>]:` where `<label>` is characters containing at least 1
non-whitespace character.

If a single long line is wrapped, created lines are indented 4 spaces, to match
[footnotes](footnotes.md).

    [link]: /url "link title"          ->     [link]: /url    ¦
                    ¦                             "link title"¦


Link reference definitions do **not** interrupt a paragraph; ie. there must be a blank
line between one and a normal paragraph. If there's no blank line it'll be treated as part
of the paragraph.

    paragraph        ¦                        paragraph text   ¦
    text [link]      ¦                 ->     [link] [link]:   ¦
    [link]: some_url ¦                        some_url         ¦


LDRs consist of just a single paragraph, and no blank line is required between multiple
LRDs.

    text [one]         ¦                      text [one] text    ¦
    text [two]         ¦                      [two]              ¦
                       ¦               ->                        ¦
    [one]: url1 description                   [one]: url1        ¦
      [two]: url2      ¦                          description    ¦
                                                [two]: url2      ¦


LRDs are terminated by any of the normal paragraph-interrupting blocks, like fenced code
block or list item.

    [one]:    ¦                               [one]: url¦
     url      ¦                               - list    ¦
    - list item                                 item    ¦
              ¦                                         ¦
    [two]:    ¦                        ->     [two]: url¦
        url   ¦                               ``` js    ¦
    ``` js    ¦                               let i;    ¦
    let i;    ¦                               ```       ¦
    ```       ¦                                         ¦
