# Keybindings: VS Code #

Rewrap's main command has the default keybinding `Alt+Q`. If you want to change it to something else, add something like this to your [keybindings.json](https://code.visualstudio.com/docs/customization/keybindings#_customizing-shortcuts) file. For example for `Ctrl+Shift+Q`:

```json5
{ 
  "key": "ctrl+shift+q", "command": "rewrap.rewrapComment"
}
```

### Custom column command ###
To add a keybinding for the **Wrap Comment / Text at column...** command, use its `rewrap.rewrapCommentAt` id:
```json5
{ 
  "key": "alt+shift+q", "command": "rewrap.rewrapCommentAt"
}
```

### Auto-wrap command ###
To add a keybinding for the auto-wrap toggle, use the command ID `rewrap.toggleAutoWrap`.

```json5
{
    "key": "shift+alt+q", "command": "rewrap.toggleAutoWrap"
}
```


## Old keybinding

The original keybinding for Rewrap was `Ctrl+K Ctrl+W`, but this has been removed in favor of `Alt+Q`.

If you prefer to use this shortcut, you can add it to your keybindings.json file:

```json5
{ 
  "key": "ctrl+k ctrl+w", "command": "rewrap.rewrapComment"
}
```
