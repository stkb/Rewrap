// '' marks the beginning or end of a section

// A single line comment with an empty selection in it. 1 2 3 4 5 6 7 8 9 0 1 2
// 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0

// A single line comment with the whole line selected. 1 2 3 4 5 6 7 8 9 0 1 2 3
// 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0

// A comment with three lines. Only the first two
// are selected

/* A multi-line comment on a single line. When this gets wrapped the next line
is indented at the same level as the '/*' of the first line */

/* This is a multi-line comment that has enough text to be wrapped but it won't be because it hasn't been selected */

// This is a line comment that has enough text to be wrapped but it won't be because it hasn't been selected.
void DoNothingWithX(int x) {
    return x;
}