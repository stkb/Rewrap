# Rewrap

[![Build Status](https://travis-ci.org/stkb/vscode-rewrap.svg?branch=master)](https://travis-ci.org/stkb/vscode-rewrap)

Reformats single and multiline code comments to a given line length. Supports many languages: C/C++, C#, CoffeeScript, F#, Javascript, Haskell, Java, PHP, PowerShell, Python, Ruby, Rust, shell script, TypeScript and more.

<style>
  .example tr > :first-child,
  .example tr > :last-child { 
    background: #002451; color: #ffefae; overflow: hidden; padding: 0 1em 0 0;
  }
  .example tr > :first-child {
    content: "f"
  }
  .example tr > :nth-child(2){
     font-size: 1.3rem;
  }
  .example pre { 
    margin: 0; margin-left: -0.5em; width: 21em; 
    padding: 0.4em 0; border-right: solid 1px #888; 
  }
</style>

<table class="example"><tr>
  <td><pre>
  
  // A comment that's too long to fit on one line gets wrapped onto multiple lines
  </pre></td>
  <td>&#10132;</td> 
  <td><pre>
  // A comment that's too long to 
  // fit on one line gets wrapped 
  // onto multiple lines.</pre></td> 
</tr></table>

## How to use ##

**TL;DR:** Put your text cursor inside a comment line or block and press ```Ctrl+K Ctrl+W``` (one after the other) to wrap the comment to the preset column (default is 80). You can also select just a few lines of a comment block or multiple comments in one selection.

You can also invoke the command ("Rewrap Comment") manually by first pressing ```F1``` (or ```Ctrl+Shift+P```) and searching for it.

### Comment width ###
The column number to wrap comments at is chosen in this way:
* If the ```"rewrap.wrappingColumn"``` setting is present in your user or workspace ```settings.json```, this value will be used (recommended, see below)
* Otherwise, if your ```"editor.wrappingColumn"``` is set to something > 0 and <= 120, this value will be used (vscode default is 300).
* Otherwise, a default value of 80 will be used.

For example, to wrap comments at column 100, add the setting to your settings file (File -> Preferences -> User Settings):

```
{
  "rewrap.wrappingColumn": 100
}
```

### Selections ###
* If you have an empty selection inside a comment block (just the text cursor), the whole comment block will be processed. 
* If you select one or more lines within a comment block, only those lines will be processed.
* You can also include multiple comment blocks within a selection, and they will all be processed. You can reformat the comments for a whole document in this way.

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

## Todo ##
* Add support for documentation comments (jsdoc, xmldoc etc) so that they don't get messed up while reformatting the whole block.
* Improve this readme.
* Add more languages as necessary.
* (Possibly) add support for other things, like long multi-line strings in code.

Rewrap is a very new extension so please report any issues or suggestions you have in [issues](https://github.com/stkb/vscode-rewrap/issues)!

----

This extension uses the [greedy-wrap](https://github.com/danilosampaio/greedy-wrap) javascript module by danilosampaio