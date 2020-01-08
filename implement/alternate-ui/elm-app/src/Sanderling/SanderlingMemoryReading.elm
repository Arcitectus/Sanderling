module Sanderling.SanderlingMemoryReading exposing
    ( MaybeVisible(..)
    , MemoryReadingUITreeNode
    , MemoryReadingUITreeNodeChild
    , countDescendantsInUITreeNode
    , decodeMemoryReadingFromString
    , getHorizontalOffsetFromParentAndWidth
    , getMostPopulousDescendantMatchingPredicate
    , getVerticalOffsetFromParent
    , listDescendantsInUITreeNode
    , memoryReadingUITreeNodeDecoder
    , unwrapMemoryReadingUITreeNodeChild
    )

import Json.Decode
import Json.Encode


type alias MemoryReadingUITreeNode =
    { originalJson : Json.Encode.Value
    , pythonObjectAddress : String
    , pythonObjectTypeName : String
    , children : Maybe (List MemoryReadingUITreeNodeChild)
    , dictEntriesOfInterest : List ( String, Json.Encode.Value )
    }


type MemoryReadingUITreeNodeChild
    = MemoryReadingUITreeNodeChild
        { originalJson : Json.Encode.Value
        , pythonObjectAddress : String
        , pythonObjectTypeName : String
        , children : Maybe (List MemoryReadingUITreeNodeChild)
        , dictEntriesOfInterest : List ( String, Json.Encode.Value )
        }


type MaybeVisible feature
    = CanNotSeeIt
    | CanSee feature


getHorizontalOffsetFromParentAndWidth : MemoryReadingUITreeNode -> Maybe { offset : Int, width : Int }
getHorizontalOffsetFromParentAndWidth uiElement =
    let
        roundedNumberFromPropertyName propertyName =
            uiElement.dictEntriesOfInterest
                |> List.filter (Tuple.first >> (==) propertyName)
                |> List.head
                |> Maybe.andThen (Tuple.second >> Json.Decode.decodeValue Json.Decode.float >> Result.toMaybe)
                |> Maybe.map round
    in
    case ( roundedNumberFromPropertyName "_displayX", roundedNumberFromPropertyName "_width" ) of
        ( Just offset, Just width ) ->
            Just { offset = offset, width = width }

        _ ->
            Nothing


getVerticalOffsetFromParent : MemoryReadingUITreeNode -> Maybe Int
getVerticalOffsetFromParent =
    .dictEntriesOfInterest
        >> List.filter (Tuple.first >> (==) "_displayY")
        >> List.head
        >> Maybe.andThen (Tuple.second >> Json.Decode.decodeValue Json.Decode.float >> Result.toMaybe)
        >> Maybe.map round


getMostPopulousDescendantMatchingPredicate : (MemoryReadingUITreeNode -> Bool) -> MemoryReadingUITreeNode -> Maybe MemoryReadingUITreeNode
getMostPopulousDescendantMatchingPredicate predicate parent =
    listDescendantsInUITreeNode parent
        |> List.filter predicate
        |> List.sortBy countDescendantsInUITreeNode
        |> List.reverse
        |> List.head


unwrapMemoryReadingUITreeNodeChild : MemoryReadingUITreeNodeChild -> MemoryReadingUITreeNode
unwrapMemoryReadingUITreeNodeChild wrappedNode =
    case wrappedNode of
        MemoryReadingUITreeNodeChild unwrapped ->
            unwrapped


countDescendantsInUITreeNode : MemoryReadingUITreeNode -> Int
countDescendantsInUITreeNode parent =
    parent.children
        |> Maybe.withDefault []
        |> List.map unwrapMemoryReadingUITreeNodeChild
        |> List.map (countDescendantsInUITreeNode >> (+) 1)
        |> List.sum


listDescendantsInUITreeNode : MemoryReadingUITreeNode -> List MemoryReadingUITreeNode
listDescendantsInUITreeNode parent =
    parent.children
        |> Maybe.withDefault []
        |> List.map unwrapMemoryReadingUITreeNodeChild
        |> List.concatMap (\child -> child :: listDescendantsInUITreeNode child)


decodeMemoryReadingFromString : String -> Result Json.Decode.Error MemoryReadingUITreeNode
decodeMemoryReadingFromString =
    Json.Decode.decodeString memoryReadingUITreeNodeDecoder >> Result.map unwrapMemoryReadingUITreeNodeChild


memoryReadingUITreeNodeDecoder : Json.Decode.Decoder MemoryReadingUITreeNodeChild
memoryReadingUITreeNodeDecoder =
    Json.Decode.map5
        (\originalJson pythonObjectAddress pythonObjectTypeName children dictEntriesOfInterest ->
            { originalJson = originalJson, pythonObjectAddress = pythonObjectAddress, pythonObjectTypeName = pythonObjectTypeName, children = children, dictEntriesOfInterest = dictEntriesOfInterest } |> MemoryReadingUITreeNodeChild
        )
        Json.Decode.value
        (Json.Decode.field "pythonObjectAddress" Json.Decode.value |> Json.Decode.map (Json.Encode.encode 0))
        (decodeOptionalField "pythonObjectTypeName" Json.Decode.string |> Json.Decode.map (Maybe.withDefault ""))
        (decodeOptionalOrNullField "children" (Json.Decode.list (Json.Decode.lazy (\_ -> memoryReadingUITreeNodeDecoder))))
        (Json.Decode.field "dictEntriesOfInterest" (Json.Decode.keyValuePairs Json.Decode.value))


decodeOptionalOrNullField : String -> Json.Decode.Decoder a -> Json.Decode.Decoder (Maybe a)
decodeOptionalOrNullField fieldName decoder =
    decodeOptionalField fieldName (Json.Decode.nullable decoder)
        |> Json.Decode.map (Maybe.andThen identity)


decodeOptionalField : String -> Json.Decode.Decoder a -> Json.Decode.Decoder (Maybe a)
decodeOptionalField fieldName decoder =
    let
        finishDecoding json =
            case Json.Decode.decodeValue (Json.Decode.field fieldName Json.Decode.value) json of
                Ok val ->
                    -- The field is present, so run the decoder on it.
                    Json.Decode.map Just (Json.Decode.field fieldName decoder)

                Err _ ->
                    -- The field was missing, which is fine!
                    Json.Decode.succeed Nothing
    in
    Json.Decode.value
        |> Json.Decode.andThen finishDecoding
