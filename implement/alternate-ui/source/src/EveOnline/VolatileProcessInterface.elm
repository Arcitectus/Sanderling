module EveOnline.VolatileProcessInterface exposing (..)

import Common.EffectOnWindow exposing (MouseButton(..), VirtualKeyCode(..), virtualKeyCodeAsInteger)
import Json.Decode
import Json.Decode.Extra
import Json.Encode
import Maybe.Extra


type RequestToVolatileHost
    = ListGameClientProcessesRequest
    | SearchUIRootAddress SearchUIRootAddressStructure
    | ReadFromWindow ReadFromWindowStructure
    | EffectSequenceOnWindow (TaskOnWindowStructure (List EffectSequenceElement))
    | EffectConsoleBeepSequence (List ConsoleBeepStructure)
    | GetImageDataFromReading GetImageDataFromSpecificReadingStructure


type ResponseFromVolatileHost
    = ListGameClientProcessesResponse (List GameClientProcessSummaryStruct)
    | SearchUIRootAddressResult SearchUIRootAddressResultStructure
    | ReadFromWindowResult ReadFromWindowResultStructure
    | GetImageDataFromReadingResult GetImageDataFromReadingResultStructure
    | FailedToBringWindowToFront String
    | CompletedEffectSequenceOnWindow


type alias GameClientProcessSummaryStruct =
    { processId : Int
    , mainWindowId : String
    , mainWindowTitle : String
    , mainWindowZIndex : Int
    }


type alias ReadFromWindowStructure =
    { windowId : String
    , uiRootAddress : String
    , getImageData : GetImageDataFromReadingStructure
    }


type alias GetImageDataFromSpecificReadingStructure =
    { readingId : String
    , getImageData : GetImageDataFromReadingStructure
    }


type alias GetImageDataFromReadingStructure =
    { screenshot1x1Rects : List Rect2dStructure
    }


type alias SearchUIRootAddressStructure =
    { processId : Int
    }


type alias SearchUIRootAddressResultStructure =
    { processId : Int
    , uiRootAddress : Maybe String
    }


type ReadFromWindowResultStructure
    = ProcessNotFound
    | Completed MemoryReadingCompletedStructure


type alias MemoryReadingCompletedStructure =
    { processId : Int
    , readingId : String
    , memoryReadingSerialRepresentationJson : Maybe String
    , windowClientRectOffset : Location2d
    , imageData : GetImageDataFromReadingResultStructure
    }


type alias GetImageDataFromReadingResultStructure =
    { screenshot1x1Rects : List ImageCrop
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


type alias Rect2dStructure =
    { x : Int
    , y : Int
    , width : Int
    , height : Int
    }


type alias ImageCrop =
    { offset : Location2d
    , pixels : List (List PixelValueRGB)
    }


type alias PixelValueRGB =
    { red : Int, green : Int, blue : Int }


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
        , Json.Decode.field "ReadFromWindowResult" decodeReadFromWindowResult
            |> Json.Decode.map ReadFromWindowResult
        , Json.Decode.field "GetImageDataFromReadingResult" jsonDecodeGetImageDataFromReadingResult
            |> Json.Decode.map GetImageDataFromReadingResult
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

        ReadFromWindow readFromWindow ->
            Json.Encode.object [ ( "ReadFromWindow", readFromWindow |> encodeReadFromWindow ) ]

        GetImageDataFromReading getImageDataFromReading ->
            Json.Encode.object [ ( "GetImageDataFromReading", getImageDataFromReading |> encodeGetImageDataFromSpecificReading ) ]

        EffectSequenceOnWindow taskOnWindow ->
            Json.Encode.object
                [ ( "EffectSequenceOnWindow"
                  , taskOnWindow |> encodeTaskOnWindow (Json.Encode.list encodeEffectSequenceElement)
                  )
                ]

        EffectConsoleBeepSequence effectConsoleBeepSequence ->
            Json.Encode.object [ ( "EffectConsoleBeepSequence", effectConsoleBeepSequence |> Json.Encode.list encodeConsoleBeep ) ]


decodeRequestToVolatileHost : Json.Decode.Decoder RequestToVolatileHost
decodeRequestToVolatileHost =
    Json.Decode.oneOf
        [ Json.Decode.field "ListGameClientProcessesRequest" (jsonDecodeSucceedWhenNotNull ListGameClientProcessesRequest)
        , Json.Decode.field "SearchUIRootAddress" (decodeSearchUIRootAddress |> Json.Decode.map SearchUIRootAddress)
        , Json.Decode.field "ReadFromWindow" (decodeReadFromWindow |> Json.Decode.map ReadFromWindow)
        , Json.Decode.field "GetImageDataFromReading" (decodeGetImageDataFromSpecificReading |> Json.Decode.map GetImageDataFromReading)
        , Json.Decode.field "EffectSequenceOnWindow" (decodeTaskOnWindow (Json.Decode.list decodeEffectSequenceElement) |> Json.Decode.map EffectSequenceOnWindow)
        , Json.Decode.field "EffectConsoleBeepSequence" (Json.Decode.list decodeConsoleBeep |> Json.Decode.map EffectConsoleBeepSequence)
        ]


encodeEffectSequenceElement : EffectSequenceElement -> Json.Encode.Value
encodeEffectSequenceElement sequenceElement =
    case sequenceElement of
        Effect effect ->
            Json.Encode.object [ ( "effect", encodeEffectOnWindowStructure effect ) ]

        DelayMilliseconds delayMilliseconds ->
            Json.Encode.object [ ( "delayMilliseconds", Json.Encode.int delayMilliseconds ) ]


decodeEffectSequenceElement : Json.Decode.Decoder EffectSequenceElement
decodeEffectSequenceElement =
    Json.Decode.oneOf
        [ Json.Decode.field "effect" (decodeEffectOnWindowStructure |> Json.Decode.map Effect)
        , Json.Decode.field "delayMilliseconds" (Json.Decode.int |> Json.Decode.map DelayMilliseconds)
        ]


jsonDecodeGameClientProcessSummary : Json.Decode.Decoder GameClientProcessSummaryStruct
jsonDecodeGameClientProcessSummary =
    Json.Decode.map4 GameClientProcessSummaryStruct
        (Json.Decode.field "processId" Json.Decode.int)
        (Json.Decode.field "mainWindowId" Json.Decode.string)
        (Json.Decode.field "mainWindowTitle" Json.Decode.string)
        (Json.Decode.field "mainWindowZIndex" Json.Decode.int)


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
encodeSearchUIRootAddress searchUIRootAddress =
    Json.Encode.object
        [ ( "processId", searchUIRootAddress.processId |> Json.Encode.int )
        ]


decodeSearchUIRootAddress : Json.Decode.Decoder SearchUIRootAddressStructure
decodeSearchUIRootAddress =
    Json.Decode.map SearchUIRootAddressStructure
        (Json.Decode.field "processId" Json.Decode.int)


encodeReadFromWindow : ReadFromWindowStructure -> Json.Encode.Value
encodeReadFromWindow readFromWindow =
    Json.Encode.object
        [ ( "windowId", readFromWindow.windowId |> Json.Encode.string )
        , ( "uiRootAddress", readFromWindow.uiRootAddress |> Json.Encode.string )
        , ( "getImageData", readFromWindow.getImageData |> encodeGetImageDataFromReading )
        ]


decodeReadFromWindow : Json.Decode.Decoder ReadFromWindowStructure
decodeReadFromWindow =
    Json.Decode.map3 ReadFromWindowStructure
        (Json.Decode.field "windowId" Json.Decode.string)
        (Json.Decode.field "uiRootAddress" Json.Decode.string)
        (Json.Decode.field "getImageData" decodeGetImageDataFromReading)


encodeGetImageDataFromSpecificReading : GetImageDataFromSpecificReadingStructure -> Json.Encode.Value
encodeGetImageDataFromSpecificReading getImageData =
    Json.Encode.object
        [ ( "readingId", getImageData.readingId |> Json.Encode.string )
        , ( "getImageData", getImageData.getImageData |> encodeGetImageDataFromReading )
        ]


decodeGetImageDataFromSpecificReading : Json.Decode.Decoder GetImageDataFromSpecificReadingStructure
decodeGetImageDataFromSpecificReading =
    Json.Decode.map2 GetImageDataFromSpecificReadingStructure
        (Json.Decode.field "readingId" Json.Decode.string)
        (Json.Decode.field "getImageData" decodeGetImageDataFromReading)


encodeGetImageDataFromReading : GetImageDataFromReadingStructure -> Json.Encode.Value
encodeGetImageDataFromReading getImageData =
    Json.Encode.object
        [ ( "screenshot1x1Rects", getImageData.screenshot1x1Rects |> Json.Encode.list jsonEncodeRect2d )
        ]


decodeGetImageDataFromReading : Json.Decode.Decoder GetImageDataFromReadingStructure
decodeGetImageDataFromReading =
    Json.Decode.map GetImageDataFromReadingStructure
        (Json.Decode.field "screenshot1x1Rects" (Json.Decode.list jsonDecodeRect2d))


decodeSearchUIRootAddressResult : Json.Decode.Decoder SearchUIRootAddressResultStructure
decodeSearchUIRootAddressResult =
    Json.Decode.map2 SearchUIRootAddressResultStructure
        (Json.Decode.field "processId" Json.Decode.int)
        (Json.Decode.Extra.optionalField "uiRootAddress" (Json.Decode.nullable Json.Decode.string) |> Json.Decode.map Maybe.Extra.join)


decodeReadFromWindowResult : Json.Decode.Decoder ReadFromWindowResultStructure
decodeReadFromWindowResult =
    Json.Decode.oneOf
        [ Json.Decode.field "ProcessNotFound" (Json.Decode.succeed ProcessNotFound)
        , Json.Decode.field "Completed" decodeMemoryReadingCompleted |> Json.Decode.map Completed
        ]


decodeMemoryReadingCompleted : Json.Decode.Decoder MemoryReadingCompletedStructure
decodeMemoryReadingCompleted =
    Json.Decode.map5 MemoryReadingCompletedStructure
        (Json.Decode.field "processId" Json.Decode.int)
        (Json.Decode.field "readingId" Json.Decode.string)
        (Json.Decode.Extra.optionalField "memoryReadingSerialRepresentationJson" Json.Decode.string)
        (Json.Decode.field "windowClientRectOffset" jsonDecodeLocation2d)
        (Json.Decode.field "imageData" jsonDecodeGetImageDataFromReadingResult)


jsonDecodeGetImageDataFromReadingResult : Json.Decode.Decoder GetImageDataFromReadingResultStructure
jsonDecodeGetImageDataFromReadingResult =
    Json.Decode.map GetImageDataFromReadingResultStructure
        (Json.Decode.Extra.optionalField "screenshot1x1Rects" (Json.Decode.nullable (Json.Decode.list jsonDecodeImageCrop))
            |> Json.Decode.map (Maybe.Extra.join >> Maybe.withDefault [])
        )


buildRequestStringToGetResponseFromVolatileHost : RequestToVolatileHost -> String
buildRequestStringToGetResponseFromVolatileHost =
    encodeRequestToVolatileHost
        >> Json.Encode.encode 0


jsonEncodeRect2d : Rect2dStructure -> Json.Encode.Value
jsonEncodeRect2d rect2d =
    Json.Encode.object
        [ ( "x", rect2d.x |> Json.Encode.int )
        , ( "y", rect2d.y |> Json.Encode.int )
        , ( "width", rect2d.width |> Json.Encode.int )
        , ( "height", rect2d.height |> Json.Encode.int )
        ]


jsonDecodeRect2d : Json.Decode.Decoder Rect2dStructure
jsonDecodeRect2d =
    Json.Decode.map4 Rect2dStructure
        (Json.Decode.field "x" Json.Decode.int)
        (Json.Decode.field "y" Json.Decode.int)
        (Json.Decode.field "width" Json.Decode.int)
        (Json.Decode.field "height" Json.Decode.int)


jsonDecodeImageCrop : Json.Decode.Decoder ImageCrop
jsonDecodeImageCrop =
    Json.Decode.map2 ImageCrop
        (Json.Decode.field "offset" jsonDecodeLocation2d)
        (Json.Decode.field "pixels_R8G8B8" (Json.Decode.list (Json.Decode.list jsonDecodePixelValue_R8G8B8)))


jsonDecodePixelValue_R8G8B8 : Json.Decode.Decoder PixelValueRGB
jsonDecodePixelValue_R8G8B8 =
    Json.Decode.int
        |> Json.Decode.map
            (\asInt ->
                { red = asInt // (256 * 256)
                , green = asInt // 256 |> modBy 256
                , blue = asInt |> modBy 256
                }
            )


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
