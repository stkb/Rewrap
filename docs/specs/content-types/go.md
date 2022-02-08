# Go

> language: "go", tabWidth: 2

https://go.dev/blog/godoc

Go uses [tabs](../features/tabs.md) for indentation. Comments in are wrapped using the
rules of the godoc parser. There are two simple rules.

Paragraphs are separated by blank lines.

    // paragraph one      ->      // paragraph  ¦
    // text       ¦               // one text   ¦
    //            ¦               //            ¦
    // paragraph two              // paragraph  ¦
    // text       ¦               // two text   ¦

Pre-formatted text must be indented relative to the surrounding comment text.

    // normal wrapped paragraph      ->      // normal wrapped  ¦
    // text            ¦                     // paragraph text  ¦
    //    preformatted {                     //    preformatted {
    //      text       ¦                     //      text       ¦
    //    }            ¦                     //    }            ¦
    // normal wrapped paragraph              // normal wrapped  ¦
    // text            ¦                     // paragraph text  ¦

Unlike in markdown:
* No blank line is required between a normal paragraph and indented code block.
* Any amount of indent (as little as a 1-space) is required for the indented
  code block, as opposed to 4 spaces.

Example

    // a b   ¦      ->      // a b c ¦
    // c d   ¦              // d     ¦
    //  a    ¦              //  a    ¦
    //  b    ¦              //  b    ¦
    // a b   ¦              // a b c ¦
    // c d   ¦              // d     ¦

Tabs also work for indented sections within the comment. However to keep things simple,
Rewrap replaces tabs within comments with the visually equivalent number of spaces. (Godoc
also does something weird with tabs: if any line contains a tab before the text, then
those lines, plus all lines with a 2 or more space indent, are treated as preformatted
text.)

    // a b   ¦      ->      // a b c ¦
    // c d   ¦              // d     ¦
    // →a    ¦              //  a    ¦
    // →b    ¦              //  b    ¦
    // a b   ¦              // a b c ¦
    // c d   ¦              // d     ¦
