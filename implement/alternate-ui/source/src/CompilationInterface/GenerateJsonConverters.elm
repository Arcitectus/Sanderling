module CompilationInterface.GenerateJsonConverters exposing (..)

import InterfaceToFrontendClient
import Json.Decode
import Json.Encode


jsonEncodeRequestFromFrontendClient : InterfaceToFrontendClient.RequestFromClient -> Json.Encode.Value
jsonEncodeRequestFromFrontendClient =
    always (Json.Encode.string "The compiler replaces this function.")


jsonDecodeRequestFromFrontendClient : Json.Decode.Decoder InterfaceToFrontendClient.RequestFromClient
jsonDecodeRequestFromFrontendClient =
    Json.Decode.fail "The compiler replaces this function."


jsonEncodeRunInVolatileProcessResponseStructure : InterfaceToFrontendClient.RunInVolatileProcessResponseStructure -> Json.Encode.Value
jsonEncodeRunInVolatileProcessResponseStructure =
    always (Json.Encode.string "The compiler replaces this function.")


jsonDecodeRunInVolatileProcessResponseStructure : Json.Decode.Decoder InterfaceToFrontendClient.RunInVolatileProcessResponseStructure
jsonDecodeRunInVolatileProcessResponseStructure =
    Json.Decode.fail "The compiler replaces this function."
