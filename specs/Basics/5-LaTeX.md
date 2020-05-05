# LaTeX #

> language: "latex"

LaTeX has its own special wrapping behaviors. Both comments and content are
wrapped.

## Line breaks ##

For any command, if it's alone on a line (including args), then line breaks
before and after will be kept.

    \begin{abstract}        ¦      ->      \begin{abstract}        ¦
    The abstract.           ¦              The abstract.           ¦
    \end{abstract}          ¦              \end{abstract}          ¦

Also some commands (eg *item*), will always keep a line break before them.

    \begin{enumerate}      ->      \begin{enumerate}
    \item Item one ¦               \item Item one ¦
    \item Item     ¦               \item Item two ¦
    two            ¦               \item Item     ¦
    \item Item three               three          ¦
    \end{enumerate}¦               \end{enumerate}¦


A line break will also be kept after a line break command (*\\\\*, *\newline*
etc.). Eg:

    a b c d e f     ¦      ->      a b c d e f g h ¦
    g h i j \\      ¦              i j \\          ¦
    a b c d e f     ¦              a b c d e f g h ¦
    g h i \\newline ¦              i \\newline     ¦
    a               ¦              a b             ¦
    b \\linebreak[4]¦              \\linebreak[4]  ¦
    a               ¦              a               ¦


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

End-of-line comments are not supported yet. (They are just wrapped with the text
before them).
