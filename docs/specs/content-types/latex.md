# LaTeX

> language: "latex"

LaTeX has its own special wrapping behaviors. Both comments and content are
wrapped.

## Line breaks ##

For any command, if it's alone on a line (including args), then line breaks
before and after will be kept.

    \begin{abstract}        ¦      ->      \begin{abstract}        ¦
    The abstract.           ¦              The abstract.           ¦
    \end{abstract}          ¦              \end{abstract}          ¦

Args can be in `{}` or `[]` brackets, and there can be any number in any order.

    \begin{definition}[term]        ¦      ->      \begin{definition}[term]        ¦
    The                             ¦              The definition
    definition                      ¦

    \newtheorem{name}[counter]{Printed output}    ->      \newtheorem{name}[counter]{Printed output}
    Text                             ¦                    Text text                        ¦
    text                             ¦

Also some commands (eg *item*), will always keep a line break before them.

    \begin{enumerate}      ->      \begin{enumerate}
    \item Item one ¦               \item Item one ¦
    \item Item     ¦               \item Item two ¦
    two            ¦               \item Item     ¦
    \item Item three               three          ¦
    \end{enumerate}¦               \end{enumerate}¦

`\[` and `$$` should, like most other commands, preserve a line break before.

    a b c   ¦      ->      a b c d ¦
    d e     ¦              e       ¦
    \[ \]   ¦              \[ \]   ¦

    a b c   ¦      ->      a b c d ¦
    d e     ¦              e       ¦
    $$ $$   ¦              $$ $$   ¦


A line break will also be kept after a line break command (*\\\\*, *\newline*
etc.). Eg:

    a b c d e f     ¦              a b c d e f g h ¦
    g h i j \\      ¦              i j \\          ¦
    a b c d e f     ¦              a b c d e f g h ¦
    g h i \\newline ¦              i \\newline     ¦
    a               ¦              a b             ¦
    b \\linebreak[4]¦      ->      \\linebreak[4]  ¦
    aaaaaaa         ¦              aaaaaaa bbbbbbb ¦
    bbbbbbb \\*     ¦              \\*             ¦
    aaaaaaa         ¦              aaaaaaa bbbbbbb ¦
    bbbbbbb \\[2in] ¦              \\[2in]         ¦

## Preserved sections ##

Anything within a *verbatim* environment is preserved without
wrapping. Text before/after it is wrapped normally.

    Normal text       ¦      ->      Normal text normal¦
    normal text.      ¦              text.             ¦
    \begin{verbatim}  ¦              \begin{verbatim}  ¦
         Verbatim text                    Verbatim text
        preserved as-is                  preserved as-is
    \end{verbatim}    ¦              \end{verbatim}    ¦
    Normal text       ¦              Normal text normal¦
    normal text.      ¦              text.             ¦

This also applies to *alltt*, and source code environments *listing* and
*lstlisting*, as well as all * variants. Note: to get this behavior, the
`\begin{...}` command must be alone on a line and not inline within a paragraph.

    a b c d e f g h   ¦      ->      a b c d e f g h i ¦
    i j               ¦              j                 ¦
    \begin{align*}    ¦              \begin{align*}    ¦
        a             ¦                  a             ¦
        b             ¦                  b             ¦
    \end{align*}      ¦              \end{align*}      ¦
    a b c d e f g h   ¦              a b c d e f g h i ¦
    i j               ¦              j                 ¦

The shortcuts `\( \[ $ $$` also create a preserved section.

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


### Math sections ###

The above also applies to math environments: *math*, *displaymath*, *equation*,
*align*, *gather*, *multline*; as well as shortcuts `\(...\)`, `$...$`,
`\[...\]`, `$$...$$`.

Used within a paragraph, they are wrapped normally.

    The well-known Pythagorean      ->      The well-known        ¦
    theorem is $a^2 + b^2 =                 Pythagorean theorem is¦
    c^2$.                 ¦                 $a^2 + b^2 = c^2$.    ¦

But with the opening section marker alone on a line, the section is preserved.

    One of the double angle      ->      One of the double   ¦
    formulas is:        ¦                angle formulas is:  ¦
    $                   ¦                $                   ¦
        \cos (2\theta) =                     \cos (2\theta) =
            \cos^2 \theta                        \cos^2 \theta
          - \sin^2 \theta                      - \sin^2 \theta
    $                   ¦                $                   ¦


## Comments ##

Line comments begin with a `%`

    % one two       ¦      ->      % one two three ¦
    % three four    ¦              % four          ¦

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
