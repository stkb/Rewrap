# Batch/Cmd files

> language: "batch file"

Comments lines start with `REM` (or `@REM`). This is case-insensitive.

    rem text       ¦                  ->      rem text text  ¦
    rem text text  ¦                          rem text       ¦

    REM text       ¦                  ->      REM text text  ¦
    REM text text  ¦                          REM text       ¦

    Rem text       ¦                  ->      Rem text text  ¦
    Rem text text  ¦                          Rem text       ¦

    @rem text      ¦                  ->      @rem text text ¦
    @rem text text ¦                          @rem text      ¦

While not strictly a comment marker, `::` is also often used for comments, so is also
supported.

    ::   text      ¦                  ->      ::   text text ¦
    ::   text text ¦                          ::   text      ¦
