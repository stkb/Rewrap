namespace Extensions

// Some extra functions

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

    
    // List.truncate has a bug in Fable.
    // https://github.com/fable-compiler/Fable/issues/1187
    let truncate<'T> : int -> list: List<'T> -> List<'T> =
        let rec loop output n input =
            match input with
                | [] -> 
                    output
                | x :: xs ->
                    if n > 0 then
                        loop (x :: output) (n - 1) xs 
                    else
                        output
        
        fun n -> loop [] n >> List.rev


    /// splitAt that doesn't throw an error if n is too great; it just returns
    /// (list, []).
    let safeSplitAt (n: int) (list: List<'T>): List<'T> * List<'T> =
        (truncate n list, safeSkip n list)


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
        if n > str.Length then "" else str.Substring(n)


    // Error-safe takes up to n chars from start of string
    let takeStart n (str: string) =
        if n > str.Length then str else str.Substring(0, n)


    let takeEnd n (str: string) =
        if n > str.Length then str else str.Substring(str.Length - n)


    let trim (str: string) =
        str.Trim()


    let trimStart (str: string) =
        str.TrimStart()