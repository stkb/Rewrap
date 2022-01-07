---
title: Rewrap
hide:
  - toc
---
<style>
    .md-typeset h1 { font-size: 2.5em; font-weight: bold; }
    .md-content__button { display: none }
</style>

<!-- START README -->
<table class="topright" align="right"><tr><td align="right">
Reformats code comments and other text to a given line length.<br/>
For <a href="https://marketplace.visualstudio.com/items?itemName=stkb.rewrap">
  <b>VS Code</b></a>
and <a href="https://marketplace.visualstudio.com/items?itemName=stkb.Rewrap-18980">
  <b>Visual Studio</b></a>.
Latest version <b>1.15.4</b>
  (<a href="https://github.com/stkb/vscode-rewrap/releases">changelog</a>)
</td></tr></table>

# Rewrap

<br/>
<img src="https://stkb.github.io/Rewrap/images/example.svg" width="800px"/><br/>

The main Rewrap command is: **Rewrap Comment / Text**, by default bound to `Alt+Q`. With
the cursor in a comment block, hit this to re-wrap the contents to the [specified wrapping
column ](https://stkb.github.io/Rewrap/Configuration/#wrapping-column).

## Features

* Re-wrap comment blocks in many languages. Also configure settings per language.
* Smart handling of contents, including java/js/xmldoc tags and code examples.
* Can select lines to wrap or multiple comments/paragraphs at once (even the whole document).
* Also works with Markdown documents, LaTeX or any kind of plain text file.

The contents of comments are usually parsed as Markdown, so you can use lists, code
samples (which are untouched) etc:

<img src="https://stkb.github.io/Rewrap/images/example1.svg" width="800px"/>

<div style="display: none">
<b><a href="https://stkb.github.io/Rewrap/">Read more...</a></b>
</div>
<!-- END README -->

## Selections

If you select just a couple of lines, only those lines will be processed.

In VS Code you can use multiple selections (alt+click) to select multiple comment blocks
or line ranges.

However you can safely select a large line range with multiple comments and code, and only
the comments will be processed (the code is untouched). You can `Ctrl+A, Alt+Q` to re-wrap
a whole document at once.


## Next

See the following pages on [configuration](configuration.md) of settings and keybindings,
and [extra features](extra-features.md).
