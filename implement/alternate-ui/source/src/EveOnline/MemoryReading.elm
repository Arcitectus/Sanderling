module EveOnline.MemoryReading exposing (..)

import Dict
import Json.Decode
import Json.Encode


type alias UITreeNode =
    { originalJson : Json.Encode.Value
    , pythonObjectAddress : String
    , pythonObjectTypeName : String
    , dictEntriesOfInterest : Dict.Dict String Json.Encode.Value
    , children : Maybe (List UITreeNodeChild)
    }


type UITreeNodeChild
    = UITreeNodeChild UITreeNode


unwrapUITreeNodeChild : UITreeNodeChild -> UITreeNode
unwrapUITreeNodeChild child =
    case child of
        UITreeNodeChild node ->
            node


countDescendantsInUITreeNode : UITreeNode -> Int
countDescendantsInUITreeNode parent =
    parent.children
        |> Maybe.withDefault []
        |> List.map unwrapUITreeNodeChild
        |> List.map (countDescendantsInUITreeNode >> (+) 1)
        |> List.sum


listDescendantsInUITreeNode : UITreeNode -> List UITreeNode
listDescendantsInUITreeNode parent =
    parent.children
        |> Maybe.withDefault []
        |> List.map unwrapUITreeNodeChild
        |> List.concatMap (\child -> child :: listDescendantsInUITreeNode child)


decodeMemoryReadingFromString : String -> Result Json.Decode.Error UITreeNode
decodeMemoryReadingFromString =
    Json.Decode.decodeString uiTreeNodeDecoder


uiTreeNodeDecoder : Json.Decode.Decoder UITreeNode
uiTreeNodeDecoder =
    Json.Decode.map5
        (\originalJson pythonObjectAddress pythonObjectTypeName dictEntriesOfInterest children ->
            { originalJson = originalJson
            , pythonObjectAddress = pythonObjectAddress
            , pythonObjectTypeName = pythonObjectTypeName
            , dictEntriesOfInterest = dictEntriesOfInterest |> Dict.fromList
            , children = children |> Maybe.map (List.map UITreeNodeChild)
            }
        )
        Json.Decode.value
        (Json.Decode.field "pythonObjectAddress" Json.Decode.string)
        (decodeOptionalField "pythonObjectTypeName" Json.Decode.string |> Json.Decode.map (Maybe.withDefault ""))
        (Json.Decode.field "dictEntriesOfInterest" (Json.Decode.keyValuePairs Json.Decode.value))
        (decodeOptionalOrNullField "children" (Json.Decode.list (Json.Decode.lazy (\_ -> uiTreeNodeDecoder))))


decodeOptionalOrNullField : String -> Json.Decode.Decoder a -> Json.Decode.Decoder (Maybe a)
decodeOptionalOrNullField fieldName decoder =
    decodeOptionalField fieldName (Json.Decode.nullable decoder)
        |> Json.Decode.map (Maybe.andThen identity)


decodeOptionalField : String -> Json.Decode.Decoder a -> Json.Decode.Decoder (Maybe a)
decodeOptionalField fieldName decoder =
    let
        finishDecoding json =
            case Json.Decode.decodeValue (Json.Decode.field fieldName Json.Decode.value) json of
                Ok _ ->
                    -- The field is present, so run the decoder on it.
                    Json.Decode.map Just (Json.Decode.field fieldName decoder)

                Err _ ->
                    -- The field was missing, which is fine!
                    Json.Decode.succeed Nothing
    in
    Json.Decode.value
        |> Json.Decode.andThen finishDecoding
