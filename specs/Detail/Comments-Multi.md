> language: "c"

Comments with multi-line comment markers can have a variety of forms. The
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
