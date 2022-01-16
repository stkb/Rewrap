<!-- This part has to be written in HTML, because doing it in markdown puts the content in
a <p>, which adds unwanted margins. It has to be in a table so it can be right-aligned on
GitHub. For GitHub we can't get rid of the border on the td nor make the font smaller as
we want-->
<table class="topright" align="right" style="font-size:90%;width:auto;margin:0;border:none">
<tr style="border:none"><td align="right" style="border:none">
Reformats code comments and other text to a given line length.<br/>
For <a href="https://marketplace.visualstudio.com/items?itemName=stkb.rewrap"><b>VS Code</b></a>,
<a href="https://open-vsx.org/extension/stkb/rewrap"><b>Open VSX</b></a> and
<a href="https://marketplace.visualstudio.com/items?itemName=stkb.Rewrap-18980">
  <b>Visual Studio</b></a>.
Latest version <b>1.16.0</b>
(<a href="https://github.com/stkb/vscode-rewrap/releases">changelog</a>)
</td></tr></table>


<h1 style="font-size: 2.5em">Rewrap</h1>

<img src="https://stkb.github.io/Rewrap/images/example.svg" width="700px"/><br/><br/>

The main Rewrap command is: <sn>**Rewrap Comment / Text**</sn>, by default bound to
`Alt+Q`. With the cursor in a comment block, hit this to re-wrap the contents to the
[specified wrapping column
](https://stkb.github.io/Rewrap/configuration/#wrapping-column).


## Features

* Re-wrap comment blocks in many languages, with per-language settings.
* Smart handling of contents, including Java-/JS-/XMLDoc tags and code examples.
* Can select lines to wrap or multiple comments/paragraphs at once (even the whole
  document).
* Also works with Markdown documents, LaTeX or any kind of plain text file.

The contents of comments are usually parsed as markdown, so you can use lists, code
samples (which are untouched) etc:

<img src="https://stkb.github.io/Rewrap/images/example1.svg" width="700px"/>

<div class="hideOnDocsSite"><br/><b><a href="https://stkb.github.io/Rewrap/">
See the docs site for more info.</a></b></div>
