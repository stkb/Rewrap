# Markdown: Lists

> language: "markdown"

    - a   ¦      ->      - a b ¦
      b c ¦                c   ¦

Any paragraph under a list item can have the second line indented less than the
list item content indent. If this is so it will be preserved.

    ·*··a b   ¦              ·*··a b c ¦
    c d e     ¦              d e       ¦
              ¦      ->                ¦
    ····a b   ¦              ····a b c ¦
    c d e     ¦              d e       ¦

However, when creating a new second line, its indent is set to match that of the first,
since that is most desirable.

    ·+··a b c d e      ->      ·+··a b c ¦
              ¦                    d e   ¦

Similarly any newly-created line in a paragraph is given the same indent as the line above
it.

    ·*··text  ¦                ·*··text  ¦
      text text                  text    ¦
              ¦                  text    ¦
    ····text text      ->                ¦
         text ¦                ····text  ¦
              ¦                     text ¦
              ¦                     text ¦


List items may be separated by blank lines

    * a b c d e      ->      * a b c d ¦
      f       ¦                e f     ¦
              ¦                        ¦
        * a b c                  * a b ¦
          d   ¦                    c d ¦
              ¦                        ¦
        * a b c                  * a b ¦
          d   ¦                    c d ¦

There may be 1-4 spaces between the list item marker and following text, and
this determines the indent for the rest of the list item. If there are more
spaces, the indent is reset to 1 space, and the text is considered an inline
code block.

    ··+     code block                 ··+     code block
    ····text in list item      ->      ····text in    ¦
                   ¦                       list item  ¦

Other

    List items:     ¦                ->    List items:     ¦
    * > Block quote text wrapped           * > Block quote ¦
                    ¦                        > text wrapped¦

    >  * > bq li¦bq      ->      >  * > bq li¦
                ¦                >    > bq   ¦


> language: "markdown", reformat: true

    * a b   ¦      ->      * a b c ¦
      c d   ¦                d     ¦
     * a b  ¦              * a b c ¦
       c d  ¦                d


    * a b   ¦      ->      * a b c ¦
      c d   ¦                d     ¦
      * a   ¦                * a b ¦
        b c ¦                  c


    * a b   ¦      ->      * a b c ¦
      c d   ¦                d     ¦
            ¦
    a                      a


    * a   ¦      ->      * a   ¦
      >b  ¦                > b ¦
     >c   ¦              > c   ¦


> language: "markdown"

## Terminating paragraphs

An unordered list item terminates a paragraph.

    text        ¦                text        ¦
    ···-  list item      ->      ···-  list  ¦
                ¦                      item  ¦

As long as the marker is indented 3 spaces or less.

    text        ¦                 text -  list¦
    ····-  list item      ->      ····item    ¦


## Terminating the list item

Any line that is not a paragraph continuation line, where the text indent is
less than that of the first line of the list item, terminates the list item and
that line is not part of it.

Here the ATX heading is not in the list item because the indent is too small:

    ·-··text     ¦      ->      ·-··text     ¦
    ···# heading ¦              ···# heading ¦

Here however, even though the heading is indented less than the text, since it
is indented 4 spaces, it's counted as a paragraph continuation line instead:

    ··-··text     ¦      ->      ··-··text #   ¦
    ····# heading ¦              ····heading   ¦

If a fenced code block inside a list item has a line with less than the required
indent, it and the list item are terminated.

    *··```   ¦              *··```   ¦
    ···foo   ¦      ->      ···foo   ¦
    ··bar    ¦              ··bar baz¦
    ··baz    ¦                       ¦

## Other tests ##

Double list-item:

    - - a  ¦      ->      - - a b¦
    b      ¦                     ¦

`- - -` is a [thematic break](thematic-breaks.md) rather than a triple list item.

    - - -                  ¦                  - - -                  ¦
          indented         ¦          ->            indented         ¦
          code block       ¦                        code block       ¦
