See also [https://github.com/stkb/vscode-rewrap/releases](https://github.com/stkb/vscode-rewrap/releases) (for working links to issues)

#### 17.8
- Fix error when using "Rewrap At..." command (#324)
#### 17.7
- New feature: Run rewrap on save (#165)
- Markdown: Fix front-matter marker rules (#321)
- Support VS Code for the Web (#314): Attempt 2
#### 17.6
- Support .rmd files as markdown (#245)
- Basic support for Textile files as markdown (#271)
- Support FIDL (#255), Pascal/Delphi (#97) & pylintrc files (#121)
- Shell script: Ignore shebangs (#129)
- Attempt at supporting VS Code for the Web (#314)
#### 17.5
- Add '//'-comments to CSS (workaround for #309)
- Fix batch files: case-insensitive "REM" and "@" prefix (#313)
- Fix the document being 'modified' if there were no actual changes (#308, #315)
- Rewritten code that applies edits to the document and fixes the selections after
  wrapping. Is more efficient and should hopefully fix issues with autowrap when typing
  too fast (#207). For the most part you should notice no changes but please report any
  bugs where the selection after wrapping is not as expected.
#### 17.4
- Make the rule of ending a line with 2 spaces to preserve the line-break after work everywhere.
- Support Prisma (#306)
#### 17.3
- MDX files: Treat as Markdown.
#### 17.2
- Lua: Support `--[=[` block comments (#290)
- VS Code: Attempt to prevent rare "TextEditor has been disposed" error (#100)
#### 17.1
- VS Code: Change extension activation event from on startup to "onStartupFinished". This
  should help to avoid slowing down VS Code startup. Testing to see if any problems come
  from this.


---


### 1.16.3

- Markdown: Fix front-matter marker rules (#321)
- Preparation for supporting VS Code for the Web


### 1.16.2

- Support .rmd files as markdown (#245)
- Basic support for Textile files as markdown (#271)
- Support FIDL (#255), Pascal/Delphi (#97) & pylintrc files (#121)
- Shell script: Ignore shebangs (#129)
- Add '//'-comments to CSS (workaround for #309)
- Fix batch files: case-insensitive "REM" and "@" prefix (#313)
- Fix the document being 'modified' if there were no actual changes (#308, #315)


### 1.16.1

- VS Code: Attempt to prevent rare "TextEditor has been disposed" error (#100)
- Lua: Support `--[=[` block comments (#290)
- MDX files: Treat as Markdown.
- Support Prisma (#306)


## 1.16.0

- New architecture (still WIP) with performance increase.
- Markdown: new parser that fixes many small bugs (inc #288), as well as supporting:
  - Front matter header (#277, #294)
  - Link reference definitions (#63, #93)
  - Footnotes (#188)
- ReStructuredText support (standalone & for Python, almost complete) (#88).
- Julia: support `#= ... =#` block comments (#302)
- Visual Studio: Wrap to rulers generated from .editorconfig by the Editor Guidelines
  extension (thereby indirectly supporting .editorconfig) (#300).


### 1.15.4

- Support svelte files (HTML) (#243, #299).


### 1.15.3

- VSCode: support untrusted workspaces (#292).
- VSCode: Fix crash when a language extension provides a line- but no block comment marker.


### 1.15.2

- Fix regression: Wrapping column < 1 should be treated as no wrapping


### 1.15.1

- VS Code: Fix broken release (VSIX file)
- Visual Studio: Fix crash in VS 17.1 Preview


## 1.15.0

This is a small release to push out unreleased changes and support Visual Studio
2022. Larger changes are coming soon.
- Supports Visual Studio 2022
- Add ERB (HTML) `.html.erb` (#265).
- Add support for [Block
  Strings](https://github.com/graphql/graphql-spec/pull/327) in GraphQL files.
  (#261)
- Add XAML type (#263).
- Fix whitespace disappearing around curly quotes (#119).


## 1.14.0

- Make `autoWrap.enabled` a per-language setting; auto-wrap toggle command a
  per-document override. (#149)
- Add Clojure, Common Lisp, Emacs Lisp, Scheme, and J (#246, #240)
- Fix per-language settings sometimes not applying.
- Declare as a UI extension so it doesn't have to be installed in the remote
  workspace (#231); fixes locally-installed language extensions not being found
  when used with remote workspaces (#201).
- Add PostgreSQL (.p(g)sql)
- Markdown: preserve line-breaks after `<br(/)>` (#223).


## 1.13.0

- Change to how the indent of comment content is handled. Now, instead of taking
  the indent of the first line and applying it to rest of the comment, the whole
  comment is taken as-is with all indents being relative to the least indented
  line (that has text). [This allows the first line of a comment to be an
  indented code
  block](https://github.com/stkb/Rewrap/issues/203#issuecomment-637767932),
  which previously wasn't possible.
- Fix issues that broke wrapping for extension-contributed languages in v1.12.0.
- Markdown: Fix preserving indents in blockquote (#204).
- Fix HTML not working in Visual Studio.


## 1.12.0

Lots of minor improvements. General:
- Can now preserve a line break after any line by ending it with two spaces (was
  just for Markdown, now for all types).
- VSCode: support new ruler objects in settings (#208).

Language-specific:
- LaTeX: break after a paragraph that ends in an end-of-line comment (#148).
- LaTeX: allow any number of optional arguments to commands (#141).
- Python & Julia: all triple-quote strings should now work (however rST
  support is not there yet) (#157, #203).
- Support Octave (#206).
- .Net XML docs: Only preserve line breaks before/after certain "block"
  elements; wrap all other elements inline (#174).


### 1.11.1

- Fix autowrap not working in embedded sections in html files in some cases
  (#162)
- VSCode: Fix warning in settings editor that settings aren't per-language
  supported (#200)
- C/C++: `///` comments now expect xml doc-comments instead of javadoc-style
  (#199)


## 1.11.0

General enhancements:
- Modify "Rewrap at custom column" command to make "unwrapping" easier (#107)

Language-specific:
- AsciiDoc: Fix only comments being wrapped (#196)
- R: Allow `#'` prefix for ROxygen comments (full ROxygen support still to come)
  (#181)
- Add Julia (#137) and Handlebars (#167)

General bugfixes:
- Fix for settings changes not applying until an editor restart (#99)
- VS: Fix an index out of bounds bug (#159)


### 1.10.1

- Lua: Fix block comments (#183)
- RST: Fix only comments being wrapped (#191)
- PostCSS: Fix by adding it to CSS (#193)
- Support JSON-C (#160) and CMake(Lists.txt) (#182)
- Fixes the Visual Studio release (#195)


## 1.10.0

- Rewrap now has basic support for any language if the user has an extension for
  it installed (VSCode only).
- Also added native support for support for:
  - Verilog/SystemVerilog (#164, #173)
  - Prolog (#105, #104)
  - Bikeshed (#95)
  - Scala (#172, #175)
- Python: Fix some issues for triple-quoted strings, including allowing
  pre-string characters [bfru] (#171, #170, #128).


### 1.9.3

VS-only release: Support all future VS2019 (v16.x) versions (#143)


### 1.9.2

VS-only release: Adds support for VS 2019.


### 1.9.1

The new setting `rewrap.autoWrap.enabled` can now be used to ensure this feature
is enabled in new installations (#87). Apart from that, the feature and the
toggle command for it work the same as before.


## 1.9.0

- Support East Asian (CJK) languages (#75)
- Support HCL/Terraform config files (#85)


### 1.8.1

- Support Vue HTML templates (#82)


## 1.8.0

Mostly bugfixes:
- Fix to entire file being re-syntax highlighted (#78)
- Elixir: Support doc, moduledoc and typedoc comments (#76)
- Yaml: Allow up to 3 #'s as line comment prefixes (#74)
- Markdown: Fix a list item bug (#79)
- Config files (.conf, .gitconfig etc): restore support (was broken)


### 1.7.1

- LaTeX: Fix wrapping after * environments (#77)


## 1.7.0

- Auto-wrap setting (on/off) now persists between sessions.
- LaTeX: Preserve verbatim and math sections (#68, #69)
- Go: comment parsing changed to work the same as godoc (#70)
  - Markdown no longer used; instead simply all indented lines are preserved.
  - Leading '*'s no longer allowed in block comments
- Crystal and MATLAB languages added. (#71, #72)


### 1.6.1

- Fixes leading '|'s in Purescript doc comments.


## 1.6.0

- **Added auto-wrap feature (#45)** ([more info](https://github.com/stkb/Rewrap/wiki/Auto-wrap)).
- Added languages: Lean and D (extra support for D doc-comments still to come)

- Fixed: With reformat setting on, code blocks in comments and markdown documents now have
  indents corrected.
- Code in a source file is no longer wrappable (#41, #51)


### 1.5.3

- Support Tcl (#54)
- VS: Add keywords for VS settings search


### 1.5.2

- Support lines starting with `*` in all C-Style blocks (`/* .. */`) in all languages that use them.


### 1.5.1

- Added support for Protocol Buffers (Protobuf) (#47)
- Added support for GraphQL (#49)
- Fixed issue with "\0" in text (#48)


## 1.5.0

- Added multiple ruler support ([VSCode](https://github.com/stkb/Rewrap/wiki/Settings-VSCode#wrapping-to-rulers), [VS](https://github.com/stkb/Rewrap/wiki/Settings-Visual-Studio#wrapping-to-rulers)). (#30)
- Added Git tag editing as a document type.
- VSCode: Removed old keybinding (ctrl+k ctrl+w) ([how to add it back](https://github.com/stkb/Rewrap/wiki/Keybindings-VSCode#old-keybinding))
- VS: Fixed bug in Options screen (#44)


### 1.4.2

- Added support for PowerShell Comment-Based Help (#43).
- Added git-commit as a document type.
- (Visual Studio only) Added per-language settings.
  - Because of a change here in how settings are stored, existing settings won't be carried over after this upgrade.


### 1.4.1

Fixed a bug with markdown block-quotes (#42).


## 1.4.0

Important: if you still use and prefer the old keybinding for Rewrap (`ctrl+k ctrl+w`), please add it to your settings manually. It will be removed in the next version.

Rewrap is now also available for Visual Studio!

The main change in this release is that all<sup>1</sup> comments now are parsed as Markdown (CommonMark spec), enabling wrapping of bulleted/numbered lists and all other Markdown features.<sup>2</sup>

Additionally, a new setting: `wholeCommment`, has been added to give more control over wrapping comments. See [here](https://github.com/stkb/Rewrap/wiki/Settings-VSCode#wrapping-whole-or-parts-of-comments).

Added languages:
- Elixir
- Dart

Other fixes and changes:
- Supports unicode (#38)
- In js/javadoc, inline tags (eg: `{@link}`) aren't broken up (#35)
- Support doc-comments in PHP (#34)

----

<sup>1</sup> Except for .Net xml-doc comments.

<sup>2</sup> (This introduces a small breaking change: where code samples (which aren't rewrapped) within a comment previously only required a 2-space indent, now they require 4 spaces, in line with the Markdown spec.)


## 1.3.0

Added a new command; "Rewrap Comment / Text at column..." (id: `rewrap.rewrapCommentAt`). This allows you to re-wrap something at a custom wrapping width (#27).


## 1.2.0

- Added support for the new [language-specific settings](https://code.visualstudio.com/updates/v1_9#_language-specific-settings). Now you can customize `editor.rulers` and Rewrap's settings per language. (#25)
- Improvements for LaTeX:
  - Most \commands starting a line will now denote a new paragraph (#24).
  - Indentation for subsequent lines in that paragraph will be cleaned up (#23).
  - Line breaks following a line-break command (eg \\\\, \newline) will be preserved.
- Added basic support for TOML files (#26).


## 1.1.0

- Wrapping now works with js and css sections embedded in an html document (#22).
- Very basic support for (La)TeX files added.


## 1.0.0

Bumping up to version 1.0.0 since it was about time.

Two new features; see the README for details.
- Added a feature from Vim `gq` and Emacs `fill-paragraph`: when lines ending in a period are wrapped, two spaces will be added after the period in the wrapped text. To turn this feature on, add `rewrap.doubleSentenceSpacing: true` to your settings.json. (#17)
- If you use rulers, Rewrap can now take the wrapping column from the first value in the `editor.rulers` setting. `rewrap.wrappingColumn` is then no longer needed. (#19)

Bug fixes:
- Cursor/selection position after wrapping has been fixed. Now the text cursor should always stay next to the same word it was at before wrapping, allowing you to keep typing from where you left off. (#18)


### 0.6.4

This doesn't affect comments, only other plain text within a file (eg. in YAML files).
Now blocks of plain text with differing indents are treated as separate paragraphs. Previously a blank line was needed to separate paragraphs.

Eg: this text was treated as one paragraph but now as two.
```
Some text
    Some more text
```

Also doesn't affect markdown files, or .txt files, which are currently treated the same as markdown.


### 0.6.3

- Added """-comments for Python (previously only supported ''') (#15)
- Added # markers for ini files (includes other types of config files, eg .gitconfig)
- Fixed an error caused by empty comments (#11, #14)

Upgraded to TypeScript 2.0; vscode v1.6+ is now required.


### 0.6.2

Fixed a bug where the last line of comment sections of some types of files wouldn't be included in the wrapping (#12).

Types affected: dockerfile, ini, makefile, perl, r, rust, shellscript, vb, yaml


### 0.6.1

No longer hard-wraps very long words, eg URLs (#10)


## 0.6.0

Changed the default keybinding to Alt+Q (#5)

- Old keybinding (Ctrl+K Ctrl+W) still works for those used to that one.
- For those with a custom binding set, nothing changes.

Added AutoHotKey (.ahk) file support

Bug fixes:
- Fixed wrapping on unsaved files (#8)
- Fixed alignment of end-comment marker (#9)


### 0.5.3

Fixed problems with extensionless filenames.


### 0.5.2

VSCode v1.1.0 broke Rewrap. This release fixes it.


### 0.5.1

Fixed a filename issue, causing the extension not to work on Mac & Linux.


## 0.5.0

This release adds a markdown feature to all document types: You can end a line with 2 spaces to force a mid-paragraph line break after it.

```js
// This line ends with 2 spaces˽˽
// Because I want this to be on a new line
```

Speaking of markdown, the main new feature for this release is full markdown support. You can safely ctrl+a and then wrap a whole document at once to reformat all paragraphs appropriately without messing anything up.

Lastly, the selection moving/expanding after wrapping has been fixed (#4)


### 0.4.2

- If file is an unknown type, still provide plain text wrapping.
- Adds some better paragraph detection for markdown.
  - Mid-pararaph line breaks (2 trailing spaces)
  - List items


## 0.4.0

Now wraps to the correct visual column when using tabbed indents. (#2)


## 0.3.0

Skipping a version number because this version adds two new features.

- Doc comments: you can now run the command on a whole doc comment and now worry about param tags, code examples etc getting messed up. Rewrap now preserves these.
- Plain text: If you select something other than a comment it will be re-wrapped as plain text instead. Useful for text, markdown or html files etc, but works on any type of file.


## 0.1.0

Add support for many more languages:

bat, groovy, less, objective-c, sass, shaderlab, swift, coffeescript,
dockerfile, makefile, perl, perl6, r, shellscript, yaml, fsharp, haskell,
elm, purescript, ini, jade, lua, perl6, php, powershell, python, rust, sql, vb


## 0.0.2

First release.
