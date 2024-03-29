# .Net XmlDoc

.Net xml doc comments are parsed as xml/html instead of markdown, so wrapping and newlines
follow the rules for html instead. This keeps tags separated on new lines.

> language: "csharp"

    /// <summary> What the method    ->      /// <summary> What the    ¦
    /// does </summary>       ¦              /// method does </summary>¦
    /// <param name="s">      ¦              /// <param name="s">      ¦
    /// The s param </param>  ¦              /// The s param </param>  ¦
    ///                       ¦              ///                       ¦
    /// <description>         ¦              /// <description>         ¦
    /// Extended info.        ¦              /// Extended info. Text   ¦
    /// Text text text.       ¦              /// text text.            ¦
    /// </description>        ¦              /// </description>        ¦

> language: "fsharp"

    /// <summary> What the method    ->      /// <summary> What the    ¦
    /// does </summary>       ¦              /// method does </summary>¦
    /// <param name="s">      ¦              /// <param name="s">      ¦
    /// The s param </param>  ¦              /// The s param </param>  ¦
    ///                       ¦              ///                       ¦
    /// <description>         ¦              /// <description>         ¦
    /// Extended info.        ¦              /// Extended info. Text   ¦
    /// Text text text.       ¦              /// text text.            ¦
    /// </description>        ¦              /// </description>        ¦

> language: "vb"

    ''' <summary> What the method    ->      ''' <summary> What the    ¦
    ''' does </summary>       ¦              ''' method does </summary>¦
    ''' <param name="s">      ¦              ''' <param name="s">      ¦
    ''' The s param </param>  ¦              ''' The s param </param>  ¦
    '''                       ¦              '''                       ¦
    ''' <description>         ¦              ''' <description>         ¦
    ''' Extended info.        ¦              ''' Extended info. Text   ¦
    ''' Text text text.       ¦              ''' text text.            ¦
    ''' </description>        ¦              ''' </description>        ¦

There is however, an extra rule. Line breaks are only preserved before/after certain
"block tags". The list of these tags is: *code, description, example, exception, include,
inheritdoc, list, listheader, item, para, param, permission, remarks, seealso, summary,
term, typeparam, typeparamref, returns, value*.

All other tags, eg `<c>` are treated as inline tags and will always be wrapped inline with
the surrounding text.

> language: "csharp"

    /// Text                      ¦           /// Text <c>TypeName</c> text ¦
    /// <c>TypeName</c>           ¦    ->     /// text.                     ¦
    /// text text.                ¦                                         ¦
