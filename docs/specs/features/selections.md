# Selections

In the examples on this page, `«` represents the start of a selection and `»`
the end. These characers are removed from the input.


## Plain text

> language: "plaintext-indentseparated"

An empty selection selects a whole paragraph (and only that paragraph)

    Line1«»        ¦      ->      Line1 Line2    ¦
    Line2          ¦                             ¦
                   ¦              Line3          ¦
    Line3          ¦              Line4          ¦
    Line4          ¦                             ¦

Multiple selections (alt+click) can be used to select multiple paragraphs

    Line1«»        ¦      ->      Line1 Line2    ¦
    Line2          ¦                             ¦
                   ¦              Line3 Line4    ¦
    Line3          ¦                  Line5      ¦
    Li«»ne4        ¦                  Line6      ¦
        Line5      ¦                             ¦
        Line6      ¦                             ¦

A single selection can too.

    «Line1         ¦      ->      Line1 Line2    ¦
    Line2          ¦                             ¦
                   ¦              Line3 Line4    ¦
    Line3          ¦                  Line5      ¦
    Line4»         ¦                  Line6      ¦
        Line5      ¦                             ¦
        Line6      ¦                             ¦

If just a few lines from a paragraph are selected, only those lines will be wrapped.

    Line1                ¦      ->      Line1                ¦
    «Line2               ¦              Line2 Line3          ¦
    Line3»               ¦              Line4                ¦
    Line4                ¦                                   ¦

Rewrap works only with whole lines, so a (non-empty) selection of part of a line is the same as selecting the whole line.

    Line1       ¦         ->      Line1       ¦
    Line«2» texttext              Line2       ¦
    Line3       ¦                 texttext    ¦
                ¦                 Line3       ¦

Multiple line selections in the same paragraph is fine.

    «Wrap» this line      ->      Wrap this ¦
    Don't wrap¦this               line      ¦
    Wrap «this» line              Don't wrap¦this
              ¦                   Wrap this ¦
              ¦                   line      ¦


## Comments ##

With wholeComment: true (default), all paragraphs in a comment block will be wrapped when an empty selection is present in it.

> language: "c", wholeComment: true

    // First paragraph«»         ->      // First       ¦
    //             ¦                     // paragraph   ¦
    // Second paragraph                  //             ¦
    bool checkFunctions() {              // Second      ¦
                   ¦                     // paragraph   ¦
                   ¦                     bool checkFunctions() {

With wholeComment: false, only that paragraph will be wrapped

> language: "c", wholeComment: false

    // First paragraph«»         ->      // First       ¦
    //             ¦                     // paragraph   ¦
    // Second paragraph                  //             ¦
    bool checkFunctions() {              // Second paragraph
                   ¦                     bool checkFunctions() {

## Comments and code ##

> language: "javascript"

In source code files, code is never wrapped, even if it's the only thing
selected.

    // Comment       ¦           ->      // Comment       ¦
    // text          ¦                   // text          ¦
    bool «»checkFunctions()              bool «»checkFunctions()

So a selection can cover code and comments.

    «// Checks answer is valid.          ->    // Checks answer is   ¦
    function isValid (answer)                  // valid.             ¦
    {                     ¦                    function isValid (answer)
      // First strip off whitespace            {                     ¦
      answer = answer.trim();                    // First strip off  ¦
    »                     ¦                      // whitespace       ¦
                          ¦                      answer = answer.trim();


## More specs ##


> language: "plaintext"

Multiple selections on the same line count as just one selection.

    Don't wrap this         ->      Don't wrap this
    «Wrap» «this» line              Wrap this¦
    Don't wrap this                 line     ¦
             ¦                      Don't wrap this

The same as multiple empty selections in the same paragraph.

    Li«»ne1                ¦      ->      Line1 Line2 Line3 Line4¦
    Line2                  ¦
    Li«»ne3                ¦
    Line4                  ¦

If a paragraph has an empty selection and a non-empty selection, the empty
selection takes precedence and the whole paragraph is wrapped

    «Line1»                ¦      ->      Line1 Line2 Line3 Line4¦
    Line2                  ¦
    Li«»ne3                ¦
    Line4                  ¦
