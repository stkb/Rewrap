> language: "graphql"

GraphQL supports comments (`#`) and Block Strings (`"""`). Comments are always
single-line, while Block Strings are inherently multi-line. Block Strings are
typically used to provide descriptions (documentation) for GraphQL types and
operations.

## Comments

    # a b c d e f g h i     ->      # a b c d¦
             ¦                      # e f g h¦
             ¦                      # i      ¦

## Block Strings

    """      ¦              ->     """
    a b c d e f g h i              a b c d e
    """      ¦                     f g h i
             ¦                     """
