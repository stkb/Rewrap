# reStructuredText: Transitions and Section Titles

> language: reStructuredText

Transitions and underlines/overlines for section titles are made from a line of one of
any of the Ascii punctiation characters: `!"#$%&'()*+,-./:;<=>?@[\]^_``{|} ~`.


## Transitions

A transition line is created from ≥ 4 punctuation characters. Transitions must have a
blank line before and after. If they don't then they may be interpreted as either invalid,
or part of a section title. However that usually won't make a difference for wrapping.

    text       ¦                              text text  ¦
    text text  ¦                              text       ¦
               ¦                                         ¦
    ////       ¦                       ->     ////       ¦
               ¦                                         ¦
    text       ¦                              text text  ¦
    text again ¦                              again      ¦


## "Wide" section titles

This page covers "wide" section titles, where the overline if present, else the underline, is
at least 4 characters wide (ie. like a valid transition line). The behavior around these
is different from ["narrow" section titles](narrow-section-titles.md).

Since the text of section titles must be on a single line, it is never wrapped.

## With underline only

An underline can only come on the second line of a paragraph; ie. there can only be one
line of text above it. If there is more than one line, then the underline will be treated
as paragraph content, no matter how long.

    Title                   ¦                 Title text =====        ¦
    text                    ¦          ->                             ¦
    =====                   ¦                                         ¦


### Make-up

Whitespace is allowed after the underline

    Title        ¦                     ->     Title        ¦
    '''''·       ¦                            '''''·       ¦

The underline must be made up of the same character. If made from mixed characters it will
be treated as paragraph text, no matter how long.

    title        ¦                     ->     title +=+=+  ¦
    +=+=+        ¦                                         ¦

If there are any characters other than whitespace after the underline, everything will be
treated as paragraph text.

    title        ¦                     ->     title *****  ¦
    ***** something                           something    ¦

While the underline should be at least as long as the title text, anything ≥ 4 characters
will count.

    title            ¦                 ->     title            ¦
    ####             ¦                        ####             ¦


### (Un)indented underline

If the underline is at a different indent from the text, it doesn't count as an underline.
The text will be treated as normal paragraph text (wrapped). The punctuation line then
starts a new block, and will be taken as an (invalid) transition line, or possibly
overline for a new section title.

    text text text                            text text   ¦
     ####       ¦                      ->     text        ¦
                ¦                              ####       ¦

    text text text                            text text   ¦
     ##############                    ->     text        ¦
                ¦                              ##############

     text text text                            text text  ¦
    ####        ¦                      ->      text       ¦
                ¦                             ####        ¦

     text text text                            text text  ¦
    ###############                    ->      text       ¦
                ¦                             ###############


## With overline and underline

Section titles can have an optional overline.

    ~~~~~~~            ¦                      ~~~~~~~            ¦
    Title              ¦               ->     Title              ¦
    ~~~~~~~            ¦                      ~~~~~~~            ¦

The title text may be indented, as long as its right edge doesn't extend past that of the
lines.

    ======            ¦                       ======            ¦
    ·Title            ¦                       ·Title            ¦
    ======            ¦                ->     ======            ¦
    paragraph         ¦                       paragraph text    ¦
    text              ¦                                         ¦

If it does though it's just a warning.

    =====             ¦                       =====             ¦
    ·Title            ¦                       ·Title            ¦
    =====             ¦                ->     =====             ¦
    paragraph         ¦                       paragraph text    ¦
    text              ¦                                         ¦


### Invalid inputs (overline not indented)

There are lots of ways to make an "invalid" 3 line section title. Each of these cases will
produce an error in an RST processor, but as in all error cases we don't wrap the content.

If there is a text after the overline but a blank line after that. The indent and length
of the text doesn't matter.

    ====               ¦                      ====               ¦
    text               ¦                      text               ¦
                       ¦               ->                        ¦
    paragraph          ¦                      paragraph text     ¦
    text               ¦                                         ¦

    ====               ¦                      ====               ¦
     text              ¦                       text              ¦
                       ¦               ->                        ¦
    paragraph          ¦                      paragraph text     ¦
    text               ¦                                         ¦

If the second line is also an unindented punctuation line, these two lines will be ignored
and processing will continue at the line after them.

    ======                   ¦                ======                   ¦
    ======                   ¦         ->     ======                   ¦
    This is normal paragraph text             This is normal paragraph ¦
    again.                   ¦                text again.              ¦

Holds true no matter how long the second line.

    ======                   ¦                ======                   ¦
    =                        ¦         ->     =                        ¦
    This is normal paragraph text             This is normal paragraph ¦
    again.                   ¦                text again.              ¦

If the overline is followed by two lines with content, and the second of those is anything
other than a valid (unindented) punctuation line, all 3 lines will be ignored. Processing
will continue immediately on the line after that.

    ======                   ¦                ======                   ¦
    line 2                   ¦                line 2                   ¦
    line 3                   ¦         ->     line 3                   ¦
    This is normal paragraph text             This is normal paragraph ¦
    again.                   ¦                text again.              ¦

    ======                   ¦                ======                   ¦
     =                       ¦                 =                       ¦
    line 3                   ¦         ->     line 3                   ¦
    This is normal paragraph text             This is normal paragraph ¦
    again.                   ¦                text again.              ¦

A valid underline means:

Not indented.

    ======            ¦                       ======            ¦
    line 2            ¦                       line 2            ¦
     ======           ¦                ->      ======           ¦
    Normal paragraph text                     Normal paragraph  ¦
    again.            ¦                       text again.       ¦

Must be the same character as the overline.

    -------           ¦                       -------           ¦
    Title             ¦                ->     Title             ¦
    +++++++           ¦                       +++++++           ¦
    Normal paragraph text                     Normal paragraph  ¦
    again.            ¦                       text again.       ¦

Must be the same length as the overline.

    &&&&              ¦                       &&&&              ¦
    Title             ¦                ->     Title             ¦
    &&&               ¦                       &&&               ¦
    Normal paragraph text                     Normal paragraph  ¦
    again.            ¦                       text again.       ¦


### Invalid inputs (indented overline)

If the overline is indented then behavior is slightly different. It can never be a (valid
or invalid) 3 line section title, so the overline is taken as an invalid transition, and
subsequent lines processed after that.

This is seen therefore seen as an invalid transition followed by an invalid section title
with underline.

     =====            ¦                        =====            ¦
     Title            ¦                ->      Title            ¦
     =====            ¦                        =====            ¦
    paragraph         ¦                       paragraph text    ¦
    text              ¦                                         ¦

Here the lines after the overline are normal paragraph text

    ·======                  ¦                ·======                  ¦
     line 2                  ¦         ->     ·line 2 line 3           ¦
     line 3                  ¦                                         ¦

### Other invalid inputs

These inputs will cause the content to be interpreted as something other than a 3-line
section title.

If there are multiple text lines between the over and underline, the first 3 lines will be
taken as an invalid section title, and prcocessing continues after that.

    =====              ¦                      =====              ¦
    Title              ¦               ->     Title              ¦
    Text               ¦               ->     Text               ¦
    =====              ¦                      =====              ¦

If the overline is indented, that line will be an invalid transition, followed by a valid
section title with underline only.

    ·=====            ¦                       ·=====            ¦
    Title             ¦                       Title             ¦
    =====             ¦                ->     =====             ¦
    paragraph         ¦                       paragraph text    ¦
    text              ¦                                         ¦

If both are indented, it's no longer interpreted as a title, but a paragraph between two
invalid transitions, meaning the text will be wrapped.

    ·=====            ¦                       ·=====            ¦
    A much longer title                       A much longer     ¦
    ·=====            ¦                ->     title             ¦
    paragraph         ¦                       ·=====            ¦
    text              ¦                       paragraph text    ¦


## Content before

If a section title comes directly after paragraph (that is at least 2 lines) at the same
indent, everything is taken as paragraph text, no matter if the overline is present or the
over/underline length.

    paragraph    ¦                            paragraph    ¦
    text         ¦                            text =====   ¦
    =====        ¦                     ->     Title =====  ¦
    Title        ¦                                         ¦
    =====        ¦                                         ¦

If the paragraph is indented (blockquote), then the section title is valid (with or
without overline).

    ·text        ¦                            ·text text   ¦
    ·text        ¦                            !!!!!        ¦
    !!!!!        ¦                     ->     Title        ¦
    Title        ¦                            !!!!!        ¦
    !!!!!        ¦                                         ¦

    ·text        ¦                            ·text text   ¦
    ·text        ¦                     ->     Title        ¦
    Title        ¦                            """""        ¦
    """""        ¦                                         ¦


## Content after


A paragraph can come immediately after a section title without even a warning.

    Title           ¦                         Title           ¦
    $$$$$           ¦                  ->     $$$$$           ¦
    paragraph       ¦                         paragraph text  ¦
    text            ¦                                         ¦

## Title content

An underline takes precedence over a literal block introduction. So if a single text line
has a literal block marker (::), but then a valid underline, it will be section title with
normal "::" characters at the end instead. Subsequent lines will not be a literal block.

    Example::        ¦                        Example::        ¦
    ====             ¦                        ====             ¦
                     ¦                 ->                      ¦
      This is a blockquote                      This is a      ¦
                     ¦                          blockquote     ¦

If the text looks like another body element, eg. a bullet item, then adding an underline
will not make it into a title.

    * text text                               * text  ¦
    ===========                        ->       text  ¦
            ¦                                 ===========

But adding an overline too does.

    ===========                               ===========
    * text text                               * text text
    ===========                        ->     ===========
    text text                                 text    ¦
            ¦                                 text    ¦
