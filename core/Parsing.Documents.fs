module internal Parsing.Documents

open Nonempty
open Rewrap
open Parsing.Core
open Extensions
open System.Text.RegularExpressions


/// Parser for standard source code files
let private sourceCode 
    (maybeSingleMarker: Option<string>)
    (maybeMultiMarkers: Option<string * string>)
    (settings: Settings)
    : TotalParser =

    let otherParsers =
        tryMany
            (List.choose id
                [ Some blankLines
                  maybeSingleMarker
                    |> Option.map 
                        (Comments.lineComment Markdown.markdown settings)
                  maybeMultiMarkers
                    |> Option.map 
                        (Comments.multiComment Markdown.markdown settings ("","") )
                ]
            )
      
    let codeParser =
        takeLinesUntil
            otherParsers
            (splitIntoChunks (onIndent settings.tabWidth)
                >> Nonempty.map (indentSeparatedParagraphBlock Block.code)
            )

    repeatUntilEnd otherParsers codeParser


/// Parser with javadoc comments
let private withJavaDoc 
    (jDocMarkers: string * string)
    (lineMarker: string)
    (multiMarkers: string * string)
    settings 
    : TotalParser =

    let javaDoc _ =
        splitIntoChunks (beforeRegex (Regex("^\\s*@")))
            >> Nonempty.collect
                (fun (Nonempty(firstLine, _) as lines) ->
                    if Line.contains (Regex("^\\s*@example")) firstLine then
                        Block.ignore lines |> Nonempty.singleton
                    else
                        lines
                            |> (splitIntoChunks
                                    (afterRegex (Regex("^\\s*@\\w+\\s*$")))
                                )
                            |> Nonempty.collect (Markdown.markdown settings)
                )

    let otherParsers =
        tryMany
            [ blankLines
              Comments.multiComment javaDoc settings ( "[*#]", " * " ) jDocMarkers
              Comments.multiComment Markdown.markdown settings ( "", "" ) multiMarkers
              Comments.lineComment Markdown.markdown settings lineMarker
            ]

    let codeParser =
        takeLinesUntil
            otherParsers
            (splitIntoChunks (onIndent settings.tabWidth)
                >> Nonempty.map (indentSeparatedParagraphBlock Block.code)
            )

    repeatUntilEnd otherParsers codeParser


/// Parser for css (also used in html)
let private css =
    sourceCode None (Some ( "/\\*", "\\*/" ))

/// Parser for java/javascript (also used in html)
let java =
    withJavaDoc ( "/\\*\\*", "\\*/" ) "//" ( "/\\*", "\\*/" )

/// Parser for html (also used in dotNet)
let private html = 
    Parsing.Html.html java css


/// Constructs a parser for .Net languages, supporting xmldoc comments
let dotNet 
    (xmlDocMarker: string) 
    (lineMarker: string)
    (maybeMultiMarkers: Option<string * string>)
    (settings: Settings)
    : TotalParser =

    let otherParsers =
        tryMany
            (List.choose id
                [ Some blankLines
                  Some (Comments.lineComment html settings xmlDocMarker)
                  Some (Comments.lineComment Markdown.markdown settings lineMarker)
                  maybeMultiMarkers
                    |> Option.map
                        (Comments.multiComment Markdown.markdown settings ( "", "" ))
                ]
            )

    let codeParser =
        takeLinesUntil
            otherParsers
            (splitIntoChunks (onIndent settings.tabWidth)
                >> Nonempty.map (indentSeparatedParagraphBlock Block.code)
            )
    in
        repeatUntilEnd otherParsers codeParser





let private parsersTable =
    // plaintext must not be in this list
    [ ( [ "ahk" ]
        , sourceCode (Some ";") (Some ( "\\/\\*", "\\*\\/" ))
      )
      ( [ "bat" ]
      , sourceCode (Some "(?:rem|::)") None
      )
      ( [ "c"; "cpp"; "go"; "groovy"; "objective-c"; "shaderlab"; "swift" ]
      , sourceCode (Some "//") (Some ( "/\\*", "\\*/" ))
      )
      ( [ "csharp" ]
      , dotNet "///" "//" (Some ( "/\\*", "\\*/" ))
      )
      ( [ "coffeescript" ]
      , withJavaDoc ( "###\\*", "###" ) "#" ( "###", "###" )
      )
      ( [ "css"; "less"; "scss" ]
      , css
      )
      ( [ "dockerfile"; "makefile"; "perl"; "r"; "shellscript"; "toml"; "yaml" ]
      , sourceCode (Some "#") None
      )
      ( [ "elm" ]
      , sourceCode (Some "--") (Some ( "{-\\|?", "-}" ))
      )
      ( [ "fsharp" ]
      , dotNet "///" "//" (Some ( "\\(\\*", "\\*\\)" ))
      )
      ( [ "handlebars"; "html"; "xml"; "xsl" ]
      , html
      )
      ( [ "haskell"; "purescript" ]
      , sourceCode (Some "--") (Some ( "{-", "-}" ))
      )
      ( [ "ini" ]
      , sourceCode (Some "[# ]") None
      )
      ( [ "jade" ]
      , sourceCode (Some "\\/\\/") None
      )
      ( [ "java"; "javascript"; "javascriptreact"; "json"; "typescript"; "typescriptreact" ]
      , java
      )
      ( [ "latex"; "tex" ]
      , Latex.latex
      )
      ( [ "lua" ]
      , sourceCode (Some "--") (Some ( "--\\[\\[", "\\]\\]" ))
      )
      ( [ "markdown" ]
      , Markdown.markdown
      )
      // Todo: multi-line comments in Perl 6
      // https://docs.perl6.org/language/syntax#Comments
      ( [ "perl6"; "ruby" ]
      , sourceCode (Some "#") (Some ( "=begin", "=end" ))
      )
      ( [ "php" ]
      , sourceCode (Some "(?:\\/\\/|#)") (Some ( "\\/\\*", "\\*\\/" ))
      )
      ( [ "powershell" ]
      , sourceCode (Some "#") (Some ( "<#", "#>" ))
      )
      ( [ "python" ]
      , sourceCode (Some "#") (Some ( "('''|\"\"\")", "('''|\"\"\")" ))
      )
      ( [ "rust" ]
      , sourceCode (Some "\\/{2}(?:\\/|\\!)?") None
      )
      ( [ "sql" ]
      , sourceCode (Some "--") (Some ( "\\/\\*", "\\*\\/" ))
      )
      ( [ "vb" ]
      , dotNet "'''" "'" None
      )
    ]


let private languagesTable : List<List<string> * string> = 
    [
        ( [ ".ahk" ], "ahk" )
        ( [ ".bbx"; "cbx"; ".cls"; ".sty" ], "tex" )
        ( [ ".cs" ], "csharp" )
        ( [ ".elm" ], "elm" )
        ( [ ".hs" ], "haskell" )
        ( [ ".js"; ".jsx"; ".ts" ], "javascript" )
        ( [ ".md" ], "markdown" )
        ( [ ".purs" ], "purescript" )
        // Pretend .sass comments are the same as .scss for basic support.
        // Actually they're slightly different.
        // http://sass-lang.com/documentation/file.INDENTED_SYNTAX.html
        ( [ ".sass" ], "scss" )
        ( [ ".tex" ], "latex" )
        ( [ ".toml" ], "toml" )
    ]



let select (language: string) (extension: string) : Settings -> TotalParser =
    let findIn table id =
            List.tryFind (fst >> List.contains id) table
                |> Option.map snd

    let plainText =
            sourceCode None None

    findIn parsersTable language
        |> Option.orElseWith
            (fun () ->
                findIn languagesTable extension
                    |> Option.bind (findIn parsersTable)
            )
        |> Option.defaultValue plainText


