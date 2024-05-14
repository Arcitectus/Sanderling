module Frontend.Cli exposing (program)
import Posix.IO as IO exposing (IO, Process)
import Posix.IO.Process as Proc
import Dict
import EveOnline.MemoryReading
import EveOnline.ParseUserInterface exposing (UITreeNodeWithDisplayRegion)
import Frontend.InspectParsedUserInterface
    exposing
        ( InputOnUINode(..)
        , ParsedUITreeViewPathNode(..)
        , TreeViewNodeChildren(..)
        )
import Json.Decode


{- parseMemoryReadingFromJson String -> Result -}
program : Process -> IO ()
program process =
    case process.argv of
        [ _, filename ] ->
            case parseMemoryReadingFromJson filename of
                Ok parsedResult ->
                    IO.do (Proc.print (memoryReadingToString parsedResult)) <| \_ ->
                    IO.return ()

                Err _ ->
                    IO.do (Proc.print "Failed")  <| \_ ->
                    IO.return ()
        _ ->
            Proc.logErr ("Usage: elm-cli <program> file\n")

memoryReadingToString : ParseMemoryReadingSuccess -> String
memoryReadingToString memoryReading =
    memoryReading
        |> Dict.toList
        |> List.map (\( pythonObjectAddress, uiNodeWithRegion ) ->
            printNodeWithChildren pythonObjectAddress uiNodeWithRegion
        )
        |> String.join "\n"

printNodeWithChildren : String -> UITreeNodeWithDisplayRegion -> String
printNodeWithChildren pythonObjectAddress uiNodeWithRegion =
    let
        {- here is where we can add more information about the node -}
        nodeString =
            pythonObjectAddress ++ " " ++ uiNodeWithRegion.uiNode.pythonObjectTypeName
    in
    case uiNodeWithRegion.children of
        Just children ->
            [ nodeString ++ "\n\t" ] ++
                List.concatMap
                    (\child ->
                        case child of
                            EveOnline.ParseUserInterface.ChildWithRegion childNode ->
                                [ printNodeWithChildren pythonObjectAddress childNode ]

                            EveOnline.ParseUserInterface.ChildWithoutRegion _ ->
                                []
                    )
                    children
                |> String.join "\n"

        Nothing ->
            nodeString
        
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
                        |> List.map (\uiNodeWithRegion -> ( uiNodeWithRegion.uiNode.pythonObjectAddress, uiNodeWithRegion ))
                        |> Dict.fromList
            )

type alias ParseMemoryReadingSuccess = Dict.Dict String UITreeNodeWithDisplayRegion