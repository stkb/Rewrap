module internal rec Line

open System.Text.RegularExpressions
open Prelude


/// Returns true if the line is empty or just whitespace
let isBlank (l: string) =
    System.String.IsNullOrWhiteSpace(l)


/// Returns true if the line matches the given regex
let contains (regex: Regex) (line: string) =
    regex.IsMatch(line)


/// Returns true if the line starts with the given string, after optional whitespace
let startsWith marker line =
    Regex(@"^\s*" + marker).IsMatch(line)

/// Tries to match a regex against a line. If a match is found returns all
/// characters up to and including the end of the match.
let tryMatch (regex: Regex) (line: string): Option<string> =
    let m = regex.Match(line)
    if m.Success then
        Some (String.takeStart (m.Index + m.Length) line)
    else
        None


/// Gets the leading whitespace for the line
let leadingWhitespace line =
    leadingWhitespaceRegex.Match(line).Value


let containsText line =
    // Don't use \w because it contains underscore. Using almost the whole
    // Unicode range but it's probably good enough. Little hack for Ruby.
    contains (Regex("[A-Za-z0-9\u00C0-\uFFFF]")) line
        && not (contains (Regex(@"^=(begin|end)\s*$")) line)

/// Splits a line into prefix and remainder. If the prefix pattern is not found
/// the prefix is an empty string and the remainder is the whole line.
let split (regex: Regex) (line: string): string * string =
    let prefix =
        tryMatch regex line |> Option.defaultValue ""

    (prefix, String.dropStart prefix.Length line)


// Converts all tabs in the line to spaces
let tabsToSpaces (tabSize: int) (str: string) =
    match str.Split([|'\t'|]) |> List.ofArray |> List.rev with

        // This case isn't possible since String.split never returns an empty array
        | [] ->
            str

        // We add padding to all but the last string
        | x :: xs ->
            xs
                |> List.map
                    (fun s -> s.PadRight((s.Length / tabSize + 1) * tabSize))
                |> (fun tail -> List.Cons (x, tail))
                |> List.rev
                |> String.concat ""


let private leadingWhitespaceRegex: Regex =
    Regex(@"^\s*")
