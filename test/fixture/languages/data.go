// Go's comments are like other c-style languages 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0

/*
Package regexp implements a simple library for regular expressions.

The syntax of the regular expressions accepted is:

    regexp:
        concatenation { '|' concatenation }
    concatenation:
        { closure }
    closure:
        term [ '*' | '+' | '?' ]
    term:
        '#'
        '$'
        '.'
        character
        '[' [ '#' ] character-ranges ']'
        '(' regexp ')'
*/
package regexp