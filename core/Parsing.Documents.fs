module internal Parsing.Documents

open System
open Extensions
open Extensions.Option
open Rewrap
open Parsing.Core
open Parsing.DocComments
open Parsing.Language
open Parsing.SourceCode


let plainText settings =
    let paragraphs =
        splitIntoChunks (onIndent settings.tabWidth)
            >> Nonempty.map (indentSeparatedParagraphBlock Block.text)

    takeUntil blankLines paragraphs |> repeatToEnd

let private configFile =
    sourceCode [ line "#" ]

let private lang = Language.create

let mutable languages = [
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
    lang "HCL" "terraform" ".hcl|.tf"
        ( sourceCode
            [ customBlock DocComments.javadoc ( "\\*?", " * " ) javadocMarkers
              cBlock
              customLine DocComments.javadoc "//[/!]"
              cLine
              line "#"
            ]
        )
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
        ( sourceCode
            [ line "#"
              block ( "([Bb][Rr]?|[Ff][Rr]?|[Rr][BbFf]?|[Uu])?\"\"\"", "\"\"\"" )
              block ( "([Bb][Rr]?|[Ff][Rr]?|[Rr][BbFf]?|[Uu])?'''", "'''" )
            ]
        )
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
    lang "SystemVerilog" "" ".sv|.svh"
        java
    lang "Tcl" "" ".tcl"
        configFile
    lang "TOML" "" ".toml"
        configFile
    lang "TypeScript" "typescriptreact" ".ts|.tsx"
        java
    lang "Verilog" "" ".v|.vh"
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
    ]

/// Creates a custom language parser. Also adds it to the list of languages
let private addCustomLanguage name (markers: CustomMarkers) =
    let escape = System.Text.RegularExpressions.Regex.Escape
    let maybeLine = Option.map (line << escape) markers.line
    let maybeBlock = Option.map (block << Tuple.map escape) markers.block
    let list = List.choose id [ maybeLine; maybeBlock ]
    let cl = lang name "" "" (sourceCode list)
    languages <- cl :: languages
    cl

/// Tries to find a known Language for the given File, using its language ID. If
/// that is empty or the default ('plaintext'), it tries using the file's
/// name/extension instead.
/// <remarks>
/// Done in this way so that if the language of the file isn't found, we get a
/// chance to try the files getMarkers callback instead. Otherwise, it mught
/// just match the file to the wrong language based on the extension (eg
/// Prolog/Perl .pl)
/// </remarks>
let languageForFile (file: File) : Option<Language> =
    let l = file.language.ToLower()
    if not (String.IsNullOrWhiteSpace(l)) || l.Equals("plaintext") then
        Seq.tryFind (Language.matchesFileLanguage l) languages
    else Seq.tryFind (Language.matchesFilePath file.path) languages

/// <summary> Selects a parser for the given File. </summary>
/// <remarks>
/// Tries to find a known language (see `languageForFile`) and returns its
/// parser. If no language is found, a default plain text parser is used.
/// </remarks>
let rec select (file: File) : Settings -> TotalParser =
    languageForFile file
        |> Option.orElseWith
            (fun () ->
                match file.getMarkers.Invoke() with
                | null -> None
                | x -> Some (addCustomLanguage file.language x)
            )
        |> option plainText Language.parser
