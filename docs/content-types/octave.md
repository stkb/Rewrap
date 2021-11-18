> language: "octave"

References:
- https://octave.org/doc/v5.2.0/Comments.html#Comments
- https://wiki.octave.org/Octave_style_guide#Comments

Line comments with `%` or `#`:

    x x x x x              x x x x x
    % a b c       ->       % a b ¦
    % d   ¦                % c d ¦
    x x x x x              x x x x x

    x x x x x              x x x x x
    # a b c       ->       # a b ¦
    # d   ¦                # c d ¦
    x x x x x              x x x x x

`##` is also allowed:

    x x x x x              x x x x x
    ## a b c       ->      ## a b ¦
    ## d   ¦               ## c d ¦
    x x x x x              x x x x x

Block comments with `%{ ... %}` or `#{ ... #}`:

    x x x x x              x x x x x
    %{ a b c       ->      %{ a b ¦
       d   ¦                  c d ¦
    %}     ¦               %}     ¦
    x x x x x              x x x x x

    x x x x x              x x x x x
    #{ a b c       ->      #{ a b ¦
       d   ¦                  c d ¦
    #}     ¦               #}     ¦
    x x x x x              x x x x x

Demo/test blocks (with `%!`) contain code so shouldn't be touched:

    %! a b c       ->      %! a b c
    %! d   ¦               %! d   ¦
