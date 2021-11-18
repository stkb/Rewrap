> language: "elixir"

Elixir has '#' line comments.

    x x x x      ->      x x x x
    # a b c              # a b ¦
    # d   ¦              # c d ¦
    x x   ¦              x x   ¦
    x x   ¦              x x   ¦

It also has @doc, @moduledoc and @typedoc comments

    x x x x x x x x x      ->       x x x x x x x x x
    @doc """      ¦                 @doc """      ¦
    a b c d e f g h i               a b c d e f g ¦
    j k           ¦                 h i j k       ¦
    """           ¦                 """           ¦
    x x           ¦                 x x           ¦
    x x           ¦                 x x           ¦


    x x x x x x x x x      ->       x x x x x x x x x
    @moduledoc """¦                 @moduledoc """¦
    a b c d e f g h i               a b c d e f g ¦
    j k           ¦                 h i j k       ¦
    """           ¦                 """           ¦
    x x           ¦                 x x           ¦
    x x           ¦                 x x           ¦

    x x x x x x x x x      ->       x x x x x x x x x
    @typedoc """  ¦                 @typedoc """  ¦
    a b c d e f g h i               a b c d e f g ¦
    j k           ¦                 h i j k       ¦
    """           ¦                 """           ¦
    x x           ¦                 x x           ¦
    x x           ¦                 x x           ¦
