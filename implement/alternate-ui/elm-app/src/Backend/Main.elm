module Backend.Main exposing
    ( State
    , interfaceToHost_initState
    , interfaceToHost_processEvent
    , processEvent
    )

import Backend.InterfaceToHost as InterfaceToHost
import InterfaceToFrontendClient
import Json.Decode
import Json.Encode
import Result.Extra
import Sanderling.Sanderling
import Sanderling.SanderlingVolatileHostSetup as SanderlingVolatileHostSetup


type alias State =
    { posixTimeMilli : Int
    , setup : SetupState
    , lastTaskIndex : Int
    , httpRequestsTasks : List { httpRequestId : String, taskId : String }
    , log : List LogEntry
    }


type alias SetupState =
    { volatileHost : Maybe ( String, VolatileHostState )
    , lastRunScriptResult : Maybe (Result String (Maybe String))
    , eveOnlineProcessesIds : Maybe (List Int)
    }


type VolatileHostState
    = Initial
    | SanderlingSetupCompleted


type alias LogEntry =
    { posixTimeMilli : Int
    , message : String
    }


initSetup : SetupState
initSetup =
    { volatileHost = Nothing
    , lastRunScriptResult = Nothing
    , eveOnlineProcessesIds = Nothing
    }


interfaceToHost_processEvent : String -> State -> ( State, String )
interfaceToHost_processEvent =
    InterfaceToHost.wrapForSerialInterface_processEvent processEvent


processEvent : InterfaceToHost.ProcessEvent -> State -> ( State, List InterfaceToHost.ProcessRequest )
processEvent hostEvent stateBefore =
    let
        ( state, responses ) =
            stateBefore
                |> updateVolatileHostIdForHostEvent hostEvent
                |> processEventExceptVolatileHostMaintenance hostEvent

        maintainVolatileHostTasks =
            state
                |> maintainVolatileHostTaskFromState
                |> Maybe.map (InterfaceToHost.StartTask >> List.singleton)
                |> Maybe.withDefault []
    in
    ( state, responses ++ maintainVolatileHostTasks )


updateVolatileHostIdForHostEvent : InterfaceToHost.ProcessEvent -> State -> State
updateVolatileHostIdForHostEvent hostEvent stateBefore =
    case hostEvent of
        InterfaceToHost.TaskComplete { taskResult } ->
            case taskResult of
                InterfaceToHost.CreateVolatileHostResponse (Ok { hostId }) ->
                    { stateBefore | setup = { initSetup | volatileHost = Just ( hostId, Initial ) } }

                InterfaceToHost.CreateVolatileHostResponse (Err _) ->
                    stateBefore

                InterfaceToHost.RunInVolatileHostResponse (Err InterfaceToHost.HostNotFound) ->
                    { stateBefore | setup = initSetup }

                InterfaceToHost.RunInVolatileHostResponse (Ok _) ->
                    stateBefore

                InterfaceToHost.CompleteWithoutResult ->
                    stateBefore

        InterfaceToHost.HttpRequest _ ->
            stateBefore


maintainVolatileHostTaskFromState : State -> Maybe InterfaceToHost.StartTaskStructure
maintainVolatileHostTaskFromState state =
    if state.setup.volatileHost /= Nothing then
        -- TODO: Add cyclic check if volatile host still exists.
        Nothing

    else
        Just { taskId = "create-volatile-host", task = InterfaceToHost.CreateVolatileHost }


processEventExceptVolatileHostMaintenance : InterfaceToHost.ProcessEvent -> State -> ( State, List InterfaceToHost.ProcessRequest )
processEventExceptVolatileHostMaintenance hostEvent stateBefore =
    case hostEvent of
        InterfaceToHost.HttpRequest httpRequestEvent ->
            -- TODO: Consolidate the different branches to reduce duplication.
            case httpRequestEvent.request.bodyAsString |> Maybe.withDefault "" |> Json.Decode.decodeString InterfaceToFrontendClient.jsonDecodeRequestFromClient of
                Err decodeError ->
                    let
                        httpResponse =
                            { httpRequestId = httpRequestEvent.httpRequestId
                            , response =
                                { statusCode = 400
                                , bodyAsString =
                                    Just ("Failed to decode request: " ++ (decodeError |> Json.Decode.errorToString))
                                , headersToAdd = []
                                }
                            }
                                |> InterfaceToHost.CompleteHttpResponse
                    in
                    ( { stateBefore | posixTimeMilli = httpRequestEvent.posixTimeMilli }, [ httpResponse ] )

                Ok requestFromClient ->
                    case requestFromClient of
                        InterfaceToFrontendClient.ReadLogRequest ->
                            let
                                httpResponse =
                                    { httpRequestId = httpRequestEvent.httpRequestId
                                    , response =
                                        { statusCode = 200
                                        , bodyAsString =
                                            -- TODO: Also transmit time of log entry.
                                            Just (stateBefore.log |> List.map .message |> String.join "\n")
                                        , headersToAdd = []
                                        }
                                    }
                                        |> InterfaceToHost.CompleteHttpResponse
                            in
                            ( { stateBefore | posixTimeMilli = httpRequestEvent.posixTimeMilli }, [ httpResponse ] )

                        InterfaceToFrontendClient.RunInVolatileHostRequest runInVolatileHostRequest ->
                            case stateBefore.setup.volatileHost of
                                Just ( volatileHostId, SanderlingSetupCompleted ) ->
                                    let
                                        taskId =
                                            "task-for-client-" ++ ((stateBefore.lastTaskIndex + 1) |> String.fromInt)

                                        httpRequestsTasks =
                                            { httpRequestId = httpRequestEvent.httpRequestId
                                            , taskId = taskId
                                            }
                                                :: stateBefore.httpRequestsTasks

                                        setupScriptTask =
                                            { taskId = taskId
                                            , task =
                                                InterfaceToHost.RunInVolatileHost
                                                    { hostId = volatileHostId
                                                    , script = Sanderling.Sanderling.buildScriptToGetResponseFromVolatileHost runInVolatileHostRequest
                                                    }
                                            }
                                    in
                                    ( { stateBefore
                                        | posixTimeMilli = httpRequestEvent.posixTimeMilli
                                        , httpRequestsTasks = httpRequestsTasks
                                        , lastTaskIndex = stateBefore.lastTaskIndex + 1
                                      }
                                    , [ setupScriptTask |> InterfaceToHost.StartTask ]
                                    )

                                _ ->
                                    let
                                        httpResponse =
                                            { httpRequestId = httpRequestEvent.httpRequestId
                                            , response =
                                                { statusCode = 200
                                                , bodyAsString = Just (InterfaceToFrontendClient.SetupNotCompleteResponse |> InterfaceToFrontendClient.jsonEncodeRunInVolatileHostResponseStructure |> Json.Encode.encode 0)
                                                , headersToAdd = []
                                                }
                                            }
                                                |> InterfaceToHost.CompleteHttpResponse
                                    in
                                    ( { stateBefore | posixTimeMilli = httpRequestEvent.posixTimeMilli }, [ httpResponse ] )

        InterfaceToHost.TaskComplete taskComplete ->
            stateBefore |> processTaskCompleteEvent taskComplete


processTaskCompleteEvent : InterfaceToHost.TaskCompleteStructure -> State -> ( State, List InterfaceToHost.ProcessRequest )
processTaskCompleteEvent taskComplete stateBefore =
    let
        maybeHttpRequestId =
            stateBefore.httpRequestsTasks
                |> List.filter (.taskId >> (==) taskComplete.taskId)
                |> List.map .httpRequestId
                |> List.head
    in
    case maybeHttpRequestId of
        Just httpRequestId ->
            let
                httpRequestsTasks =
                    stateBefore.httpRequestsTasks
                        |> List.filter (.taskId >> (/=) taskComplete.taskId)

                sanderlingResponseResult =
                    case taskComplete.taskResult of
                        InterfaceToHost.RunInVolatileHostResponse runInVolatileHostResponse ->
                            runInVolatileHostResponse
                                |> Result.mapError (always "runInVolatileHostResponse is Error")

                        _ ->
                            Err "Unexpected response from Host."

                httpResponseBody =
                    sanderlingResponseResult
                        |> Result.map (InterfaceToFrontendClient.RunInVolatileHostCompleteResponse >> InterfaceToFrontendClient.jsonEncodeRunInVolatileHostResponseStructure >> Json.Encode.encode 0)
                        |> Result.Extra.merge

                httpResponse =
                    { httpRequestId = httpRequestId
                    , response =
                        { statusCode = 200
                        , bodyAsString = Just httpResponseBody
                        , headersToAdd = []
                        }
                    }
            in
            ( { stateBefore | httpRequestsTasks = httpRequestsTasks }
            , [ httpResponse |> InterfaceToHost.CompleteHttpResponse ]
            )

        Nothing ->
            processFrameworkTaskCompleteEvent taskComplete stateBefore


processFrameworkTaskCompleteEvent : InterfaceToHost.TaskCompleteStructure -> State -> ( State, List InterfaceToHost.ProcessRequest )
processFrameworkTaskCompleteEvent taskComplete stateBefore =
    case taskComplete.taskResult of
        InterfaceToHost.CompleteWithoutResult ->
            ( stateBefore |> addLogEntry "Completed task without result.", [] )

        InterfaceToHost.CreateVolatileHostResponse createVolatileHostResponse ->
            case createVolatileHostResponse of
                Err _ ->
                    ( stateBefore |> addLogEntry "Failed to create volatile host.", [] )

                Ok { hostId } ->
                    let
                        setupScriptTask =
                            { taskId = "create-volatile-host"
                            , task =
                                InterfaceToHost.RunInVolatileHost
                                    { hostId = hostId
                                    , script = SanderlingVolatileHostSetup.sanderlingSetupScript
                                    }
                            }
                    in
                    ( stateBefore
                        |> addLogEntry ("Created volatile host with id '" ++ hostId ++ "'.")
                    , [ setupScriptTask |> InterfaceToHost.StartTask ]
                    )

        InterfaceToHost.RunInVolatileHostResponse runInVolatileHostResponse ->
            case runInVolatileHostResponse of
                Err InterfaceToHost.HostNotFound ->
                    ( stateBefore |> addLogEntry "HostNotFound", [] )

                Ok runInVolatileHostComplete ->
                    case runInVolatileHostComplete.exceptionToString of
                        Just exceptionToString ->
                            ( stateBefore |> addLogEntry ("Run in volatile host failed with exception: " ++ exceptionToString), [] )

                        Nothing ->
                            if runInVolatileHostComplete.returnValueToString == Just "Sanderling Setup Completed" then
                                let
                                    setupBefore =
                                        stateBefore.setup

                                    volatileHost =
                                        setupBefore.volatileHost |> Maybe.map (Tuple.mapSecond (always SanderlingSetupCompleted))
                                in
                                ( { stateBefore | setup = { setupBefore | volatileHost = volatileHost } }
                                    |> addLogEntry "Completed volatile host setup."
                                , []
                                )

                            else
                                let
                                    returnValueAsHttpResponseResult =
                                        runInVolatileHostComplete.returnValueToString
                                            |> Maybe.withDefault ""
                                            |> Sanderling.Sanderling.deserializeResponseFromVolatileHost
                                in
                                case returnValueAsHttpResponseResult of
                                    Err decodeError ->
                                        ( stateBefore |> addLogEntry ("Failed to parse response from volatile host: " ++ (decodeError |> Json.Decode.errorToString)), [] )

                                    Ok _ ->
                                        ( stateBefore |> addLogEntry "Succeeded parsing task result.", [] )


addLogEntry : String -> State -> State
addLogEntry logMessage stateBefore =
    let
        log =
            { posixTimeMilli = stateBefore.posixTimeMilli, message = logMessage } :: stateBefore.log |> List.take 10
    in
    { stateBefore | log = log }


interfaceToHost_initState : State
interfaceToHost_initState =
    { posixTimeMilli = 0
    , setup = initSetup
    , lastTaskIndex = 0
    , httpRequestsTasks = []
    , log = []
    }
