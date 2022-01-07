> language: "markdown"

Some markdown implementations support a *Yaml header*: an extra section at the top of the
file that starts and ends with lines with triple dashes (`---`). For example
[DocFX](https://dotnet.github.io/docfx/spec/docfx_flavored_markdown.html) and
[Assemble](https://assemble.io/docs/YAML-front-matter.html).

Nothing is wrapped in the yaml header (this could possibly be changed to the yaml parser
later).

    ---    ¦      ->      ---    ¦
    a:     ¦              a:     ¦
    b: c   ¦              b: c   ¦
    foo: bar              foo: bar
    ---    ¦              ---    ¦
    xx     ¦              xx yy  ¦
    yy zz  ¦              zz     ¦

A yaml list should stay intact.

    ---           ¦               ---           ¦
    files:        ¦               files:        ¦
        - 00-zero.md                  - 00-zero.md
        - 01-one.md       ->          - 01-one.md
    ---           ¦               ---           ¦

The yaml header start on the first line of the document. If it doesn't then it will be
interpreted differently.

    Text            ¦               Text            ¦
                    ¦                               ¦
    ---             ¦               ---             ¦
    Not a yaml header.      ->      Not a yaml      ¦
    Actually a      ¦               header. Actually
    setext heading                  a setext heading
    ---             ¦               ---             ¦
