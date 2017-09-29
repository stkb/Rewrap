module internal Parsing.Documents

open System
open Extensions
open Rewrap
open Parsing.Core
open Parsing.DocComments
open Parsing.SourceCode


type Language = {
    name: string
    aliases: string[]
    extensions: string[]
    parser: Settings -> TotalParser
}

/// Constructs a Language
let private lang (name: string) (aliases: string) (extensions: string) parser : Language =
    {
        name = name
        aliases = aliases.Split([|'|'|], StringSplitOptions.RemoveEmptyEntries)
        extensions = extensions.Split([|'|'|], StringSplitOptions.RemoveEmptyEntries)
        parser = parser
    }


let private configFile =
    sourceCode [ line "#" ]


let languages : Language[] = [|
    lang "AutoHotkey" "ahk" ".ahk" 
        ( sourceCode [ line ";"; cBlock ] )
    lang "Basic" "vb" ".vb"
        ( sourceCode [ customLine html "'''"; line "'" ] )
    lang "Batch file" "bat" ".bat"
        ( sourceCode [ line "(?:rem|::)" ] )
    lang "C/C++" "c|c++|cpp" ".c|.cpp|.h"
        c
    lang "C#" "csharp" ".cs"
        ( sourceCode [ customLine html "///"; cLine; cBlock ] )
    lang "CoffeeScript" "" ".coffee"
        ( sourceCode 
            [ customBlock javadoc ("[*#]", " * ") ( "###\\*", "###" )
              block ( "###", "###" )
              line "#"
            ]
        )
    lang "CSS" "" ".css"
        css
    lang "Dart" "" ".dart"
        ( sourceCode
            [ customLine dartdoc "///"
              cLine
              customBlock dartdoc ( "\*", " * " ) javadocMarkers
              cBlock
            ]
        )
    lang "Dockerfile" "docker" "dockerfile"
        configFile
    lang "Elixir" "" ".ex|.exs"
        configFile
    lang "Elm" "" ".elm"
        ( sourceCode [ line "--"; block ( "{-\\|?", "-}" ) ] )
    lang "F#" "fsharp" ".fs|.fsx"
        ( sourceCode [ customLine html "///"; cLine; block ( @"\(\*", @"\*\)" ) ] )
    lang "Go" "" ".go"
        c
    lang "Git commit" "git-commit" "tag_editmsg"
        Markdown.markdown
    lang "GraphQL" "graphql" ".graphql"
        configFile
    lang "Groovy" "" ".groovy"
        c
    lang "Haskell" "" ".hs"
        ( sourceCode [ line "--\\|?"; block ( "{-\\|?", "-}" ) ] )
    lang "HTML" "" ".htm|.html"
        html
    lang "INI" "" ".ini"
        ( sourceCode [ line "[#;]" ] )
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
        ( sourceCode [ line "--"; block ( "--\\[\\[", "\\]\\]" ) ] )
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
        ( sourceCode
            [ customBlock javadoc ( "\\*", " * " ) javadocMarkers
              cBlock
              line @"(?://|#)"
            ]
        )
    lang "PowerShell" "" ".ps1|.psd1|.psm1"
        ( sourceCode [ customLine psdoc "#"; customBlock psdoc ( "", "" ) ( "<#", "#>" ) ] )
    lang "Pug" "jade" ".jade|.pug"
        ( sourceCode [ cLine ] )
    lang "PureScript" "" ".purs"
        ( sourceCode [ line "--\\|?"; block ( "{-\\|?", "-}" ) ] )
    lang "Python" "" ".py"
        ( sourceCode [ line "#"; block ( "('''|\"\"\")", "('''|\"\"\")" ) ] )
    lang "R" "" ".r"
        configFile
    lang "Ruby" "" ".rb"
        ( sourceCode [ line "#"; block ( "=begin", "=end" ) ] )
    lang "Rust" "" ".rs"
        ( sourceCode [ line @"\/\/(?:\/|\!)?" ] )
    lang "SCSS" "" ".scss"
    // Sass still needs to be supported.
    // -  http://sass-lang.com/documentation/file.INDENTED_SYNTAX.html
        c
    lang "Shaderlab" "" ".shader"
        c
    lang "Shell script" "shellscript" ".sh"
        configFile
    lang "SQL" "" ".sql"
        ( sourceCode [ line "--"; cBlock ] )
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


/// Gets a language ID from a given file path.
/// </summary>
let private languageFromFileName (filePath: string) : Option<Language> =
    
    let fileName = 
        filePath.Split('\\', '/') |> Array.last

    // Get file extension or if no extension, the whole filename
    let extensionOrName =
        match fileName.ToLower().Split('.') with
            | [| name |] -> name
            | arr -> "." + Array.last arr
    
    languages
        |> Array.tryFind (fun l -> Array.contains extensionOrName l.extensions)


/// <summary>
/// Looks for a known language from either the given language name or the file path.
/// </summary>
let findLanguage name filePath : Option<Language> =

    let findName (name: string) : Option<Language> =
        languages
            |> Array.tryFind 
                (fun l -> 
                    l.name.ToLower() = name.ToLower() 
                        || Array.contains (name.ToLower()) l.aliases
                )
    
    findName name
        |> Option.orElseWith (fun () -> languageFromFileName filePath)


/// <summary>
/// Selects a parser from the given language and file path.
/// </summary>
/// <remarks>
/// First the language is checked. If this is fails to find a parser, the file
/// name is checked. If this also fails, a default plain text parser is used.
/// </remarks>
let rec select (language: string) (filePath: string) : Settings -> TotalParser =

    let plainText =
            sourceCode []
    
    findLanguage language filePath
        |> Option.map (fun l -> l.parser)
        |> Option.defaultValue plainText
