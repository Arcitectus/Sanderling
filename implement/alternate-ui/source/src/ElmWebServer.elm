module ElmWebServer exposing (..)

{-| The `ElmWebServer` module provides the types to build web server declarations.
The type declarations in this module mirror the interface of the Elm Time executable file and enable type-checking for compatibility.
-}


{-| Describes a web server program. A web server can handle HTTP requests and spawn volatile processes to integrate other software
components.
-}
type alias WebServerConfig state =
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
    | RequestToVolatileProcess (RequestToVolatileProcessStruct state)
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
    , bodyAsBase64 : Maybe String
    , headers : List HttpHeader
    }


type alias RespondToHttpRequestStruct =
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


type alias CreateVolatileProcessStruct state =
    { programCode : String
    , update : CreateVolatileProcessResult -> state -> ( state, Commands state )
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


type alias RequestToVolatileProcessResult =
    Result RequestToVolatileProcessError RequestToVolatileProcessComplete


type RequestToVolatileProcessError
    = ProcessNotFound


type alias RequestToVolatileProcessComplete =
    { exceptionToString : Maybe String
    , returnValueToString : Maybe String
    , durationInMilliseconds : Int
    }


type alias TerminateVolatileProcessStruct =
    { processId : String }
