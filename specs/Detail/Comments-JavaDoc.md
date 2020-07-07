# JavaDoc Comments #

> language: "javascript"

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

## Stars in content ##

In this example, the 

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
