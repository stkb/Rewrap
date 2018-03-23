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


let plainText settings =
    let paragraphs =
        splitIntoChunks (onIndent settings.tabWidth)
            >> Nonempty.map (indentSeparatedParagraphBlock Block.text)

    takeUntil blankLines paragraphs |> repeatToEnd


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
        java
    lang "C#" "csharp" ".cs"
        ( sourceCode
            [ customLine html "///"
              cLine
              customBlock javadoc ( "\\*?", " * " ) javadocMarkers
              cBlock
            ]
        )
    lang "CoffeeScript" "" ".coffee"
        ( sourceCode 
            [ customBlock javadoc ("[*#]", " * ") ( "###\\*", "###" )
              block ( "###", "###" )
              line "#"
            ]
        )
    lang "Configuration" "properties" ".conf|.gitconfig"
        configFile
    lang "Crystal" "" ".cr"
        ( sourceCode [ line "#" ] )
    lang "CSS" "" ".css"
        css
    // Special rules for DDoc need adding
    lang "D" "" ".d"
        ( sourceCode
            [ customLine ddoc "///"
              cLine
              customBlock ddoc ( "\*", " * " ) javadocMarkers
              customBlock ddoc ( "\+", " + " ) ("/\+\+", "\+/")
              cBlock
              block ( "/\+", "\+/" )
            ]
        )
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
        ( sourceCode [ line "#"; block ("@(module|type|)doc\s+\"\"\"", "\"\"\"") ] )
    lang "Elm" "" ".elm"
        ( sourceCode [ line "--"; block ( "{-\|?", "-}" ) ] )
    lang "F#" "fsharp" ".fs|.fsx"
        ( sourceCode [ customLine html "///"; cLine; block ( @"\(\*", @"\*\)" ) ] )
    lang "Go" "" ".go"
        ( sourceCode
            [ customBlock DocComments.godoc ( "", "" ) javadocMarkers
              cBlock
              customLine DocComments.godoc "//"
              cLine
            ]
        )
    lang "Git commit" "git-commit" "tag_editmsg"
        Markdown.markdown
    lang "GraphQL" "" ".graphql|.gql"
        configFile
    lang "Groovy" "" ".groovy"
        java
    lang "Haskell" "" ".hs"
        ( sourceCode [ line "--"; block ( "{-\s*\|?", "-}" ) ] )
    lang "HTML" "vue" ".htm|.html|.vue"
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
    lang "Lean" "" ".lean"
        ( sourceCode [ line "--"; block ( "/-[-!]?", "-/" ) ] )
    lang "Less" "" ".less"
        java
    lang "Lua" "" ".lua"
        ( sourceCode [ line "--"; block ( "--\\[\\[", "\\]\\]" ) ] )
    lang "Makefile" "make" "makefile"
        configFile
    lang "Markdown" "" ".md"
        Markdown.markdown
    lang "MATLAB" "" "" // MATLAB uses .m but that's already taken for Objective-C
        ( sourceCode [ line "%(?![%{}])"; block ( "%\{", "%\}" ) ] )
    lang "Objective-C" "" ".m|.mm"
        java
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
    lang "Protobuf" "proto|proto3" ".proto"
        ( sourceCode [ cLine ] )
    lang "Pug" "jade" ".jade|.pug"
        ( sourceCode [ cLine ] )
    lang "PureScript" "" ".purs"
        ( sourceCode
            // Treat blocks with and without leading pipes as separate blocks,
            // otherwise pipes will be added to those without, possibly adding
            // those lines to documentation where it wasn't intended.
            [ line "--\s*\|"
              line "--"
              block ( "{-\s*\|?", "-}" ) 
            ] 
        )    
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
        java
    lang "Shaderlab" "" ".shader"
        java
    lang "Shell script" "shellscript" ".sh"
        configFile
    lang "SQL" "" ".sql"
        ( sourceCode [ line "--"; cBlock ] )
    lang "Swift" "" ".swift"
        java
    lang "Tcl" "" ".tcl"
        configFile
    lang "TOML" "" ".toml"
        configFile
    lang "TypeScript" "typescriptreact" ".ts|.tsx"
        java
    lang "XML" "xsl" ".xml|.xsl"
        html
    lang "YAML" "" ".yaml|.yml"
        /// Also allow text paragraphs to be wrapped. Though wrapping the whole
        /// file at once will mess it up.
        (fun settings -> 
            let comments =
                line "#{1,3}" settings

            takeUntil comments (plainText settings) |> repeatToEnd
        )
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
    findLanguage language filePath
        |> Option.map (fun l -> l.parser)
        |> Option.defaultValue plainText
