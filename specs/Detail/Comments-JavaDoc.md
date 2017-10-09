# JavaDoc Comments #

> language: "javascript"

A `*` at the beginning of each line is conventional but not required. The prefix
for the whole comment is determined by the second line.

Conventional:

    /**        ¦        ->      /**        ¦
     * aaa bbb ccc               * aaa bbb ¦
       ddd     ¦                 * ccc ddd ¦
     */        ¦                 */        ¦

Star absent on first line:

    /**        ¦        ->      /**        ¦
       aaa bbb ccc                 aaa bbb ¦
     * ddd     ¦                   ccc ddd ¦
     */        ¦                 */        ¦

Alternative placement (or should this be corrected?):

    /**        ¦        ->      /**        ¦
    *  aaa bbb ccc              *  aaa bbb ¦
     * ddd     ¦                *  ccc ddd ¦
    */         ¦                */         ¦

Since the *'s are optional, they should not be consumed if used as bullets in a
comment where leading *'s are absent. 

    /**         ¦      ->      /**         ¦
    List:       ¦              List:       ¦
     * Item A   ¦               * Item A   ¦
     * Item B   ¦               * Item B   ¦
    */          ¦              */          ¦

Unfortunately the following case fails. The bullets are incorrectly interpreted
as leading *'s.

``` js
/** List:
* Item A
* Item B
*/
```