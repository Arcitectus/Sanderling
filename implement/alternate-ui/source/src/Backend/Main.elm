module Backend.Main exposing
    ( State
    , interfaceToHost_initState
    , interfaceToHost_processEvent
    , processEvent
    )

import Backend.InterfaceToHost as InterfaceToHost
import Base64
import Bytes
import Bytes.Decode
import Bytes.Encode
import ElmFullstackCompilerInterface.ElmMake
import EveOnline.VolatileHostInterface
import EveOnline.VolatileHostScript as VolatileHostScript
import InterfaceToFrontendClient
import Json.Decode
import Json.Encode
import Result.Extra
import Url
import Url.Parser


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


type Route
    = ApiRoute
    | FrontendWithInspectorRoute


routeFromUrl : Url.Url -> Maybe Route
routeFromUrl =
    Url.Parser.parse
        (Url.Parser.oneOf
            [ Url.Parser.map ApiRoute (Url.Parser.s "api")
            , Url.Parser.map FrontendWithInspectorRoute (Url.Parser.s "with-inspector")
            ]
        )


initSetup : SetupState
initSetup =
    { volatileHostId = Nothing
    , lastRunScriptResult = Nothing
    , eveOnlineProcessesIds = Nothing
    }


interfaceToHost_processEvent : String -> State -> ( State, String )
interfaceToHost_processEvent =
    InterfaceToHost.wrapForSerialInterface_processEvent processEvent


processEvent : InterfaceToHost.AppEvent -> State -> ( State, InterfaceToHost.AppEventResponse )
processEvent hostEvent stateBefore =
    let
        ( state, responseExceptMaintainVolatileHost ) =
            stateBefore
                |> updateVolatileHostIdForHostEvent hostEvent
                |> processEventExceptVolatileHostMaintenance hostEvent

        maintainVolatileHostTasks =
            state
                |> maintainVolatileHostTaskFromState
                |> Maybe.map List.singleton
                |> Maybe.withDefault []
    in
    ( state
    , responseExceptMaintainVolatileHost |> InterfaceToHost.withStartTasksAdded maintainVolatileHostTasks
    )


updateVolatileHostIdForHostEvent : InterfaceToHost.AppEvent -> State -> State
updateVolatileHostIdForHostEvent hostEvent stateBefore =
    case hostEvent of
        InterfaceToHost.TaskCompleteEvent { taskResult } ->
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

        InterfaceToHost.HttpRequestEvent _ ->
            stateBefore

        InterfaceToHost.ArrivedAtTimeEvent _ ->
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


processEventExceptVolatileHostMaintenance : InterfaceToHost.AppEvent -> State -> ( State, InterfaceToHost.AppEventResponse )
processEventExceptVolatileHostMaintenance hostEvent stateBefore =
    case hostEvent of
        InterfaceToHost.HttpRequestEvent httpRequestEvent ->
            processEventHttpRequest httpRequestEvent stateBefore

        InterfaceToHost.TaskCompleteEvent taskComplete ->
            stateBefore |> processTaskCompleteEvent taskComplete

        InterfaceToHost.ArrivedAtTimeEvent _ ->
            ( stateBefore, InterfaceToHost.passiveAppEventResponse )


processEventHttpRequest : InterfaceToHost.HttpRequestEventStructure -> State -> ( State, InterfaceToHost.AppEventResponse )
processEventHttpRequest httpRequestEvent stateBefore =
    let
        respondWithFrontendHtmlDocument { enableInspector } =
            ( stateBefore
            , InterfaceToHost.passiveAppEventResponse
                |> InterfaceToHost.withCompleteHttpResponsesAdded
                    [ { httpRequestId = httpRequestEvent.httpRequestId
                      , response =
                            { statusCode = 200
                            , bodyAsBase64 =
                                Just
                                    (if enableInspector then
                                        ElmFullstackCompilerInterface.ElmMake.elm_make__debug__base64____src_FrontendWeb_Main_elm

                                     else
                                        ElmFullstackCompilerInterface.ElmMake.elm_make__base64____src_FrontendWeb_Main_elm
                                    )
                            , headersToAdd = []
                            }
                      }
                    ]
            )
    in
    case httpRequestEvent.request.uri |> Url.fromString |> Maybe.andThen routeFromUrl of
        Nothing ->
            respondWithFrontendHtmlDocument { enableInspector = False }

        Just FrontendWithInspectorRoute ->
            respondWithFrontendHtmlDocument { enableInspector = True }

        Just ApiRoute ->
            -- TODO: Consolidate the different branches to reduce duplication.
            case
                httpRequestEvent.request.bodyAsBase64
                    |> Maybe.map (Base64.toBytes >> Maybe.map (decodeBytesToString >> Maybe.withDefault "Failed to decode bytes to string") >> Maybe.withDefault "Failed to decode from base64")
                    |> Maybe.withDefault "Missing HTTP body"
                    |> Json.Decode.decodeString InterfaceToFrontendClient.jsonDecodeRequestFromClient
            of
                Err decodeError ->
                    let
                        httpResponse =
                            { httpRequestId = httpRequestEvent.httpRequestId
                            , response =
                                { statusCode = 400
                                , bodyAsBase64 =
                                    ("Failed to decode request: " ++ (decodeError |> Json.Decode.errorToString))
                                        |> encodeStringToBytes
                                        |> Base64.fromBytes
                                , headersToAdd = []
                                }
                            }
                    in
                    ( { stateBefore | posixTimeMilli = httpRequestEvent.posixTimeMilli }
                    , InterfaceToHost.passiveAppEventResponse
                        |> InterfaceToHost.withCompleteHttpResponsesAdded [ httpResponse ]
                    )

                Ok requestFromClient ->
                    case requestFromClient of
                        InterfaceToFrontendClient.ReadLogRequest ->
                            let
                                httpResponse =
                                    { httpRequestId = httpRequestEvent.httpRequestId
                                    , response =
                                        { statusCode = 200
                                        , bodyAsBase64 =
                                            -- TODO: Also transmit time of log entry.
                                            (stateBefore.log |> List.map .message |> String.join "\n")
                                                |> encodeStringToBytes
                                                |> Base64.fromBytes
                                        , headersToAdd = []
                                        }
                                    }
                            in
                            ( { stateBefore | posixTimeMilli = httpRequestEvent.posixTimeMilli }
                            , InterfaceToHost.passiveAppEventResponse
                                |> InterfaceToHost.withCompleteHttpResponsesAdded [ httpResponse ]
                            )

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
                                    , InterfaceToHost.passiveAppEventResponse
                                        |> InterfaceToHost.withStartTasksAdded [ setupScriptTask ]
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
                                                , bodyAsBase64 =
                                                    (InterfaceToFrontendClient.SetupNotCompleteResponse setupStatusDescription |> InterfaceToFrontendClient.jsonEncodeRunInVolatileHostResponseStructure |> Json.Encode.encode 0)
                                                        |> encodeStringToBytes
                                                        |> Base64.fromBytes
                                                , headersToAdd = []
                                                }
                                            }
                                    in
                                    ( { stateBefore | posixTimeMilli = httpRequestEvent.posixTimeMilli }
                                    , InterfaceToHost.passiveAppEventResponse
                                        |> InterfaceToHost.withCompleteHttpResponsesAdded [ httpResponse ]
                                    )


processTaskCompleteEvent : InterfaceToHost.TaskCompleteEventStructure -> State -> ( State, InterfaceToHost.AppEventResponse )
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
                        , bodyAsBase64 = httpResponseBody |> encodeStringToBytes |> Base64.fromBytes
                        , headersToAdd = []
                        }
                    }
            in
            ( { stateBefore | httpRequestsTasks = httpRequestsTasks }
            , InterfaceToHost.passiveAppEventResponse
                |> InterfaceToHost.withCompleteHttpResponsesAdded [ httpResponse ]
            )

        Nothing ->
            ( processFrameworkTaskCompleteEvent taskComplete stateBefore
            , InterfaceToHost.passiveAppEventResponse
            )


processFrameworkTaskCompleteEvent : InterfaceToHost.TaskCompleteEventStructure -> State -> State
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


decodeBytesToString : Bytes.Bytes -> Maybe String
decodeBytesToString bytes =
    bytes |> Bytes.Decode.decode (Bytes.Decode.string (bytes |> Bytes.width))


encodeStringToBytes : String -> Bytes.Bytes
encodeStringToBytes =
    Bytes.Encode.string >> Bytes.Encode.encode


interfaceToHost_initState : State
interfaceToHost_initState =
    { posixTimeMilli = 0
    , setup = initSetup
    , lastTaskIndex = 0
    , httpRequestsTasks = []
    , log = []
    }
