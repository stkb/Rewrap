# PowerShell

> language: "powershell"

PowerShell supports comment-based help documentation.

Basic example

    <#                       ¦                      <#                       ¦
    .SYNOPSIS                ¦                      .SYNOPSIS                ¦
       What the function does¦                         What the function does¦
    .DESCRIPTION             ¦                      .DESCRIPTION             ¦
       Longer explanation of what it does.    ->       Longer explanation of ¦
                             ¦                         what it does.         ¦
       Another paragraph.    ¦                                               ¦
                             ¦                         Another paragraph.    ¦
    .PARAMETER ToString      ¦                                               ¦
       Add to output         ¦                      .PARAMETER ToString      ¦
       the result            ¦                         Add to output the     ¦
       as a string           ¦                         result as a string    ¦
                             ¦                                               ¦
    .EXAMPLE                 ¦                      .EXAMPLE                 ¦
       Test-Function         ¦                         Test-Function         ¦
                             ¦                                               ¦
    .EXAMPLE                 ¦                      .EXAMPLE                 ¦
       Test-Function -ToString                         Test-Function -ToString
       Tests the function and outputs                  Tests the function and¦
       the result            ¦                         outputs the result as ¦
       as a string.          ¦                         a string.             ¦
    #>                       ¦                      #>                       ¦

All lines beginning with keywords (. and then one or more capital letters) are not
wrapped.

    <#          ¦       ->      <#          ¦
     one two three               one two    ¦
     four       ¦                three four ¦
     .A         ¦                .A         ¦
     one two three               one two    ¦
     four       ¦                three four ¦
    #>          ¦               #>          ¦

CBH can also be in `#` line comments

    # one two three      ->      # one two    ¦
    # four       ¦               # three four ¦
    # .A         ¦               # .A         ¦
    # one two three              # one two    ¦
    # four       ¦               # three four ¦

Some keywords (eg .PARAMETER) take arguments. These must be kept on the same
line.

    <#            ¦                   ->       <#            ¦
    .PARAMETER LongParameterName               .PARAMETER LongParameterName
        aaaaa bbbbb                                aaaaa     ¦
        ccc       ¦                                bbbbb ccc ¦
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


## Example sections ##

.EXAMPLE sections get some special treatment. The first line is assumed to be
code, so won't be wrapped. (PowerShell makes this assumption too, and when the
help is shown, adds `PS C:\>` to the beginning of the line and a blank line
after it.) Lines after that will be wrapped.

    # .EXAMPLE            ¦            ->      # .EXAMPLE            ¦
    #   Get-Greeting -name Sam                 #   Get-Greeting -name Sam
    #   Gets a greeting for "Sam"              #   Gets a greeting   ¦
                          ¦                    #   for "Sam"         ¦

Extra code lines can also be declared by starting them with `PS C:\>`. A line
starting with this won't be wrapped.

    # .EXAMPLE        ¦          ->      # .EXAMPLE        ¦
    #   PS C:\>$a = 123                  #   PS C:\>$a = 123
    #   PS C:\>$b = 456                  #   PS C:\>$b = 456
    #   PS C:\>$c = $a + $b              #   PS C:\>$c = $a + $b
    #                 ¦                  #                 ¦
    #   This line is still               #   This line is  ¦
    #   wrapped.      ¦                  #   still wrapped.¦


Other lines are processed as markdown, so large code or output sections can also
be indented 4 spaces.

    <#                     ¦                  <#                     ¦
     .EXAMPLE              ¦                  ·.EXAMPLE              ¦
                           ¦                                         ¦
       Get-User -name [D-E]*                     Get-User -name [D-E]*
                           ¦                                         ¦
       Gets all users with names      ->         Gets all users with ¦
       beginning D-E:      ¦                     names beginning D-E:¦
                           ¦                                         ¦
           Name      Password                        Name      Password
           ----      --------                        ----      --------
           Dave      123456                          Dave      123456
           Eric      Password1!                      Eric      Password1!
    #>                     ¦                  #>                     ¦
