> language: "plaintext"

If the wrapping column is set at < 1, it behaves as if it's infinite (everything is
"unwrapped").

    a        ->      a b c
    b c              ¦
    ¦                ¦


If there are several blocks where the prefix on the first line is different,
this must be handled correctly when lines are added to a one-line block

> language: "markdown"

    * - + triple bullet      ->      * - + triple  ¦
                  ¦                        bullet  ¦
