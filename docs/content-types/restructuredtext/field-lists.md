# reStructuredText: Field Lists

> language: reStructuredText

A field list item has the form `:<fieldname>: <content>`

The line break before each field list item is preserved

    :Date: 2001-08-16           ¦      ->     :Date: 2001-08-16           ¦
    :Version: 1                 ¦             :Version: 1                 ¦


## Child content

The content of field list items is not restricted to one line. The initial paragraph can
multiple lines, with two conditions: the second line must be indented further than the
field name marker, and subsequent lines must have the same indent as the second line.

    :item: the content may                    :item: the content ¦
      spread over multiple             ->       may spread over  ¦
      lines.           ¦                        multiple lines.  ¦

This is in contrast to [bullet items](bullet-lists.md) and [enumerated
items](enumerated-lists.md), subsequent lines must have the same indent as that of the
first.

If a single line field list item is too long, wrapped lines are given a 4 space indent
relative to the field name marker.

    :Field: A line that's too long     ->     :Field: A line that's ¦
                          ¦                       too long          ¦

There must be whitespace between the closing `:` and the content.

    :this:is not a field list item            :this:is not a field ¦
                         ¦                    list item            ¦
    :this: is a field list item        ->                          ¦
                         ¦                    :this: is a field    ¦
                         ¦                        list item        ¦


## Field name

The field name may contain spaces, but not line breaks.

    :a field name: 1             ¦     ->     :a field name: 1             ¦
    :another field name: 2       ¦            :another field name: 2       ¦

The field name may contain `:` characters only if they are escaped with `\`.


## Address field

In an address field, content is taken as literal content.

    :Address: 999 Letsby Avenue               :Address: 999 Letsby Avenue
              Sheffield    ¦                            Sheffield    ¦
                           ¦           ->                            ¦
    text                   ¦                  text text              ¦
    text                   ¦                                         ¦

"Address" is case sensitive.

    :address: Not a real address       ->     :address: Not a real   ¦
                           ¦                      address            ¦

## Content before

A field list doesn't interrupt a paragraph

    paragraph            ¦                    paragraph text :abc: ¦
    text                 ¦             ->     123                  ¦
    :abc: 123            ¦                                         ¦

## Content after

A paragraph or any body content can come after a field list item (with a warning)

    :abc: 123             ¦                   :abc: 123             ¦
    paragraph             ¦            ->     paragraph text        ¦
    text                  ¦                                         ¦
