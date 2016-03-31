Markdown has a few features. The first one is that paragraphs are separated by a blank line.

This line has 2 spaces at the end. In this case it's treated as a line-break within a paragraph in markdown.  
Therefore the line needs to be wrapped separately. The 2 spaces at the end of
the line also need to be preserved.

This paragraph has a list that comes right after it. It's wrapped without the list being affected.
* This is a list item with a star. This should be wrapped too. Text on subsequent lines will be indented the same level as text on the first.
-  This is a list item with a dash. This should be wrapped too. The text has been indented an extra space, so following lines will copy that.
+ This is a list item with a plus. This should be wrapped too. a b c d e f g h i j k l m n o p q r s t u v w x y z.
    + This list item has been indented. a b c d e f g h i j k l m n o p q r s t u v w x y z.
* And this is another list item.

> Block quote.
2nd line of block quote

> Another block quote.
> This line also has a >

### Headings are ignored, even if the lines are too long, because they can only be on 1 line in markdown

Headings with underlines too. a b c d e f g h i j k l m n o p q r s t u v w x y z.
----------------------------------------------------------------------------------

And also with a double underline. a b c d e f g h i j k l m n o p q r s t u v w x y z.
======================================================================================

```
Code blocks are left alone too.
    Unless they are specifically selected
```

| This | is | a | table
|------|----|---|------
| It's | ignored