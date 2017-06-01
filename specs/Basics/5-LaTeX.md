Line comments in LaTeX begin with a `%`

> language: "latex"

    % one two       ¦      ->      % one two three ¦
    % three four    ¦              % four

Some commands have no content. These are all kept on separate lines

    \begin{abstract}        ¦
    The abstract.           ¦
    \end{abstract}          ¦



A line break can be added with any of these commands:

    aaaaaaa         ¦      ->      aaaaaaa bbbbbbb ¦
    bbbbbbb \\      ¦              \\              ¦
    aaaaaaa         ¦              aaaaaaa bbbbbbb ¦
    bbbbbbb \\*     ¦              \\*             ¦
    aaaaaaa         ¦              aaaaaaa bbbbbbb ¦
    bbbbbbb \\[2in] ¦              \\[2in]         ¦
    aaaa            ¦              aaaa bbbb       ¦
    bbbb \\newline  ¦              \\newline       ¦
    a               ¦              a b             ¦
    b \\linebreak[4]¦              \\linebreak[4]  ¦
    a               ¦              a               ¦
