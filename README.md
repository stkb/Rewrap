# Rewrap

**Latest version: 1.4.0 ([view changes](https://github.com/stkb/vscode-rewrap/releases))**
 
Reformats code comments and other text to a given line length. For VSCode and **now also for Visual Studio**.

![Example](https://github.com/stkb/Rewrap/wiki/images/example.png)

Similar to wrap/fill paragraph in Sublime (alt+q) Emacs (M-q) or Vim (gq); but more powerful.


## Features ##

* Re-wrap single & multiline comment blocks in many languages. Also configure settings per language.
* Smart handling of contents, including java/js/xmldoc tags and code examples.
* Can select multiple comments/paragraphs at once (even the whole document).
* Also works with Markdown documents, LaTeX or any kind of plain text file.


## Using ##

The main Rewrap command is: **Rewrap Comment / Text**, by default bound to ```Alt+Q```.

Put the text cursor inside a comment line, block or plain text paragraph and invoke the command to wrap to the preset column (default is 80). You can also select just a few lines, or multiple comments in one selection.

**See the [Features](https://github.com/stkb/Rewrap/wiki/Features) page for more examples on how to use.**

A second command, **Rewrap Comment / Text a column...** allows you do a wrap at a custom column. This has no default keybinding, but is available in the command pallete (F1).


## Configuring ##

See the [wiki](https://github.com/stkb/vscode-rewrap/wiki) for pages on configuring settings and keybindings.


## Supported languages ##

The full list of types currently supported is:

AutoHotKey, Bash/shell script, Batch file, C, C#, C++, CoffeeScript, CSS, Dart, Docker file, Elixir, Elm, F#, Go, Groovy, Haskell, Html, Ini, Jade, Java, JavaScript, LaTeX, Less, Lua, Makefile, Markdown, Objective-C, Perl, PHP, PowerShell, PureScript, Python, R, Ruby, Rust, Sass, SQL, Swift, Toml, TypeScript, Visual Basic, Xml, Xsl, Yaml.

For any other type of file, plain-text paragraph wrapping is still supported.

Please report any issues, suggestions or feature requests you have in [issues](https://github.com/stkb/vscode-rewrap/issues)!
