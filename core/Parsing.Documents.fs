module internal Parsing.Documents

open System
open System.Text.RegularExpressions
open Prelude
open Block
open Rewrap
open Parsing.Core
open Parsing.DocComments
open Parsing.Language
open Parsing.SourceCode


let plainText settings =
    let paragraphs =
        splitIntoChunks (onIndent settings.tabWidth)
            >> Nonempty.concatMap (splitIntoChunks (afterRegex (Regex("  $"))))
            >> map (indentSeparatedParagraphBlock textBlock)

    takeUntil blankLines paragraphs |> repeatToEnd

let private configFile =
    sourceCode [ line "#" ]


// Takes 4 args to create a Language:
//  1. display name (used only in VS)
//  2. string of aliases (language IDs used by the client. Not needed if they
//     only differ from display name by casing)
//  3. string of file extensions (including `.`). Used to give support to files
//     that are not known by the client.
//  4. parser
//
// Aliases and extensions are separated by `|`.
// This has to be aliased pointfully because of a bug in Fable
let private lang name aliases exts parser =
    Language.create name aliases exts parser

let mutable languages = [
    lang "AsciiDoc" "" ".adoc|.asciidoc"
        plainText
    lang "AutoHotkey" "ahk" ".ahk"
        ( sourceCode [ line ";"; cBlock ] )
    lang "Basic" "vb" ".vb"
        ( sourceCode [ customLine xmldoc "'''"; line "'" ] )
    lang "Batch file" "bat" ".bat"
        ( sourceCode [ line "(?:rem|::)" ] )
    lang "Bikeshed" "" ".bs"
        Markdown.markdown
    lang "C/C++" "c|c++|cpp" ".c|.cpp|.h"
        ( sourceCode
            [ customBlock DocComments.javadoc ( "*", " * " ) javadocMarkers
              cBlock
              customLine xmldoc "///"
              customLine DocComments.javadoc "//!?"
              cLine
            ]
        )
    lang "C#" "csharp" ".cs"
        ( sourceCode
            [ customLine xmldoc "///"; cLine
              customBlock javadoc ( "*", " * " ) javadocMarkers; cBlock
            ]
        )
    lang "Clojure" "" ".clj|.cljs|.cljc|.cljx|.edn"
        ( sourceCode [ line ";+" ] )
    lang "CMake" "" "CMakeLists.txt"
        configFile
    lang "CoffeeScript" "" ".coffee"
        ( sourceCode
            [ customBlock javadoc ("*#", " * ") ( "###\\*", "###" )
              block ( "###", "###" )
              line "#"
            ]
        )
    lang "Common Lisp" "commonlisp|lisp" ".lisp"
        ( sourceCode
            [ line ";+"
              block ( @"#\|", @"\|#" )
            ]
        )
    lang "Configuration" "properties" ".conf|.gitconfig"
        configFile
    lang "Crystal" "" ".cr"
        ( sourceCode [ line "#" ] )
    lang "CSS" "postcss" ".css|.pcss|.postcss"
        css
    // Special rules for DDoc need adding
    lang "D" "" ".d"
        ( sourceCode
            [ customLine ddoc "///"
              cLine
              customBlock ddoc ( "*", " * " ) javadocMarkers
              customBlock ddoc ( "+", " + " ) ("/\+\+", "\+/")
              cBlock
              block ( "/\+", "\+/" )
            ]
        )
    lang "Dart" "" ".dart"
        ( sourceCode
            [ customLine dartdoc "///"
              cLine
              customBlock dartdoc ( "*", " * " ) javadocMarkers
              cBlock
            ]
        )
    lang "Dockerfile" "docker" "dockerfile"
        configFile
    lang "Elixir" "" ".ex|.exs"
        ( sourceCode [ line "#"; block ("@(module|type|)doc\s+\"\"\"", "\"\"\"") ] )
    lang "Elm" "" ".elm"
        ( sourceCode [ line "--"; block ( "{-\|?", "-}" ) ] )
    lang "Emacs Lisp" "elisp|emacslisp" ".el"
        ( sourceCode [ line ";+" ] )
    lang "F#" "fsharp" ".fs|.fsx"
        ( sourceCode
            [ customLine xmldoc "///"; cLine;
              block ( @"\(\*", @"\*\)" )
            ]
        )
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
        ( sourceCode
            [ line "#"
              block ( @".*?""""""", "\"\"\"" )
            ]
        )
    lang "Groovy" "" ".groovy"
        java
    lang "Handlebars" "" ".handlebars|.hbs"
        ( sourceCode
            [ block ("{{!--", "--}}")
              block ("{{!", "}}")
              block ("<!--", "-->")
            ]
        )
    lang "Haskell" "" ".hs"
        ( sourceCode [ line "--"; block ( "{-\s*\|?", "-}" ) ] )
    lang "HCL" "terraform" ".hcl|.tf"
        ( sourceCode
            [ customBlock DocComments.javadoc ( "*", " * " ) javadocMarkers
              cBlock
              customLine DocComments.javadoc "//[/!]"
              cLine
              line "#"
            ]
        )
    lang "HTML" "htmlx|vue|erb" ".htm|.html|.vue"
        html
    lang "INI" "" ".ini"
        ( sourceCode [ line "[#;]" ] )
    lang "J" "" ".ijs"
        ( sourceCode [ line @"NB\." ] )
    lang "Java" "" ".java"
        java
    lang "JavaScript" "javascriptreact|js" ".js|.jsx"
        java
    lang "Julia" "" ".jl"
        ( sourceCode
            [ line "#"
              block ( @".*?""""""", "\"\"\"" )
            ]
        )
    lang "JSON" "json5|jsonc" ".json|.json5|.jsonc"
        java
    lang "LaTeX" "tex" ".bbx|.cbx|.cls|.sty|.tex"
        Latex.latex
    lang "Lean" "" ".lean"
        ( sourceCode [ line "--"; block ( "/-[-!]?", "-/" ) ] )
    lang "Less" "" ".less"
        java
    lang "Lua" "" ".lua"
        ( sourceCode [ block ( @"--\[\[", @"\]\]" ); line "--" ] )
    lang "Makefile" "make" "makefile"
        configFile
    lang "Markdown" "" ".md"
        Markdown.markdown
    lang "MATLAB" "" "" // MATLAB uses .m but that's already taken for Objective-C
        ( sourceCode [ line "%(?![%{}])"; block ( "%\{", "%\}" ) ] )
    lang "Objective-C" "" ".m|.mm"
        java
    lang "Octave" "" ""
        ( sourceCode [ block ("#\{", "#\}"); block ("%\{", "%\}"); line "##?"; line "%[^!]" ])
    lang "Perl" "perl6" ".p6|.pl|.pl6|.pm|.pm6"
        // Putting Perl & Perl6 together. Perl6 also has a form of block comment
        // which still needs to be supported.
        // https://docs.perl6.org/language/syntax#Comments
        configFile
    lang "PHP" "" ".php"
        ( sourceCode
            [ customBlock javadoc ( "*", " * " ) javadocMarkers
              cBlock
              line @"(?://|#)"
            ]
        )
    lang "PowerShell" "" ".ps1|.psd1|.psm1"
        ( sourceCode [ customLine psdoc "#"; customBlock psdoc ( "", "" ) ( "<#", "#>" ) ] )
    lang "Prolog" "" ""
        ( sourceCode
            [ customBlock DocComments.javadoc ( "*", " * " ) javadocMarkers
              cBlock
              line "%[%!]?"
            ]
        )
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
              block ( @".*?""""""", "\"\"\"" )
              block ( @".*?'''", "'''" )
            ]
        )
    lang "R" "" ".r"
        ( sourceCode [ line "#'?" ] )
    lang "reStructuredText" "" ".rst|.rest"
        plainText
    lang "Ruby" "" ".rb"
        ( sourceCode [ line "#"; block ( "=begin", "=end" ) ] )
    lang "Rust" "" ".rs"
        ( sourceCode [ line @"\/\/(?:\/|\!)?" ] )
    lang "SCSS" "" ".scss"
    // Sass still needs to be supported.
    // -  http://sass-lang.com/documentation/file.INDENTED_SYNTAX.html
        java
    lang "Scala" "" ".scala"
        java
    lang "Scheme" "" ".scm|.ss|.sch|.rkt"
        ( sourceCode
            [ line ";+"
              block ( @"#\|", @"\|#" )
            ]
        )
    lang "Shaderlab" "" ".shader"
        java
    lang "Shell script" "shellscript" ".sh"
        configFile
    lang "SQL" "postgres" ".pgsql|.psql|.sql"
        ( sourceCode [ line "--"; cBlock ] )
    lang "Swift" "" ".swift"
        java
    lang "Tcl" "" ".tcl"
        configFile
    lang "TOML" "" ".toml"
        configFile
    lang "TypeScript" "typescriptreact" ".ts|.tsx"
        java
    lang "Verilog/SystemVerilog" "systemverilog|verilog" ".sv|.svh|.v|.vh|.vl"
        java
    lang "XAML" "" ".xaml"
        html
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

/// Creates a custom language parser, if the given CustomMarkers are valid. Also
/// adds it to the list of languages
let private maybeAddCustomLanguage name (markers: CustomMarkers) : Option<Language> =
    let escape = System.Text.RegularExpressions.Regex.Escape
    let isInvalid = String.IsNullOrEmpty
    let maybeLine = if isInvalid markers.line then None else Some (line (escape markers.line))
    let maybeBlock =
      if isInvalid (fst markers.block) || isInvalid (snd markers.block) then None
      else Some (block (map escape markers.block))
    let list = [maybeBlock; maybeLine] |> List.choose id
    if List.isEmpty list then None else
    let cl = lang name "" "" (sourceCode list)
    languages <- cl :: languages
    Some cl

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
    if not (String.IsNullOrWhiteSpace(l) || l.Equals("plaintext")) then
        Seq.tryFind (Language.matchesFileLanguage l) languages
    else Seq.tryFind (Language.matchesFilePath file.path) languages

/// <summary> Selects a parser for the given File. </summary>
/// <remarks>
/// Tries to find a known language (see `languageForFile`) and returns its
/// parser. If no language is found, a default plain text parser is used.
/// </remarks>
let rec select (file: File) : Settings -> TotalParser<string> =
    languageForFile file
        |> Option.orElseWith
            (fun () -> maybeAddCustomLanguage file.language (file.getMarkers.Invoke()))
        |> maybe plainText Language.parser
