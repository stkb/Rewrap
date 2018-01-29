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
