module EveOnline.VolatileHostInterface exposing
    ( ConsoleBeepStructure
    , EffectOnWindowStructure(..)
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
    , effectMouseClickAtLocation
    , effectsForDragAndDrop
    , encodeRequestToVolatileHost
    )

import Common.EffectOnWindow exposing (MouseButton(..), VirtualKeyCode(..), virtualKeyCodeAsInteger, virtualKeyCodeFromMouseButton)
import Json.Decode
import Json.Decode.Extra
import Json.Encode


type RequestToVolatileHost
    = ListGameClientProcessesRequest
    | SearchUIRootAddress SearchUIRootAddressStructure
    | GetMemoryReading GetMemoryReadingStructure
    | EffectOnWindow (TaskOnWindowStructure EffectOnWindowStructure)
    | EffectConsoleBeepSequence (List ConsoleBeepStructure)


type ResponseFromVolatileHost
    = ListGameClientProcessesResponse (List GameClientProcessSummaryStruct)
    | SearchUIRootAddressResult SearchUIRootAddressResultStructure
    | GetMemoryReadingResult GetMemoryReadingResultStructure


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
    | SimpleMouseClickAtLocation MouseClickAtLocation
    | KeyDown VirtualKeyCode
    | KeyUp VirtualKeyCode


type alias MouseMoveToStructure =
    { location : Location2d }


type alias WindowId =
    String


type alias MouseClickAtLocation =
    { location : Location2d
    , mouseButton : MouseButton
    }


type alias Location2d =
    { x : Int, y : Int }


type alias ConsoleBeepStructure =
    { frequency : Int
    , durationInMs : Int
    }


effectsForDragAndDrop : { startLocation : Location2d, mouseButton : MouseButton, endLocation : Location2d } -> List EffectOnWindowStructure
effectsForDragAndDrop { startLocation, mouseButton, endLocation } =
    [ MouseMoveTo { location = startLocation }
    , KeyDown (virtualKeyCodeFromMouseButton mouseButton)
    , MouseMoveTo { location = endLocation }
    , KeyUp (virtualKeyCodeFromMouseButton mouseButton)
    ]


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

        EffectOnWindow taskOnWindow ->
            Json.Encode.object [ ( "EffectOnWindow", taskOnWindow |> encodeTaskOnWindow encodeEffectOnWindowStructure ) ]

        EffectConsoleBeepSequence effectConsoleBeepSequence ->
            Json.Encode.object [ ( "EffectConsoleBeepSequence", effectConsoleBeepSequence |> Json.Encode.list encodeConsoleBeep ) ]


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
        , Json.Decode.field "EffectOnWindow" (decodeTaskOnWindow decodeEffectOnWindowStructure |> Json.Decode.map EffectOnWindow)
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

        SimpleMouseClickAtLocation mouseClickAtLocation ->
            Json.Encode.object
                [ ( "simpleMouseClickAtLocation", mouseClickAtLocation |> encodeMouseClickAtLocation )
                ]

        KeyDown virtualKeyCode ->
            Json.Encode.object
                [ ( "keyDown", virtualKeyCode |> encodeKey )
                ]

        KeyUp virtualKeyCode ->
            Json.Encode.object
                [ ( "keyUp", virtualKeyCode |> encodeKey )
                ]


decodeEffectOnWindowStructure : Json.Decode.Decoder EffectOnWindowStructure
decodeEffectOnWindowStructure =
    Json.Decode.oneOf
        [ Json.Decode.field "simpleMouseClickAtLocation" (decodeMouseClickAtLocation |> Json.Decode.map SimpleMouseClickAtLocation)
        ]


encodeKey : VirtualKeyCode -> Json.Encode.Value
encodeKey virtualKeyCode =
    Json.Encode.object [ ( "virtualKeyCode", virtualKeyCode |> virtualKeyCodeAsInteger |> Json.Encode.int ) ]


encodeMouseMoveTo : MouseMoveToStructure -> Json.Encode.Value
encodeMouseMoveTo mouseMoveTo =
    Json.Encode.object
        [ ( "location", mouseMoveTo.location |> encodeLocation2d )
        ]


encodeMouseClickAtLocation : MouseClickAtLocation -> Json.Encode.Value
encodeMouseClickAtLocation mouseClickAtLocation_ =
    Json.Encode.object
        [ ( "location", mouseClickAtLocation_.location |> encodeLocation2d )
        , ( "mouseButton", mouseClickAtLocation_.mouseButton |> encodeMouseButton )
        ]


decodeMouseClickAtLocation : Json.Decode.Decoder MouseClickAtLocation
decodeMouseClickAtLocation =
    Json.Decode.map2 MouseClickAtLocation
        (Json.Decode.field "location" jsonDecodeLocation2d)
        (Json.Decode.field "mouseButton" jsonDecodeMouseButton)


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


encodeMouseButton : MouseButton -> Json.Encode.Value
encodeMouseButton mouseButton =
    (case mouseButton of
        MouseButtonLeft ->
            "left"

        MouseButtonRight ->
            "right"
    )
        |> Json.Encode.string


jsonDecodeMouseButton : Json.Decode.Decoder MouseButton
jsonDecodeMouseButton =
    Json.Decode.string
        |> Json.Decode.andThen
            (\asString ->
                case asString of
                    "left" ->
                        Json.Decode.succeed MouseButtonLeft

                    "right" ->
                        Json.Decode.succeed MouseButtonRight

                    _ ->
                        Json.Decode.fail ("Unrecognized mouse button type: " ++ asString)
            )


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
        (Json.Decode.field "uiRootAddress" (Json.Decode.maybe Json.Decode.string))


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


effectMouseClickAtLocation : MouseButton -> Location2d -> EffectOnWindowStructure
effectMouseClickAtLocation mouseButton location =
    SimpleMouseClickAtLocation
        { location = location, mouseButton = mouseButton }


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
