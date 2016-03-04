# Examples #

## Basics ##

<table><tr>
  <td style="background: #002451; color: #ffefae; overflow: hidden; padding: 0 3em 0 0;"><pre style="margin: 0 0 0 -0.5em; width: 21em; padding: 0.4em 0; border-right: solid 1px #888">
  // Put the cursor<span style="background: #003F8E; border-right: solid 2px white"></span> in a comment block,
  // and run the command, to re-wrap 
  // the whole block.
  </pre></td>
  <td style="font-size: 1.3rem">&#10132;</td> 
  <td style="background: #002451; color: #ffefae; overflow: hidden; padding: 0 3em 0 0;"><pre style="margin: 0 0 0 -0.5em; width: 21em; padding: 0.4em 0; border-right: solid 1px #888">
  // Put the cursor in a comment
  // block, and run the command,
  // to re-wrap the whole block.
</tr></table>
<br/>

<table><tr>
  <td style="background: #002451; color: #ffefae; overflow: hidden; padding: 0 3em 0 0;"><pre style="margin: 0 0 0 -0.5em; width: 21em; padding: 0.4em 0; border-right: solid 1px #888">
  &lt;!-- Multi-line comments work too, like
  this html comment --&gt;<span style="background: #003F8E; border-right: solid 2px white"></span> 
  </pre></td>
  <td style="font-size: 1.3rem">&#10132;</td> 
  <td style="background: #002451; color: #ffefae; overflow: hidden; padding: 0 3em 0 0;"><pre style="margin: 0 0 0 -0.5em; width: 21em; padding: 0.4em 0; border-right: solid 1px #888">
  &lt;!-- Multi-line comments work 
  too, like this html comment --&gt;
  </pre>
</tr></table>
<br/>

<table><tr>
  <td style="background: #002451; color: white; overflow: hidden; padding: 0 3em 0 0;"><pre style="margin: 0 0 0 -0.5em; width: 21em; padding: 0.4em 0; border-right: solid 1px #888">
  It also works with plain text, in 
  markdown, html, or actually any type 
  of file.<span style="background: #003F8E; border-right: solid 2px white"></span>  </pre></td>
  <td style="font-size: 1.3rem">&#10132;</td> 
  <td style="background: #002451; color: white; overflow: hidden; padding: 0 3em 0 0;"><pre style="margin: 0 0 0 -0.5em; width: 21em; padding: 0.4em 0; border-right: solid 1px #888">
  It also works with plain text,
  in markdown, html, or actually 
  any type of file.
</tr></table>
<br/>

## Doc comments ##

<table><tr>
  <td style="background: #002451; color: #ffefae; overflow: hidden; padding: 0 3em 0 0;"><pre style="margin: 0 0 0 -0.5em; width: 21em; padding: 0.4em 0; border-right: solid 1px #888"><br/>
  /&#42;&#42;
   &#42; You can easily reformat a whole
   &#42; javadoc/jsdoc comment block. Lines
   &#42; starting with @tags will remain on
   &#42; separate lines.<span style="background: #003F8E; border-right: solid 2px white"></span>
   &#42; 
   &#42; @param other Another foo to compare.
   &#42; @returns true if the other foo is the same as this foo, otherwise false.
   &#42;/<br/>
  </pre></td>
  <td style="font-size: 1.3rem">&#10132;</td> 
  <td style="background: #002451; color: #ffefae; overflow: hidden; padding: 0 3em 0 0;"><pre style="margin: 0 0 0 -0.5em; width: 21em; padding: 0.4em 0; border-right: solid 1px #888">
  /&#42;&#42;
   &#42; You can easily reformat a 
   &#42; whole javadoc/jsdoc comment
   &#42; block. Lines starting with 
   &#42; @tags will remain on separate 
   &#42; lines.
   &#42; 
   &#42; @param other Another foo to 
   &#42; compare.
   &#42; @returns true if the other foo
   &#42; is the same as this foo, 
   &#42; otherwise false.
   &#42;/  </pre>
</tr></table>
<br/>

<table><tr>
  <td style="background: #002451; color: #ffefae; overflow: hidden; padding: 0 3em 0 0;"><pre style="margin: 0 0 0 -0.5em; width: 21em; padding: 0.4em 0; border-right: solid 1px #888">
  /// &lt;summary&gt;
  /// The same applies to C# xml doc 
  /// comments.<span style="background: #003F8E; border-right: solid 2px white"></span>
  /// &lt;/summary&gt;
  ///
  /// &lt;param name="other"&gt; Another foo to compare.&lt;/param"&gt;
  /// &lt;returns&gt; true if the other foo is 
  /// the same as this foo, otherwise 
  /// false.&lt;/returns&gt;
  </pre></td>
  <td style="font-size: 1.3rem">&#10132;</td> 
  <td style="background: #002451; color: #ffefae; overflow: hidden; padding: 0 3em 0 0;"><pre style="margin: 0 0 0 -0.5em; width: 21em; padding: 0.4em 0; border-right: solid 1px #888">
  /// &lt;summary&gt;
  /// The same applies to C# xml 
  /// doc comments.
  /// &lt;/summary&gt;
  ///
  /// &lt;param name="other"&gt; Another 
  /// foo to compare.&lt;/param"&gt;
  /// &lt;returns&gt; true if the other
  /// foo is the same as this foo, 
  /// otherwise false.&lt;/returns&gt;</pre>
</tr></table>
<br/>

<table><tr>
  <td style="background: #002451; color: #ffefae; overflow: hidden; padding: 0 3em 0 0;"><pre style="margin: 0 0 0 -0.5em; width: 21em; padding: 0.4em 0; border-right: solid 1px #888">
  {-| Also, you can write code examples
  by indenting two spaces or more. 
  These will be preserved as-is.<span style="background: #003F8E; border-right: solid 2px white"></span><br/>
  &nbsp;&nbsp;&nbsp;&nbsp;fromList ['e','l','m'] == "elm"
  -}
  </pre></td>
  <td style="font-size: 1.3rem">&#10132;</td> 
  <td style="background: #002451; color: #ffefae; overflow: hidden; padding: 0 3em 0 0;"><pre style="margin: 0 0 0 -0.5em; width: 21em; padding: 0.4em 0; border-right: solid 1px #888">
  {-| Also, you can write code 
  examples by indenting two spaces 
  or more. These will be preserved 
  as-is.<br/>
  &nbsp;&nbsp;&nbsp;&nbsp;fromList ['e','l','m'] == "elm"
  -}  </pre>
</tr></table>
<br/>

## Selections ##

<table><tr>
  <td style="background: #002451; color: #ffefae; overflow: hidden; padding: 0 3em 0 0;"><pre style="margin: 0 0 0 -0.5em; width: 21em; padding: 0.4em 0; border-right: solid 1px #888">
  &#35; If you select<span style="background: #003F8E; border-right: solid 2px white"> just a couple of lines, 
  &#35; only those li</span>nes 
  &#35; will be processed 
  </pre></td>
  <td style="font-size: 1.3rem">&#10132;</td> 
  <td style="background: #002451; color: #ffefae; overflow: hidden; padding: 0 3em 0 0;"><pre style="margin: 0 0 0 -0.5em; width: 21em; padding: 0.4em 0; border-right: solid 1px #888">
  &#35; If you select just a couple of 
  &#35; lines, only those lines 
  &#35; will be processed 
  </pre>
</tr></table>
<br/>

<table><tr>
  <td style="background: #002451; color: #ffefae; overflow: hidden; padding: 0 3em 0 0;"><pre style="margin: 0 0 0 -0.5em; width: 21em; padding: 0.4em 0; border-right: solid 1px #888">
  <span style="background: #003F8E; border-right: solid 2px white">-- You can also select
  &#45;- multiple comment blocks
  &#45;- in one selection.<br/>  
  &#45;- These will then be processed</span> separately.
  </pre></td>
  <td style="font-size: 1.3rem">&#10132;</td> 
  <td style="background: #002451; color: #ffefae; overflow: hidden; padding: 0 3em 0 0;"><pre style="margin: 0 0 0 -0.5em; width: 21em; padding: 0.4em 0; border-right: solid 1px #888">
  &#45;- You can also select multiple
  &#45;- comment blocks in one 
  &#45;- selection.<br/>  
  &#45;- These will then be processed 
  &#45;- separately.</pre>
</tr></table>
<br/>