> language: "markdown"

    - a   ¦      ->      - a b ¦
      b c ¦                c   ¦
      --  ¦                --  

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


    * a   ¦      ->      * a
      >b  ¦                > b
     >c   ¦              > c