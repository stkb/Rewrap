## Single-line comment markers ##

> language: "javascript"

Every type of source code document has its own comment markers. These are preserved and added or removed for each line as necessary.

    // Line comment markers      ->      // Line comment ¦
                    ¦                    // markers      ¦

Any whitespace around the comment markers is preserved.

    ··// Comment indented 2 spaces      ->      ··// Comment indented ¦
                          ¦                     ··// 2 spaces         ¦


## Multi-line comment markers ##

> language: "c"


Also multi-line comments

    /* One Three Two      ->      /* One Three   ¦
       Four */     ¦                 Two Four */ ¦

The indent of the text relative to the comment markers is preserved. Also the indents of the start/end markers and if they are on separate lines from the text.

    /*··One Three Two      ->      /*··One Three  ¦
        Four */    ¦                   Two Four */¦
                   ¦                              ¦
    /*One Three    ¦       ->      /*One Three Two¦
      Two Four*/   ¦                 Four*/       ¦
                   ¦                              ¦
    /*             ¦       ->      /*             ¦
    One Three      ¦               One Three Two  ¦
    Two Four       ¦               Four           ¦
    */             ¦               */             ¦
                   ¦                              ¦
    /*             ¦       ->      /*             ¦
    ····One Three Two              ····One Three  ¦
    ····Four       ¦               ····Two Four   ¦
    ·*/            ¦               ·*/            ¦

The first line can be indented relative to the rest of the text because of the comment marker, and this is preserved.

    /* One Three Two      ->      /* One Three   ¦
    Four */        ¦              Two Four */    ¦
