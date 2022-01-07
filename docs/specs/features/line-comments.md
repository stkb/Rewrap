# Line Comments

A comment block is taken from consecutive lines that have the line comment marker (eg. in
the case of C: `//`) at the start of the line, with only whitespace before it.

> language: c

    // Line comment markers      ->      // Line comment ¦
                    ¦                    // markers      ¦

Only comments at the beginning of lines (except for whitespace) can be wrapped. Comments
at the end of lines with other text in front of them are not supported.

    // This is wrapped        ->      // This is   ¦
                 ¦                    // wrapped   ¦
    int i; // This isn't                           ¦
                 ¦                    int i; // This isn't

## Comment indent

A line comment block is denoted by consecutive lines that have the same comment marker at
the same visual indent. Tabs or spaces are allowed.

> language: c, tabWidth: 2

    // a   ¦      ->      // a b ¦
    // b c ¦              // c   ¦

    ·// a   ¦      ->      ·// a b ¦
    ·// b c ¦              ·// c   ¦

    -→-→// a   ¦      ->      -→-→// a b ¦
    -→-→// b c ¦              -→-→// c   ¦

Where width of the indent changes, it's taken to denote a new comment block.

    ····// a   ¦     ->      ····// a b ¦
    ····// b c ¦             ····// c   ¦
    ··// a b   ¦             ··// a b c ¦
    ··// c d   ¦             ··// d     ¦

The indents can contain any combination of tabs and spaces and will be counted as the same
as long as they have the same visual width.

> language: c, tabWidth: 4

    ········// a   ¦      ->      ········// a b ¦
    ··-→····// b c ¦              ··-→····// c   ¦

## Content indent

The content indent (whitespace between comment marker and text) is preserved with
reformat: off and changed to 1 space with reformat on.

    //a   ¦      ->      //a b ¦     -or-     // a b¦
    //b c ¦              //c   ¦              // c  ¦


    //-→a  ¦      ->      //-→a b¦     -or-     // a b ¦
    //-→b c¦              //-→c  ¦              // c   ¦

Where the indent is not the same for all lines, the "base" indent is taken from the line
in the block with the least indent, providing this line contains text.

    //····indented code          //····indented code            //·····indented code
    //        ¦            ->    //        ¦            -or-    //        ¦
    //a b c d e                  //a b c d ¦                    //·a b c d¦
    //f g     ¦                  //e f g   ¦                    //·e f g  ¦

Only lines with text are counted, so that "decorative" lines are preserved, exactly as
they are, regardless of the reformat setting.

    ///////¦              ///////¦              ///////¦
    //··a  ¦      ->      //··a b¦     -or-     //·a b ¦
    //··b c¦              //··c  ¦              //·c   ¦
    //=====¦              //=====¦              //=====¦

## Content

A completely empty comment remains the same.

    x      ¦      ->      x      ¦
    //     ¦              //     ¦
    x      ¦              x      ¦

    x      ¦      ->      x      ¦
    //     ¦              //     ¦
    //     ¦              //     ¦
    x      ¦              x      ¦

All blank lines are trimmed at the end. (This is true of all non-wrapping lines)

    //··    ¦      ->      //      ¦     -or-     //      ¦
    //····  ¦              //      ¦              //      ¦
