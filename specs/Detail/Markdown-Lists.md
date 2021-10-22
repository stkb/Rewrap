> language: "markdown"

    - a   ¦      ->      - a b ¦
      b c ¦                c   ¦
      --  ¦                --

Any paragraph under a list item can have the second line indented less than the
list item content indent. (With reformat off this currently has the undesired
effect of reformatting the paragraph.)

    ·*··a     ¦      ->      ·*··a     ¦     -or-     *·a       ¦
              ¦                        ¦                        ¦
    ····a b   ¦              ····a b c ¦              ··a b c d ¦
    c d e     ¦                  d e   ¦                e       ¦

List items may be separated by blank lines

    * a b c d e      ->      * a b c d ¦
      f       ¦                e f     ¦
              ¦                        ¦
        * a b c                  * a b ¦
          d   ¦                    c d ¦
              ¦                        ¦
        * a b c                  * a b ¦
          d   ¦                    c d ¦

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
