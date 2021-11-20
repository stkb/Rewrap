# reStructuredText: Bullet Lists

> language: reStructuredText

There must be whitespace after the bullet marker.

    *not a bullet item                 ->     *not a bullet ¦
                  ¦                           item          ¦


## Child content

A bullet item can have from the first line: a blank line, a line block, another bullet
item, a (ignored) transition line, or a normal paragraph.

A bullet item can contain a sub bullet item on the first line.

    + + This is a sub-item             ->     + + This is a  ¦
                   ¦                              sub-item   ¦

The text indent for the first line determines the base indent for all content.

    *       Text can be indented any          *       Text can be indented¦
            amount from the bullet                    any amount from the ¦
            point as long       ¦      ->             bullet point as long¦
            as it's all         ¦                     as it's all indented¦
            indented the same amount                  the same amount     ¦

    + First paragraph    ¦                    + First paragraph    ¦
       This is a         ¦             ->        This is a separate¦
       separate paragraph¦                       paragraph         ¦


## Content before

A bullet item can't come straight after a paragraph without a blank line inbetween. It
will be treated as part of the paragraph.

    paragraph text       ¦             ->     paragraph text * not ¦
    * not a bullet       ¦                    a bullet             ¦

However, as always, that only applies if they're at the same indent. If the paragraph
and list item are at different indents, then they're separate.

    paragraph      ¦                          paragraph text ¦
    text           ¦                   ->      - bullet in a ¦
     - bullet in a definition                    definition  ¦


## Content after

    - A paragraph        ¦                    - A paragraph in a   ¦
      in a bullet point  ¦                      bullet point       ¦
     but this is a       ¦                     but this is a new   ¦
     new paragraph       ¦              ->     paragraph           ¦
                         ¦                                         ¦


## Sibling and sub-items

Multiple list items don't require a blank line between them.

    * first     ¦                             * first item¦
      item      ¦                      ->     * second    ¦
    * second item                               item      ¦

    * first     ¦                             * first item¦
      item      ¦                      ->      * second   ¦
     * second item                               item     ¦

     * first     ¦                             * first item¦
       item      ¦                     ->     * second     ¦
    * second item.                              item.      ¦

However when the bullet marker is at the same text indent as the first line, it will be
treated as the same paragraph.

    - Line one           ¦             ->     - Line one - This is ¦
      - This is the same paragraph              the same paragraph ¦

Except if the first line is blank

    +                 ¦                       +                 ¦
     + This is a new paragraph                 + This is a new  ¦
                      ¦                          paragraph      ¦
    *                 ¦                ->                       ¦
      * This is a new paragraph               *                 ¦
                      ¦                         * This is a new ¦
                      ¦                           paragraph     ¦
