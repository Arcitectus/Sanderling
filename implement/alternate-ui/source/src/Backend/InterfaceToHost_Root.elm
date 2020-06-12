module Backend.InterfaceToHost_Root exposing
    ( interfaceToHost_deserializeState
    , interfaceToHost_initState
    , interfaceToHost_processEvent
    , interfaceToHost_serializeState
    , main
    )

import Backend.Main
import Platform


interfaceToHost_initState =
    Backend.Main.interfaceToHost_initState


interfaceToHost_processEvent =
    Backend.Main.interfaceToHost_processEvent


interfaceToHost_serializeState : Backend.Main.State -> String
interfaceToHost_serializeState =
    always "not supported"


interfaceToHost_deserializeState : String -> Backend.Main.State
interfaceToHost_deserializeState =
    always interfaceToHost_initState



-- Support function-level dead code elimination (https://elm-lang.org/blog/small-assets-without-the-headache) Elm code needed to inform the Elm compiler about our entry points.


main : Program Int Backend.Main.State String
main =
    Platform.worker
        { init = \_ -> ( interfaceToHost_initState, Cmd.none )
        , update =
            \event stateBefore ->
                interfaceToHost_processEvent event (stateBefore |> interfaceToHost_serializeState |> interfaceToHost_deserializeState) |> Tuple.mapSecond (always Cmd.none)
        , subscriptions = \_ -> Sub.none
        }
