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

If the input is only 1 line, then created lines are given the same prefix as the
first

    > a b c d e f      ->      > a b c ¦
            ¦                  > d e f ¦

    >a b c d e f      ->      >a b c ¦
           ¦                  >d e f ¦

    >  a b c d e f      ->      >  a b c ¦
             ¦                  >  d e f ¦

If the a line doesn't start with the `>` marker, then the blockquote section has
terminated, unless the line is a paragraph continuation line.

    >··```   ¦              >··```   ¦
    ···foo   ¦      ->      ···foo   ¦
    ··bar    ¦              ··bar baz¦
    ··baz    ¦                       ¦

The first (optional) space after the `>` marker is treated as part of the
blockquote marker, so an indented code block has to be indented 5 spaces.

    >·····code block              >·····code block
    >····text     ¦       ->      >····text text¦
    >····text     ¦                             ¦

    > one··  ¦                > one··  ¦
    two      ¦        ->      two three¦
    > three four              > four
