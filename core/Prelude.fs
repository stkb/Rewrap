module Prelude

// ================ Pseudo-typeclasses ================ //

type Functor = Functor with
  static member inline map (Functor, f: 'a -> 'b, x: Option<'a>) = Option.map f x
  static member inline map (Functor, f: 'a -> 'b, x: List<'a>) = List.map f x
  static member inline map (Functor, f: 'a -> 'b, (x: 'a, y: 'a)) = (f x, f y)
  static member inline map (Functor, f: 'a -> 'b, x: 'r -> 'a) = f << x

let inline map (f: ^a -> ^b) (x: ^x) =
  ((^x or ^Functor): (static member map: ^Functor * (^a -> ^b) * ^x -> ^r) (Functor, f, x))
let inline (<<|>) f x = map f x
// Would add <|>> but it caused an issue in Fable


// ================ Common types ================ //

/// Non-empty list
type Nonempty<'T> = Nonempty of 'T * List<'T> with
  // Overloading `@` seems not to be allowed
  static member (+) (Nonempty(aHead, aTail), Nonempty(bHead, bTail)) =
    Nonempty (aHead, aTail @ (bHead :: bTail))

  static member inline map (Functor, f: 'a -> 'b, Nonempty(h, t): Nonempty<'a>) =
    Nonempty(f h, map f t)

  interface seq<'T> with
    member self.GetEnumerator() =
      let (Nonempty (h, t)) = self
      (Seq.ofList <| (h :: t)).GetEnumerator()
  interface System.Collections.IEnumerable with
    member r.GetEnumerator () =
      (r :> seq<'T>).GetEnumerator() :> System.Collections.IEnumerator

let inline (.@) x xs = Nonempty (x, xs)
let inline singleton x = x .@ []

/// This or that or both
type These<'a, 'b> = This of 'a | That of 'b | These of 'a * 'b
  with
  static member maybeThis (maybeA: 'a Option) (b: 'b) : These<'a, 'b> =
    match maybeA with
      | Some a -> These(a, b)
      | None -> That b
  static member maybeThat a maybeB =
    match maybeB with
      | Some b -> These(a, b)
      | None -> This a
  static member mapThis<'c> (f: 'a -> 'c) (these: These<'a, 'b>) : These<'c, 'b> =
    match these with
      | This a -> This (f a)
      | That b -> That b
      | These (a, b) -> These (f a, b)


// ================ Other common functions ================ //

let inline always (b: 'a) (a: 'a) : 'a = b

// Maybe functions for Option
let inline fromMaybe (b: 'a) (x: Option<'a>) : 'a = Option.defaultValue b x
let inline (|?) (x: Option<'a>) (b: 'a) : 'a = Option.defaultValue b x
let inline maybe (b: 'b) (f: 'a -> 'b) (x: Option<'a>) : 'b = fromMaybe b (map f x)

// List extensions
module List =
  /// Returns the tail of the list, or None if the list is empty
  let tryTail : 'a List -> ('a List) Option =
    function | [] -> None | _ :: xs -> Some xs

  /// Returns all but the last element of the list, or None if the list is empty
  let tryInit : 'a List -> 'a List Option =
    fun list -> list |> List.rev |> tryTail |> map List.rev

  /// Skip that doesn't error if n is too great (though not stack-safe)
  let rec safeSkip : int -> 'a list -> 'a list =
    fun n list ->
    if n = 0 then list else tryTail list |> maybe [] (safeSkip (n - 1))

  /// Splits a list into two, as long as the predicate holds
  let span : ('a -> bool) -> 'a list -> 'a list * 'a list =
    fun predicate ->
    let rec loop output remaining =
      match remaining with
      | [] -> List.rev output, []
      | head :: rest ->
        if predicate head then loop (head :: output) rest
        else List.rev output, remaining
    loop []

  /// splitAt that doesn't throw an error if n is too great; it just returns
  /// (list, []).
  let safeSplitAt (n: int) (list: List<'T>): List<'T> * List<'T> =
    List.truncate n list, safeSkip n list

/// String extensions
module internal String =
  // Error-safe drops up to n chars from start of string
  let dropStart n (str: string) =
    if n > str.Length then "" else str.Substring(max n 0)

  // Error-safe takes up to n chars from start of string
  let takeStart n (str: string) =
    if n > str.Length then str else str.Substring(0, max n 0)

  let inline trim (str: string) = str.Trim()
  let inline trimStart (str: string) = str.TrimStart()
  let inline trimEnd (str: string) = str.TrimEnd()

// Tuple extensions
module internal Tuple =
  let inline mapFirst f (a, b) = (f a, b)
  let inline mapSecond f (a, b) = (a, f b)
  let inline replaceFirst x (_, b) = (x, b)
  let inline replaceSecond x (a, _) = (a, x)
let inline tuple a b = a, b
