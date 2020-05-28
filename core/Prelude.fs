module Prelude

type Nonempty<'T> = Nonempty of 'T * List<'T>
    with
    // Overloading `@` seems not to be allowed
    static member (+) (Nonempty(aHead, aTail), Nonempty(bHead, bTail)) =
        Nonempty (aHead, aTail @ (bHead :: bTail))


// Fabel compiler doesn't like the `type These ... module These` pattern, so we
// use static members instead
type These<'A, 'B> =
    | This of 'A
    | That of 'B
    | These of 'A * 'B
    with
    static member maybeThis (maybeA: Option<'A>) (b: 'B) : These<'A, 'B> =
        match maybeA with
            | Some a -> These(a, b)
            | None -> That b
    static member maybeThat a maybeB =
        match maybeB with
            | Some b -> These(a, b)
            | None -> This a
    static member mapThis<'C> (f: 'A -> 'C) (these: These<'A, 'B>) : These<'C, 'B> =
        match these with
            | This a -> This (f a)
            | That b -> That b
            | These (a, b) -> These (f a, b)


let maybe (def: 'B) (f: 'A -> 'B) (x: Option<'A>) : 'B =
    x |> Option.map f |> Option.defaultValue def


module internal Tuple =

    let map f (a, b) =
        (f a, f b)

    let mapFirst f (a, b) =
        (f a, b)

    let mapSecond f (a, b) =
        (a, f b)

    let replaceFirst x (a, b) =
        (x, b)

    let replaceSecond x (a, b) =
        (a, x)


module List =

    // Skip that doesn't error if n is too great (though not stack-safe)
    let rec safeSkip (n: int) (list: List<'T>) =
        if n > 0 then
            match list with
                | [] -> []
                | _ :: xs -> safeSkip (n - 1) xs
        else
            list


    let span predicate: List<'T> -> List<'T> * List<'T> =
        let rec loop output remaining =
            match remaining with
                | [] ->
                    (List.rev output, [])
                | head :: rest ->
                    if predicate head then
                        loop (head :: output) rest
                    else
                         (List.rev output, remaining)

        loop []


    /// splitAt that doesn't throw an error if n is too great; it just returns
    /// (list, []).
    let safeSplitAt (n: int) (list: List<'T>): List<'T> * List<'T> =
        (List.truncate n list, safeSkip n list)


    let tryTail (list: List<'T>) : Option<List<'T>> =
        match list with
            | _ :: xs ->
                Some xs
            | [] ->
                None


    let tryInit (list: List<'T>) : Option<List<'T>> =
       list |> List.rev |> tryTail |> Option.map List.rev


module internal String =

    // Error-safe drops up to n chars from start of string
    let dropStart n (str: string) =
        if n > str.Length then "" else str.Substring(max n 0)


    // Error-safe takes up to n chars from start of string
    let takeStart n (str: string) =
        if n > str.Length then str else str.Substring(0, max n 0)


    let trim (str: string) =
        str.Trim()


    let trimStart (str: string) =
        str.TrimStart()


    let trimEnd (str: string) =
        str.TrimEnd()
