module Backend.InterfaceToHost exposing
    ( HttpHeader
    , HttpRequestContext
    , HttpRequestEvent
    , HttpRequestProperties
    , HttpResponse
    , HttpResponseRequest
    , ProcessEvent(..)
    , ProcessRequest(..)
    , RunInVolatileHostError(..)
    , StartTaskStructure
    , Task(..)
    , TaskCompleteStructure
    , TaskResultStructure(..)
    , decodeOptionalField
    , wrapForSerialInterface_processEvent
    )

import Json.Decode
import Json.Encode


type ProcessEvent
    = HttpRequest HttpRequestEvent
    | TaskComplete TaskCompleteStructure


type ProcessRequest
    = CompleteHttpResponse HttpResponseRequest
    | StartTask StartTaskStructure


type alias TaskCompleteStructure =
    { taskId : TaskId
    , taskResult : TaskResultStructure
    }


type TaskResultStructure
    = CreateVolatileHostResponse (Result CreateVolatileHostError CreateVolatileHostComplete)
    | RunInVolatileHostResponse (Result RunInVolatileHostError RunInVolatileHostComplete)
    | CompleteWithoutResult


type alias RunInVolatileHostComplete =
    { exceptionToString : Maybe String
    , returnValueToString : Maybe String
    , durationInMilliseconds : Int
    }


type alias CreateVolatileHostError =
    ()


type alias CreateVolatileHostComplete =
    { hostId : String }


type RunInVolatileHostError
    = HostNotFound


type alias StartTaskStructure =
    { taskId : TaskId
    , task : Task
    }


type alias RunInVolatileHostStructure =
    { hostId : String
    , script : String
    }


type Task
    = CreateVolatileHost
    | RunInVolatileHost RunInVolatileHostStructure
    | ReleaseVolatileHost ReleaseVolatileHostStructure


type alias ReleaseVolatileHostStructure =
    { hostId : String }


type alias HttpRequestEvent =
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
    , bodyAsString : Maybe String
    , headers : List HttpHeader
    }


type ResponseOverSerialInterface
    = DecodeEventError String
    | DecodeEventSuccess (List ProcessRequest)


type alias HttpResponseRequest =
    { httpRequestId : String
    , response : HttpResponse
    }


type alias HttpResponse =
    { statusCode : Int
    , bodyAsString : Maybe String
    , headersToAdd : List HttpHeader
    }


type alias HttpHeader =
    { name : String
    , values : List String
    }


type alias TaskId =
    String


wrapForSerialInterface_processEvent : (ProcessEvent -> state -> ( state, List ProcessRequest )) -> String -> state -> ( state, String )
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


decodeProcessEvent : Json.Decode.Decoder ProcessEvent
decodeProcessEvent =
    Json.Decode.oneOf
        [ Json.Decode.field "httpRequest" decodeHttpRequestEvent |> Json.Decode.map HttpRequest
        , Json.Decode.field "taskComplete" decodeTaskComplete
            |> Json.Decode.map TaskComplete
        ]


decodeTaskComplete : Json.Decode.Decoder TaskCompleteStructure
decodeTaskComplete =
    Json.Decode.map2 TaskCompleteStructure
        (Json.Decode.field "taskId" Json.Decode.string)
        (Json.Decode.field "taskResult" decodeTaskResult)


decodeTaskResult : Json.Decode.Decoder TaskResultStructure
decodeTaskResult =
    Json.Decode.oneOf
        [ Json.Decode.field "createVolatileHostResponse" (decodeResult (Json.Decode.succeed ()) decodeCreateVolatileHostComplete)
            |> Json.Decode.map CreateVolatileHostResponse
        , Json.Decode.field "runInVolatileHostResponse" (decodeResult decodeRunInVolatileHostError decodeRunInVolatileHostComplete)
            |> Json.Decode.map RunInVolatileHostResponse
        , Json.Decode.field "completeWithoutResult" (Json.Decode.succeed CompleteWithoutResult)
        ]


decodeCreateVolatileHostComplete : Json.Decode.Decoder CreateVolatileHostComplete
decodeCreateVolatileHostComplete =
    Json.Decode.map CreateVolatileHostComplete
        (Json.Decode.field "hostId" Json.Decode.string)


decodeRunInVolatileHostComplete : Json.Decode.Decoder RunInVolatileHostComplete
decodeRunInVolatileHostComplete =
    Json.Decode.map3 RunInVolatileHostComplete
        (decodeOptionalField "exceptionToString" Json.Decode.string)
        (decodeOptionalField "returnValueToString" Json.Decode.string)
        (Json.Decode.field "durationInMilliseconds" Json.Decode.int)


decodeRunInVolatileHostError : Json.Decode.Decoder RunInVolatileHostError
decodeRunInVolatileHostError =
    Json.Decode.oneOf
        [ Json.Decode.field "hostNotFound" (Json.Decode.succeed HostNotFound)
        ]


decodeHttpRequestEvent : Json.Decode.Decoder HttpRequestEvent
decodeHttpRequestEvent =
    Json.Decode.map4 HttpRequestEvent
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
        (decodeOptionalField "bodyAsString" Json.Decode.string)
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
                Ok val ->
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
            [ ( "decodeEventError", error |> Json.Encode.string ) ]

        DecodeEventSuccess response ->
            [ ( "decodeEventSuccess", response |> Json.Encode.list encodeProcessRequest ) ]
    )
        |> Json.Encode.object


encodeProcessRequest : ProcessRequest -> Json.Encode.Value
encodeProcessRequest request =
    case request of
        CompleteHttpResponse httpResponse ->
            [ ( "completeHttpResponse", httpResponse |> encodeHttpResponseRequest )
            ]
                |> Json.Encode.object

        StartTask startTask ->
            Json.Encode.object [ ( "startTask", startTask |> encodeStartTask ) ]


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
        CreateVolatileHost ->
            Json.Encode.object [ ( "createVolatileHost", Json.Encode.object [] ) ]

        RunInVolatileHost runInVolatileHost ->
            Json.Encode.object
                [ ( "runInVolatileHost"
                  , Json.Encode.object
                        [ ( "hostId", runInVolatileHost.hostId |> Json.Encode.string )
                        , ( "script", runInVolatileHost.script |> Json.Encode.string )
                        ]
                  )
                ]

        ReleaseVolatileHost releaseVolatileHost ->
            Json.Encode.object
                [ ( "releaseVolatileHost"
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
    [ ( "statusCode", httpResponse.statusCode |> Json.Encode.int |> Just )
    , ( "headersToAdd", httpResponse.headersToAdd |> Json.Encode.list encodeHttpHeader |> Just )
    , ( "bodyAsString", httpResponse.bodyAsString |> Maybe.map Json.Encode.string )
    ]
        |> filterTakeOnlyWhereTupleSecondNotNothing
        |> Json.Encode.object


encodeHttpHeader : HttpHeader -> Json.Encode.Value
encodeHttpHeader httpHeader =
    [ ( "name", httpHeader.name |> Json.Encode.string )
    , ( "values", httpHeader.values |> Json.Encode.list Json.Encode.string )
    ]
        |> Json.Encode.object


filterTakeOnlyWhereTupleSecondNotNothing : List ( first, Maybe second ) -> List ( first, second )
filterTakeOnlyWhereTupleSecondNotNothing =
    List.filterMap
        (\( first, maybeSecond ) ->
            maybeSecond |> Maybe.map (\second -> ( first, second ))
        )


decodeResult : Json.Decode.Decoder error -> Json.Decode.Decoder ok -> Json.Decode.Decoder (Result error ok)
decodeResult errorDecoder okDecoder =
    Json.Decode.oneOf
        [ Json.Decode.field "err" errorDecoder |> Json.Decode.map Err
        , Json.Decode.field "ok" okDecoder |> Json.Decode.map Ok
        ]
