# Extra Features

## Wrap at custom column command

(VS Code only)

The <sn>Rewrap/Unwrap Text at Column...</sn> command lets you do a single wrap at the
given custom column value. You can also use it to "unwrap" text ([see below](#unwrapping).)


## Multiple rulers set

If you have multiple rulers set, you can choose which ruler to wrap to while editing.

<img src="../images/rulers1.gif" width="700" alt="cycles through rulers" />

Just press Alt+Q multiple times to wrap to each ruler in turn. The ruler chosen is then
remembered for that document for the rest of the session. It cycles through the rulers in
the order in which they appear in settings; so if you have a most commonly used ruler, you
want to put that first.

### VS Code

Example setting:

``` js
{
  "editor.rulers": [80, 72, 100]
}
```

### Visual Studio

[Add rulers using the Editor Guidelines extension
](configuration-visual-studio.md/#wrapping-to-rulers). It cycles through the rulers in the
order they were added.


## Unwrapping

(VS Code only)

Sometimes you might want to "unwrap" text so it's no longer hard wrapped.

This is actually just the same as re-wrapping with an infinite (or very large) wrapping
column setting. There are two ways to achieve this.

For occasional use, use the <sn>Rewrap/Unwrap Text at Column...</sn> command (search for
'unwrap') mentioned above. Then make sure the input field is blank and press Enter. This
will remove wrapping.

The second way is to add an additional ruler with a value of <sv>0</sv> (this will be
taken as infinite wrapping column), and then use the "multiple rulers" feature above to
switch to this ruler and unwrap instead.

``` js
{
  "editor.rulers": [80, 0]
}
```

However that ruler at column zero gives you a quite ugly line at the left of the editor.
But it can be solved by setting the ruler to a transparent color.

``` js
{
  "editor.rulers": [
    { "column": 80, "color": "#aaa" },
    { "column": 0, "color": "#0000" } // with alpha-channel 0 = transparent
  ]
}
```


## Preserving line breaks

You can add two spaces to the end of any line, and the line-break after it will be
preserved. [(More info)](specs/features/spaces.md#at-the-end-of-a-line)
