> language: "markdown"

A fenced code block begins with at least 3 backticks or at least 3 tildes.

    ```       ¦      ->      ```       ¦
    code      ¦              code      ¦
    block     ¦              block     ¦

    ~~~       ¦      ->      ~~~       ¦
    code      ¦              code      ¦
    block     ¦              block     ¦

Fewer than 3 doesn't start a code block. (However the `` on its own line counts
as a non-text line, which we don't wrap.)

    ``        ¦      ->      ``        ¦
    no        ¦              no code   ¦
    code block¦              block     ¦

    ~~        ¦      ->      ~~        ¦
    no        ¦              no code   ¦
    code block¦              block     ¦

As with other markdown they may be indented up to inc 3 spaces.

    ···~~~    ¦      ->      ···~~~    ¦
    code                     code      ¦
    block                    block     ¦

4 or more and it doesn't count.

    ····~~~   ¦              ->      ····~~~   ¦
    Not in a code block              Not in a  ¦
                                     code block¦

If the opening code fence is tildes, any characters may follow it.

    ~~~a bc`~`~              ~~~a bc`~`~
    code      ¦              code      ¦
    block     ¦      ->      block     ¦
    ~~~       ¦              ~~~       ¦
    one       ¦              one two   ¦
    two       ¦                        ¦

If the opening code fence is backticks, any characters except backticks may
follow it.

    ```a bc~~~~              ```a bc~~~~
    code      ¦              code      ¦
    block     ¦      ->      block     ¦
    ```       ¦              ```       ¦
    text      ¦              text text ¦
    text      ¦                        ¦

If there are backticks following, then the text between is treated as inline code
instead.

    ```a bc``¦              ```a bc``
    not in   ¦      ->      not in a ¦
    a code   ¦              code     ¦
    block    ¦              block    ¦

The closing fence must use the same character as the opening fence.

    ```       ¦                ```       ¦
    code      ¦                code      ¦
    block     ¦                block     ¦
    ~~~       ¦                ~~~       ¦
    still     ¦        ->      still     ¦
    in code block              in code block
    ```       ¦                ```       ¦
    outside code               outside   ¦
    block     ¦                code block¦

    ~~~       ¦                ~~~       ¦
    code      ¦                code      ¦
    block     ¦                block     ¦
    ```       ¦                ```       ¦
    still     ¦        ->      still     ¦
    in code block              in code block
    ~~~       ¦                ~~~       ¦
    outside code               outside   ¦
    block     ¦                code block¦

If the closing fence is indented more than 3 spaces (relative to the container
not the opening fence), it doesn't count.

    ··```     ¦                ··```     ¦
    code      ¦                code      ¦
    block     ¦                block     ¦
    ····```   ¦                ····```   ¦
    still     ¦        ->      still     ¦
    in code block              in code block
    ```       ¦                ```       ¦
    outside code               outside   ¦
    block     ¦                code block¦

If the opening fence is 4 or more characters, the closing fence must be as least
as long.

    ~~~~      ¦                ~~~~      ¦
    code      ¦                code      ¦
    block     ¦                block     ¦
    ~~~       ¦                ~~~       ¦
    still     ¦        ->      still     ¦
    in code block              in code block
    ~~~~~     ¦                ~~~~~     ¦
    outside code               outside   ¦
    block     ¦                code block¦

If there is anything other than whitespace on the line after the closing fence,
it doesn't count.

    ```       ¦                ```       ¦
    code      ¦                code      ¦
    block     ¦                block     ¦
    ``` abc   ¦                ``` abc   ¦
    still     ¦        ->      still     ¦
    in code block              in code block
    ```       ¦                ```       ¦
    outside code               outside   ¦
    block     ¦                code block¦
