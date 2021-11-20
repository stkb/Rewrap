# reStructuredText: Literal Blocks

> language: "reStructuredText"

Whitespace after the `::` is allowed.

    Example::··                               Example::··
              ¦                                         ¦
        code block                     ->         code block
              ¦                                         ¦

But there are two conditions. The first is that there's a blank line between the paragraph
and literal block:

    Example::            ¦                    Example::            ¦
      no blank line;     ¦             ->       no blank line;     ¦
      taken as normal paragraph                 taken as normal    ¦
                         ¦                      paragraph          ¦

    Example::            ¦                    Example:: if the     ¦
    if the indent is the same,         ->     indent is the same,  ¦
    taken as the same paragraph               taken as the same    ¦
                         ¦                    paragraph

Though there can be any number of blank lines

    Example:: ¦                               Example:: ¦
              ¦                                         ¦
              ¦                                         ¦
              ¦                                         ¦
        code block                     ->         code block
              ¦                                         ¦


The second is that the indent of the literal block, while it doesn't have to be 4 spaces,
must be greater than that of the paragraph introducing it.

    Example ::         ¦                      Example ::         ¦
                       ¦               ->                        ¦
     1-space indent is fine                    1-space indent is fine

    Example ::         ¦                      Example ::         ¦
                       ¦               ->                        ¦
    no indent - normal paragraph              no indent - normal ¦
                       ¦                      paragraph          ¦

    ·Indented blockquote :: ¦                 ·Indented blockquote :: ¦
                            ¦          ->                             ¦
     not indented further - normal             not indented further - ¦
     paragraph              ¦                  normal paragraph       ¦

    ·Indented blockquote :: ¦                 ·Indented blockquote :: ¦
                            ¦                                         ¦
    ··indented further - literal              ··indented further - literal
       block                ¦                    block                ¦
                            ¦          ->                             ¦
    ··still in the literal block              ··still in the literal block
                            ¦                                         ¦
    ·normal paragraph       ¦                 ·normal paragraph again ¦
     again                  ¦                                         ¦


## Quoted literal blocks

With quoted literal blocks, the above rules for indentation can be ignored, and instead of
being indented, the lines of the literal block are prefixed with one of the following
characters: `! " # $ % & ' ( ) * + , - . / : ; < = > ? @ [ \ ] ^ _ ` { | } ~`.

    ::                ¦                       ::                ¦
                      ¦                ->                       ¦
    ! literal         ¦                       ! literal         ¦
    ! block           ¦                       ! block           ¦

As above, there must be a blank line before them.

    ::                ¦                       :: > this is still¦
    > this is still the                ->     the > same        ¦
    > same paragraph  ¦                       paragraph         ¦

If a quoted literal block is also indented, then it counts as an indented literal block,
and those rules apply.

    ::              ¦                         ::              ¦
                    ¦                                         ¦
        # just an indented                        # just an indented
        # literal block                ->         # literal block
      # still in the literal block              # still in the literal block
    # now a normal paragraph                  # now a normal
                                              paragraph

The lines of the literal block must all have the same prefix char. If it changes then the
literal block ends.

    ::               ¦                        ::               ¦
                     ¦                                         ¦
    $ just another literal                    $ just another literal
    $ block          ¦                 ->     $ block          ¦
    @ now this is a normal                    @ now this is a  ¦
    @ paragraph again¦                        normal @         ¦
                                              paragraph again  ¦
