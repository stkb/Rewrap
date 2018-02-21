> language: "go", tabWidth: 2

Only 1 space or tab is required for an indented block

    // a b   ¦      ->      // a b c ¦
    // c d   ¦              // d     ¦
    //  a    ¦              //  a    ¦
    //  b    ¦              //  b    ¦
    // a b   ¦              // a b c ¦
    // c d   ¦              // d     ¦

However to keep things simple, all tabs in comments are replaced with the
visually equivalent number of spaces. (Godoc also does something weird with
tabs: if any line contains a tab before the text, then those lines, plus all
lines with a 2 or more space indent, are treated as preformatted text.)

    // a b   ¦      ->      // a b c ¦
    // c d   ¦              // d     ¦
    // →a    ¦              //  a    ¦
    // →b    ¦              //  b    ¦
    // a b   ¦              // a b c ¦
    // c d   ¦              // d     ¦