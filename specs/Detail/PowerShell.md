> language: "powershell"

All lines beginning with keywords (. and then one or more capital letters) are not
wrapped.

    <#          ¦       ->      <#          ¦
     one two three               one two    ¦
     four       ¦                three four ¦
     .A         ¦                .A         ¦
     one two three               one two    ¦
     four       ¦                three four ¦
    #>          ¦               #>          ¦

CBH can be in `<# ... #>` or `#` comments

    # one two three      ->      # one two    ¦
    # four       ¦               # three four ¦
    # .A         ¦               # .A         ¦
    # one two three              # one two    ¦
    # four       ¦               # three four ¦

Some keywords (eg .PARAMETER) take arguments. These must be kept on the same
line.

    <#            ¦                   ->       <#            ¦
    .PARAMETER LongParameterName               .PARAMETER LongParameterName
    #>            ¦                            #>            ¦


Each section has its own independent indent. The indent is determined by the
first line of text in the section.

    # .SYNOPSIS    ¦      ->      # .SYNOPSIS    ¦
    #      aaa bbb ccc            #      aaa bbb ¦
    #     ddd      ¦              #      ccc ddd ¦

    <#             ¦      ->      <#             ¦
      .SYNOPSIS    ¦                .SYNOPSIS    ¦
           aaa bbb ccc                   aaa bbb ¦
          ddd      ¦                     ccc ddd ¦
    #>             ¦              #>             ¦