> language: "julia"

Julia is like Python with slight differences. There are only triple double-quote
strings (no triple single-quote). These can effectively be preceeded by any
user-defined string

    r"""¦    ->        r"""¦
    a b c              a b ¦
    d   ¦              c d ¦
    """ ¦              """ ¦

    doc"""¦    ->      doc"""
    a b c d            a b c ¦
    e     ¦            d e   ¦
    """   ¦            """   ¦

This can also be preceeded by the `@doc` macro

    @doc raw"""¦     ->      @doc raw"""
    a b c d e f g            a b c d e f
    h i        ¦             g h i      ¦
    """        ¦             """        ¦
