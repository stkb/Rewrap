# Python

> language: "python"

Whole paragraphs of text can be written between triple " or '

    """ ¦      ->      """ ¦
    a b c              a b ¦
    d   ¦              c d ¦
    """ ¦              """ ¦

    ''' ¦      ->      ''' ¦
    a b c              a b ¦
    d   ¦              c d ¦
    ''' ¦              ''' ¦

''' Could appear inside a """-block and vice-versa.

    """ ¦      ->      """ ¦
    a b c              a b ¦
    d   ¦              c d ¦
    ''' ¦              ''' ¦
    a   ¦              a b ¦
    b   ¦              """ ¦
    """ ¦

    ''' ¦      ->      ''' ¦
    a b c              a b ¦
    d   ¦              c d ¦
    """ ¦              """ ¦
    a   ¦              a b ¦
    b   ¦              ''' ¦
    ''' ¦

Triple-quoted strings can be preceded with a character. Valid combinations are
b, f, r, u, br, rb, fr, rf, and with any comnbination of upper and lower case.

    b"""      ->       b"""¦
    a b c              a b ¦
    d   ¦              c d ¦
    """ ¦              """ ¦

    f"""      ->       f"""¦
    a b c              a b ¦
    d   ¦              c d ¦
    """ ¦              """ ¦

    r'''      ->       r'''¦
    a b c              a b ¦
    d   ¦              c d ¦
    ''' ¦              ''' ¦

    u'''      ->       u'''¦
    a b c              a b ¦
    d   ¦              c d ¦
    ''' ¦              ''' ¦

    fr"""     ->       fr"""
    a b c              a b ¦
    d   ¦              c d ¦
    """ ¦              """ ¦

    rF'''     ->       rF'''
    a b c              a b ¦
    d   ¦              c d ¦
    ''' ¦              ''' ¦

    Br"""     ->       Br"""
    a b c              a b ¦
    d   ¦              c d ¦
    """ ¦              """ ¦


    RB'''     ->       RB'''
    a b c              a b ¦
    d   ¦              c d ¦
    ''' ¦              ''' ¦

Starting the string later on the line is also (imperfectly) supported.

    var = """¦     ->    var = """
    a b c d e f          a b c d e
    g h      ¦           f g h    ¦
    """      ¦           """      ¦
    i        ¦           i        ¦
    j        ¦           j        ¦

Imperfectly, because it's not a full language parser.

Strings on one line are wrapped too,

    var = """a b c"""      ->      var = """a b
    x x x x x x                          c"""  ¦
                ¦                  x x x x x x

Though these should maybe be left alone in a large (eg whole-document)
selection, and only wrapped if explicitly asked


## reStructuredText

The content of Python docstrings is reStructuredText.

    """                 ¦                     """                 ¦
    :Author: Me         ¦                     :Author: Me         ¦
    :version: 1         ¦                     :version: 1         ¦
                        ¦                                         ¦
    term                ¦                     term                ¦
        definition      ¦              ->         definition text ¦
        text text       ¦                         text            ¦
                        ¦                                         ¦
    ::                  ¦                     ::                  ¦
                        ¦                                         ¦
      literal           ¦                       literal           ¦
      block             ¦                       block             ¦
    """                 ¦                     """                 ¦
