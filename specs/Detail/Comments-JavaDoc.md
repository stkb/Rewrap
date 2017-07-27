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

Alternative placement:

    /**        ¦        ->      /**        ¦
    *  aaa bbb ccc              *  aaa bbb ¦
     * ddd     ¦                *  ccc ddd ¦
    */         ¦                */         ¦
