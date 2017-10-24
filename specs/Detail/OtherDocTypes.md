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