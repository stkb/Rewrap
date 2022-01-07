# Embedded Sections

> language: "html"

In eg an HTML file, an embedded script section must behave the same if there is
no closing tag, as if there were one.

    <script>       ¦                          <script>       ¦
      // one two three                 ->       // one two   ¦
                   ¦                            // three     ¦
