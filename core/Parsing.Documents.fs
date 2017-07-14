module internal Parsing.Documents

open Nonempty
open Extensions
open Rewrap
open Parsing.Core
open Parsing.Comments
open Parsing.DocComments
open Parsing.SourceCode


let private parsersTable =
    // plaintext must not be in this list
    [ ( [ "ahk" ]
        , sourceCode (Some ";") (Some ( "\\/\\*", "\\*\\/" ))
      )
      ( [ "basic"; "vb" ]
      , customSourceCode [ lineComment html "'''"; stdLineComment "'" ]
      )
      ( [ "bat" ]
      , sourceCode (Some "(?:rem|::)") None
      )
      ( [ "c"; "cpp"; "go"; "groovy"; "objective-c"; "shaderlab"; "swift" ]
      , sourceCode (Some "//") (Some ( "/\\*", "\\*/" ))
      )
      ( [ "csharp" ]
      , customSourceCode [ lineComment html "///"; stdLineComment "//"; stdBlockComment cBlockMarkers ]
      )
      ( [ "coffeescript" ]
      , customSourceCode 
            [ blockComment javadoc ("[*#]", " * ") ( "###\\*", "###" )
              stdBlockComment ( "###", "###" )
              stdLineComment "#"
            ]
      )
      ( [ "css"; "less"; "scss" ]
      , css
      )
      ( [ "dart" ]
      , customSourceCode
            [ lineComment dartdoc "///"
              stdLineComment "//"
              blockComment dartdoc ( "\*", " * " ) javadocMarkers
              stdBlockComment cBlockMarkers
            ]
      )
      ( [ "dockerfile"; "elixir"; "makefile"; "perl"; "r"; "shellscript"; "toml"; "yaml" ]
      , sourceCode (Some "#") None
      )
      ( [ "elm" ]
      , sourceCode (Some "--") (Some ( "{-\\|?", "-}" ))
      )
      ( [ "f#"; "fsharp" ]
      , customSourceCode 
            [ lineComment html "///"
              stdLineComment "//"
              stdBlockComment ( "\\(\\*", "\\*\\)" )
            ]
      )
      ( [ "handlebars"; "html"; "xml"; "xsl" ]
      , html
      )
      ( [ "haskell"; "purescript" ]
      , sourceCode (Some "--") (Some ( "{-", "-}" ))
      )
      ( [ "ini" ]
      , sourceCode (Some "[#;]") None
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
      // Todo: block comments in Perl 6
      // https://docs.perl6.org/language/syntax#Comments
      ( [ "perl"; "perl6"; "ruby" ]
      , sourceCode (Some "#") (Some ( "=begin", "=end" ))
      )
      ( [ "php" ]
      , customSourceCode
            [ blockComment javadoc ( "\\*", " * " ) javadocMarkers
              stdBlockComment cBlockMarkers
              stdLineComment @"(?://|#)"
            ]
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
    ]


let private languagesTable : List<List<string> * string> = 
    [
        ( [ ".ahk" ], "ahk" )
        ( [ ".bat" ], "bat" )
        ( [ ".bbx"; ".cbx"; ".cls"; ".sty" ], "tex" )
        ( [ ".c"; ".cpp"; ".h"; ".m"; ".mm" ], "c" )
        ( [ ".coffee" ], "coffeescript" )
        ( [ ".cs" ], "csharp" )
        // Pretend .sass comments are the same as .scss for basic support.
        // Actually they're slightly different.
        // http://sass-lang.com/documentation/file.INDENTED_SYNTAX.html
        ( [ ".css"; ".less"; ".sass"; ".scss" ], "scss" )
        ( [ ".dart" ], "dart" )
        ( [ "dockerfile" ], "dockerfile" )
        ( [ ".elm" ], "elm" )
        ( [ ".ex"; ".exs" ], "elixir" )
        ( [ ".fs"; ".fsx" ], "fsharp" )
        ( [ ".go" ], "go" )
        ( [ ".groovy" ], "groovy" )
        ( [ ".hs" ], "haskell" )
        ( [ ".ini" ], "ini" )
        ( [ ".java" ], "java" )
        ( [ ".js"; ".jsx"; ".ts"; ".tsx" ], "javascript" )
        ( [ ".lua" ], "lua" )
        ( [ "makefile" ], "makefile" )
        ( [ ".md" ], "markdown" )
        ( [ ".php" ], "php")
        ( [ ".pl"; ".pm" ], "perl")
        ( [ ".ps1"; ".psm1" ], "powershell" )
        ( [ ".purs" ], "purescript" )
        ( [ ".py" ], "python" )
        ( [ ".r" ], "r" )
        ( [ ".rs" ], "rust" )
        ( [ ".sh" ], "shellscript" )
        ( [ ".sql" ], "sql" )
        ( [ ".swift" ], "swift" )
        ( [ ".tex" ], "latex" )
        ( [ ".toml" ], "toml" )
        ( [ ".vb" ], "vb" )
        ( [ ".xml"; ".xsl" ], "xml" )
        ( [ ".yaml" ], "yaml" )
    ]


let private findIn table id =
        List.tryFind (fst >> List.contains id) table
            |> Option.map snd


let languageFromFileName (filePath: string) : Option<string> =
    
    let fileName = 
        filePath.Split('\\', '/') |> Array.last

    // Get file extension or if no extension, the whole filename
    let extensionOrName =
        match fileName.ToLower().Split('.') with
            | [| name |] -> name
            | arr -> "." + Array.last arr

    findIn languagesTable extensionOrName


let select (language: string) (filePath: string) : Settings -> TotalParser =

    let plainText =
            sourceCode None None

    findIn parsersTable (language.ToLower())
        |> Option.orElseWith
            (fun () ->
                languageFromFileName filePath
                    |> Option.bind (findIn parsersTable)
            )
        |> Option.defaultValue plainText


