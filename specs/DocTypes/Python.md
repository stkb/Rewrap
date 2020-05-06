> language: "python"

Whole paragraphs of text can be written between triple " or '

    """ ¦      ->      """
    a b c              a b
    d   ¦              c d
    """ ¦              """

    ''' ¦      ->      '''
    a b c              a b
    d   ¦              c d
    ''' ¦              '''

''' Could appear inside a """-block and vice-versa.

    """ ¦      ->      """
    a b c              a b
    d   ¦              c d
    ''' ¦              '''
    a                  a b
    b                  """
    """ ¦

    ''' ¦      ->      '''
    a b c              a b
    d   ¦              c d
    """ ¦              """
    a                  a b
    b                  '''
    ''' ¦

Triple-quoted strings can be preceded with a character. Valid combinations are
b, f, r, u, br, rb, fr, rf, and with any comnbination of upper and lower case.

    b"""      ->       b"""
    a b c              a b
    d   ¦              c d
    """ ¦              """

    f"""      ->       f"""
    a b c              a b
    d   ¦              c d
    """ ¦              """

    r'''      ->       r'''
    a b c              a b
    d   ¦              c d
    ''' ¦              '''

    u'''      ->       u'''
    a b c              a b
    d   ¦              c d
    ''' ¦              '''

    fr"""     ->       fr"""
    a b c              a b
    d   ¦              c d
    """ ¦              """

    rF'''     ->       rF'''
    a b c              a b
    d   ¦              c d
    ''' ¦              '''

    Br"""     ->       Br"""
    a b c              a b
    d   ¦              c d
    """ ¦              """


    RB'''     ->       RB'''
    a b c              a b
    d   ¦              c d
    ''' ¦              '''
