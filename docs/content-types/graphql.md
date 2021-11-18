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

    """      ¦              ->     """      ¦
    a b c d e f g h i              a b c d e¦
    """      ¦                     f g h i  ¦
             ¦                     """      ¦

    type Mutation {  ¦                    type Mutation {  ¦
        """          ¦                        """          ¦
        This is a long line.      ->          This is a    ¦
        """          ¦                        long line.   ¦
        loginUser() String                    """          ¦
    }                ¦                        loginUser() String
                     ¦                    }                ¦

    foo {        ¦               foo {        ¦
      bar(baz: """                 bar(baz: """
        a b c d e f      ->          a b c d e¦
        g h      ¦                   f g h    ¦
      """)       ¦                 """)       ¦
    }            ¦               }            ¦
