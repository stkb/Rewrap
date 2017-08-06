namespace Extensions

// Some extra functions, and also implementations of F# 4.1 functions that are
// not yet supported in Fable

module internal Tuple =

    let mapFirst f (a, b) =
        (f a, b)

    let mapSecond f (a, b) =
        (a, f b)

    let replaceFirst x (a, b) =
        (x, b)

    let replaceSecond x (a, b) =
        (a, x)


module Option =

    // Not supported in Fable
    let defaultValue (def: 'T) (opt: Option<'T>): 'T =
        match opt with
            | Some x -> x
            | None -> def

    // Not supported in Fable
    let defaultWith (thunk: unit -> 'T) (opt: Option<'T>): 'T =
        match opt with
            | Some x -> x
            | None -> thunk ()

    // Not supported in Fable
    let orElse (ifNone: Option<'T>) (option: Option<'T>) =
        match option with
            | None -> ifNone
            | _ -> option

    // Not supported in Fable
    let orElseWith (thunk: unit -> Option<'T>) (opt: Option<'T>): Option<'T> =
        match opt with
            | None -> thunk ()
            | _ -> opt


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

    
    // Seems to have a bug in Fable
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


    // Not supported in Fable
    let splitAt (n: int) (list: List<'T>): List<'T> * List<'T> =
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