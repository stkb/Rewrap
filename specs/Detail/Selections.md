> language: "plaintext"

Multiple selections on the same line count as just one selection.

    Don't wrap this         ->      Don't wrap this
    «Wrap» «this»¦line              Wrap this¦
    Don't wrap this                 line     ¦
                 ¦                  Don't wrap this

The same as multiple empty selections in the same paragraph.

    Li«»ne1                ¦      ->      Line1 Line2 Line3 Line4
    Line2                  ¦
    Li«»ne3                ¦
    Line4                  ¦

If a paragraph has an empty selection and a non-empty selection, the empty selection takes precedence and the whole paragraph is wrapped

    «Line1»                ¦      ->      Line1 Line2 Line3 Line4
    Line2                  ¦
    Li«»ne3                ¦
    Line4                  ¦
