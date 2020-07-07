module internal Nonempty

open Prelude

let toList : 'a Nonempty -> 'a List = fun (Nonempty (h, t)) -> h :: t

// ================ Creating ================ //

// Duplicate for now so I don't have to modify old code
let inline singleton x = x .@ []

let fromList : 'a List -> 'a Nonempty Option =
  function | [] -> None | x :: xs -> Some (x .@ xs)

let fromSeqUnsafe : 'a seq -> 'a Nonempty =
  fun xs -> Seq.head xs .@ List.ofSeq (Seq.tail xs)

let cons : 'a -> 'a Nonempty -> 'a Nonempty =
  fun h neList -> h .@ toList neList

/// Appends an element to the end of the Nonempty list
let snoc : 'a -> 'a Nonempty -> 'a Nonempty =
  fun last (Nonempty(h, t)) -> h .@ t @ [last]

let append : 'a Nonempty -> 'a Nonempty -> 'a Nonempty =
  fun (Nonempty(h, t)) b -> h .@ t @ toList b

let appendToList listA neListB =
    match fromList listA with
        | Some neListA -> append neListA neListB
        | None -> neListB

// ================ Getting elements or other ================ //

let head : 'a Nonempty -> 'a = fun (Nonempty (h, _)) -> h
let tail : 'a Nonempty -> 'a List = fun (Nonempty (_, t)) -> t
let last : 'a Nonempty -> 'a =
  fun (Nonempty (h, t)) -> fromMaybe h (List.tryLast t)

let tryFind : ('a -> bool) -> 'a Nonempty -> 'a Option =
  fun predicate -> toList >> List.tryFind predicate


// ================ Transforming ================ //

let rev : 'a Nonempty -> 'a Nonempty =
  fun list -> list |> toList |> List.rev |> fromSeqUnsafe

let mapHead : ('a -> 'a) -> 'a Nonempty -> 'a Nonempty =
  fun fn (Nonempty (h, t)) -> fn h .@ t

let mapTail : ('a -> 'a) -> 'a Nonempty -> 'a Nonempty =
  fun fn (Nonempty (h, t)) -> h .@ List.map fn t

let mapInit : ('a -> 'a) -> 'a Nonempty -> 'a Nonempty =
  fun fn -> rev >> mapTail fn >> rev

let mapLast : ('a -> 'a) -> 'a Nonempty -> 'a Nonempty =
  fun fn -> rev >> mapHead fn >> rev

let mapFold : ('s -> 'a -> 'b * 's) -> 's -> 'a Nonempty -> 'b Nonempty * 's =
  fun fn s (Nonempty (h,t)) ->
    let h', s' = fn s h in List.mapFold fn s' t |> Tuple.mapFirst ((.@) h')

let replaceHead : 'a -> 'a Nonempty -> 'a Nonempty =
  fun h -> mapHead (fun _ -> h)

let concatMap : ('a -> 'b Nonempty) -> 'a Nonempty -> 'b Nonempty =
  fun fn neList ->
  let rec loop output = function
    | [] -> output | x :: xs -> loop (append (fn x) output) xs
  rev neList |> (fun (Nonempty(head, tail)) -> loop (fn head) tail)

/// Splits the list at the given position. If n is less than 1 then n = 1
let splitAt : int -> 'a Nonempty -> ('a Nonempty * 'a Nonempty Option) =
  fun n (Nonempty(head, tail)) ->
  let rec loop count leftAcc maybeRightAcc =
    match maybeRightAcc with
      | None -> leftAcc, None
      | Some (Nonempty(x, xs)) ->
          if count < 1 then leftAcc, maybeRightAcc
          else loop (count - 1) (cons x leftAcc) (fromList xs)
  loop (n - 1) (singleton head) (fromList tail) |> Tuple.mapFirst rev

/// Takes a predicate and a list, and Optionally returns a Tuple, of the longest
/// prefix of the list for which the predicate holds, and the rest of the list.
/// If that prefix is empty, returns None.
let span : ('a -> bool) -> 'a Nonempty -> ('a Nonempty * 'a Nonempty Option) Option =
  fun predicate ->
  let rec loop output maybeRemaining =
    match maybeRemaining with
      | Some (Nonempty(h, t)) when predicate h -> loop (h :: output) (fromList t)
      | _ -> fromList (List.rev output) |> map (fun o -> o, maybeRemaining)
  Some >> loop []

/// Like span, but instead of a function that returns a bool, uses one that
/// returns an Option.
let spanMaybes : ('a -> 'b Option) -> 'a Nonempty -> ('b Nonempty * 'a Nonempty Option) Option =
  fun fn ->
  let rec loop output maybeRemaining =
    let inline finish () = fromList (List.rev output) |> map (fun o -> o, maybeRemaining)
    match maybeRemaining with
      | Some (Nonempty(h, t)) ->
          match fn h with Some x -> loop (x :: output) (fromList t) | None -> finish ()
      | _ -> finish ()
  Some >> loop []

/// Splits after the first element where the predicate evaluates true
let splitAfter : ('a -> bool) -> 'a Nonempty -> 'a Nonempty * 'a Nonempty Option =
  fun predicate ->
  let rec loop output (Nonempty(h, t)) =
    match fromList t with
      | Some nextList when not (predicate h) -> loop (h :: output) nextList
      | x -> h .@ output, x
  loop [] >> Tuple.mapFirst rev

let unfold : ('b -> 'a * 'b Option) -> 'b -> 'a Nonempty =
  fun fn ->
  let rec loop output input =
    match fn input with
      | (res, Some nextInput) -> loop (res :: output) nextInput
      | (res, None) ->  Nonempty(res, output)
  loop [] >> rev
