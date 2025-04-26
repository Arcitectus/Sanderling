module Platform.WebService exposing (..)

{-| This module contains the types describing the Pine / Elm web service platform.
To build a web service app in Elm, copy this module file into your project and add a declaration with the name `webServiceMain` to an Elm module.

For the latest version of the documentation, see <https://pine-vm.org>

-}

import Bytes


{-| Use the type `WebServiceConfig` on a declaration named `webServiceMain` to declare a web service program in an Elm module.
A web service can subscribe to incoming HTTP requests and respond to them. It can also start and manage volatile processes to integrate other software.
-}
type alias WebServiceConfig state =
    { init : ( state, Commands state )
    , subscriptions : state -> Subscriptions state
    }


type alias Subscriptions state =
    { httpRequest : HttpRequestEventStruct -> state -> ( state, Commands state )
    , posixTimeIsPast :
        Maybe
            { minimumPosixTimeMilli : Int
            , update : { currentPosixTimeMilli : Int } -> state -> ( state, Commands state )
            }
    }


type alias Commands state =
    List (Command state)


type Command state
    = RespondToHttpRequest RespondToHttpRequestStruct
    | CreateVolatileProcess (CreateVolatileProcessStruct state)
      {-
         We use the `runtimeIdentifier` and `osPlatform` properties to select the right executable files when creating a (native) volatile process.
         The properties returned by this command comes from the `RuntimeInformation` documented at <https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.runtimeinformation>
      -}
    | ReadRuntimeInformationCommand (ReadRuntimeInformationCommandStruct state)
    | CreateVolatileProcessNativeCommand (CreateVolatileProcessNativeCommandStruct state)
    | RequestToVolatileProcess (RequestToVolatileProcessStruct state)
    | WriteToVolatileProcessNativeStdInCommand (WriteToVolatileProcessNativeStdInStruct state)
    | ReadAllFromVolatileProcessNativeCommand (ReadAllFromVolatileProcessNativeStruct state)
    | TerminateVolatileProcess TerminateVolatileProcessStruct


type alias HttpRequestEventStruct =
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
    , body : Maybe Bytes.Bytes
    , headers : List HttpHeader
    }


type alias RespondToHttpRequestStruct =
    { httpRequestId : String
    , response : HttpResponse
    }


type alias HttpResponse =
    { statusCode : Int
    , body : Maybe Bytes.Bytes
    , headersToAdd : List HttpHeader
    }


type alias HttpHeader =
    { name : String
    , values : List String
    }


type alias CreateVolatileProcessStruct state =
    { programCode : String
    , update : CreateVolatileProcessResult -> state -> ( state, Commands state )
    }


type alias ReadRuntimeInformationCommandStruct state =
    Result String RuntimeInformationRecord -> state -> ( state, Commands state )


type alias RuntimeInformationRecord =
    { runtimeIdentifier : String
    , osPlatform : Maybe String
    }


type alias CreateVolatileProcessNativeCommandStruct state =
    { request : CreateVolatileProcessNativeRequestStruct
    , update : CreateVolatileProcessResult -> state -> ( state, Commands state )
    }


type alias CreateVolatileProcessNativeRequestStruct =
    { executableFile : LoadDependencyStruct
    , arguments : String
    , environmentVariables : List ProcessEnvironmentVariableStruct
    }


type alias CreateVolatileProcessResult =
    Result CreateVolatileProcessErrorStruct CreateVolatileProcessComplete


type alias CreateVolatileProcessErrorStruct =
    { exceptionToString : String
    }


type alias CreateVolatileProcessComplete =
    { processId : String }


type alias RequestToVolatileProcessStruct state =
    { processId : String
    , request : String
    , update : RequestToVolatileProcessResult -> state -> ( state, Commands state )
    }


type alias WriteToVolatileProcessNativeStdInStruct state =
    { processId : String
    , stdInBytes : Bytes.Bytes
    , update :
        Result RequestToVolatileProcessError ()
        -> state
        -> ( state, Commands state )
    }


type alias ReadAllFromVolatileProcessNativeStruct state =
    { processId : String
    , update :
        Result RequestToVolatileProcessError ReadAllFromVolatileProcessNativeSuccessStruct
        -> state
        -> ( state, Commands state )
    }


type alias RequestToVolatileProcessResult =
    Result RequestToVolatileProcessError RequestToVolatileProcessComplete


type alias ReadAllFromVolatileProcessNativeSuccessStruct =
    { stdOutBytes : Bytes.Bytes
    , stdErrBytes : Bytes.Bytes
    , exitCode : Maybe Int
    }


type RequestToVolatileProcessError
    = ProcessNotFound
    | RequestToVolatileProcessOtherError String


type alias RequestToVolatileProcessComplete =
    { exceptionToString : Maybe String
    , returnValueToString : Maybe String
    , durationInMilliseconds : Int
    }


type alias TerminateVolatileProcessStruct =
    { processId : String }


type alias ProcessEnvironmentVariableStruct =
    { key : String
    , value : String
    }


type alias LoadDependencyStruct =
    { hashSha256Base16 : String
    , hintUrls : List String
    }
