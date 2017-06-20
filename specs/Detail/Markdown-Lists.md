> language: "markdown"

    - a   ¦      ->      - a b ¦
      b c ¦                c   ¦
      --  ¦                --  

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