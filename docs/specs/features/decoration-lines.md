# Decoration Lines

Decoration lines are lines of just symbols with no text, that are sometimes used
in comment blocks.

> language: "java"

## Line comments ##

A "decoration line" is defined as a line where the content after the comment
marker (eg //):
1. Is not just whitespace
2. Does not contain any text
3. Has no preceding whitespace (between the comment marker and the content)

We need to handle these as a specially, in the case that reformat is on. With
reformat on, the whitespace between the comment marker and content is
normally normalized to one space. But these lines need to be preserved as they
are, with no extra space added.

    //////// ¦      ->      //////// ¦     -or-     //////// ¦
    //  a    ¦              //  a    ¦              // a     ¦
    //------ ¦              //------ ¦              //------ ¦

If there is whitespace between the comment marker and the decoration symbols,
then that line is treated like the rest of the markdown content - as a
non-wrapping line - but the indent is preserved in the case of reformat off and
normalized in the case of reformat on.


## Block comments ##

A decoration line is defined as a line which:
1. Is the first line of the comment, and
    1) Has no text after the start-comment marker.
    2) Has no whitespace between the start-comment marker and the rest of the line.
2. Or, contains the end-comment marker and contains no text before it
3. Or, contains no text, and
    1) Has more than one non-whitespace character
    2) Has no whitespace between a conventional line prefix character for the
       type of comment block (eg `*` for c-style comments), and the rest of the
       line
    3) Has a lesser indent than the tail text indent

Decoration lines in block comments are output exactly as input. There may be
other decoration lines in the comment that are not detected as such lines but
it doesn't matter because in these cases they still won't be wrapped and their
indent won't be adjusted.

    /*text */ ¦       ->      /*text */ ¦     -or-     /* text */¦
    /*-*-* */ ¦               /*-*-* */ ¦              /*-*-* */ ¦

    /********** ¦       ->      /********** ¦     -or-     /********** ¦
     * aaa      ¦                * aaa bbb  ¦               * aaa bbb  ¦
     * bbb      ¦                ********** ¦               ********** ¦
     ********** ¦                * ccccc    ¦               * ccccc    ¦
     * ccccc ddddd               * ddddd    ¦               * ddddd    ¦
     ---------- ¦                ---------- ¦               ---------- ¦
     **********/¦                **********/¦               **********/¦
