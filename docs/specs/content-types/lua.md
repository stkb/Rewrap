> language: "lua"

Line comment

    --a b c      ->      --a b ¦
    --d   ¦              --c d ¦
    z y x w              z y x w
    v u   ¦              v u   ¦

Block comment

    --[[                                      --[[
      a b c                           ->        a b ¦
      d   ¦                                     c d ¦
    ]]--  ¦                                   ]]--  ¦
    z y x w                                   z y x w
    v u   ¦                                   v u   ¦


Block comment markers can have any number or '='s in them as long as they match

    --[=[                                     --[=[
      a b c                           ->        a b ¦
      d   ¦                                     c d ¦
    ]=]-- ¦                                   ]=]-- ¦
    z y x w                                   z y x w
    v u   ¦                                   v u   ¦

    --[=====[                                 --[=====[
      a b c                           ->        a b ¦
      d   ¦                                     c d ¦
    ]=====]--                                 ]=====]--
    z y x w                                   z y x w
    v u   ¦                                   v u   ¦
