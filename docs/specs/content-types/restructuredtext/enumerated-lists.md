# reStructuredText: Enumerated lists

Enumerated (or numbered) lists behave similarly to [bullet lists](bullet-lists.md). The
rules for list item markers are more complex than for example markdown, however.

The marker format for items can be `X.`, `X)` or `(X)` where `X` is the enumerator. The
various accepted emumerators are given below.

> language: "reStructuredText"

Arabic numerals: 1, 2, 3 ... (no upper limit in RST, though we have the limit of a signed
int32).

    1) item one text                          1) item one ¦
    2) item     ¦                      ->        text     ¦
       two      ¦                             2) item two ¦

    999999998) item one text                  999999998) item one ¦
    999999999) item     ¦              ->                text     ¦
               two      ¦                     999999999) item two ¦

Letters: A ,B, C ... Z. Upper or lower-case, but cannot mix case within a list.

    A. item one text                          A. item one ¦
    B. item     ¦                      ->        text     ¦
       two      ¦                             B. item two ¦

    a. item one text                          a. item one ¦
    b. item     ¦                      ->        text     ¦
       two      ¦                             b. item two ¦

Roman numerals: I, II, III, IV ... MMMMCMXCIX (4999). Again can be upper or lower case,
but can't be mixed.

    (i)  item one text                        (i)  item one  ¦
    (ii) item      ¦                   ->          text      ¦
         two       ¦                          (ii) item two  ¦

    (MMMMCMXCVIII) item one text               (MMMMCMXCVIII) item one  ¦
    (MMMMCMXCIX)   item      ¦         ->                     text      ¦
                   two       ¦                 (MMMMCMXCIX)   item two  ¦

## Auto enumerator

A `#` can be used in place of an enumerator.

    #. item one text                          #. item one ¦
    #. item     ¦                      ->        text     ¦
       two      ¦                             #. item two ¦

## Validation

While enumerated lists don't have to start with "1" or equivalent, following items must
follow sequentially ("2", "3" etc). If they don't, then they are not considered part of
the same list.

If there are blank lines between list items, this has no effect on wrapping. But if there
are no blank lines, the whole text is considered a normal paragraph instead.

This rule is not yet enfored in Rewrap.
