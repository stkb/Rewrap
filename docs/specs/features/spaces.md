# Spaces

> language: plaintext

When some text has multiple spaces between two words, this is preserved...

    Eiusmod ad····ea sunt non          ->     Eiusmod ad····ea sunt ¦
    reprehenderit elit    ¦                   non reprehenderit elit¦

Unless the split goes over a line boundary. Then all whitespace is removed at that point.

    Et velit velit duis···elit nisi    ->     Et velit velit duis   ¦
                          ¦                   elit nisi             ¦

All trailing whitespace is trimmed from every line of any "wrapping block" except for the
last line, where it is preserved. This is the case even if the whitespace goes over the
wrapping column.

    Minim adipisicing voluptate·              Minim adipisicing¦
    in ipsum consequat····             ->     voluptate in     ¦
                     ¦                        ipsum consequat····

## At the end of a line

Two spaces at the end of a line preserves the line break after it. This comes
from Markdown and should work for any content.

> language: "plaintext"

    Foo bar.··  ¦    ->      Foo bar.··  ¦
    Baz foo     ¦            Baz foo bar ¦
    bar baz.    ¦            baz.        ¦

> language: "markdown"

    Foo bar.··  ¦    ->      Foo bar.··  ¦
    Baz foo     ¦            Baz foo bar ¦
    bar baz.    ¦            baz.        ¦

> language: "latex"

    Foo bar.··  ¦    ->      Foo bar.··  ¦
    Baz foo     ¦            Baz foo bar ¦
    bar baz.    ¦            baz.        ¦

> language: "csharp"

    /// <remarks>   ¦            /// <remarks>   ¦
    /// Foo bar.··  ¦    ->      /// Foo bar.··  ¦
    /// Baz foo     ¦            /// Baz foo bar ¦
    /// bar baz.    ¦            /// baz.        ¦
    /// </remarks>  ¦            /// </remarks>  ¦

> language: "go"

    // Foo bar.··  ¦    ->      // Foo bar.··  ¦
    // Baz foo     ¦            // Baz foo bar ¦
    // bar baz.    ¦            // baz.        ¦

> language: "powershell"

    <#          ¦            <#          ¦
    Foo bar.··  ¦    ->      Foo bar.··  ¦
    Baz foo     ¦            Baz foo bar ¦
    bar baz.    ¦            baz.        ¦
    #>          ¦            #>          ¦

> language: "rst"

    Foo bar.··  ¦    ->      Foo bar.··  ¦
    Baz foo     ¦            Baz foo bar ¦
    bar baz.    ¦            baz.        ¦
