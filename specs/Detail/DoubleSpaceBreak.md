Two spaces at the end of a line preserves the line break after it. This comes
from Markdown should work for any content.

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
