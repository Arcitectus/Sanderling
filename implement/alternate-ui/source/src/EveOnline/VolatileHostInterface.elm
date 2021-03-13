module EveOnline.VolatileHostInterface exposing
    ( ConsoleBeepStructure
    , EffectOnWindowStructure(..)
    , EffectSequenceElement(..)
    , GameClientProcessSummaryStruct
    , GetMemoryReadingResultStructure(..)
    , Location2d
    , RequestToVolatileHost(..)
    , ResponseFromVolatileHost(..)
    , SearchUIRootAddressResultStructure
    , SearchUIRootAddressStructure
    , WindowId
    , buildRequestStringToGetResponseFromVolatileHost
    , decodeRequestToVolatileHost
    , deserializeResponseFromVolatileHost
    , encodeRequestToVolatileHost
    )

import Common.EffectOnWindow exposing (MouseButton(..), VirtualKeyCode(..), virtualKeyCodeAsInteger)
import Json.Decode
import Json.Decode.Extra
import Json.Encode
import Maybe.Extra


type RequestToVolatileHost
    = ListGameClientProcessesRequest
    | SearchUIRootAddress SearchUIRootAddressStructure
    | GetMemoryReading GetMemoryReadingStructure
    | EffectSequenceOnWindow (TaskOnWindowStructure (List EffectSequenceElement))
    | EffectConsoleBeepSequence (List ConsoleBeepStructure)


type ResponseFromVolatileHost
    = ListGameClientProcessesResponse (List GameClientProcessSummaryStruct)
    | SearchUIRootAddressResult SearchUIRootAddressResultStructure
    | GetMemoryReadingResult GetMemoryReadingResultStructure
    | FailedToBringWindowToFront String
    | CompletedEffectSequenceOnWindow


type alias GameClientProcessSummaryStruct =
    { processId : Int
    , mainWindowTitle : String
    , mainWindowZIndex : Int
    }


type alias GetMemoryReadingStructure =
    { processId : Int
    , uiRootAddress : String
    }


type alias SearchUIRootAddressStructure =
    { processId : Int
    }


type alias SearchUIRootAddressResultStructure =
    { processId : Int
    , uiRootAddress : Maybe String
    }


type GetMemoryReadingResultStructure
    = ProcessNotFound
    | Completed MemoryReadingCompletedStructure


type alias MemoryReadingCompletedStructure =
    { mainWindowId : WindowId
    , serialRepresentationJson : Maybe String
    }


type alias TaskOnWindowStructure task =
    { windowId : WindowId
    , bringWindowToForeground : Bool
    , task : task
    }


type EffectSequenceElement
    = Effect EffectOnWindowStructure
    | DelayMilliseconds Int


{-| Using names from Windows API and <https://www.nuget.org/packages/InputSimulator/>
-}
type
    EffectOnWindowStructure
    {-
       = MouseMoveTo MouseMoveToStructure
       | MouseButtonDown MouseButtonChangeStructure
       | MouseButtonUp MouseButtonChangeStructure
       | MouseHorizontalScroll Int
       | MouseVerticalScroll Int
       | KeyboardKeyDown VirtualKeyCode
       | KeyboardKeyUp VirtualKeyCode
       | TextEntry String
    -}
    = MouseMoveTo MouseMoveToStructure
    | KeyDown VirtualKeyCode
    | KeyUp VirtualKeyCode


type alias MouseMoveToStructure =
    { location : Location2d }


type alias WindowId =
    String


type alias Location2d =
    { x : Int, y : Int }


type alias ConsoleBeepStructure =
    { frequency : Int
    , durationInMs : Int
    }


deserializeResponseFromVolatileHost : String -> Result Json.Decode.Error ResponseFromVolatileHost
deserializeResponseFromVolatileHost =
    Json.Decode.decodeString decodeResponseFromVolatileHost


decodeResponseFromVolatileHost : Json.Decode.Decoder ResponseFromVolatileHost
decodeResponseFromVolatileHost =
    Json.Decode.oneOf
        [ Json.Decode.field "ListGameClientProcessesResponse" (Json.Decode.list jsonDecodeGameClientProcessSummary)
            |> Json.Decode.map ListGameClientProcessesResponse
        , Json.Decode.field "SearchUIRootAddressResult" decodeSearchUIRootAddressResult
            |> Json.Decode.map SearchUIRootAddressResult
        , Json.Decode.field "GetMemoryReadingResult" decodeGetMemoryReadingResult
            |> Json.Decode.map GetMemoryReadingResult
        , Json.Decode.field "FailedToBringWindowToFront" (Json.Decode.map FailedToBringWindowToFront Json.Decode.string)
        , Json.Decode.field "CompletedEffectSequenceOnWindow" (jsonDecodeSucceedWhenNotNull CompletedEffectSequenceOnWindow)
        ]


encodeRequestToVolatileHost : RequestToVolatileHost -> Json.Encode.Value
encodeRequestToVolatileHost request =
    case request of
        ListGameClientProcessesRequest ->
            Json.Encode.object [ ( "ListGameClientProcessesRequest", Json.Encode.object [] ) ]

        SearchUIRootAddress searchUIRootAddress ->
            Json.Encode.object [ ( "SearchUIRootAddress", searchUIRootAddress |> encodeSearchUIRootAddress ) ]

        GetMemoryReading getMemoryReading ->
            Json.Encode.object [ ( "GetMemoryReading", getMemoryReading |> encodeGetMemoryReading ) ]

        EffectSequenceOnWindow taskOnWindow ->
            Json.Encode.object
                [ ( "EffectSequenceOnWindow"
                  , taskOnWindow |> encodeTaskOnWindow (Json.Encode.list encodeEffectSequenceElement)
                  )
                ]

        EffectConsoleBeepSequence effectConsoleBeepSequence ->
            Json.Encode.object [ ( "EffectConsoleBeepSequence", effectConsoleBeepSequence |> Json.Encode.list encodeConsoleBeep ) ]


encodeEffectSequenceElement : EffectSequenceElement -> Json.Encode.Value
encodeEffectSequenceElement sequenceElement =
    case sequenceElement of
        Effect effect ->
            Json.Encode.object [ ( "Effect", encodeEffectOnWindowStructure effect ) ]

        DelayMilliseconds delayMilliseconds ->
            Json.Encode.object [ ( "DelayMilliseconds", Json.Encode.int delayMilliseconds ) ]


decodeEffectSequenceElement : Json.Decode.Decoder EffectSequenceElement
decodeEffectSequenceElement =
    Json.Decode.oneOf
        [ Json.Decode.field "Effect" (decodeEffectOnWindowStructure |> Json.Decode.map Effect)
        , Json.Decode.field "DelayMilliseconds" (Json.Decode.int |> Json.Decode.map DelayMilliseconds)
        ]


jsonDecodeGameClientProcessSummary : Json.Decode.Decoder GameClientProcessSummaryStruct
jsonDecodeGameClientProcessSummary =
    Json.Decode.map3 GameClientProcessSummaryStruct
        (Json.Decode.field "processId" Json.Decode.int)
        (Json.Decode.field "mainWindowTitle" Json.Decode.string)
        (Json.Decode.field "mainWindowZIndex" Json.Decode.int)


decodeRequestToVolatileHost : Json.Decode.Decoder RequestToVolatileHost
decodeRequestToVolatileHost =
    Json.Decode.oneOf
        [ Json.Decode.field "ListGameClientProcessesRequest" (jsonDecodeSucceedWhenNotNull ListGameClientProcessesRequest)
        , Json.Decode.field "SearchUIRootAddress" (decodeSearchUIRootAddress |> Json.Decode.map SearchUIRootAddress)
        , Json.Decode.field "GetMemoryReading" (decodeGetMemoryReading |> Json.Decode.map GetMemoryReading)
        , Json.Decode.field "EffectSequenceOnWindow" (decodeTaskOnWindow (Json.Decode.list decodeEffectSequenceElement) |> Json.Decode.map EffectSequenceOnWindow)
        , Json.Decode.field "EffectConsoleBeepSequence" (Json.Decode.list decodeConsoleBeep |> Json.Decode.map EffectConsoleBeepSequence)
        ]


encodeTaskOnWindow : (task -> Json.Encode.Value) -> TaskOnWindowStructure task -> Json.Encode.Value
encodeTaskOnWindow taskEncoder taskOnWindow =
    Json.Encode.object
        [ ( "windowId", taskOnWindow.windowId |> Json.Encode.string )
        , ( "bringWindowToForeground", taskOnWindow.bringWindowToForeground |> Json.Encode.bool )
        , ( "task", taskOnWindow.task |> taskEncoder )
        ]


decodeTaskOnWindow : Json.Decode.Decoder task -> Json.Decode.Decoder (TaskOnWindowStructure task)
decodeTaskOnWindow taskDecoder =
    Json.Decode.map3 (\windowId bringWindowToForeground task -> { windowId = windowId, bringWindowToForeground = bringWindowToForeground, task = task })
        (Json.Decode.field "windowId" Json.Decode.string)
        (Json.Decode.field "bringWindowToForeground" Json.Decode.bool)
        (Json.Decode.field "task" taskDecoder)


encodeEffectOnWindowStructure : EffectOnWindowStructure -> Json.Encode.Value
encodeEffectOnWindowStructure effectOnWindow =
    case effectOnWindow of
        MouseMoveTo mouseMoveTo ->
            Json.Encode.object
                [ ( "MouseMoveTo", mouseMoveTo |> encodeMouseMoveTo )
                ]

        KeyDown virtualKeyCode ->
            Json.Encode.object
                [ ( "KeyDown", virtualKeyCode |> encodeKey )
                ]

        KeyUp virtualKeyCode ->
            Json.Encode.object
                [ ( "KeyUp", virtualKeyCode |> encodeKey )
                ]


decodeEffectOnWindowStructure : Json.Decode.Decoder EffectOnWindowStructure
decodeEffectOnWindowStructure =
    Json.Decode.oneOf
        [ Json.Decode.field "MouseMoveTo" (decodeMouseMoveTo |> Json.Decode.map MouseMoveTo)
        , Json.Decode.field "KeyDown" (decodeKey |> Json.Decode.map KeyDown)
        , Json.Decode.field "KeyUp" (decodeKey |> Json.Decode.map KeyUp)
        ]


encodeKey : VirtualKeyCode -> Json.Encode.Value
encodeKey virtualKeyCode =
    Json.Encode.object [ ( "virtualKeyCode", virtualKeyCode |> virtualKeyCodeAsInteger |> Json.Encode.int ) ]


decodeKey : Json.Decode.Decoder VirtualKeyCode
decodeKey =
    Json.Decode.field "virtualKeyCode" Json.Decode.int |> Json.Decode.map VirtualKeyCodeFromInt


encodeMouseMoveTo : MouseMoveToStructure -> Json.Encode.Value
encodeMouseMoveTo mouseMoveTo =
    Json.Encode.object
        [ ( "location", mouseMoveTo.location |> encodeLocation2d )
        ]


decodeMouseMoveTo : Json.Decode.Decoder MouseMoveToStructure
decodeMouseMoveTo =
    Json.Decode.field "location" jsonDecodeLocation2d |> Json.Decode.map MouseMoveToStructure


encodeLocation2d : Location2d -> Json.Encode.Value
encodeLocation2d location =
    Json.Encode.object
        [ ( "x", location.x |> Json.Encode.int )
        , ( "y", location.y |> Json.Encode.int )
        ]


jsonDecodeLocation2d : Json.Decode.Decoder Location2d
jsonDecodeLocation2d =
    Json.Decode.map2 Location2d
        (Json.Decode.field "x" Json.Decode.int)
        (Json.Decode.field "y" Json.Decode.int)


encodeSearchUIRootAddress : SearchUIRootAddressStructure -> Json.Encode.Value
encodeSearchUIRootAddress getMemoryReading =
    Json.Encode.object
        [ ( "processId", getMemoryReading.processId |> Json.Encode.int )
        ]


decodeSearchUIRootAddress : Json.Decode.Decoder SearchUIRootAddressStructure
decodeSearchUIRootAddress =
    Json.Decode.map SearchUIRootAddressStructure
        (Json.Decode.field "processId" Json.Decode.int)


encodeGetMemoryReading : GetMemoryReadingStructure -> Json.Encode.Value
encodeGetMemoryReading getMemoryReading =
    Json.Encode.object
        [ ( "processId", getMemoryReading.processId |> Json.Encode.int )
        , ( "uiRootAddress", getMemoryReading.uiRootAddress |> Json.Encode.string )
        ]


decodeGetMemoryReading : Json.Decode.Decoder GetMemoryReadingStructure
decodeGetMemoryReading =
    Json.Decode.map2 GetMemoryReadingStructure
        (Json.Decode.field "processId" Json.Decode.int)
        (Json.Decode.field "uiRootAddress" Json.Decode.string)


decodeSearchUIRootAddressResult : Json.Decode.Decoder SearchUIRootAddressResultStructure
decodeSearchUIRootAddressResult =
    Json.Decode.map2 SearchUIRootAddressResultStructure
        (Json.Decode.field "processId" Json.Decode.int)
        (Json.Decode.Extra.optionalField "uiRootAddress" (Json.Decode.nullable Json.Decode.string) |> Json.Decode.map Maybe.Extra.join)


decodeGetMemoryReadingResult : Json.Decode.Decoder GetMemoryReadingResultStructure
decodeGetMemoryReadingResult =
    Json.Decode.oneOf
        [ Json.Decode.field "ProcessNotFound" (Json.Decode.succeed ProcessNotFound)
        , Json.Decode.field "Completed" decodeMemoryReadingCompleted |> Json.Decode.map Completed
        ]


decodeMemoryReadingCompleted : Json.Decode.Decoder MemoryReadingCompletedStructure
decodeMemoryReadingCompleted =
    Json.Decode.map2 MemoryReadingCompletedStructure
        (Json.Decode.field "mainWindowId" Json.Decode.string)
        (Json.Decode.Extra.optionalField "serialRepresentationJson" Json.Decode.string)


buildRequestStringToGetResponseFromVolatileHost : RequestToVolatileHost -> String
buildRequestStringToGetResponseFromVolatileHost =
    encodeRequestToVolatileHost
        >> Json.Encode.encode 0


encodeConsoleBeep : ConsoleBeepStructure -> Json.Encode.Value
encodeConsoleBeep consoleBeep =
    Json.Encode.object
        [ ( "frequency", consoleBeep.frequency |> Json.Encode.int )
        , ( "durationInMs", consoleBeep.durationInMs |> Json.Encode.int )
        ]


decodeConsoleBeep : Json.Decode.Decoder ConsoleBeepStructure
decodeConsoleBeep =
    Json.Decode.map2 ConsoleBeepStructure
        (Json.Decode.field "frequency" Json.Decode.int)
        (Json.Decode.field "durationInMs" Json.Decode.int)


jsonDecodeSucceedWhenNotNull : a -> Json.Decode.Decoder a
jsonDecodeSucceedWhenNotNull valueIfNotNull =
    Json.Decode.value
        |> Json.Decode.andThen
            (\asValue ->
                if asValue == Json.Encode.null then
                    Json.Decode.fail "Is null."

                else
                    Json.Decode.succeed valueIfNotNull
            )
