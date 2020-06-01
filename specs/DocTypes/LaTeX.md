> language: "LaTeX"

Various forms of line-break command will preserve a line break after them.

    a b c d e f     ¦      ->      a b c d e f g h ¦
    g h i j \\      ¦              i j \\          ¦
    aaaaaaa         ¦              aaaaaaa bbbbbbb ¦
    bbbbbbb \\*     ¦              \\*             ¦
    aaaaaaa         ¦              aaaaaaa bbbbbbb ¦
    bbbbbbb \\[2in] ¦              \\[2in]         ¦
    aaaa            ¦              aaaa bbbb       ¦
    bbbb \\newline  ¦              \\newline       ¦
    a               ¦              a b             ¦
    b \\linebreak[4]¦              \\linebreak[4]  ¦
    a               ¦              a               ¦

`\[` and `$$` should, like most other commands, preserve a line break before.

    a b c   ¦      ->      a b c d ¦
    d e     ¦              e       ¦
    \[ \]   ¦              \[ \]   ¦

    a b c   ¦      ->      a b c d ¦
    d e     ¦              e       ¦
    $$ $$   ¦              $$ $$   ¦

Check for breaks before/after verbatim section

    a b c d e f g h   ¦      ->      a b c d e f g h i ¦
    i j               ¦              j                 ¦
    \begin{verbatim}  ¦              \begin{verbatim}  ¦
        a             ¦                  a             ¦
        b             ¦                  b             ¦
    \end{verbatim}    ¦              \end{verbatim}    ¦
    a b c d e f g h   ¦              a b c d e f g h i ¦
    i j               ¦              j                 ¦

Should also work with * variants, eg align*

    a b c d e f g h   ¦      ->      a b c d e f g h i ¦
    i j               ¦              j                 ¦
    \begin{align*}    ¦              \begin{align*}    ¦
        a             ¦                  a             ¦
        b             ¦                  b             ¦
    \end{align*}      ¦              \end{align*}      ¦
    a b c d e f g h   ¦              a b c d e f g h i ¦
    i j               ¦              j                 ¦

The shortcuts `\( \[ $ $$` create a preserved section.

    a b c d e f g h   ¦      ->      a b c d e f g h i ¦
    i j               ¦              j                 ¦
    \(                ¦              \(                ¦
        a             ¦                  a             ¦
        b             ¦                  b             ¦
    \)                ¦              \)                ¦
    a b c d e f g h   ¦      ->      a b c d e f g h i ¦
    i j               ¦              j                 ¦
    \[                ¦              \[                ¦
        a             ¦                  a             ¦
        b             ¦                  b             ¦
    \]                ¦              \]                ¦
    a b c d e f g h   ¦              a b c d e f g h i ¦
    i j               ¦              j                 ¦
    $                 ¦              $                 ¦
        a             ¦                  a             ¦
        b             ¦                  b             ¦
    $                 ¦              $                 ¦
    a b c d e f g h   ¦              a b c d e f g h i ¦
    i j               ¦              j                 ¦
    $$                ¦              $$                ¦
        a             ¦                  a             ¦
        b             ¦                  b             ¦
    $$                ¦              $$                ¦
    a b c d e f g h   ¦              a b c d e f g h i ¦
    i j               ¦              j                 ¦

## End-of-line comments

In general, like for all languages, end-of-line comments are not properly
supported. But a line-break after comment that comes after text on the same line
will be preserved.

    a b c d e f g h   ¦      ->      a b c d e f g h i ¦
    i j % z y x       ¦              j % z y x         ¦
    k l m             ¦              k l m             ¦

Long comments cannot yet be wrapped, however. If too long, they will be put on
the next line.

    a b c d e f g h   ¦          ->      a b c d e f g h i ¦
    i j % z y x w v u t s r              j                 ¦
    k l m                                % z y x w v u t s r
                      ¦                  k l m             ¦
