# LaTeX #

LaTeX has its own special wrapping behaviors. In LaTeX all content is considered
equal when it comes to selections; selecting a whole document will wrap comments
and other content alike.

> language: "latex"

Line comments begin with a `%`

    % one two       ¦      ->      % one two three ¦
    % three four    ¦              % four

Most commands will begin a new paragraph for wrapping.

Some commands have no content. These are all kept on separate lines

    \begin{abstract}        ¦      ->      \begin{abstract}        ¦
    The abstract.           ¦              The abstract.           ¦
    \end{abstract}          ¦              \end{abstract}          ¦

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
