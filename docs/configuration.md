# Configuration

!!! info ""

    This is for VS Code. [Go here for configuration in Visual
    Studio](configuration-visual-studio.md).

Configure though the settings UI (<sn>Preferences: Open Settings (UI) > Rewrap section</sn>)
where possible, or add them to your [user or workspace settings file](
https://code.visualstudio.com/docs/getstarted/settings).

All settings can be set per language/file type ([see below](#language-specific-settings)).


## Summary

| Setting                                             | Json ID                               | Values (default first) |
| --------------------------------------------------- | --------------------------------      | ---------------------- |
| [Auto Wrap: Enabled](#enabling)                     | <si>rewrap.autoWrap.enabled</si>      | false \| true          |
| [Auto Wrap: Notification](#notification)            | <si>rewrap.autoWrap.notification</si> | "icon" \| "text"       |
| [Double Sentence Spacing](#double-sentence-spacing) | <si>rewrap.doubleSentenceSpacing</si> | false \| true          |
| [Whole Comment](#whole-comment)                     | <si>rewrap.wholeComment</si>          | true \| false          |
| [Wrapping Column](#wrapping-column)                 | <si>rewrap.wrappingColumn</si>        | &lt;number&gt;         |



## Wrapping column

The recommended way to set a wrapping column is to use a ruler (see below), which also
gives a visual indication. If you don't want to do that, then you can use the <sn>Wrapping
Column</sn> setting. This takes precedence over other options.

``` js
{
  // Wraps after 72 characters
  "rewrap.wrappingColumn": 72
}
```

If neither this nor any rulers are set, then VS Code's <si>editor.wordWrapColumn</si>
setting is used. This has a default of 80.

### Wrapping to rulers

To enable wrapping to rulers, <si>rewrap.wrappingColumn</si> must **not** be set.

If you have a ruler set up in VS Code, Rewrap will wrap to that ruler. If you have
multiple rulers, you can choose which ruler to wrap to while editing.

<img src="../images/rulers1.gif" width="700" alt="cycles through rulers" />

Just press Alt+Q multiple times to wrap to each ruler in turn. The ruler chosen is then
remembered for that document for the rest of the session. It cycles through the rulers in
the order in which they appear in settings; so if you have a most commonly used ruler, you
probably want to put that first.




## Whole Comment

!!! json ""

    ``` js
    "rewrap.wholeComment": true`
    ```

When this is `true`, Rewrap will wrap a whole comment block when a text cursor is inside
it.

<img src="../images/wholeCommentTrue.png" width="400" alt="wholeComment: true" />

But when set to `false`, Rewrap will only wrap that paragraph within the comment.

<img src="../images/wholeCommentFalse.png" width="400" alt="wholeComment: false" />

(Here the second paragraph is not wrapped)

**Note:** This setting only affects empty selections. You can always manually select the
lines to wrap.


## Double Sentence Spacing

!!! json ""

    ``` js
      "rewrap.doubleSentenceSpacing": false
    ```

If enabled, whenever a paragraph is wrapped, for any lines that end in ".", "?" or "!",
two spaces will be added after that sentence when the paragraph is rewrapped. This is not
enabled by default.

([details](specs/features/double-sentence-spacing.md))


## Auto-wrap

Auto-wrap works like the [auto-fill mode](
https://www.gnu.org/software/emacs/manual/html_node/emacs/Auto-Fill.html) in Emacs. When
pressing &lt;space&gt; or &lt;enter&gt; after the cursor is past the wrapping column, that
line is wrapped onto the next.

<img src="../images/autoWrapExample.gif" width="400" alt="Auto-wrap" />

Like in the Emacs function, auto-wrap does not attempt to re-flow the entire paragraph; it
only adds a break in the current line. It's handy when typing new text, but won't
automatically fix a paragraph after inserting/removing text. For this you can always do a
standard `alt+q` after making edits.

### Enabling

Auto-wrap is disabled by default, but can be enabled with the <sn>Rewrap > Auto Wrap:
Enabled</sn> setting. This setting (<si>rewrap.autoWrap.enabled</si>) can also be [set per
language/document type ](#language-specific-settings).

Even when enabled, it will only activate within comments in code files, not on code lines,
so it's usually ok to leave it enabled.

To temporarily enable or disable auto-wrap *for the current document*, you can use the
<sn>Rewrap: Toggle Auto-Wrap</sn> command in the command palette.

<img src="../images/toggleAutoWrapCommand.png" width="400" alt="Toggle Auto-wrap command" />

There's no default keybinding for the toggle command, but you can add one, eg
`shift+alt+q`, by binding to the command ID <si>rewrap.toggleAutoWrap</si> [in the
Keyboard Shortcuts editor](https://code.visualstudio.com/docs/getstarted/keybindings).

### Notification

To help keep track of when auto-wrap is on or off, there is a small (optional, but on by
default) icon/notification in the right of the statusbar, that shows the auto-wrap state
for the current document. It has 4 states:

<div class="notification-table"></div>
|   |   |
| --- | --- |
| ![normal](images/vsc-autowrap-icon-on.png) (normal) | on (enabled in settings)
| ![hidden](images/vsc-autowrap-icon-off.png) (hidden) | off (disabled in settings)
| ![orange](images/vsc-autowrap-icon-tempon.png) (orange) | temporarily enabled (from toggle command)
| ![gray](images/vsc-autowrap-icon-tempoff.png) (gray) | temporarily disabled (from toggle command)

Hovering over this icon will give more information.

If you don't like the icon, [you can hide
it](https://code.visualstudio.com/updates/v1_36#_hide-individual-status-bar-items), or
change the <sn>Rewrap > AutoWrap: Notification</sn> setting to <sv>"text"</sv>. Then it
will instead show a brief text message in the status bar for a few seconds whenever
auto-wrap is toggled on or off.

<img src="../images/vscAutoWrapStatusBar.png" width="500" alt="Auto-wrap" />


## Language-specific settings

See also [the VS Code documentation](
https://code.visualstudio.com/docs/getstarted/settings#_languagespecific-editor-settings).


This example sets the wrapping at column 72 for Python, with doubleSentenceSpacing on, and the wrapping column at 90 for Javascript.

``` js
{
  "[python]": {
    "editor.rulers": [72, 79],
    "rewrap.doubleSentenceSpacing": true
  },

  "[javascript]": {
    "editor.rulers": [90]
  }
}
```

VS Code has a helper command for adding language sections to your settings file: press F1
and search for `Preferences: Configure Language Specific Settings...`.

**Note:** Be aware that a global <si>rewrap.wrappingColumn</si> setting will take
precedence over a language-specific <si>editor.rulers</si> setting.


## Keybindings

Rewrap's main command has the default keybinding `Alt+Q`. If you want to change it to
something else, add something like this to your [keybindings.json
](https://code.visualstudio.com/docs/customization/keybindings#_customizing-shortcuts)
file. For example for `Ctrl+Shift+Q`:

```js
{
  "key": "ctrl+shift+q", "command": "rewrap.rewrapComment"
}
```

There are two other commands you can add keybindings for (eg `alt+shift+q`):

| Command                                 | ID                       |
| --------------------------------------- | ------------------------ |
| "Wrap Comment / Text at column..."      | <si>rewrap.rewrapCommentAt</si> |
| "Toggle Auto-Wrap for Current Document" | <si>rewrap.toggleAutoWrap</si>  |
