module Backend.InterfaceToHost exposing (..)

import Json.Decode
import Json.Encode


type AppEvent
    = HttpRequestEvent HttpRequestEventStructure
    | TaskCompleteEvent TaskCompleteEventStructure
    | ArrivedAtTimeEvent { posixTimeMilli : Int }


type alias AppEventResponse =
    { startTasks : List StartTaskStructure
    , notifyWhenArrivedAtTime : Maybe { posixTimeMilli : Int }
    , completeHttpResponses : List HttpResponseRequest
    }


type alias TaskCompleteEventStructure =
    { taskId : TaskId
    , taskResult : TaskResultStructure
    }


type TaskResultStructure
    = CreateVolatileHostResponse (Result CreateVolatileHostErrorStructure CreateVolatileHostComplete)
    | RequestToVolatileHostResponse (Result RequestToVolatileHostError RequestToVolatileHostComplete)
    | CompleteWithoutResult


type alias RequestToVolatileHostComplete =
    { exceptionToString : Maybe String
    , returnValueToString : Maybe String
    , durationInMilliseconds : Int
    }


type alias CreateVolatileHostErrorStructure =
    { exceptionToString : String
    }


type alias CreateVolatileHostComplete =
    { hostId : String }


type RequestToVolatileHostError
    = HostNotFound


type alias StartTaskStructure =
    { taskId : TaskId
    , task : Task
    }


type alias RequestToVolatileHostStructure =
    { hostId : String
    , request : String
    }


type Task
    = CreateVolatileHost CreateVolatileHostStructure
    | RequestToVolatileHost RequestToVolatileHostStructure
    | ReleaseVolatileHost ReleaseVolatileHostStructure


type alias CreateVolatileHostStructure =
    { script : String }


type alias ReleaseVolatileHostStructure =
    { hostId : String }


type alias HttpRequestEventStructure =
    { httpRequestId : String
    , posixTimeMilli : Int
    , requestContext : HttpRequestContext
    , request : HttpRequestProperties
    }


type alias HttpRequestContext =
    { clientAddress : Maybe String
    }


type alias HttpRequestProperties =
    { method : String
    , uri : String
    , bodyAsBase64 : Maybe String
    , headers : List HttpHeader
    }


type ResponseOverSerialInterface
    = DecodeEventError String
    | DecodeEventSuccess AppEventResponse


type alias HttpResponseRequest =
    { httpRequestId : String
    , response : HttpResponse
    }


type alias HttpResponse =
    { statusCode : Int
    , bodyAsBase64 : Maybe String
    , headersToAdd : List HttpHeader
    }


type alias HttpHeader =
    { name : String
    , values : List String
    }


type alias TaskId =
    String


passiveAppEventResponse : AppEventResponse
passiveAppEventResponse =
    { startTasks = []
    , completeHttpResponses = []
    , notifyWhenArrivedAtTime = Nothing
    }


withStartTasksAdded : List StartTaskStructure -> AppEventResponse -> AppEventResponse
withStartTasksAdded startTasksToAdd responseBefore =
    { responseBefore | startTasks = responseBefore.startTasks ++ startTasksToAdd }


withCompleteHttpResponsesAdded : List HttpResponseRequest -> AppEventResponse -> AppEventResponse
withCompleteHttpResponsesAdded httpResponsesToAdd responseBefore =
    { responseBefore | completeHttpResponses = responseBefore.completeHttpResponses ++ httpResponsesToAdd }


concatAppEventResponse : List AppEventResponse -> AppEventResponse
concatAppEventResponse responses =
    let
        notifyWhenArrivedAtTimePosixMilli =
            responses
                |> List.filterMap .notifyWhenArrivedAtTime
                |> List.map .posixTimeMilli
                |> List.minimum

        notifyWhenArrivedAtTime =
            case notifyWhenArrivedAtTimePosixMilli of
                Nothing ->
                    Nothing

                Just posixTimeMilli ->
                    Just { posixTimeMilli = posixTimeMilli }

        startTasks =
            responses |> List.concatMap .startTasks

        completeHttpResponses =
            responses |> List.concatMap .completeHttpResponses
    in
    { notifyWhenArrivedAtTime = notifyWhenArrivedAtTime
    , startTasks = startTasks
    , completeHttpResponses = completeHttpResponses
    }


wrapForSerialInterface_processEvent : (AppEvent -> state -> ( state, AppEventResponse )) -> String -> state -> ( state, String )
wrapForSerialInterface_processEvent update serializedEvent stateBefore =
    let
        ( state, response ) =
            case serializedEvent |> Json.Decode.decodeString decodeProcessEvent of
                Err error ->
                    ( stateBefore
                    , ("Failed to deserialize event: " ++ (error |> Json.Decode.errorToString))
                        |> DecodeEventError
                    )

                Ok hostEvent ->
                    stateBefore
                        |> update hostEvent
                        |> Tuple.mapSecond DecodeEventSuccess
    in
    ( state, response |> encodeResponseOverSerialInterface |> Json.Encode.encode 0 )


decodeProcessEvent : Json.Decode.Decoder AppEvent
decodeProcessEvent =
    Json.Decode.oneOf
        [ Json.Decode.field "ArrivedAtTimeEvent" (Json.Decode.field "posixTimeMilli" Json.Decode.int)
            |> Json.Decode.map (\posixTimeMilli -> ArrivedAtTimeEvent { posixTimeMilli = posixTimeMilli })
        , Json.Decode.field "TaskCompleteEvent" decodeTaskCompleteEventStructure |> Json.Decode.map TaskCompleteEvent
        , Json.Decode.field "HttpRequestEvent" decodeHttpRequestEventStructure |> Json.Decode.map HttpRequestEvent
        ]


decodeTaskCompleteEventStructure : Json.Decode.Decoder TaskCompleteEventStructure
decodeTaskCompleteEventStructure =
    Json.Decode.map2 TaskCompleteEventStructure
        (Json.Decode.field "taskId" Json.Decode.string)
        (Json.Decode.field "taskResult" decodeTaskResult)


decodeTaskResult : Json.Decode.Decoder TaskResultStructure
decodeTaskResult =
    Json.Decode.oneOf
        [ Json.Decode.field "CreateVolatileHostResponse" (decodeResult decodeCreateVolatileHostError decodeCreateVolatileHostComplete)
            |> Json.Decode.map CreateVolatileHostResponse
        , Json.Decode.field "RequestToVolatileHostResponse" (decodeResult decodeRequestToVolatileHostError decodeRequestToVolatileHostComplete)
            |> Json.Decode.map RequestToVolatileHostResponse
        , Json.Decode.field "CompleteWithoutResult" (Json.Decode.succeed CompleteWithoutResult)
        ]


decodeCreateVolatileHostError : Json.Decode.Decoder CreateVolatileHostErrorStructure
decodeCreateVolatileHostError =
    Json.Decode.map CreateVolatileHostErrorStructure
        (Json.Decode.field "exceptionToString" Json.Decode.string)


decodeCreateVolatileHostComplete : Json.Decode.Decoder CreateVolatileHostComplete
decodeCreateVolatileHostComplete =
    Json.Decode.map CreateVolatileHostComplete
        (Json.Decode.field "hostId" Json.Decode.string)


decodeRequestToVolatileHostComplete : Json.Decode.Decoder RequestToVolatileHostComplete
decodeRequestToVolatileHostComplete =
    Json.Decode.map3 RequestToVolatileHostComplete
        (decodeOptionalField "exceptionToString" Json.Decode.string)
        (decodeOptionalField "returnValueToString" Json.Decode.string)
        (Json.Decode.field "durationInMilliseconds" Json.Decode.int)


decodeRequestToVolatileHostError : Json.Decode.Decoder RequestToVolatileHostError
decodeRequestToVolatileHostError =
    Json.Decode.oneOf
        [ Json.Decode.field "HostNotFound" (Json.Decode.succeed HostNotFound)
        ]


decodeHttpRequestEventStructure : Json.Decode.Decoder HttpRequestEventStructure
decodeHttpRequestEventStructure =
    Json.Decode.map4 HttpRequestEventStructure
        (Json.Decode.field "httpRequestId" Json.Decode.string)
        (Json.Decode.field "posixTimeMilli" Json.Decode.int)
        (Json.Decode.field "requestContext" decodeHttpRequestContext)
        (Json.Decode.field "request" decodeHttpRequest)


decodeHttpRequestContext : Json.Decode.Decoder HttpRequestContext
decodeHttpRequestContext =
    Json.Decode.map HttpRequestContext
        (decodeOptionalField "clientAddress" Json.Decode.string)


decodeHttpRequest : Json.Decode.Decoder HttpRequestProperties
decodeHttpRequest =
    Json.Decode.map4 HttpRequestProperties
        (Json.Decode.field "method" Json.Decode.string)
        (Json.Decode.field "uri" Json.Decode.string)
        (decodeOptionalField "bodyAsBase64" Json.Decode.string)
        (Json.Decode.field "headers" (Json.Decode.list decodeHttpHeader))


decodeHttpHeader : Json.Decode.Decoder HttpHeader
decodeHttpHeader =
    Json.Decode.map2 HttpHeader
        (Json.Decode.field "name" Json.Decode.string)
        (Json.Decode.field "values" (Json.Decode.list Json.Decode.string))


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


encodeResponseOverSerialInterface : ResponseOverSerialInterface -> Json.Encode.Value
encodeResponseOverSerialInterface responseOverSerialInterface =
    (case responseOverSerialInterface of
        DecodeEventError error ->
            [ ( "DecodeEventError", error |> Json.Encode.string ) ]

        DecodeEventSuccess response ->
            [ ( "DecodeEventSuccess", response |> encodeAppEventResponse ) ]
    )
        |> Json.Encode.object


encodeAppEventResponse : AppEventResponse -> Json.Encode.Value
encodeAppEventResponse request =
    [ ( "notifyWhenArrivedAtTime"
      , request.notifyWhenArrivedAtTime
            |> Maybe.map (\time -> [ ( "posixTimeMilli", time.posixTimeMilli |> Json.Encode.int ) ] |> Json.Encode.object)
            |> Maybe.withDefault Json.Encode.null
      )
    , ( "startTasks", request.startTasks |> Json.Encode.list encodeStartTask )
    , ( "completeHttpResponses", request.completeHttpResponses |> Json.Encode.list encodeHttpResponseRequest )
    ]
        |> Json.Encode.object


encodeStartTask : StartTaskStructure -> Json.Encode.Value
encodeStartTask startTaskAfterTime =
    Json.Encode.object
        [ ( "taskId", startTaskAfterTime.taskId |> encodeTaskId )
        , ( "task", startTaskAfterTime.task |> encodeTask )
        ]


encodeTaskId : TaskId -> Json.Encode.Value
encodeTaskId =
    Json.Encode.string


encodeTask : Task -> Json.Encode.Value
encodeTask task =
    case task of
        CreateVolatileHost createVolatileHost ->
            Json.Encode.object
                [ ( "CreateVolatileHost", Json.Encode.object [ ( "script", createVolatileHost.script |> Json.Encode.string ) ] ) ]

        RequestToVolatileHost processRequestToVolatileHost ->
            Json.Encode.object
                [ ( "RequestToVolatileHost"
                  , Json.Encode.object
                        [ ( "hostId", processRequestToVolatileHost.hostId |> Json.Encode.string )
                        , ( "request", processRequestToVolatileHost.request |> Json.Encode.string )
                        ]
                  )
                ]

        ReleaseVolatileHost releaseVolatileHost ->
            Json.Encode.object
                [ ( "ReleaseVolatileHost"
                  , Json.Encode.object
                        [ ( "hostId", releaseVolatileHost.hostId |> Json.Encode.string )
                        ]
                  )
                ]


encodeHttpResponseRequest : HttpResponseRequest -> Json.Encode.Value
encodeHttpResponseRequest httpResponseRequest =
    Json.Encode.object
        [ ( "httpRequestId", httpResponseRequest.httpRequestId |> Json.Encode.string )
        , ( "response", httpResponseRequest.response |> encodeHttpResponse )
        ]


encodeHttpResponse : HttpResponse -> Json.Encode.Value
encodeHttpResponse httpResponse =
    [ ( "statusCode", httpResponse.statusCode |> Json.Encode.int )
    , ( "headersToAdd", httpResponse.headersToAdd |> Json.Encode.list encodeHttpHeader )
    , ( "bodyAsBase64", httpResponse.bodyAsBase64 |> Maybe.map Json.Encode.string |> Maybe.withDefault Json.Encode.null )
    ]
        |> Json.Encode.object


encodeHttpHeader : HttpHeader -> Json.Encode.Value
encodeHttpHeader httpHeader =
    [ ( "name", httpHeader.name |> Json.Encode.string )
    , ( "values", httpHeader.values |> Json.Encode.list Json.Encode.string )
    ]
        |> Json.Encode.object


decodeResult : Json.Decode.Decoder error -> Json.Decode.Decoder ok -> Json.Decode.Decoder (Result error ok)
decodeResult errorDecoder okDecoder =
    Json.Decode.oneOf
        [ Json.Decode.field "Err" errorDecoder |> Json.Decode.map Err
        , Json.Decode.field "Ok" okDecoder |> Json.Decode.map Ok
        ]
