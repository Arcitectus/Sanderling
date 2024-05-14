module Frontend.Cli exposing (program)

import EveOnline.MemoryReading exposing (UITreeNodeChild)
import EveOnline.ParseUserInterface exposing (DisplayRegion, UITreeNodeWithDisplayRegion)
import Json.Decode
import Json.Encode as E
import Posix.IO as IO exposing (IO, Process)
import Posix.IO.File as File
import Posix.IO.Process as Proc
import Dict

program : Process -> IO ()
program process =
    case process.argv of
        [ _, filename ] ->
            IO.do
                (File.contentsOf filename
                    |> IO.exitOnError identity
                )
            <|
                \content ->
                    case parseMemoryReadingFromJson content of
                        Ok parsedResult ->
                            IO.do (Proc.print (E.encode 0 (encodeParsedResult parsedResult))) <|
                                \_ ->
                                    IO.return ()

                        Err err ->
                            IO.do (Proc.print ("Failed " ++ printError err)) <|
                                \_ ->
                                    IO.return ()

        _ ->
            Proc.logErr "Usage: elm-cli <program> file\n"


printError : Json.Decode.Error -> String
printError err =
    case err of
        Json.Decode.Field fieldName subErr ->
            "Error in field '" ++ fieldName ++ "': " ++ printError subErr

        Json.Decode.Index index subErr ->
            "Error at index " ++ String.fromInt index ++ ": " ++ printError subErr

        Json.Decode.OneOf subErrors ->
            let
                errorMsgs =
                    List.map printError subErrors
            in
            "One of the following errors occurred:\n" ++ String.concat errorMsgs

        Json.Decode.Failure message value ->
            "Failure: " ++ message ++ ", Value: " ++ E.encode 0 value


encodeParsedResult : ParseMemoryReadingSuccess -> E.Value
encodeParsedResult parsedResult =
    parsedResult
        |> List.map encodeNodeWithChildren
        |> E.list identity


encodeChildren : Maybe (List EveOnline.ParseUserInterface.ChildOfNodeWithDisplayRegion) -> E.Value
encodeChildren maybeChildren =
    case maybeChildren of
        Just children ->
            children
                |> List.map
                    (\child ->
                        case child of
                            EveOnline.ParseUserInterface.ChildWithRegion childNode ->
                                encodeNodeWithChildren childNode

                            EveOnline.ParseUserInterface.ChildWithoutRegion uiNode ->
                                encodeUiNodeWithChildren uiNode
                    )
                |> E.list identity

        Nothing ->
            E.null


encodeUiNodeWithChildren : EveOnline.MemoryReading.UITreeNode -> E.Value
encodeUiNodeWithChildren node =
    E.object
        [ ( "pythonObjectAddress", E.string node.pythonObjectAddress )
        , ( "pythonObjectTypeName", E.string node.pythonObjectTypeName )
        , ( "dictEntriesOfInterest", encodeDictEntriesOfInterest node.dictEntriesOfInterest )
        , ( "children", encodeUiChildren node.children )
        ]


encodeUiChildren : Maybe (List UITreeNodeChild) -> E.Value
encodeUiChildren maybeChildren =
    case maybeChildren of
        Just children ->
            children
                |> List.map
                    (\child ->
                        case child of
                            EveOnline.MemoryReading.UITreeNodeChild uiNode ->
                                encodeUiNodeWithChildren uiNode
                    )
                |> E.list identity

        Nothing ->
            E.null


encodeDictEntriesOfInterest : Dict.Dict String E.Value -> E.Value
encodeDictEntriesOfInterest _ =
    E.null


encodeTotalDisplayRegion : DisplayRegion -> E.Value
encodeTotalDisplayRegion _ =
    E.null


encodeTotalDisplayRegionVisible : DisplayRegion -> E.Value
encodeTotalDisplayRegionVisible _ =
    E.null


encodeSelfDisplayRegion : DisplayRegion -> E.Value
encodeSelfDisplayRegion _ =
    E.null


encodeNodeWithChildren : UITreeNodeWithDisplayRegion -> E.Value
encodeNodeWithChildren node =
    E.object
        [ ( "uiNode", encodeUiNodeWithChildren node.uiNode )
        , ( "totalDisplayRegion", encodeTotalDisplayRegion node.totalDisplayRegion )
        , ( "totalDisplayRegionVisible", encodeTotalDisplayRegionVisible node.totalDisplayRegionVisible )
        , ( "selfDisplayRegion", encodeSelfDisplayRegion node.selfDisplayRegion )
        , ( "children", encodeChildren node.children )
        ]


parseMemoryReadingFromJson : String -> Result Json.Decode.Error ParseMemoryReadingSuccess
parseMemoryReadingFromJson =
    EveOnline.MemoryReading.decodeMemoryReadingFromString
        >> Result.map
            (\uiTree ->
                let
                    uiTreeWithDisplayRegion =
                        uiTree |> EveOnline.ParseUserInterface.parseUITreeWithDisplayRegionFromUITree
                in
                uiTreeWithDisplayRegion
                    :: (uiTreeWithDisplayRegion |> EveOnline.ParseUserInterface.listDescendantsWithDisplayRegion)
            )


type alias ParseMemoryReadingSuccess =
    List UITreeNodeWithDisplayRegion
