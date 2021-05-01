module ListDict exposing
    ( Dict
    , empty, singleton, insert, update, remove
    , isEmpty, member, get, size
    , keys, values, fromList
    , map, filter
    , union
    , toListWithInsertionOrder
    )

{-| A dictionary mapping unique keys to values.


# Dictionaries

@docs Dict


# Build

@docs empty, singleton, insert, update, remove


# Query

@docs isEmpty, member, get, size


# Lists

@docs keys, values, toList, fromList


# Transform

@docs map, foldl, foldr, filter, partition


# Combine

@docs union, intersect, diff, merge

-}


type Dict key value
    = Dict (List ( key, value ))


empty : Dict key value
empty =
    Dict []


singleton : key -> value -> Dict key value
singleton key value =
    empty |> insert key value


insert : key -> value -> Dict key value -> Dict key value
insert key value dictBefore =
    case dictBefore of
        Dict listBefore ->
            ((listBefore |> List.filter (Tuple.first >> (/=) key)) ++ [ ( key, value ) ]) |> Dict


update : key -> (Maybe value -> Maybe value) -> Dict key value -> Dict key value
update key valueMap originalDict =
    let
        insertion =
            case originalDict |> get key |> valueMap of
                Nothing ->
                    identity

                Just newValue ->
                    insert key newValue
    in
    originalDict
        |> remove key
        |> insertion


remove : key -> Dict key value -> Dict key value
remove key dictBefore =
    case dictBefore of
        Dict list ->
            list |> List.filter (Tuple.first >> (/=) key) |> Dict


isEmpty : Dict key value -> Bool
isEmpty =
    toListWithInsertionOrder >> List.isEmpty


member : key -> Dict key value -> Bool
member key dict =
    case dict of
        Dict list ->
            list |> List.any (\( k, _ ) -> k == key)


get : key -> Dict key value -> Maybe value
get key dict =
    case dict of
        Dict list ->
            list
                |> List.filter (Tuple.first >> (==) key)
                |> List.reverse
                |> List.head
                |> Maybe.map Tuple.second


size : Dict key value -> Int
size =
    toListWithInsertionOrder >> List.length


keys : Dict key value -> List key
keys =
    toListWithInsertionOrder >> List.map Tuple.first


values : Dict key value -> List value
values =
    toListWithInsertionOrder >> List.map Tuple.second


fromList : List ( key, value ) -> Dict key value
fromList =
    List.foldl (\( key, value ) dictBefore -> dictBefore |> insert key value) empty


toListWithInsertionOrder : Dict key value -> List ( key, value )
toListWithInsertionOrder dict =
    case dict of
        Dict list ->
            list


filter : (key -> value -> Bool) -> Dict key value -> Dict key value
filter predicate =
    toListWithInsertionOrder >> List.filter (\( k, v ) -> predicate k v) >> fromList


{-| Combine two dictionaries. For keys contained in both dictionaries, prefer the value from the second dictionary.
-}
union : Dict key value -> Dict key value -> Dict key value
union firstDict secondDict =
    [ firstDict, secondDict ] |> List.map toListWithInsertionOrder |> List.concat |> fromList


map : (key -> a -> b) -> Dict key a -> Dict key b
map entryMap =
    toListWithInsertionOrder >> List.map (\( k, v ) -> ( k, entryMap k v )) >> fromList
