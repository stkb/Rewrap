# Markdown: tables

> language: markdown

Tables are not in commonmark, and the minimum that represents a table varies greatly
between markdown flavors. Some require separator rows (`| --- | --- |`), some don't, and
some specify where they can occur

- [Github Flavored Markdown spec](https://github.github.com/gfm/#tables-extension-)

Rewrap has quite a permissive specification to try to accommodate all of thee. To
differentiate from normal paragraph text, the following is considered a table:
- Two or more consecutive lines that:
    - are indented no more than 3 spaces relative to the parent block, and
    - contain at least one pipe (`|`) character not preceded by a backslash (`\`)
- And, at least one of the lines contains only the characters `|` `:` `-` and space, with
  at least 1 `|` and one `-`.

    a | table                  ¦      ->      a | table                  ¦
    - | -                      ¦              - | -                      ¦

    not   | a                  ¦      ->      not   | a table | no       ¦
    table | no                 ¦                                         ¦

A table can interrupt a paragraph.
