# Markdown: Paragraphs

> language: "markdown"

The first line might be shorter than the indent of the others.

    a     ¦      ->      a b c ¦
     b    ¦                    ¦
     c    ¦                    ¦

The "shape" of any paragraph is kept; meaning the indent for each line. Any
lines added at the end have the indent of the line before it.

    one two      ->      one   ¦
      three                two ¦
     four ¦               three¦
                          four ¦

### Paragraph terminators ###

Blank line:

    a   ¦      ->      a b ¦
    b c ¦              c   ¦
        ¦                  ¦
    a   ¦              a   ¦

The following blocks can interrupt a paragraph. However they are only treated as
such a block if they have an indent less than 4 spaces.

Fenced code block:

    a b c   ¦      ->      a b c d ¦
    d e     ¦              e       ¦
    ~~~ a   ¦              ~~~ a   ¦

    a b c   ¦      ->      a b c d ¦
    d e     ¦              e       ¦
    ``` a   ¦              ``` a   ¦

ATX heading:

    a b   ¦      ->      a b c ¦
    c d   ¦              d     ¦
    ## a  ¦              ## a  ¦

But not:

    a b   ¦      ->      a b c ¦
    c d   ¦              d ## a¦
        ## a                   ¦


Non-text line:

    a b c   ¦      ->      a b c d ¦
    d e     ¦              e       ¦
    ***     ¦              ***     ¦

List item:

    a b   ¦      ->      a b c ¦
    c d   ¦              d     ¦
    - a   ¦              - a   ¦

    a b   ¦      ->      a b c ¦
    c d   ¦              d     ¦
    * a   ¦              * a   ¦

    a b   ¦      ->      a b c ¦
    c d   ¦              d     ¦
    + a   ¦              + a   ¦

Block quote:

    a b   ¦      ->      a b c ¦
    c d   ¦              d     ¦
    > a   ¦              > a   ¦

[Html block types 1 to 6](http://spec.commonmark.org/0.28/#html-block):

    a b c d   ¦      ->      a b c d e ¦
    e f       ¦              f         ¦
    <script   ¦              <script   ¦

    a b c d   ¦      ->      a b c d e ¦
    e f       ¦              f         ¦
    <!-- a    ¦              <!-- a    ¦

    a b c d   ¦      ->      a b c d e ¦
    e f       ¦              f         ¦
    <?a       ¦              <?a       ¦

    a b c d   ¦      ->      a b c d e ¦
    e f       ¦              f         ¦
    <!A       ¦              <!A       ¦

    a b c d    ¦      ->      a b c d e f¦
    e f g      ¦              g          ¦
    <![CDATA[  ¦              <![CDATA[  ¦

    a b c d   ¦      ->      a b c d e ¦
    e f       ¦              f         ¦
    <DD       ¦              <DD       ¦

    a b c d   ¦      ->      a b c d e ¦
    e f       ¦              f         ¦
    </DD>     ¦              </DD>     ¦
