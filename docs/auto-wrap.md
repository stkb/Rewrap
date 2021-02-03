# Auto-wrap #

Auto-wrap works like the [auto-fill mode](https://www.gnu.org/software/emacs/manual/html_node/emacs/Auto-Fill.html) in Emacs. When pressing \<space\> or \<enter\> after the cursor is past the wrapping column, that line is wrapped onto the next.

<img src="images/autoWrapExample.gif" width="400" alt="Auto-wrap" />
<br/><br/><br/>

Like in Emacs function, auto-wrap does not attempt to re-flow the entire paragraph; it only adds a break in the current line. It's handy when typing new text, but won't automatically fix a paragraph after inserting/removing text. For this you can always do a standard `alt+q` after making edits.

To suggest any improvements to this feature, please [create an issue](https://github.com/stkb/Rewrap/issues)!

## Activating ##

Auto-wrap is turned off by default. Turning it on is global: it affects all documents edited. In code files, only comments will be affected, so it's pretty safe to leave it on.

### VSCode ###

The Command **Rewrap: Toggle Auto-Wrap** in the command palette (`F1`) toggles it on/off.

<img src="images/toggleAutoWrapCommand.png" width="500" alt="Auto-wrap" />
<br/><br/><br/>

A message shows in the status bar for a few seconds indicating the status.

<img src="images/vscAutoWrapStatusBar.png" width="500" alt="Auto-wrap" />
<br/><br/><br/>

There's no default keybinding for this command, but you can add one by binding to the command ID `rewrap.toggleAutoWrap`.

```json5
{
    "key": "shift+alt+q",
    "command": "rewrap.toggleAutoWrap"
}
```

### Visual Studio ###

On the **Edit** menu, under the *Rewrap Lines* command, is the *Toggle Auto-Wrap* item.

<img src="images/editToggleAutoWrap.png" width="300" alt="Auto-wrap" />
<br/><br/><br/>

The check-mark shows the status and a brief statusbar message is shown when turning on/off.

<img src="images/vsAutoWrapStatusBar.png" width="500" alt="Auto-wrap" />
<br/><br/><br/>

To add a keybinding, go to *Tools* -> *Options*, then *Environment* -> *Keyboard*. In this dialog pane search for the command `Edit.ToggleAutoWrap` and you can assign a shortcut key to it.
