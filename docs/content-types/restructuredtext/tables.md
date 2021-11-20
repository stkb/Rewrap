# reStructuredText: Tables

> language: "reStructuredText"

No wrapping is attempted on the contents of tables. They are left as-is.

## Grid tables

Grid tables are detected by a line that starts and ends with `+`, with 3 `-` inbetween,
and then a second line with the same indent, that starts with `|`. Lines after that must
start with `|` or `+`, and be at the same indent.

    +---+         ¦                           +---+         ¦
    | a |         ¦                           | a |         ¦
    +---+         ¦                    ->     +---+         ¦
                  ¦                                         ¦
    normal paragraph                          normal        ¦
                  ¦                           paragraph     ¦

An indented grid table (ie: in a blockquote) is fine.

      +---+       ¦                             +---+       ¦
      | a |       ¦                             | a |       ¦
      +---+       ¦                    ->       +---+       ¦
                  ¦                                         ¦
    normal paragraph                          normal        ¦
                  ¦                           paragraph     ¦

Like everything else, grid tables can't interrupt a paragraph at the same indent.
Everything will be treated as normal paragraph text.

    normal paragraph                          normal         ¦
    +---+          ¦                   ->     paragraph +---+¦
    | a |          ¦                          | a | +---+    ¦
    +---+          ¦                                         ¦

At a different indent does work (though may give a warning)

    normal paragraph                          normal        ¦
      +---+       ¦                           paragraph     ¦
      | a |       ¦                    ->       +---+       ¦
      +---+       ¦                             | a |       ¦
                  ¦                             +---+       ¦

      normal paragraph                          normal      ¦
    +---+         ¦                             paragraph   ¦
    | a |         ¦                    ->     +---+         ¦
    +---+         ¦                           | a |         ¦
                  ¦                           +---+         ¦

A table ends as soon as a line is encountered that doesn't start with `|` or `+` or has a
different indent. A normal paragraph *can* therefore come directly after a table (with a
warning).

    +---+         ¦                           +---+         ¦
    | a |         ¦                           | a |         ¦
    +---+         ¦                    ->     +---+         ¦
    normal paragraph                          normal        ¦
                  ¦                           paragraph     ¦

If a line that looks like the start of a table is followed by a line that doesn't continue
the table, the first line is ignored.

    +---+         ¦                           +---+         ¦
    normal paragraph                   ->     normal        ¦
                  ¦                           paragraph     ¦


## Simple tables

Simple tables begin with a line that contains 2 sets of at least one `=` character,
separated by at least one space. They end if a line at the same indent begins with `=` and
has a blank line following.

    =====  =====  =======          ¦          =====  =====  =======          ¦
      A      B    A and B          ¦            A      B    A and B          ¦
    =====  =====  =======          ¦          =====  =====  =======          ¦
    False  False  False            ¦   ->     False  False  False            ¦
    True   False  False            ¦          True   False  False            ¦
    False  True   False            ¦          False  True   False            ¦
    True   True   True             ¦          True   True   True             ¦
    =====  =====  =======          ¦          =====  =====  =======          ¦

It doesn't matter to us whether the table is malformed or not since the result is the
same: it's not wrapped.

A complete table because a blank line comes after the 2nd `= =`.

    = =          ¦                            = =          ¦
    = =          ¦                            = =          ¦
                 ¦                     ->                  ¦
    normal       ¦                            normal       ¦
    text         ¦                            text         ¦

If the closing line is followed by a blank line, its only conditions are:
- It must not be indented
- It must consist only of `=` and whitespace characters

    = =          ¦                            = =          ¦
     = =         ¦                             = =         ¦
                 ¦                     ->                  ¦
    still        ¦                            still        ¦
    in table     ¦                            in table     ¦

    = =          ¦                            = =          ¦
    = %          ¦                            = %          ¦
                 ¦                     ->                  ¦
    still        ¦                            still        ¦
    in table     ¦                            in table     ¦

    = =          ¦                            = =          ¦
    =            ¦                            =            ¦
                 ¦                     ->                  ¦
    normal       ¦                            normal       ¦
    text         ¦                            text         ¦

    = =          ¦                            = =          ¦
    ====         ¦                            ====         ¦
                 ¦                     ->                  ¦
    normal       ¦                            normal       ¦
    text         ¦                            text         ¦

An internal line of `=`s may be present as long it is not followed as a blank line. This
denotes the end of the table header. If this line is present, then the next valid line of
`=`s will unconditionally end the table, meaning a blank line after is not needed.

    ===  ===  ===          ¦                  ===  ===  ===          ¦
    in1  in2  out          ¦                  in1  in2  out          ¦
    ===  ===  ===          ¦                  ===  ===  ===          ¦
     1    2    5           ¦           ->      1    2    5           ¦
     3    1    5           ¦           ->      3    1    5           ¦
    ===  ===  ===          ¦                  ===  ===  ===          ¦
    normal                 ¦                  normal text            ¦
    text                   ¦                                         ¦

    = =          ¦                            = =          ¦
    = =          ¦                            = =          ¦
    = =          ¦                     ->     = =          ¦
    normal       ¦                            normal text  ¦
    text         ¦                                         ¦


The rest of the way a simple table works cannot currently be supported by Rewrap. It scans
following lines (possibly the whole document) for a closing line, and if none is found,
gives lines back to be processed as other blocks

### Indented

Simple tables may be indented (inside a blockquote or other container).

Whenever a line is encountered with a lesser indent, it terminates the table.

    ·= =          ¦                           ·= =          ¦
    a b           ¦                    ->     a b text      ¦
    text          ¦                                         ¦
