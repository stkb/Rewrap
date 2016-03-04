# Rewrap

 **Latest version: 0.3.0 ([view changes](https://github.com/stkb/vscode-rewrap/releases/tag/v0.3.0))**
 
[![Build Status](https://travis-ci.org/stkb/vscode-rewrap.svg?branch=master)](https://travis-ci.org/stkb/vscode-rewrap)

Reformats plain text, single and multi-line code comments to a given line length. Supports many languages: C/C++, C#, CoffeeScript, F#, Javascript, Haskell, HTML, Java, PHP, PowerShell, Python, Ruby, Rust, shell script, TypeScript and more.

![Example](http://stkb.github.io/vscode-rewrap/example.png)

## How to use ##

**TL;DR:** Put the text cursor inside a comment line, block or plain text paragraph and press ```Ctrl+K Ctrl+W``` (one after the other) to wrap the comment to the preset column (default is 80). You can also select just a few lines, or multiple comments in one selection.

You can also invoke the command ("Rewrap Comment / Text") manually by first pressing ```F1``` (or ```Ctrl+Shift+P```) and searching for it.

**See the [Examples](docs/Examples.md) page for more examples on how to use.**

### Comment width ###
The column number to wrap comments at is chosen in this way:
* If the ```"rewrap.wrappingColumn"``` setting is present in your [user or workspace ```settings.json```](https://code.visualstudio.com/docs/customization/userandworkspace), this value will be used (recommended, see below).
* Otherwise, if your ```"editor.wrappingColumn"``` is set to something > 0 and <= 120, this value will be used (vscode default is 300).
* Otherwise, a default value of 80 will be used.

For example, to wrap comments at column 100, add the setting to your settings file (File -> Preferences -> User Settings):

```
{
  "rewrap.wrappingColumn": 100
}
```

### Keyboard shortcut ###
The default keyboard shortcut (```Ctrl+K Ctrl+W```) is chosen to be in keeping with the already-existing comment-related commands in vscode:
* Add line Comment: ```Ctrl+K Ctrl+C```
* Remove Line Comment: ```Ctrl+K Ctrl+U```

But if you want to use another shortcut instead, you can do so by [adding a custom keybinding](https://code.visualstudio.com/docs/customization/keybindings#customizing-shortcuts) (File -> Preferences -> Keyboard Shortcuts). Then add your own shortcut for the ```rewrap.rewrapComment``` command to your ```keybindings.json``` file.

For example if you want to use the shortcut ```Ctrl+Shift+Q``` instead:

```
[ { "key": "ctrl+shift+q", "command": "rewrap.rewrapComment" }	
]
```

## Todo ##
* Extra languages or features, as requested.

Please report any issues or suggestions you have in [issues](https://github.com/stkb/vscode-rewrap/issues)!

----

This extension uses the [greedy-wrap](https://github.com/danilosampaio/greedy-wrap) javascript module by danilosampaio