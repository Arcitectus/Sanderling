module EveOnline.VolatileHostInterface exposing
    ( ConsoleBeepStructure
    , EffectOnWindowStructure(..)
    , GetMemoryReadingResultStructure(..)
    , Location2d
    , MouseButton(..)
    , RequestToVolatileHost(..)
    , ResponseFromVolatileHost(..)
    , VirtualKeyCode(..)
    , WindowId
    , buildScriptToGetResponseFromVolatileHost
    , decodeRequestToVolatileHost
    , deserializeResponseFromVolatileHost
    , effectMouseClickAtLocation
    , encodeRequestToVolatileHost
    )

import Json.Decode
import Json.Decode.Extra
import Json.Encode


type RequestToVolatileHost
    = GetEveOnlineProcessesIds
    | GetMemoryReading GetMemoryReadingStructure
    | EffectOnWindow (TaskOnWindowStructure EffectOnWindowStructure)
    | EffectConsoleBeepSequence (List ConsoleBeepStructure)


type ResponseFromVolatileHost
    = EveOnlineProcessesIds (List Int)
    | GetMemoryReadingResult GetMemoryReadingResultStructure


type alias GetMemoryReadingStructure =
    { processId : Int }


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
    = SimpleMouseClickAtLocation MouseClickAtLocation
    | SimpleDragAndDrop SimpleDragAndDropStructure
    | KeyDown VirtualKeyCode
    | KeyUp VirtualKeyCode


type alias MouseMoveToStructure =
    { location : LocationRelativeToWindow }


type alias MouseButtonChangeStructure =
    { location : LocationRelativeToWindow
    , button : MouseButton
    }


type VirtualKeyCode
    = VirtualKeyCodeFromInt Int
      -- Names from https://docs.microsoft.com/en-us/windows/desktop/inputdev/virtual-key-codes
    | VK_SHIFT
    | VK_CONTROL
    | VK_MENU
    | VK_ESCAPE


type LocationRelativeToWindow
    = ClientArea Location2d -- https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-clienttoscreen


type alias WindowId =
    String


type alias MouseClickAtLocation =
    { location : Location2d
    , mouseButton : MouseButton
    }


type alias SimpleDragAndDropStructure =
    { startLocation : Location2d
    , mouseButton : MouseButton
    , endLocation : Location2d
    }


type MouseButton
    = MouseButtonLeft
    | MouseButtonRight


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
        [ Json.Decode.field "eveOnlineProcessesIds" (Json.Decode.list Json.Decode.int)
            |> Json.Decode.map EveOnlineProcessesIds
        , Json.Decode.field "GetMemoryReadingResult" decodeGetMemoryReadingResult
            |> Json.Decode.map GetMemoryReadingResult
        ]


encodeRequestToVolatileHost : RequestToVolatileHost -> Json.Encode.Value
encodeRequestToVolatileHost request =
    case request of
        GetEveOnlineProcessesIds ->
            Json.Encode.object [ ( "getEveOnlineProcessesIds", Json.Encode.object [] ) ]

        GetMemoryReading getMemoryReading ->
            Json.Encode.object [ ( "GetMemoryReading", getMemoryReading |> encodeGetMemoryReading ) ]

        EffectOnWindow taskOnWindow ->
            Json.Encode.object [ ( "effectOnWindow", taskOnWindow |> encodeTaskOnWindow encodeEffectOnWindowStructure ) ]

        EffectConsoleBeepSequence effectConsoleBeepSequence ->
            Json.Encode.object [ ( "EffectConsoleBeepSequence", effectConsoleBeepSequence |> Json.Encode.list encodeConsoleBeep ) ]


decodeRequestToVolatileHost : Json.Decode.Decoder RequestToVolatileHost
decodeRequestToVolatileHost =
    Json.Decode.oneOf
        [ Json.Decode.field "getEveOnlineProcessesIds" (Json.Decode.succeed GetEveOnlineProcessesIds)
        , Json.Decode.field "GetMemoryReading" (decodeGetMemoryReading |> Json.Decode.map GetMemoryReading)
        , Json.Decode.field "effectOnWindow" (decodeTaskOnWindow decodeEffectOnWindowStructure |> Json.Decode.map EffectOnWindow)
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
        SimpleMouseClickAtLocation mouseClickAtLocation ->
            Json.Encode.object
                [ ( "simpleMouseClickAtLocation", mouseClickAtLocation |> encodeMouseClickAtLocation )
                ]

        SimpleDragAndDrop dragAndDrop ->
            Json.Encode.object
                [ ( "simpleDragAndDrop", dragAndDrop |> encodeSimpleDragAndDrop )
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


encodeSimpleDragAndDrop : SimpleDragAndDropStructure -> Json.Encode.Value
encodeSimpleDragAndDrop simpleDragAndDrop =
    Json.Encode.object
        [ ( "startLocation", simpleDragAndDrop.startLocation |> encodeLocation2d )
        , ( "mouseButton", simpleDragAndDrop.mouseButton |> encodeMouseButton )
        , ( "endLocation", simpleDragAndDrop.endLocation |> encodeLocation2d )
        ]


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


encodeGetMemoryReading : GetMemoryReadingStructure -> Json.Encode.Value
encodeGetMemoryReading getMemoryReading =
    Json.Encode.object [ ( "processId", getMemoryReading.processId |> Json.Encode.int ) ]


decodeGetMemoryReading : Json.Decode.Decoder GetMemoryReadingStructure
decodeGetMemoryReading =
    Json.Decode.map GetMemoryReadingStructure
        (Json.Decode.field "processId" Json.Decode.int)


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


buildScriptToGetResponseFromVolatileHost : RequestToVolatileHost -> String
buildScriptToGetResponseFromVolatileHost request =
    "serialRequest("
        ++ (request
                |> encodeRequestToVolatileHost
                |> Json.Encode.encode 0
                |> Json.Encode.string
                |> Json.Encode.encode 0
           )
        ++ ")"


encodeConsoleBeep : ConsoleBeepStructure -> Json.Encode.Value
encodeConsoleBeep consoleBeep =
    Json.Encode.object
        [ ( "frequency", consoleBeep.frequency |> Json.Encode.int )
        , ( "durationInMs", consoleBeep.durationInMs |> Json.Encode.int )
        ]


effectMouseClickAtLocation : MouseButton -> Location2d -> EffectOnWindowStructure
effectMouseClickAtLocation mouseButton location =
    SimpleMouseClickAtLocation
        { location = location, mouseButton = mouseButton }


virtualKeyCodeAsInteger : VirtualKeyCode -> Int
virtualKeyCodeAsInteger keyCode =
    -- Mapping from https://docs.microsoft.com/en-us/windows/desktop/inputdev/virtual-key-codes
    case keyCode of
        VirtualKeyCodeFromInt asInt ->
            asInt

        VK_SHIFT ->
            0x10

        VK_CONTROL ->
            0x11

        VK_MENU ->
            0x12

        VK_ESCAPE ->
            0x1B
