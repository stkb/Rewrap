# Non-text lines

> language: markdown

It's important that non-text lines that are part of another block aren't mis-parsed as a
general non-text line.

                    ¦                                         ¦
    <!--            ¦                         <!--            ¦
    Comment         ¦                         Comment         ¦
    not wrapped (yet)                         not wrapped (yet)
    -->             ¦                  ->     -->             ¦
                    ¦                                         ¦
    new             ¦                         new paragraph   ¦
    paragraph       ¦                                         ¦
