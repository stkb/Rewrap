> language: "markdown"

An indented code block can't immediately follow a paragraph. This is because
paragraph text after the first line can be indented to any level and it's still
considered part of the paragraph. (After wrapping the 2nd & 3rd lines remain
indented because Rewrap preserves all paragraph line indentation.)

    paragraph    ¦              paragraph not¦
    ····not a    ¦      ->      ····a code   ¦
    ····code block              ····block    ¦

Since an ATX heading must be on a single line, an indented code block can
immediately follow it (unlike with a normal paragraph).

    # Heading      ¦              # Heading      ¦
    ····code       ¦      ->      ····code       ¦
    ····block      ¦              ····block      ¦
