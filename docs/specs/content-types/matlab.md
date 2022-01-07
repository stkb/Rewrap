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