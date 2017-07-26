module internal Parsing.Documents

open Extensions
open Rewrap
open Parsing.Core
open Parsing.Comments
open Parsing.DocComments
open Parsing.SourceCode

type Language = {
    name: string
    aliases: string[]
    extensions: string[]
    parser: Settings -> TotalParser
}

/// <summary>
/// Constructs a Language
/// </summary>
let private lang (name: string) (aliases: string) (extensions: string) parser : Language =
    {
        name = name
        aliases = aliases.Split('|')
        extensions = extensions.Split('|')
        parser = parser
    }


let private c =
    sourceCode (Some "//") (Some ( "/\\*", "\\*/" ))

let private configFile =
    sourceCode (Some "#") None


let languages : Language[] = [|
    lang "AutoHotkey" "ahk" ".ahk" 
        ( sourceCode (Some ";") (Some ( "\\/\\*", "\\*\\/" )) )
    lang "Basic" "vb" ".vb"
        ( customSourceCode [ lineComment html "'''"; stdLineComment "'" ] )
    lang "Batch file" "bat" ".bat"
        ( sourceCode (Some "(?:rem|::)") None )
    lang "C/C++" "c|c++|cpp" ".c|.cpp|.h"
        c
    lang "C#" "csharp" ".cs"
        ( customSourceCode 
            [ lineComment html "///"
              stdLineComment "//"
              stdBlockComment cBlockMarkers 
            ]
        )
    lang "CoffeeScript" "" ".coffee"
        ( customSourceCode 
            [ blockComment javadoc ("[*#]", " * ") ( "###\\*", "###" )
              stdBlockComment ( "###", "###" )
              stdLineComment "#"
            ]
        )
    lang "CSS" "" ".css"
        css
    lang "Dart" "" ".dart"
        ( customSourceCode
            [ lineComment dartdoc "///"
              stdLineComment "//"
              blockComment dartdoc ( "\*", " * " ) javadocMarkers
              stdBlockComment cBlockMarkers
            ]
        )
    lang "Dockerfile" "docker" "dockerfile"
        configFile
    lang "Elixir" "" ".ex|.exs"
        configFile
    lang "Elm" "" ".elm"
        ( sourceCode (Some "--") (Some ( "{-\\|?", "-}" )) )
    lang "F#" "fsharp" ".fs|.fsx"
        ( customSourceCode 
            [ lineComment html "///"
              stdLineComment "//"
              stdBlockComment ( "\\(\\*", "\\*\\)" )
            ]
        )
    lang "Go" "" ".go"
        c
    lang "Groovy" "" ".groovy"
        c
    lang "Haskell" "" ".hs"
        ( sourceCode (Some "--\\|?") (Some ( "{-\\|?", "-}" )) )
    lang "HTML" "" ".htm|.html"
        html
    lang "INI" "" ".ini"
        ( sourceCode (Some "[#;]") None )
    lang "Java" "" ".java"
        java
    lang "JavaScript" "javascriptreact|js" ".js|.jsx"
        java
    lang "JSON" "json5" ".json|.json5"
        java
    lang "LaTeX" "tex" ".bbx|.cbx|.cls|.sty|.tex"
        Latex.latex
    lang "Less" "" ".less"
        c
    lang "Lua" "" ".lua"
        ( sourceCode (Some "--") (Some ( "--\\[\\[", "\\]\\]" )) )
    lang "Makefile" "make" "makefile"
        configFile
    lang "Markdown" "" ".md"
        Markdown.markdown
    lang "Objective-C" "" ".m|.mm"
        c
    lang "Perl" "perl6" ".p6|.pl|.pl6|.pm|.pm6"
        // Putting Perl & Perl6 together. Perl6 also has a form of block comment
        // which still needs to be supported.
        // https://docs.perl6.org/language/syntax#Comments
        configFile
    lang "PHP" "" ".php"
        ( customSourceCode
            [ blockComment javadoc ( "\\*", " * " ) javadocMarkers
              stdBlockComment cBlockMarkers
              stdLineComment @"(?://|#)"
            ]
        )
    lang "PowerShell" "" ".ps1|.psm1"
        ( sourceCode (Some "#") (Some ( "<#", "#>" )) )
    lang "Pug" "jade" ".jade|.pug"
        ( sourceCode (Some "\\/\\/") None )
    lang "Purescript" "" ".purs"
        ( sourceCode (Some "--\\|?") (Some ( "{-\\|?", "-}" )) )
    lang "Python" "" ".py"
        ( sourceCode (Some "#") (Some ( "('''|\"\"\")", "('''|\"\"\")" )) )
    lang "R" "" ".r"
        configFile
    lang "Ruby" "" ".rb"
        ( sourceCode (Some "#") (Some ( "=begin", "=end" )) )
    lang "Rust" "" ".rs"
        ( sourceCode (Some "\\/{2}(?:\\/|\\!)?") None )
    lang "SCSS" "" ".scss"
    // Sass still needs to be supported.
    // -  http://sass-lang.com/documentation/file.INDENTED_SYNTAX.html
        c
    lang "Shaderlab" "" ".shader"
        c
    lang "Shell script" "shellscript" ".sh"
        configFile
    lang "SQL" "" ".sql"
        ( sourceCode (Some "--") (Some ( "\\/\\*", "\\*\\/" )) )
    lang "Swift" "" ".swift"
        c
    lang "TOML" "" ".toml"
        configFile
    lang "TypeScript" "typescriptreact" ".ts|.tsx"
        java
    lang "XML" "xsl" ".xml|.xsl"
        html
    lang "YAML" "" ".yaml|.yml"
        configFile
|]


/// <summary>
/// Gets a language ID from a given file path.
/// </summary>
let languageFromFileName (filePath: string) : Option<string> =
    
    let fileName = 
        filePath.Split('\\', '/') |> Array.last

    // Get file extension or if no extension, the whole filename
    let extensionOrName =
        match fileName.ToLower().Split('.') with
            | [| name |] -> name
            | arr -> "." + Array.last arr
    
    languages
        |> Array.tryFind (fun l -> Array.contains extensionOrName l.extensions)
        |> Option.map(fun l -> l.name)


/// <summary>
/// Selects a parser from the given language and file path.
/// </summary>
/// <remarks>
/// First the language is checked. If this is fails to find a parser, the file
/// name is checked. If this also fails, a default plain text parser is used.
/// </remarks>
let rec select (language: string) (filePath: string) : Settings -> TotalParser =

    let findName (name: string) : Option<Language> =
        languages
            |> Array.tryFind 
                (fun l -> 
                    l.name.ToLower() = name.ToLower() 
                        || Array.contains (name.ToLower()) l.aliases
                )

    let plainText =
            sourceCode None None
    
    findName language
        |> Option.orElseWith
            (fun () ->
                languageFromFileName filePath |> Option.bind findName
            )
        |> Option.map (fun l -> l.parser)
        |> Option.defaultValue plainText
