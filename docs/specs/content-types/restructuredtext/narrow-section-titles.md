# reStructuredText: "Narrow" Section Titles

"Narrow" section titles are those where the overline if present, otherwise the underline,
are <= 3 characters in width. The distinction is made here, because if these are not
correctly formed, they are treated as paragraph text and therefore wrapped, whereas wider
section titles that are invalid are not wrapped.

> language: reStructuredText

### With underline only

The underline must be at least as wide as the title text. Less than that and it will be
taken as paragraph text.

    abc     ¦                          ->     abc     ¦
    ###     ¦                                 ###     ¦

    title            ¦                 ->     title ###        ¦
    ###              ¦                                         ¦

Even a 1-character title is allowed

    a       ¦                          ->     a       ¦
    -       ¦                                 -       ¦

Double-width characters are taken into account. They take 2 underline characters.

    再      ¦                                 再=     ¦
    =       ¦                                         ¦
            ¦                          ->     再      ¦
    再      ¦                                 ==      ¦
    ==      ¦                                         ¦


#### (Un)indented underline

    text text text                            text text   ¦
     ###        ¦                      ->     text        ¦
                ¦                              ###        ¦

     text text text                            text text  ¦
    ###         ¦                      ->      text       ¦
                ¦                             ###         ¦

### With over and underline

The over and underline must be the same length. If not, everything is taken as paragraph
content instead.

    ***                ¦                      *** Title *****    ¦
    Title              ¦               ->                        ¦
    *****              ¦                                         ¦

If the overline is < 4 characters, then this is interpreted as: a paragraph of punctuation
characters, a blockquote, and an invalid section title with just an overline.

    ===               ¦                       ===               ¦
    ·This is now a blockquote                 ·This is now a    ¦
    =====             ¦                ->      blockquote       ¦
    invalid section title                     =====             ¦
    with just overline¦                       invalid section title
                                              with just overline¦

## Parsing Strategy

There are lots of permutations. This runs through the cases where a punctuation line is
discovered that is < 4 characters. It can possibly be a valid section title if the title
text and underline are also the same length. If not valid, it will either be normal
paragraph text or a combination of other body elements.


### Overline indented

First taking the case that the overline is indented. This can never be a section title
(valid or invalid). The rules for other content are followed instead.

What looks like a section title is instead parsed as a normal paragraph, no matter the
length of the text or underline.

    ·===             ¦                        ·=== abc ===     ¦
     abc             ¦                 ->                      ¦
     ===             ¦                                         ¦

    ·===             ¦                        ·=== abcd ====   ¦
     abcd            ¦                 ->                      ¦
     ====            ¦                                         ¦

As long as the second line is at the same indent, it's a continuation of the paragraph, as
normal.

If the second line has a different indent, then it starts a new block.


### Overline not indented

If we found a possible overline that was short and not indented then we have to inspect
the next line.

#### 2nd line indented

We take first the case that that line is indented, because it's simpler. It almost always
means a new block started (as is normally the case with a difference in indent).

    ===     ¦                                 ===     ¦
     text text                         ->      text   ¦
            ¦                                  text   ¦

    ===     ¦                                 ===     ¦
     * text text                       ->      * text ¦
            ¦                                    text ¦

    ===     ¦                                 ===     ¦
     | text text                       ->      | text ¦
            ¦                                    text ¦

For almost all cases we can forget the possibility of a section title and continue
processing. There is only one case where it is still possible, where the line content ends
at or before the overline (regardless of what that content is).

    ===     ¦                          ->     ===     ¦
     ab     ¦                                  ab     ¦

    ===     ¦                          ->     ===     ¦
     ==     ¦                                  ==     ¦


Here we have to keep in mind that it can still become a section title with the correct
underline. If the third line is an underline that matches the overline exactly, then we
have a section title (not wrapped).

    ===     ¦                                 ===     ¦
     ab     ¦                          ->      ab     ¦
    ===     ¦                                 ===     ¦

But in all other cases, it's processed as a paragraph that begins on the second line.

    ===     ¦                          ->     ===     ¦
     ab     ¦                                  ab more¦
     more text                                 text   ¦

This includes the possibility that it is an (invalid) section title after all, with just
an underline.

    ===     ¦                                 ===     ¦
     ab     ¦                                  ab     ¦
     ==     ¦                          ->      ==     ¦
     more text                                 more   ¦
            ¦                                  text   ¦

    ===     ¦                                 ===     ¦
     ab     ¦                                  ab     ¦
     ^^^^   ¦                          ->      ^^^^   ¦
     more text                                 more   ¦
            ¦                                  text   ¦

And of course, a difference in indent starts a new block.

    ===     ¦                                 ===     ¦
     ab     ¦                                  ab     ¦
    more text                          ->     more    ¦
            ¦                                 text    ¦

    ===     ¦                                 ===     ¦
     ab     ¦                                  ab     ¦
      + item text                      ->       + item¦
            ¦                                     text¦

#### 2nd line not indented

Back to the second line, for the case that it's not indented.

Firstly, it's possible that this line is a valid underline. These examples produce a
section title "===", and we would continue processing anew from the line after that.

    ===              ¦                 ->     ===              ¦
    ===              ¦                        ===              ¦
    paragraph        ¦                        paragraph text   ¦
    text             ¦                                         ¦

    ===              ¦                 ->     ===              ¦
    &&&&             ¦                        &&&&             ¦
    paragraph        ¦                        paragraph text   ¦
    text             ¦                                         ¦

If it's anything else, then we currently have wrappable paragraph text.

    ===              ¦                 ->     === abc          ¦
    abc              ¦                                         ¦

    ===              ¦                 ->     === abcd         ¦
    abcd             ¦                                         ¦

    ===              ¦                 ->     === * a          ¦
    * a              ¦                                         ¦

    ===              ¦                 ->     === | a          ¦
    | a              ¦                                         ¦

But as above, we still have the possibility of a section title as long as the 2nd line
isn't longer than the first.

##### With a (not indented) underline

With text not indented, but ending past the overline, it's all taken as a paragraph,
regardless of the underline length

    ===              ¦                        === abcd ===     ¦
    abcd             ¦                 ->                      ¦
    ===              ¦                                         ¦

    ===              ¦                        === abcd ====    ¦
    abcd             ¦                 ->                      ¦
    ====             ¦                                         ¦

With text not indented and within the overline: if the underline length is the same as the
overline length, it's a valid section title (not wrapped).

    ~~~              ¦                        ~~~              ¦
    abc              ¦                        abc              ¦
    ~~~              ¦                 ->     ~~~              ¦
    paragraph        ¦                        paragraph text   ¦
    text             ¦                                         ¦

If the underline is any other length, it's all paragraph text

    ~~~              ¦                        ~~~ abc ~~       ¦
    abc              ¦                 ->                      ¦
    ~~               ¦                                         ¦

    ~~~              ¦                        ~~~ abc ~~~~     ¦
    abc              ¦                 ->                      ¦
    ~~~~             ¦                                         ¦

The text can be indented as long as (1) its end column isn't further than that of the
overline and (2) the over- and underlines are the same length.

    ~~~              ¦                        ~~~              ¦
     bc              ¦                         bc              ¦
    ~~~              ¦                 ->     ~~~              ¦
    paragraph        ¦                        paragraph text   ¦
    text             ¦                                         ¦

If either the text is too long, or the underline a different length, then both the text
and the underline start new blocks (if the underline is >= 4 characters, it'll be a
transition line).

    ~~~              ¦                        ~~~              ¦
     bc              ¦                         bc              ¦
    ~~               ¦                 ->     ~~ text          ¦
    text             ¦                                         ¦

    ~~~              ¦                        ~~~              ¦
     bc              ¦                         bc              ¦
    ~~~~             ¦                 ->     ~~~~             ¦
    text             ¦                        text             ¦

    ~~~        ¦                              ~~~        ¦
     title too long                            title too ¦
    ~~~        ¦                       ->      long      ¦
    text       ¦                              ~~~ text   ¦
    text       ¦                              text       ¦

##### With an indented underline

An undented underline doesn't count as part of the section title. So the results for the
first two lines will be the same as with no underline. The underline, depending on its
length and the content following, then starts a new /possible section
title with overline or transition or normal paragraph.

    ===              ¦                        === abc          ¦
    abc              ¦                 ->      == bc           ¦
     ==              ¦                                         ¦
     bc              ¦                                         ¦
