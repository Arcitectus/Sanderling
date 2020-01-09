module InterfaceToFrontendClient exposing
    ( RequestFromClient(..)
    , RunInVolatileHostResponseStructure(..)
    , jsonDecodeRequestFromClient
    , jsonDecodeRunInVolatileHostResponseStructure
    , jsonEncodeRequestFromClient
    , jsonEncodeRunInVolatileHostResponseStructure
    )

import Json.Decode
import Json.Encode
import Json.Encode.Extra
import Sanderling.Sanderling


type RequestFromClient
    = ReadLogRequest
    | RunInVolatileHostRequest Sanderling.Sanderling.RequestToVolatileHost


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
            [ ( "RunInVolatileHostRequest", runInVolatileHostRequest |> Sanderling.Sanderling.encodeRequestToVolatileHost ) ] |> Json.Encode.object


jsonDecodeRequestFromClient : Json.Decode.Decoder RequestFromClient
jsonDecodeRequestFromClient =
    Json.Decode.oneOf
        [ Json.Decode.field "ReadLogRequest" (Json.Decode.succeed ReadLogRequest)
        , Json.Decode.field "RunInVolatileHostRequest" (Sanderling.Sanderling.decodeRequestToVolatileHost |> Json.Decode.map RunInVolatileHostRequest)
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


jsonEncodeResult : (err -> Json.Encode.Value) -> (ok -> Json.Encode.Value) -> Result err ok -> Json.Encode.Value
jsonEncodeResult encodeErr encodeOk result =
    case result of
        Err err ->
            [ ( "Err", err |> encodeErr ) ] |> Json.Encode.object

        Ok ok ->
            [ ( "Ok", ok |> encodeOk ) ] |> Json.Encode.object


jsonDecodeResult : Json.Decode.Decoder error -> Json.Decode.Decoder ok -> Json.Decode.Decoder (Result error ok)
jsonDecodeResult errorDecoder okDecoder =
    Json.Decode.oneOf
        [ Json.Decode.field "Err" errorDecoder |> Json.Decode.map Err
        , Json.Decode.field "Ok" okDecoder |> Json.Decode.map Ok
        ]


jsonEncodeMaybe : (just -> Json.Encode.Value) -> Maybe just -> Json.Encode.Value
jsonEncodeMaybe encodeJust =
    Maybe.map encodeJust >> Maybe.withDefault Json.Encode.null


jsonDecodeMaybe : Json.Decode.Decoder just -> Json.Decode.Decoder (Maybe just)
jsonDecodeMaybe =
    Json.Decode.nullable
