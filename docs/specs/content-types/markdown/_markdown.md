# Markdown

Markdown (mostly based on the [CommonMark spec](https://spec.commonmark.org/latest)) is
used for the contents of code comments (in most languages), as well as normal Markdown
files.

> language: "markdown"

## Paragraphs

([details](paragraphs.md))

Long lines are split at whitespace and wrapped.

    A long line.      ->      A long  ¦
            ¦                 line.   ¦

Short lines are concatenated and then wrapped as appropriate.

    Three.        ¦      ->      Three. Short. ¦
    Short.        ¦              Lines.        ¦
    Lines.        ¦

Normal paragraphs are separated by spaces.

    Paragraph one.      ->      Paragraph  ¦
               ¦                one.       ¦
    Para       ¦                           ¦
    two.       ¦                Para two.  ¦

A line break can be forced within a paragraph by ending a line with a single `\`
or two spaces.

    A                ¦      ->      A break\         ¦
    break\           ¦              Another line     ¦
    Another line break··            break··          ¦
    More text        ¦              More text        ¦

Line breaks are also preserved after a line that ends in `<br>` or `<br/>`

    HTML           ¦              HTML line      ¦
    line break<br> ¦      ->      break<br>      ¦
    HTML           ¦              HTML line      ¦
    line break<br/>¦              break<br/>     ¦
    Text           ¦              Text           ¦


## Markdown features ##

Any line without text (used for decoration) is left alone.

    ###############      ->      ###############
    ***************              ***************
    ===============              ===============
    Text text¦text               Text text¦
    ---------------              text     ¦
             ¦                   ---------------

ATX headings. The text must be on a single line, so these are left alone too.

    ### Heading style 1 ¦          ->      ### Heading style 1 ¦
    ### With trailing #'s ###              ### With trailing #'s ###

However setext headings (with underlines) can be wrapped. But the underline
remains unchanged (this is maybe something that could be added in the future).

    Heading with underline      ->      Heading with         ¦
    ---------------------¦              underline            ¦
    With double          ¦              ---------------------¦
    underline            ¦              With double underline¦
    ==================   ¦              ==================   ¦

## Bullet lists

([details](lists.md))

They can immediately follow a paragraph.

    List items:    ¦              ->      List items:    ¦
    - With dash    ¦                      - With dash    ¦
    * With star    ¦                      * With star    ¦
    + With plus text wrapped              + With plus    ¦
                   ¦                        text wrapped ¦

## Numbered lists

    1) Item 1                 ¦      ->      1) Item 1                 ¦
    2. Item 2                 ¦              2. Item 2                 ¦
    999999999) Item 999999999 ¦              999999999) Item 999999999 ¦


## Block quotes

([details](blockquotes.md))

    > one two three      ->      > one two   ¦
    four        ¦                three four  ¦

    > one two three      ->      > one two   ¦
    > four      ¦                > three four¦

## Indented code blocks

([details](indented-code-blocks.md))


    ····a = 1;   ¦      ->      ····a = 1;   ¦
    ····b = 2;   ¦              ····b = 2;   ¦
    text text text              text text    ¦
                 ¦              text         ¦

## Fenced code blocks

([details](fenced-code-blocks.md))

Can come directly after a paragraph.

    Some       ¦          ->      Some text  ¦
    text       ¦                  ``` c      ¦
    ``` c      ¦                  a = 1;     ¦
    a = 1;     ¦                  b = 2;     ¦
    b = 2;     ¦                  ```        ¦
    ```        ¦                  ··~~~ javascript
    ··~~~ javascript              a = 1;     ¦
    a = 1;     ¦                  b = 2;     ¦
    b = 2;     ¦                  ·~~~       ¦
    ·~~~       ¦                             ¦


## Other

- [HTML](html.md)
- [Footnotes](footnotes.md) and [Link Reference Definitions](linkrefdefs.md)
- [Comments](comments.md)
- [Front Matter](front-matter.md)
