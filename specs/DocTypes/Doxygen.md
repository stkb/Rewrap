> language: "c++"

    /**    ¦       ->      /**    ¦
     * a b c                * a b ¦
     * @t d e               * c   ¦
     */    ¦                * @t d¦
                            * e   ¦
                            */    ¦

    /*!    ¦       ->      /*!    ¦
     * a b c                * a b ¦
     * @t d e               * c   ¦
     */    ¦                * @t d¦
                            * e   ¦
                            */    ¦

    //! a b c      ->      //! a b ¦
    //! @t d¦e             //! c   ¦
            ¦              //! @t d¦
            ¦              //! e   ¦

    /// a b c      ->      /// a b ¦
    /// @t d¦e             /// c   ¦
            ¦              /// @t d¦
            ¦              /// e   ¦