# Rewrap

Reformats code comments to a given line length. Currently only supports C-style comments (```//``` and ```/* ... */```).

## How to use ##
Put the text cursor inside a comment (or select lines thereof) and press F1 (or Ctrl+Shift+P) and find the command "Rewrap Comments". Alternatively use the keyboard shortcut ```Ctrl+K Ctrl+W``` (press one after the other).

### Selections ###
* If you have an empty selection inside a comment block (just the text cursor), the whole comment block will be processed. 
* If you select one or more lines within a comment block, only those lines will be processed.
* You can also include multiple comment blocks within a selection, and they will all be processed. You can reformat the comments for a whole document in this way.

### Comment width ###
The column number to wrap comments at is chosen in this way:
* If the ```"rewrap.wrappingColumn"``` setting is present in your user or workspace ```settings.json```, this value will be used (recommended, see below)
* Otherwise, if your ```"editor.wrappingColumn"``` is set to something > 0 and <= 120, this value will be used (vscode default is 300).
* Otherwise, a default value of 80 will be used.

For example, to wrap comments at column 100, add the setting to your settings file (File -> Preferences -> User Settings):

```
{
  "rewrap.wrappingColumn": 80
}
```

### Keyboard shortcut ###
The default keyboard shortcut (```Ctrl+K Ctrl+W```) is chosen to be in keeping with the already-existing comment-related commands in vscode:
* Add line Comment: ```Ctrl+K Ctrl+C```
* Remove Line Comment: ```Ctrl+K Ctrl+U```

But if you want to use another shortcut instead, you can do so using vscode's keybinding customization system (File -> Preferences -> Keyboard Shortcuts). Then add your own shortcut for the ```rewrap.rewrapComment``` command to your ```keybindings.json``` file. 

For example if you want to use the shortcut ```Ctrl+Shift+Q``` instead:

```
[ { "key": "ctrl+shift+q", "command": "rewrap.rewrapComment" }	
]
```

----

This extension uses the [greedy-wrap](https://github.com/danilosampaio/greedy-wrap) javascript module by danilosampaio