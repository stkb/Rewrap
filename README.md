# Rewrap

**Latest version: 1.0.0 ([view changes](https://github.com/stkb/vscode-rewrap/releases))**
 
Reformats code comments and other text to a given line length.

![Example](https://stkb.github.io/vscode-rewrap/example.png)

Similar to wrap/fill paragraph in Sublime (alt+q) Emacs (M-q) or Vim (gq); but more powerful.

## Features ##

* Re-wrap single & multiline comment blocks in many languages.
* [Smart handling of contents, including java/js/xmldoc tags and code examples.](docs/Examples.md#doc-comments)
* [Can select multiple comments/paragraphs at once (even the whole document) and wrap them all in one go.](docs/Examples.md#selections)
* Re-wrap any sort of plain-text paragraph too.
* [Full support for Markdown documents.](https://github.com/stkb/vscode-rewrap/releases/tag/v0.5.0)

## Using ##

Rewrap adds one command to vscode: **Rewrap Comment / Text**, by default bound to ```Alt+Q```.

Put the text cursor inside a comment line, block or plain text paragraph and invoke the command to wrap to the preset column (default is 80). You can also select just a few lines, or multiple comments in one selection.

**See the [Examples](docs/Examples.md) page for more examples on how to use.**

## Settings ##

Add these settings to your [user or workspace settings file](https://code.visualstudio.com/docs/customization/userandworkspace). (File -> Preferences -> User Settings)

### Wrapping width ###

You can set the column to wrap at directly with the `rewrap.wrappingColumn` setting. This takes precedence over other options.
``` json5
{
  // Wraps after 72 characters
  "rewrap.wrappingColumn": 72
}
```

However maybe you already have rulers set up in vscode. In this case you don't need the above setting; rewrap will use the first value in the list of rulers.
``` json5
{
  // Also wraps after 72 characters
  "editor.rulers": [72, 80]
}
```

Thirdly, if neither of the above two settings are present, vscode's `"editor.wrappingColumn"` setting is used if set.

Finally, a default value of `80` is used if no other settings are found.

### Double sentence spacing ###

The wrap/fill commands in Vim and Emacs have another feature, where if lines end with a period (or ? or !), two spaces will be added after that sentence when the paragraph is rewrapped. This is not enabled by default in rewrap, but you can enable it with a setting.

``` json5
{
  "rewrap.doubleSentenceSpacing": true
}
```

### Keyboard shortcut ###
If you want to use another shortcut instead, you can do so by [adding a custom keybinding](https://code.visualstudio.com/docs/customization/keybindings#customizing-shortcuts) (File -> Preferences -> Keyboard Shortcuts). Then add your own shortcut for the ```rewrap.rewrapComment``` command to your ```keybindings.json``` file.

For example if you want to use the shortcut ```Ctrl+Shift+Q``` instead:

``` json5
[ { "key": "ctrl+shift+q", "command": "rewrap.rewrapComment" }	
]
```


## Supported languages ##

The full list of types currently supported is: 

AutoHotKey, Bash/shell script, Batch file, C, C#, C++, CoffeeScript, CSS, Docker file, Elm, F#, Go, Groovy, Haskell, Html, Ini, Jade, Java, JavaScript, Less, Lua, Makefile, Markdown, Objective-C, Perl, PHP, PowerShell, PureScript, Python, R, Ruby, Rust, Sass, SQL, Swift, TypeScript, Visual Basic, Xml, Xsl, Yaml.

For any other type of file, plain-text paragraph wrapping is still supported.

Please report any issues, suggestions or feature requests you have in [issues](https://github.com/stkb/vscode-rewrap/issues)!

----

Uses [textlint/markdown-to-ast](https://github.com/textlint/markdown-to-ast) for markdown support.

Actual wrapping done with [jonschlinkert/word-wrap](https://github.com/jonschlinkert/word-wrap)
