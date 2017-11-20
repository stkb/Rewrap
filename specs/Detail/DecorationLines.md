# Decoration lines #

Decoration lines are lines of just symbols with no text, that are sometimes used
in comment blocks.

> language: "java"

== Line comments ==

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

If the comment markers themselves are misaligned however, these are always
re-aligned; the rest of the line moving with them.

    ··//------   ¦      ->      ··//------ ¦     -or-     ··//------ ¦
    ····//****** ¦              ··//****** ¦              ··//****** ¦
    ////////     ¦              ··//////// ¦              ··//////// ¦

If there is whitespace between the comment marker and the decoration symbols,
then that line is treated like the rest of the markdown content - as a
non-wrapping line - but the indent is preserved in the case of reformat off and
normalized in the case of reformat on.