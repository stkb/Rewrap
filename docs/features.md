# Features #

This page gives a brief overview of features.

Put the text cursor in a comment block and hit `Alt+Q` to re-wrap the contents to the column that's been specified in settings.

<table><tr></tr><tr><td>

``` clike
// Starts the Foo that                       
// has the        ¦
// given id.      ¦
void StartFoo(int id) {
```
</td><td><h3>&#10140;</h3></td><td>

``` clike
// Starts the Foo ¦                          
// that has the   ¦
// given id.      ¦
void StartFoo(int id) {
```
</td></tr></table>

The content of (most) comments is parsed as Markdown, so you can use lists, code samples (which are untouched) etc.

<table><tr></tr><tr><td>

``` clike
/// Takes up to n characters from the¦string.
/// * If n > str.length,             ¦
///   the whole string is returned.  ¦
/// * If n < 0, characters are taken from 
/// the end of the string.           ¦
///                                  ¦
/// Examples:                        ¦
///                                  ¦
///     takeChars(2, "Simple") // "Si"
///     takeChars(-3, "Fidget") // "get"
String takeChars(int n, String str) {¦
```
</td><td><h3>&#10140;</h3></td><td>

``` clike
/// Takes up to n characters from the¦       
/// string.                          ¦
/// * If n > str.length, the whole   ¦
///   string is returned.            ¦
/// * If n < 0, characters are taken ¦
///   from the end of the string.    ¦
///                                  ¦
/// Examples:                        ¦
///                                  ¦
///     takeChars(2, "Simple") // "Si"
///     takeChars(-3, "Fidget") // "get"
String takeChars(int n, String str) {¦
```
</td></tr></table>

Doc-comments with tags are no problem either.

<table><tr></tr><tr><td>

``` js
/**                                  ¦
 * Creates a new pair of shoes with the given
 * size and color.                   ¦
 * @constructor                      ¦
 * @param {Number} size - The        ¦
 * shoe size (European).             ¦
 * @param {SHOE_COLORS} color - The  ¦
 * shoe color. Must be a {@link SHOE_COLORS}.
 */                                  ¦
function Shoe(size, color) {         ¦
```
</td><td><h3>&#10140;</h3></td><td>

``` js
/**                                  ¦       
 * Creates a new pair of shoes with  ¦
 * the given size and color.         ¦
 * @constructor                      ¦
 * @param {Number} size - The shoe   ¦
 * size (European).                  ¦
 * @param {SHOE_COLORS} color - The  ¦
 * shoe color. Must be a             ¦
 * {@link SHOE_COLORS}.              ¦
 */                                  ¦
function Shoe(size, color) {         ¦
```
</td></tr></table>

Rewrap also works with Markdown or LaTeX files, or any plain text document.

### Selections ###

If you select just a couple of lines, only those lines will be processed.

In VS Code you can use multiple selections (alt+click) to select multiple comment blocks or line ranges.

However you can safely select a large line range with multiple comments and code, and only the comments will be processed (the code is untouched). You can `Ctrl+A, Alt+Q` to re-wrap a whole document at once.

----

More information on the specifics is available [here](https://github.com/stkb/Rewrap/blob/master/specs/Basics/README.md).

In general everything should just work as expected, so if it doesn't, [please file an issue](https://github.com/stkb/Rewrap/issues)!
