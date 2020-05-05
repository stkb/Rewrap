> language: "c"

Block comments can have a variety of forms. The
relative positions of the begin and end markers (on separate lines or in-line
with the text) are preserved.


Begin and end markers on separate lines. The indent before the content lines is
preserved:

    /*  ¦      ->      /*  ¦
    a   ¦              a b ¦
    b c ¦              c   ¦
    */  ¦              */  ¦

    ··/*      ¦      ->      ··/*      ¦
    ······a   ¦              ······a b ¦
    ······b c ¦              ······c   ¦
    ··*/      ¦              ··*/      ¦

    /*      ¦      ->      /*      ¦
    ····a b c              ····a b ¦
    */      ¦              ····c   ¦
            ¦              */      ¦


Begin and end markers inline with the text. Here the indent of the first line is
preserved and the indent for the following lines is taken from that of the
second line.

    /* a   ¦      ->      /* a b ¦
    b c */ ¦              c */   ¦

    /* a   ¦      ->      /* a b ¦
    ···b c */             ···c */¦

    /*··a   ¦      ->      /*··a b ¦
    ·b c */ ¦              ·c */   ¦

If there is only one line before wrapping, the added lines will be
lined up with the begin comment marker.

    ··/* a b c */     ->      ··/* a b¦
            ¦                 ··c */  ¦

If the end marker is on a separate line, it remains where it is.

    /* a   ¦      ->      /* a b ¦
    b c    ¦              c      ¦
    */     ¦              */     ¦

    /* a   ¦      ->      /* a b ¦
       b c ¦                 c   ¦
           */                    */

### Unusual alignment ###

    ··/* a   ¦      ->      ··/* a b ¦
    ··b      ¦                       ¦
             ¦              ··c */   ¦
    c */     ¦


#### Javadoc ####

> language: "javascript"

Having `*` characters in front of every line is a convention, but is not
required, so in a comment that lacks them, they will remain absent.

    /**                   ¦              ->      /**                   ¦
       Gets the absolute value of n                 Gets the absolute  ¦
       @param n {number}  ¦                         value of n         ¦
    */                    ¦                         @param n {number}  ¦
                          ¦                      */                    ¦

The `*` characters can be mis-aligned or absent. As always, the "prefix" for the
middle lines is taken from the second line, and then applied to the rest,
tidying things up.

    ··/**    ¦      ->      ··/**
       * a   ¦                 * a b
     * b c   ¦                 * c d
     d e f   ¦                 * e f
    ···*/    ¦                 */

If a java/jsdoc comment only has one line before wrapping, a default prefix of
` * ` is used for created lines.

    ··/** Foo bar¦baz */      ->      ··/** Foo bar¦
                 ¦                       * baz */  ¦
