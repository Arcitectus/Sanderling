module Sanderling.SanderlingMemoryReading exposing
    ( ChildOfNodeWithDisplayOffset(..)
    , MaybeVisible(..)
    , MemoryReadingUITreeNode
    , MemoryReadingUITreeNodeWithDisplayOffset
    , asUITreeNodeWithTotalDisplayOffset
    , countDescendantsInUITreeNode
    , decodeMemoryReadingFromString
    , getHorizontalOffsetFromParentAndWidth
    , getMostPopulousDescendantMatchingPredicate
    , getVerticalOffsetFromParent
    , listDescendantsInUITreeNode
    , listDescendantsWithDisplayOffsetInUITreeNode
    , memoryReadingUITreeNodeDecoder
    , unwrapMemoryReadingUITreeNodeChild
    )

import BigInt
import Json.Decode
import Json.Encode


type alias MemoryReadingUITreeNodeWithDisplayOffset =
    { rawNode : MemoryReadingUITreeNode
    , children : Maybe (List ChildOfNodeWithDisplayOffset)
    , totalDisplayOffset : { x : Int, y : Int }
    }


type ChildOfNodeWithDisplayOffset
    = ChildWithOffset MemoryReadingUITreeNodeWithDisplayOffset
    | ChildWithoutOffset MemoryReadingUITreeNode


type alias MemoryReadingUITreeNode =
    { originalJson : Json.Encode.Value
    , pythonObjectAddress : String
    , pythonObjectTypeName : String
    , dictEntriesOfInterest : List ( String, Json.Encode.Value )
    , children : Maybe (List MemoryReadingUITreeNodeChild)
    }


type MemoryReadingUITreeNodeChild
    = MemoryReadingUITreeNodeChild MemoryReadingUITreeNode


type MaybeVisible feature
    = CanNotSeeIt
    | CanSee feature


asUITreeNodeWithTotalDisplayOffset : { x : Int, y : Int } -> MemoryReadingUITreeNode -> MemoryReadingUITreeNodeWithDisplayOffset
asUITreeNodeWithTotalDisplayOffset totalDisplayOffset rawNode =
    { rawNode = rawNode
    , children = rawNode.children |> Maybe.map (List.map (unwrapMemoryReadingUITreeNodeChild >> asUITreeNodeWithInheritedOffset totalDisplayOffset))
    , totalDisplayOffset = totalDisplayOffset
    }


asUITreeNodeWithInheritedOffset : { x : Int, y : Int } -> MemoryReadingUITreeNode -> ChildOfNodeWithDisplayOffset
asUITreeNodeWithInheritedOffset inheritedOffset rawNode =
    case rawNode |> getDisplayOffsetXY of
        Nothing ->
            ChildWithoutOffset rawNode

        Just selfOffset ->
            ChildWithOffset
                (asUITreeNodeWithTotalDisplayOffset
                    { x = inheritedOffset.x + selfOffset.x, y = inheritedOffset.y + selfOffset.y }
                    rawNode
                )


getDisplayOffsetXY : MemoryReadingUITreeNode -> Maybe { x : Int, y : Int }
getDisplayOffsetXY uiElement =
    let
        fixedNumberFromJsonValue jsonValue =
            let
                asString =
                    Json.Encode.encode 0 jsonValue
            in
            -- Looks like the EVE Online client uses intobjects but sometimes stores other stuff in the upper bits, so remove upper bits.let
            if (asString |> String.length) < 10 then
                String.toInt asString |> Result.fromMaybe "Failed to parse as integer."

            else
                case asString |> BigInt.fromIntString of
                    Nothing ->
                        Err "Failed to parse as integer."

                    Just bigIntWithSign ->
                        let
                            hexString =
                                (bigIntWithSign |> BigInt.abs)
                                    |> BigInt.toHexString
                                    |> String.right 8
                                    |> String.toLower

                            hexStringWithSign =
                                if (hexString |> String.length) == 8 && (hexString |> String.left 1) == "f" then
                                    "-8" ++ (hexString |> String.right 7)

                                else
                                    hexString
                        in
                        case hexStringWithSign |> BigInt.fromHexString of
                            Nothing ->
                                Err "Failed BigInt.fromHexString"

                            Just truncated ->
                                truncated
                                    |> BigInt.toString
                                    |> String.toInt
                                    |> Result.fromMaybe "Failed to parse truncated as integer."

        fixedNumberFromPropertyName propertyName =
            uiElement.dictEntriesOfInterest
                |> List.filter (Tuple.first >> (==) propertyName)
                |> List.head
                |> Maybe.map Tuple.second
                |> Maybe.andThen (fixedNumberFromJsonValue >> Result.toMaybe)
    in
    case ( fixedNumberFromPropertyName "_displayX", fixedNumberFromPropertyName "_displayY" ) of
        ( Just displayX, Just displayY ) ->
            Just { x = displayX, y = displayY }

        _ ->
            Nothing


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
unwrapMemoryReadingUITreeNodeChild child =
    case child of
        MemoryReadingUITreeNodeChild node ->
            node


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


listDescendantsWithDisplayOffsetInUITreeNode : MemoryReadingUITreeNodeWithDisplayOffset -> List MemoryReadingUITreeNodeWithDisplayOffset
listDescendantsWithDisplayOffsetInUITreeNode parent =
    parent.children
        |> Maybe.withDefault []
        |> List.filterMap
            (\child ->
                case child of
                    ChildWithoutOffset _ ->
                        Nothing

                    ChildWithOffset childWithOffset ->
                        Just childWithOffset
            )
        |> List.concatMap (\child -> child :: listDescendantsWithDisplayOffsetInUITreeNode child)


decodeMemoryReadingFromString : String -> Result Json.Decode.Error MemoryReadingUITreeNode
decodeMemoryReadingFromString =
    Json.Decode.decodeString memoryReadingUITreeNodeDecoder


memoryReadingUITreeNodeDecoder : Json.Decode.Decoder MemoryReadingUITreeNode
memoryReadingUITreeNodeDecoder =
    Json.Decode.map5
        (\originalJson pythonObjectAddress pythonObjectTypeName dictEntriesOfInterest children ->
            { originalJson = originalJson
            , pythonObjectAddress = pythonObjectAddress
            , pythonObjectTypeName = pythonObjectTypeName
            , dictEntriesOfInterest = dictEntriesOfInterest
            , children = children |> Maybe.map (List.map MemoryReadingUITreeNodeChild)
            }
        )
        Json.Decode.value
        (Json.Decode.field "pythonObjectAddress" Json.Decode.value |> Json.Decode.map (Json.Encode.encode 0))
        (decodeOptionalField "pythonObjectTypeName" Json.Decode.string |> Json.Decode.map (Maybe.withDefault ""))
        (Json.Decode.field "dictEntriesOfInterest" (Json.Decode.keyValuePairs Json.Decode.value))
        (decodeOptionalOrNullField "children" (Json.Decode.list (Json.Decode.lazy (\_ -> memoryReadingUITreeNodeDecoder))))


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
