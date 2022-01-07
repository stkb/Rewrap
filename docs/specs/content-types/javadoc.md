# JavaDoc

Java, Java/Type/CoffeeScript and PHP all support java/jsdoc (/** ... */), which
makes use of @tags. The tag structure is preserved when rewrapping.

> language: "javascript"

    /**                        ¦         ->      /**                        ¦
     * Description of the function.               * Description of the      ¦
     * @param {string} str     ¦                  * function.               ¦
     * @returns {string}       ¦                  * @param {string} str     ¦
     * The return value        ¦                  * @returns {string} The   ¦
     */                        ¦                  * return value            ¦
    function convert(str) {                       */
                                                 function convert(str) {

> language: "coffeescript"

Coffeescript has two common comment styles, and in each case the positioning of
the #/* characters is preserved.

    ###*                       ¦         ->      ###*                       ¦
     * Description of the function.               * Description of the      ¦
     * @param {string} str     ¦                  * function.               ¦
     * @returns {string}       ¦                  * @param {string} str     ¦
     * The return value        ¦                  * @returns {string} The   ¦
     ###                       ¦                  * return value            ¦
                                                  ###

    ###*                      ¦         ->      ###*                      ¦
    # Description of the function.              # Description of the      ¦
    # @param {string} str     ¦                 # function.               ¦
    # @returns {string}       ¦                 # @param {string} str     ¦
    # The return value        ¦                 # @returns {string} The   ¦
    ###                       ¦                 # return value            ¦
                                                ###

### Tags ###

> language: "javascript"

Where a line starts with a tag, it starts a new paragraph for wrapping.

    /**            ¦       ->      /**            ¦
     * Text        ¦                * Text        ¦
     * @tag text text               * @tag text   ¦
     */            ¦                * text        ¦
                                    */

Where a tag stands alone on a line, the line break after it is preserved and
text on the following line is not wrapped with it. Sometimes this style is
preferred:

    /**                    ¦          ->      /**                    ¦
     * @description        ¦                   * @description        ¦
     * Long description that                   * Long description    ¦
     * starts on the next line.                * that starts on the  ¦
     */                    ¦                   * next line.          ¦
                           ¦                   */                    ¦

Also, the @example tag is a special case: all lines between it and the following
tag are assumed to be code and therefore ignored for wrapping.

    /**                          ¦      ->      /**                          ¦
     * @example                  ¦               * @example                  ¦
     * // returns 2              ¦               * // returns 2              ¦
     * Math.abs(3 - 5)           ¦               * Math.abs(3 - 5)           ¦
     *                           ¦               *                           ¦
     * // returns 5              ¦               * // returns 5              ¦
     * Math.abs(1 - 6)           ¦               * Math.abs(1 - 6)           ¦
     * @returns {Number}         ¦               * @returns {Number} Absolute¦
     * Absolute value of         ¦               * value of the number given.¦
     * the number given.         ¦               */                          ¦
     */                          ¦                                           ¦

An @example tag with text on the same line however, is treated as any other
(used in php docs)

> language: "php"

    /**             ¦          ->      /**             ¦
     * text         ¦                   * text         ¦
     * @example url text1               * @example url ¦
     * text2        ¦                   * text1 text2  ¦
     */             ¦                   */             ¦


#### Inline tags ####

Inline tags are not broken up

    /**            ¦           ->      /**            ¦
     * {@link a b \} c} d               * {@link a b \} c}
     * e           ¦                    * d e         ¦
     */            ¦                    */            ¦


## Technical details ##

A `*` at the beginning of each line is conventional but not required. The prefix
or each line is preserved.

Conventional:

    /**    ¦      ->   /**    ¦
     * a   ¦            * a b ¦
     * b c ¦            * c   ¦
     */    ¦            */    ¦

Star absent on first line:

    /**        ¦        ->      /**        ¦
       aaa bbb ccc                 aaa bbb ¦
     * ddd     ¦                 * ccc ddd ¦
     */        ¦                 */        ¦

Irregular placement:

    /**        ¦        ->      /**        ¦
    *  aaa bbb ccc              *  aaa bbb ¦
     * ddd     ¦                 * ccc ddd ¦
    */         ¦                */         ¦

### Stars in content ###

    /**            ¦      ->      /**            ¦
     * List:       ¦               * List:       ¦
     * * Item A    ¦               * * Item A    ¦
     * * Item B    ¦               * * Item B    ¦
     */            ¦               */            ¦

Since the *'s are optional, they should not be consumed if used as bullets in a
comment where leading *'s are absent.

    /**         ¦      ->      /**         ¦
    List:       ¦              List:       ¦
     * Item A   ¦               * Item A   ¦
     * Item B   ¦               * Item B   ¦
    */          ¦              */          ¦

Since the *'s are optional, they should not be consumed if used as bullets in a
comment where leading *'s are absent.

Unfortunately the following case fails. The bullets are incorrectly interpreted
as leading *'s.

``` js
/** List:
* Item A
* Item B
*/
```

Relevant: https://www.doxygen.nl/manual/markdown.html#mddox_stars
