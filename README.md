# Rewrap

 **Latest version: 0.4.2 ([view changes](https://github.com/stkb/vscode-rewrap/releases))**
 
Reformats code comments and plain text to a given line length. Supports C/C++, C#, CoffeeScript, F#, Javascript, Haskell, HTML, Java, Markdown, PHP, PowerShell, Python, Ruby, Rust, TypeScript and more.

![Example](http://stkb.github.io/vscode-rewrap/example.png)

## Installing ##
Run the `Install Extension` command and search for `rewrap`. ([More info on installing extensions](https://code.visualstudio.com/docs/editor/extension-gallery).)

## Using ##

Put the text cursor inside a comment line, block or plain text paragraph and press ```Ctrl+K Ctrl+W``` (one after the other) to wrap the comment to the preset column (default is 80). You can also select just a few lines, or multiple comments in one selection.

You can also invoke the command ("Rewrap Comment / Text") manually by first pressing ```F1``` (or ```Ctrl+Shift+P```) and searching for it.

**See the [Examples](docs/Examples.md) page for more examples on how to use.**

## Settings ##

### Comment width ###

The extension provides one setting: the column that text will be wrapped at:
```json
{
  "rewrap.wrappingColumn": 72
}
```
Add it to your [user or workspace settings file](https://code.visualstudio.com/docs/customization/userandworkspace). (File -> Preferences -> User Settings)

If this setting isn't present, vscode's own `"editor.wrappingColumn"` setting is used instead (as long as it's set &le; 120). Otherwise, a default value of `80` is used.




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
* Improvements to markdown support
* Extra languages or features, as requested.

Please report any issues or suggestions you have in [issues](https://github.com/stkb/vscode-rewrap/issues)!

----

This extension uses the [greedy-wrap](https://github.com/danilosampaio/greedy-wrap) javascript module by danilosampaio