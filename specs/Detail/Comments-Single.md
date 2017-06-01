> language: "c"

A comment block is taken from consecutive lines that have the line comment
marker (in the case of C: `//`) at the start of the line, with only whitespace
before it. Comments at the end of lines with other text in front of them are
ignored.


A completely empty comment remains the same.

    z      ¦
    //     ¦
    z      ¦

    z      ¦
    //     ¦
    //     ¦
    z      ¦


Any amount whitespace before the comment markers is preserved

    // a   ¦      ->      // a b
    // b c ¦              // c

    ·// a   ¦      ->      ·// a b
    ·// b c ¦              ·// c

    ·····// a   ¦      ->      ·····// a b
    ·····// b c ¦              ·····// c

Also any amount of whitespace between the comment markers and text is preserved.

    //a   ¦      ->      //a b
    //b c ¦              //c

    //·····a   ¦      ->      //·····a b
    //·····b c ¦              //·····c


The "prefix" (whitespace + marker + whitespace) is taken from the first line
with text. This is because of comments like the following, where just taking it
from the first line would give a prefix without whitespace after the comment
markers.

    ///////¦      ->      ///////¦
    // a   ¦              // a b ¦
    // b c ¦              // c   ¦
    ///////¦              ///////¦

    //            ->      //
    // a   ¦              // a b ¦
    // b c ¦              // c   ¦

This prefix is then applied to all other lines in the comment that are
processed, and any new lines created.

    ···//       ¦      ->      ···//       ¦
    ···//···a b c              ···//···a b ¦
                               ···//···c   ¦


Only comments at the beginning of lines (except for whitespace) can be wrapped.
Comments with text in front of them cannot be.

    // This is wrapped        ->      // This is   ¦
                 ¦                    // wrapped   ¦
    int i; // This isn't                           ¦
                 ¦                    int i; // This isn't


### Unusual alignment ###

After the prefix for the comment block is taken, following lines are processed,
taking into account the alignment of the text content rather than the comment
markers. Therefore the two paragraphs in this comment block are considered to
have the same indent.

    ······// a   ¦     ->      ······// a b
    ······// b c ¦             ······// c
    ······//     ¦             ······//
    ··//     a   ¦             ······// a b
    ··//     b c ¦             ······// c

Here the second paragraph is indented 4 spaces, and therefore treated as a code
block and not wrapped:

    // a   ¦        ->      // a b
    // b c ¦                // c
    //     ¦                //
    ····// a                ····// a
    ····// b c              ····// b c

(The comment markers are also not fixed, because ignored lines don't get touched
at all. This should probably be fixed but it's an edge case.)

Two other approaches could have been taken for these sorts of cases:
* Line up the comment markers and then work with the whitespace between comment
  markers and text.
* Have a difference in indent of comment markers mean a new comment block. 
There weren't strong reasons for the approach chosen.

