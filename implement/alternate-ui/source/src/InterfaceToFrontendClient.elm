module InterfaceToFrontendClient exposing
    ( RequestFromClient(..)
    , RunInVolatileHostResponseStructure(..)
    , jsonDecodeRequestFromClient
    , jsonDecodeRunInVolatileHostResponseStructure
    , jsonEncodeRequestFromClient
    , jsonEncodeRunInVolatileHostResponseStructure
    )

import EveOnline.VolatileHostInterface
import Json.Decode
import Json.Encode
import Json.Encode.Extra


type RequestFromClient
    = ReadLogRequest
    | RunInVolatileHostRequest EveOnline.VolatileHostInterface.RequestToVolatileHost


type RunInVolatileHostResponseStructure
    = SetupNotCompleteResponse String
    | RunInVolatileHostCompleteResponse RunInVolatileHostComplete


type alias RunInVolatileHostComplete =
    { exceptionToString : Maybe String
    , returnValueToString : Maybe String
    , durationInMilliseconds : Int
    }


jsonEncodeRequestFromClient : RequestFromClient -> Json.Encode.Value
jsonEncodeRequestFromClient requestFromClient =
    case requestFromClient of
        ReadLogRequest ->
            [ ( "ReadLogRequest", [] |> Json.Encode.object ) ] |> Json.Encode.object

        RunInVolatileHostRequest runInVolatileHostRequest ->
            [ ( "RunInVolatileHostRequest", runInVolatileHostRequest |> EveOnline.VolatileHostInterface.encodeRequestToVolatileHost ) ] |> Json.Encode.object


jsonDecodeRequestFromClient : Json.Decode.Decoder RequestFromClient
jsonDecodeRequestFromClient =
    Json.Decode.oneOf
        [ Json.Decode.field "ReadLogRequest" (Json.Decode.succeed ReadLogRequest)
        , Json.Decode.field "RunInVolatileHostRequest" (EveOnline.VolatileHostInterface.decodeRequestToVolatileHost |> Json.Decode.map RunInVolatileHostRequest)
        ]


jsonEncodeRunInVolatileHostResponseStructure : RunInVolatileHostResponseStructure -> Json.Encode.Value
jsonEncodeRunInVolatileHostResponseStructure response =
    case response of
        SetupNotCompleteResponse setupNotCompleteResponse ->
            [ ( "SetupNotCompleteResponse", setupNotCompleteResponse |> Json.Encode.string ) ] |> Json.Encode.object

        RunInVolatileHostCompleteResponse runInVolatileHostCompleteResponse ->
            [ ( "RunInVolatileHostCompleteResponse", runInVolatileHostCompleteResponse |> encodeRunInVolatileHostComplete ) ] |> Json.Encode.object


jsonDecodeRunInVolatileHostResponseStructure : Json.Decode.Decoder RunInVolatileHostResponseStructure
jsonDecodeRunInVolatileHostResponseStructure =
    Json.Decode.oneOf
        [ Json.Decode.field "SetupNotCompleteResponse" (Json.Decode.string |> Json.Decode.map SetupNotCompleteResponse)
        , Json.Decode.field "RunInVolatileHostCompleteResponse" (decodeRunInVolatileHostComplete |> Json.Decode.map RunInVolatileHostCompleteResponse)
        ]


encodeRunInVolatileHostComplete : RunInVolatileHostComplete -> Json.Encode.Value
encodeRunInVolatileHostComplete runInVolatileHostComplete =
    [ ( "exceptionToString", runInVolatileHostComplete.exceptionToString |> Json.Encode.Extra.maybe Json.Encode.string )
    , ( "returnValueToString", runInVolatileHostComplete.returnValueToString |> Json.Encode.Extra.maybe Json.Encode.string )
    , ( "durationInMilliseconds", runInVolatileHostComplete.durationInMilliseconds |> Json.Encode.int )
    ]
        |> Json.Encode.object


decodeRunInVolatileHostComplete : Json.Decode.Decoder RunInVolatileHostComplete
decodeRunInVolatileHostComplete =
    Json.Decode.map3 RunInVolatileHostComplete
        (Json.Decode.field "exceptionToString" (Json.Decode.nullable Json.Decode.string))
        (Json.Decode.field "returnValueToString" (Json.Decode.nullable Json.Decode.string))
        (Json.Decode.field "durationInMilliseconds" Json.Decode.int)
