## Plain text ##

> language: "plaintext"

An empty selection selects a whole paragraph (and only that paragraph)

    Line1«»        ¦      ->      Line1 Line2
    Line2          ¦                  
                                  Line3
    Line3          ¦              Line4
    Line4          ¦              

Multiple selections (alt+click) can be used to select multiple paragraphs

    Line1«»        ¦      ->      Line1 Line2
    Line2          ¦                  
                                  Line3 Line4
    Line3          ¦                  Line5
    Li«»ne4        ¦                  Line6
        Line5      ¦
        Line6      ¦

A single selection can too.

    «Line1         ¦      ->      Line1 Line2
    Line2          ¦                  
                                  Line3 Line4
    Line3          ¦                  Line5
    Line4»         ¦                  Line6
        Line5      ¦
        Line6      ¦

If just a few lines from a paragraph are selected, only those lines will be wrapped.

    Line1                ¦      ->      Line1
    «Line2               ¦              Line2 Line3
    Line3»               ¦              Line4
    Line4                ¦

Rewrap works only with whole lines, so a (non-empty) selection of part of a line is the same as selecting the whole line.

    Line1       ¦      ->      Line1
    Line«2»     ¦              Line2
    Line3       ¦              Line3

Multiple line selections in the same paragraph is fine.

    «Wrap» this¦line      ->      Wrap this¦
    Don't wrap this               line     ¦
    Wrap «this»¦line              Don't wrap this
               ¦                  Wrap this¦
               ¦                  line     ¦

## Comments ##

With wholeComment: true (default), all paragraphs in a comment block will be wrapped when an empty selection is present in it.

> language: "c", wholeComment: true

    // First paragraph«»         ->      // First
    //             ¦                     // paragraph
    // Second paragraph                  //
    bool checkFunctions() {              // Second 
                                         // paragraph
                                         bool checkFunctions() {

With wholeComment: false, only that paragraph will be wrapped

> language: "c", wholeComment: false

    // First paragraph«»         ->      // First
    //             ¦                     // paragraph
    // Second paragraph                  //
    bool checkFunctions() {              // Second paragraph
                                         bool checkFunctions() {

## Comments and code ##

In source code files, with a mix of comments and code, comments always take
precedence in selections. So if one or more comments appear in a selection, only
the comments will be wrapped and the code will be untouched.

> language: "javascript"

    «// Checks answer is valid.          ->    // Checks answer is
    function isValid (answer)                  // valid.
    {                     ¦                    function isValid (answer)
      // First strip off whitespace            {
      answer = answer.trim();                    // First strip off
    »                     ¦                      // whitespace
                          ¦                      answer = answer.trim();
    
