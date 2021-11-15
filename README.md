**Latest version: 1.15.3**. New in this version ([full
changelog](https://github.com/stkb/vscode-rewrap/releases)):
- VSCode: support untrusted workspaces (#292).
- VSCode: Fix crash when a language extension provides a line- but no block comment
  marker.

1.15.0: This is a small release to push out unreleased changes and support Visual Studio
2022. Larger changes are coming soon.
- Supports Visual Studio 2022
- Add ERB (HTML) `.html.erb`
- Add support for [Block
  Strings](https://github.com/graphql/graphql-spec/pull/327) in GraphQL files
- Add XAML type
- Fix whitespace disappearing around curly quotes


# Rewrap

Reformats code comments and other text to a given line length. For VSCode and
[Visual Studio](https://marketplace.visualstudio.com/items?itemName=stkb.Rewrap-18980).

<!-- VS
![Example](/268780/1/example-smaller.png)
<!-- VSCODE -->
![Example](https://stkb.github.io/Rewrap/images/example.png)
<!-- -->

Similar to wrap/fill paragraph in Sublime (alt+q) Emacs (M-q) or Vim (gq); but
more powerful.


## Features ##

* Re-wrap single & multiline comment blocks in many languages. Also configure
  settings per language.
* Smart handling of contents, including java/js/xmldoc tags and code examples.
* Can select lines to wrap or multiple comments/paragraphs at once (even the whole document).
* Also works with Markdown documents, LaTeX or any kind of plain text file.


## Using ##

<!-- VS
Adds the **Rewrap Lines** item to the Edit menu, by default bound to `Alt+Q`.
<!-- VSCODE -->
The main Rewrap command is: **Rewrap Comment / Text**, by default bound to
`Alt+Q`.
<!-- -->

Put the text cursor inside a comment line, block or plain text paragraph and
invoke the command to wrap to the preset column (default is 80). You can also
select just a few lines, or multiple comments in one selection.

Also available is an Emacs-style [auto-wrap/fill
mode](https://stkb.github.io/Rewrap/#/auto-wrap).

**See the [Features](https://stkb.github.io/Rewrap/#/features) page for more
examples on how to use.**


## Configuring ##

<!-- VS
Go to _Tools -> Options -> Rewrap_ to configure.
<!-- -->

See the [docs](https://stkb.github.io/Rewrap) for pages on
configuring settings and keybindings.


## Supported languages ##

Most common languages are supported. For any other type of file, plain-text
paragraph wrapping is still supported.

Please report any issues, suggestions or feature requests you have in
[issues](https://github.com/stkb/Rewrap/issues)!
