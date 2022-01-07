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

Triple-slash comments expect xml (https://www.doxygen.nl/manual/xmlcmds.html)

    /// <summary> What the method    ->      /// <summary> What the    ¦
    /// does </summary>       ¦              /// method does </summary>¦
    /// <param name="s">      ¦              /// <param name="s">      ¦
    /// The s param </param>  ¦              /// The s param </param>  ¦
    ///                       ¦              ///                       ¦
    /// <description>         ¦              /// <description>         ¦
    /// Extended info.        ¦              /// Extended info. Text   ¦
    /// Text text text.       ¦              /// text text.            ¦
    /// </description>        ¦              /// </description>        ¦
