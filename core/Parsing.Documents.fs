module internal Parsing.Documents

open System
open Prelude
open Block
open Rewrap
open Parsing_
open Parsers
open Parsing.Core
open Parsing.DocComments
open Parsing.Language
open Parsing.SourceCode
open Parsing_SourceCode


let oldPlainText settings =
  let paragraphs =
      splitIntoChunks (onIndent settings.tabWidth)
          >> map (indentSeparatedParagraphBlock textBlock)

  takeUntil blankLines paragraphs |> repeatToEnd

let plainText : DocumentProcessor = docOf Parsers.plainText

// For creating source code doc types
let private sc comments = Parsing_SourceCode.sourceCode comments

// For creating line comments
let private line' m p = lineComment m p
let private line m = line' m markdown

// For creating block comments. In the start marker, anything in the 1st capture group
// will be replaced with whitespace if a 1-line comment is wrapped onto a 2nd line. In the
// end marker, $n can be used for the result of the nth captured group in the start marker
let block' innerMarkers outerMarkers = blockComment innerMarkers outerMarkers
let block (startMarker, endMarker) = block' ("", "") (startMarker, endMarker) markdown

// Common comment markers/types
let cBlock = block' ("*", "") (@"/\*", @"\*/") markdown
let javadocMarkers = (@"/\*[*!]", @"\*/")
let jsDocBlock = block' ("*", " * ") javadocMarkers jsdoc_markdown


// Common doc types
let private configFile = sc [line "#"]
let java : DocumentProcessor =
  sc [ jsDocBlock; cBlock; line' "//[/!]" jsdoc_markdown; line "//" ]

// Takes 4 args to create a Language:
//  1. display name (used only in VS)
//  2. string of aliases (language IDs used by the client. Not needed if they only differ
//     from display name by casing)
//  3. string of file extensions (including `.`). Used to give support to files that are
//     not known by the client.
//  4. parser
//
// Aliases and extensions are separated by `|`. This has to be aliased pointfully because
// of a bug in Fable
let private lang name aliases exts parser =
    Language.create name aliases exts parser

let mutable languages = [
    lang "AsciiDoc" "" ".adoc|.asciidoc"
        plainText
    lang "AutoHotkey" "ahk" ".ahk" <| sc [line ";"; cBlock]
    lang "Basic" "vb" ".vb"
        ( oldSourceCode [ customLine xmldoc "'''"; oldLine "'" ] )
    lang "Batch file" "bat" ".bat" <| sc [line "(?:@?rem|::)"]
    lang "Bikeshed" "" ".bs" <| docOf markdown
    lang "C/C++" "c|c++|cpp" ".c|.cpp|.h"
        ( oldSourceCode
            [ customBlock DocComments.javadoc ( "*", " * " ) javadocMarkers
              Parsing.SourceCode.cBlock
              customLine xmldoc "///"
              customLine DocComments.javadoc "//!?"
              cLine
            ]
        )
    lang "C#" "csharp" ".cs"
        ( oldSourceCode
            [ customLine xmldoc "///"; cLine
              customBlock javadoc ( "*", " * " ) javadocMarkers; Parsing.SourceCode.cBlock
            ]
        )
    lang "Clojure" "" ".clj|.cljs|.cljc|.cljx|.edn" <| sc [line ";+"]
    lang "CMake" "" "CMakeLists.txt" <| configFile
    lang "CoffeeScript" "" ".coffee" <| sc
      [ block' ("*#", " * ") ("###\\*", "###") jsdoc_markdown
        block ("###", "###"); line "#" ]
    lang "Common Lisp" "commonlisp|lisp" ".lisp" <| sc [line ";+"; block (@"#\|", @"\|#")]
    lang "Configuration" "properties" ".conf|.gitconfig|.pylintrc|pylintrc" <| configFile
    lang "Crystal" "" ".cr" <| configFile
    lang "CSS" "postcss" ".css|.pcss|.postcss"
        css
    // Special rules for DDoc need adding
    lang "D" "" ".d"
        ( oldSourceCode
            [ customLine ddoc "///"
              cLine
              customBlock ddoc ( "*", " * " ) javadocMarkers
              customBlock ddoc ( "+", " + " ) ("/\+\+", "\+/")
              Parsing.SourceCode.cBlock
              oldBlock ( "/\+", "\+/" )
            ]
        )
    lang "Dart" "" ".dart" <| sc
      [ line' "///" (dartdoc_markdown); line "//"
        block' ("*"," * ") javadocMarkers dartdoc_markdown; cBlock ]
    lang "Dockerfile" "docker" "dockerfile" <| configFile
    lang "Elixir" "" ".ex|.exs" <| sc [line "#"; block ("@(?:module|type|)doc\s+\"\"\"", "\"\"\"")]
    lang "Elm" "" ".elm" <| sc [line "--"; block ("{-\|?", "-}")]
    lang "Emacs Lisp" "elisp|emacslisp" ".el" <| sc [line ";+"]
    lang "F#" "fsharp" ".fs|.fsx"
        ( oldSourceCode
            [ customLine xmldoc "///"; cLine;
              oldBlock ( @"\(\*", @"\*\)" )
            ]
        )
    lang "FIDL" "" ".fidl" <| sc [line "///?"]
    lang "Go" "" ".go" <| sc [block' ("", "") (@"/\*", @"\*/") godoc; line' "//" godoc]
    lang "Git commit" "git-commit" "tag_editmsg" <| docOf markdown
    lang "GraphQL" "" ".graphql|.gql" <| sc [line "#"; block (@".*?""""""", "\"\"\"")]
    lang "Groovy" "" ".groovy" java
    lang "Handlebars" "" ".handlebars|.hbs" <| sc [block ("{{!--", "--}}"); block ("{{!", "}}"); block ("<!--", "-->")]
    lang "Haskell" "" ".hs" <| sc [line "--"; block ("{-\s*\|?", "-}")]
    lang "HCL" "terraform" ".hcl|.tf" <| sc [ jsDocBlock; cBlock; line' "//[/!]" jsdoc_markdown; line @"(?://|#)" ]
    lang "HTML" "erb|htmlx|svelte|vue" ".htm|.html|.svelte|.vue"
        html
    lang "INI" "" ".ini" <| sc [line "[#;]"]
    lang "J" "" ".ijs" <| sc [line @"NB\."]
    lang "Java" "" ".java" java
    lang "JavaScript" "javascriptreact|js" ".js|.jsx" java
    lang "Julia" "" ".jl" <| sc [block ("#=", "=#"); line "#"; block (@".*?""""""", "\"\"\"")]
    lang "JSON" "json5|jsonc" ".json|.json5|.jsonc" java
    lang "LaTeX" "tex" ".bbx|.cbx|.cls|.sty|.tex"
        <| toNewDocProcessor Latex.latex
    lang "Lean" "" ".lean" <| sc [line "--"; block ("/-[-!]?", "-/")]
    lang "Less" "" ".less" java
    lang "Lua" "" ".lua" <| sc [block (@"--\[(=*)\[", @"\]$1\]"); line "--"]
    lang "Makefile" "make" "makefile" <| configFile
    lang "Markdown" "mdx" ".md|.mdx|.rmd" <| docOf markdown
    // MATLAB uses .m but that's already taken for Objective-C
    lang "MATLAB" "" "" <| sc [line "%(?![%{}])"; block ("%\{", "%\}")]
    lang "Objective-C" "" ".m|.mm" java
    lang "Octave" "" "" <| sc [block ("#\{", "#\}"); block ("%\{", "%\}"); line "##?"; line "%[^!]"]
    lang "Pascal" "delphi" ".pas" <| sc [block (@"\(\*", @"\*\)"); block (@"\{(?!\$)", @"\}"); line "///?"]
    // Putting Perl & Perl6 together. Perl6 also has a form of block comment which still
    // needs to be supported. https://docs.perl6.org/language/syntax#Comments
    lang "Perl" "perl6" ".p6|.pl|.pl6|.pm|.pm6" <| configFile
    lang "PHP" "" ".php" <| sc [ jsDocBlock; cBlock; line "(?://|#)"]
    lang "PlainText-IndentSeparated" "" "" <| docOf plainText_indentSeparated
    lang "PowerShell" "" ".ps1|.psd1|.psm1"
        ( oldSourceCode [ customLine psdoc "#"; customBlock psdoc ( "", "" ) ( "<#", "#>" ) ] )
    lang "Prisma" "" ".prisma" <| sc [line "///?"]
    lang "Prolog" "" ""
        ( oldSourceCode
            [ customBlock DocComments.javadoc ( "*", " * " ) javadocMarkers
              Parsing.SourceCode.cBlock
              oldLine "%[%!]?"
            ]
        )
    lang "Protobuf" "proto|proto3" ".proto" <| sc [line "//"]
    lang "Pug" "jade" ".jade|.pug" <| sc [line "//"]
    // Treat blocks with and without leading pipes as separate blocks, otherwise pipes
    // will be added to those without, possibly adding those lines to documentation where
    // it wasn't intended.
    lang "PureScript" "" ".purs" <| sc [line "--\s*\|"; line "--"; block ("{-\s*\|?", "-}")]
    lang "Python" "" ".py" <| sc [line "#"; block' ("","") (@"(.*?)""""""", "\"\"\"") rst; block' ("","") (@"(.*?)'''", "'''") rst]
    lang "R" "" ".r" <| sc [line "#'?"]
    lang "reStructuredText" "rst" ".rst|.rest" <| docOf rst
    lang "Ruby" "" ".rb" <| sc [line "#"; block ("=begin", "=end")]
    lang "Rust" "" ".rs" <| sc [line @"//[/!]?"]
    lang "SCSS" "" ".scss" java
    // Sass still needs to be supported.
    // -  http://sass-lang.com/documentation/file.INDENTED_SYNTAX.html
    lang "Scala" "" ".scala" java
    lang "Scheme" "" ".scm|.ss|.sch|.rkt" <| sc [line ";+"; block (@"#\|", @"\|#")]
    lang "Shaderlab" "" ".shader" java
    lang "Shell script" "shellscript" ".sh" <| sc [line @"#(?!\!)"]
    lang "SQL" "postgres" ".pgsql|.psql|.sql" <| sc [line "--"; cBlock]
    lang "Swift" "" ".swift" java
    lang "Tcl" "" ".tcl" <| configFile
    lang "Textile" "" ".textile" <| docOf markdown
    lang "TOML" "" ".toml" <| configFile
    lang "TypeScript" "typescriptreact" ".ts|.tsx" java
    lang "Verilog/SystemVerilog" "systemverilog|verilog" ".sv|.svh|.v|.vh|.vl" java
    lang "XAML" "" ".xaml"
        html
    lang "XML" "xsl" ".xml|.xsl"
        html
    lang "YAML" "" ".yaml|.yml"
        // Also allow text paragraphs to be wrapped. Though wrapping the whole file at
        // once will mess it up.
        <| toNewDocProcessor (fun settings ->
            let comments = oldLine "#{1,3}" settings
            takeUntil comments (oldPlainText settings) |> repeatToEnd)
    ]

/// Creates a custom language parser, if the given CustomMarkers are valid. Also
/// adds it to the list of languages
let private maybeAddCustomLanguage name (markers: CustomMarkers) : Option<Language> =
    let escape = System.Text.RegularExpressions.Regex.Escape
    let isInvalid = String.IsNullOrEmpty
    let maybeLine = if isInvalid markers.line then None else Some (oldLine (escape markers.line))
    let maybeBlock =
      if isInvalid (fst markers.block) || isInvalid (snd markers.block) then None
      else Some (oldBlock (map escape markers.block))
    let list = [maybeBlock; maybeLine] |> List.choose id
    if List.isEmpty list then None else
    let cl = lang name "" "" (oldSourceCode list)
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
let rec select (file: File) : DocumentProcessor =
    languageForFile file
        |> Option.orElseWith
            (fun () -> maybeAddCustomLanguage file.language (file.getMarkers.Invoke()))
        |> maybe plainText Language.parser
