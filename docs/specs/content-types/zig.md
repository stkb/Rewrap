> language: "zig"

Zig uses comment formats that might be familiar if you use Doxygen (C/C++) but there are
no multi-line block comments.

    // a b c       ->      // a b ¦
    // d   ¦               // c d ¦

Doc comments:

    /// a b c       ->      /// a b ¦
    /// d   ¦               /// c d ¦

Top-level doc comments:

    //! a b c       ->      //! a b ¦
    //! d   ¦               //! c d ¦
