# Special Characters

Wrapping should work with any characters.

> language: "markdown"

    я я я      ->      я я ¦
    я   ¦              я я ¦

> language: "csharp"

    // я я я      ->      // я я ¦
    // я   ¦              // я я ¦


## East Asian characters

East Asian (CJK) languages are also supported. Most CJK glyphs take up two
columns, and wrapping can come after most characters.

> language: markdown

Chinese

    再舞心現張否里案      ->      再舞心現 ¦
             ¦                    張否里案

Japanese

    政秋造レ員芸でか      ->      政秋造レ ¦
             ¦                    員芸でか

In Korean, lines are split on spaces

    힘차게        ¦      ->      힘차게 이상   ¦
    이상 사랑의   ¦              사랑의        ¦

Some characters may not appear at the start of a line (this applies to all text in all
languages):

```
})]?,;¢°′″‰℃、。｡､￠，．：；？！％・･ゝゞヽヾーァィゥェォッャュョヮヵヶぁぃぅぇぉっゃゅょゎゕゖ
ㇰㇱㇲㇳㇴㇵㇶㇷㇸㇹㇺㇻㇼㇽㇾㇿ々〻ｧｨｩｪｫｬｭｮｯｰ”〉》」』】〕）］｝｣
```

    ああああぁあ      ->      あああ   ¦
             ¦                あぁあ   ¦

Unless there was a space before

    ああああ ぁあ      ->      ああああ ¦
             ¦                 ぁあ     ¦

And some may not end a line: `([{‘“〈《「『【〔（［｛｢£¥＄￡￥＋`

    诶诶诶《诶诶诶      ->      诶诶诶   ¦
             ¦                  《诶诶诶 ¦

Unless there was a space after

    诶诶诶《 诶诶诶      ->      诶诶诶《 ¦
             ¦                   诶诶诶   ¦
