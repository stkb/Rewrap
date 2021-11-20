# reStructuredText: Doctest Blocks

> language: reStructuredText

Doctest blocks can be indented (ie: inside blockquotes).

    ··>>> print 'Doctest block'        ->     ··>>> print 'Doctest block'
      Doctest block       ¦                     Doctest block       ¦

They are terminated by a blank line, or something at a lesser indent.

    >>> print 'Doctest block'                 >>> print 'Doctest block'
    Doctest block       ¦                     Doctest block       ¦
                        ¦              ->                         ¦
    Normal              ¦                     Normal paragraph    ¦
    paragraph           ¦                                         ¦

    ··>>> print 'Doctest block'               ··>>> print 'Doctest block'
      Doctest block       ¦            ->       Doctest block       ¦
    Normal                ¦                   Normal paragraph      ¦
    paragraph             ¦                                         ¦

    ··>>> print 'Doctest block'               ··>>> print 'Doctest block'
      Doctest block       ¦            ->       Doctest block       ¦
    * Bullet              ¦                   * Bullet item         ¦
      item                ¦                                         ¦

The space after `>>>` is required.

    >>> This is a doctest block               >>> This is a doctest block
                          ¦            ->                           ¦
    >>>This is a normal paragraph             >>>This is a normal   ¦
                          ¦                   paragraph             ¦

Or a line ending.

    >>>              ¦                        >>>              ¦
    print 'Doctest block'              ->     print 'Doctest block'
    Doctest block    ¦                        Doctest block    ¦

But then there must be no blank lines after the `>>>`.

    >>>              ¦                        >>>              ¦
                     ¦                 ->                      ¦
    Now a normal paragraph                    Now a normal     ¦
                     ¦                        paragraph        ¦

Doctest blocks do not interrupt a paragraph at the same indent.

    Paragraph text     ¦                      Paragraph text >>> ¦
    >>> This is still part             ->     This is still part ¦
    of the paragraph   ¦                      of the paragraph   ¦

But are fine at a greater indent (ie: part of a definition)

    Paragraph          ¦                      Paragraph text     ¦
    text               ¦               ->       >>> This is a doctest block
      >>> This is a doctest block                                ¦

Or a lesser indent

    ····Paragraph text                        ····Paragraph   ¦
    >>> This is a doctest block        ->         text        ¦
                    ¦                         >>> This is a doctest block

Further-indented lines remain part of the block

    >>> Doctest block¦                        >>> Doctest block¦
      more           ¦                          more           ¦
      lines          ¦                 ->       lines          ¦
                     ¦                                         ¦
    Normal paragraph again.                   Normal paragraph ¦
                                              again.           ¦
