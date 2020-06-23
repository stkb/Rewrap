> language: "markdown"

Issue 204

    > a  ¦              > a  ¦
    >    ¦      ->      >    ¦
    > a b c             > a b¦
    > d  ¦              > c d¦

Content can have varying prefixes. With reformat off this needs to be preserved.

    > a  ¦              > a  ¦
    >    ¦      ->      >    ¦
    >a b c              >a b ¦
    >d   ¦              >c d ¦

    > a  ¦              > a  ¦
    >    ¦      ->      >    ¦
    >a b c              >a b ¦
    >d   ¦              >c d ¦
    >    ¦              >    ¦
    > a  ¦              > a  ¦

    > a  ¦              > a  ¦
    >    ¦      ->      >    ¦
    >>a b c             >>a b¦
    d    ¦              c d  ¦
    >    ¦              >    ¦
    >>> a¦              >>> a¦

    > a  ¦               > a  ¦
    >>> a b      ->      >>> a¦
    >>>  ¦               >>> b¦
                         >>>  ¦
