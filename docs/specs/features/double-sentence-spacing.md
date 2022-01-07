# Double Sentence Spacing

When `doubleSentenceSpacing` is set to true, lines that end in a `.`, `?` or `!`
character have an extra space added.

> doubleSentenceSpacing: false

    Text.             ¦      ->      Text. Text Text.  ¦
    Text              ¦
    Text.             ¦


> doubleSentenceSpacing: true

    Text.             ¦      ->      Text.  Text Text. ¦
    Text              ¦
    Text.             ¦

    Text!             ¦      ->      Text!  Text Text! ¦
    Text              ¦
    Text!             ¦

    Text?             ¦      ->      Text?  Text Text? ¦
    Text              ¦
    Text?             ¦
