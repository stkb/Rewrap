> language: "d"

    // a b c       ->      // a b ¦
    // d   ¦               // c d ¦
    /* a b c               /* a b ¦
       d   ¦                  c d ¦
    */     ¦               */     ¦
    /+ a b c               /+ a b ¦
       d   ¦                  c d ¦
    +/     ¦               +/     ¦
    x x x x x              x x x x x

    /// a b c       ->      /// a b ¦
    /// d   ¦               /// c d ¦

> language: "lean"

    -- a b c       ->      -- a b ¦
    -- d   ¦               -- c d ¦
    x x x x x              x x x x x
    /- a b c               /- a b ¦
       d   ¦                  c d ¦
    -/     ¦               -/     ¦
    x x x x x              x x x x x

> language: "matlab"

In MATLAB, comments starting with `%%` are "code section titles", which must not
be wrapped.

    %% Section title      ->      %% Section title
    % a b c d e                   % a b c d
    % f g     ¦                   % e f g   ¦
    x = 0:1:6*pi;                 x = 0:1:6*pi;
    y = sin(x);                   y = sin(x);
    plot(x,y) ¦                   plot(x,y) ¦
              ¦                             ¦
    %{        ¦                   %{        ¦
    a b c d e f g                 a b c d e ¦
    h i       ¦                   f g h i   ¦
    %}        ¦                   %}        ¦

> language: "proto"

    // a b c       ->      // a b ¦
    // d   ¦               // c d ¦
    x x x x x              x x x x x

> language: "python"

    """ ¦      ->      """
    a b c              a b
    d   ¦              c d
    """ ¦              """

    ''' ¦      ->      '''
    a b c              a b
    d   ¦              c d
    ''' ¦              '''

> language: "tcl"

    # a b c      ->      # a b ¦
    # d   ¦              # c d ¦
    z y x w              z y x w
    v u   ¦              v u   ¦

> language: "yaml"

In yaml (like markdown), the content is treated as wrappable text rather than
code. This allows multiline text content to be wrapped if desired.

    short values:   ¦         ->      short values:   ¦
        a: 10       ¦                     a: 10       ¦
        b: 20       ¦                     b: 20       ¦
    long line: >    ¦                 long line: >    ¦
        aaa«» bbb ccc ddd                 aaa bbb ccc ¦
    short lines: >  ¦                     ddd         ¦
        aaa«»       ¦                 short lines: >  ¦
        bbb         ¦                     aaa bbb ccc ¦
        ccc         ¦                     ddd         ¦
        ddd         ¦                                 ¦

Paragraphs are only determined by blank lines or significant differences (>=2)
in indent. Therefore care must be taken to select only the desired paragraphs,
because with normal key: value lines undesirable wrapping will happen. 

    short values: ¦         ->      short values: ¦
        a: 10     ¦                     a: 10 b:  ¦
        b: 20     ¦                     20        ¦

Full support for yaml files (like markdown, latex) may be added in the future.
