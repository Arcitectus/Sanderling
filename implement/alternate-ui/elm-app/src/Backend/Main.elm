module Backend.Main exposing
    ( State
    , interfaceToHost_initState
    , interfaceToHost_processEvent
    , processEvent
    )

import Backend.InterfaceToHost as InterfaceToHost
import EveOnline.VolatileHostInterface
import EveOnline.VolatileHostScript as VolatileHostScript
import InterfaceToFrontendClient
import Json.Decode
import Json.Encode
import Result.Extra


type alias State =
    { posixTimeMilli : Int
    , setup : SetupState
    , lastTaskIndex : Int
    , httpRequestsTasks : List { httpRequestId : String, taskId : String }
    , log : List LogEntry
    }


type alias SetupState =
    { volatileHostId : Maybe String
    , lastRunScriptResult : Maybe (Result String (Maybe String))
    , eveOnlineProcessesIds : Maybe (List Int)
    }


type alias LogEntry =
    { posixTimeMilli : Int
    , message : String
    }


initSetup : SetupState
initSetup =
    { volatileHostId = Nothing
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
                    { stateBefore | setup = { initSetup | volatileHostId = Just hostId } }

                InterfaceToHost.CreateVolatileHostResponse (Err _) ->
                    stateBefore

                InterfaceToHost.RequestToVolatileHostResponse (Err InterfaceToHost.HostNotFound) ->
                    { stateBefore | setup = initSetup }

                InterfaceToHost.RequestToVolatileHostResponse (Ok _) ->
                    stateBefore

                InterfaceToHost.CompleteWithoutResult ->
                    stateBefore

        InterfaceToHost.HttpRequest _ ->
            stateBefore


maintainVolatileHostTaskFromState : State -> Maybe InterfaceToHost.StartTaskStructure
maintainVolatileHostTaskFromState state =
    if state.setup.volatileHostId /= Nothing then
        -- TODO: Add cyclic check if volatile host still exists.
        Nothing

    else
        Just
            { taskId = "create-volatile-host"
            , task = InterfaceToHost.CreateVolatileHost { script = VolatileHostScript.setupScript }
            }


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
                            case stateBefore.setup.volatileHostId of
                                Just volatileHostId ->
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
                                                InterfaceToHost.RequestToVolatileHost
                                                    { hostId = volatileHostId
                                                    , request = EveOnline.VolatileHostInterface.buildRequestStringToGetResponseFromVolatileHost runInVolatileHostRequest
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
                                        setupStatusDescription =
                                            "Last run script result: "
                                                ++ (stateBefore.setup.lastRunScriptResult
                                                        |> Maybe.map
                                                            (\result ->
                                                                case result of
                                                                    Ok okResult ->
                                                                        "Ok: " ++ (okResult |> Maybe.withDefault "")

                                                                    Err errorResult ->
                                                                        "Err: " ++ errorResult
                                                            )
                                                        |> Maybe.withDefault "Nothing"
                                                   )

                                        httpResponse =
                                            { httpRequestId = httpRequestEvent.httpRequestId
                                            , response =
                                                { statusCode = 200
                                                , bodyAsString = Just (InterfaceToFrontendClient.SetupNotCompleteResponse setupStatusDescription |> InterfaceToFrontendClient.jsonEncodeRunInVolatileHostResponseStructure |> Json.Encode.encode 0)
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
                        InterfaceToHost.RequestToVolatileHostResponse runInVolatileHostResponse ->
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
            ( processFrameworkTaskCompleteEvent taskComplete stateBefore, [] )


processFrameworkTaskCompleteEvent : InterfaceToHost.TaskCompleteStructure -> State -> State
processFrameworkTaskCompleteEvent taskComplete stateBefore =
    case taskComplete.taskResult of
        InterfaceToHost.CompleteWithoutResult ->
            stateBefore |> addLogEntry "Completed task without result."

        InterfaceToHost.CreateVolatileHostResponse createVolatileHostResponse ->
            case createVolatileHostResponse of
                Err _ ->
                    stateBefore |> addLogEntry "Failed to create volatile host."

                Ok { hostId } ->
                    stateBefore |> addLogEntry ("Created volatile host with id '" ++ hostId ++ "'.")

        InterfaceToHost.RequestToVolatileHostResponse requestToVolatileHostResponse ->
            case requestToVolatileHostResponse of
                Err InterfaceToHost.HostNotFound ->
                    stateBefore |> addLogEntry "HostNotFound"

                Ok runInVolatileHostComplete ->
                    case runInVolatileHostComplete.exceptionToString of
                        Just exceptionToString ->
                            let
                                setupStateBefore =
                                    stateBefore.setup

                                setupState =
                                    { setupStateBefore
                                        | lastRunScriptResult = Just (Err ("Exception: " ++ exceptionToString))
                                    }
                            in
                            { stateBefore | setup = setupState }
                                |> addLogEntry ("Run in volatile host failed with exception: " ++ exceptionToString)

                        Nothing ->
                            let
                                returnValueAsHttpResponseResult =
                                    runInVolatileHostComplete.returnValueToString
                                        |> Maybe.withDefault ""
                                        |> EveOnline.VolatileHostInterface.deserializeResponseFromVolatileHost
                            in
                            case returnValueAsHttpResponseResult of
                                Err decodeError ->
                                    stateBefore |> addLogEntry ("Failed to parse response from volatile host: " ++ (decodeError |> Json.Decode.errorToString))

                                Ok _ ->
                                    stateBefore |> addLogEntry "Succeeded parsing task result."


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
