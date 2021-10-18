> language: "markdown"

# Basic YAML Header must stay intact

    ---                              ¦    ->      ---  ¦
    uid:                             ¦            uid: ¦
    title: "High Level Architecture" ¦            title: "High Level Architecture" ¦
    ---                              ¦            ---  ¦

# YAML Header with embedded items must stay intact

    ---                    ¦              ---  ¦
    md_files:              ¦              md_files: ¦
      - 00-index.md        ¦                - 00-index.md ¦
      - 01-introduction.md ¦      ->        - 01-introduction.md ¦
    ---                    ¦              ---  ¦

# YAML Header with list of items must stay intact

    ---                                             ¦              ---  ¦
    md_files: ["00-index.md", "01-introduction.md"] ¦      ->      md_files: ["00-index.md", "01-introduction.md"] ¦
    ---                                             ¦              ---  ¦
