# Dart

DartDoc comments can be in the form `///` or `/** ... */`. It generally does not
use tags, except for a few special ones: @nodoc, @template, @endtemplate and
@macro.

> language: "dart"

    /// aaaaaaaa        ¦      ->      /// aaaaaaaa bbbbbb ¦
    /// bbbbbb c        ¦              /// c
    /// @nodoc          ¦              /// @nodoc
    /// dddddddd        ¦              /// dddddddd eeeeee
    /// eeeeee f        ¦              /// f
    /// {@template a}   ¦              /// {@template a}
    /// gggggggg        ¦              /// gggggggg hhhhhh
    /// hhhhhh i        ¦              /// i
    /// {@endtemplate}  ¦              /// {@endtemplate}
    /// jjjjjjjj        ¦              /// jjjjjjjj kkkkkk
    /// kkkkkk l        ¦              /// l
    /// {@macro a}      ¦              /// {@macro a}
    /// mmmmmmmm        ¦              /// mmmmmmmm nnnnnn
    /// nnnnnn o        ¦              /// o
