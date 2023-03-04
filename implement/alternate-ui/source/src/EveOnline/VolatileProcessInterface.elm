module EveOnline.VolatileProcessInterface exposing (..)

import Common.EffectOnWindow exposing (MouseButton(..), VirtualKeyCode(..), virtualKeyCodeAsInteger)
import Json.Decode
import Json.Encode
import Maybe.Extra


type RequestToVolatileHost
    = ListGameClientProcessesRequest
    | SearchUIRootAddress SearchUIRootAddressStructure
    | ReadFromWindow ReadFromWindowStructure
    | EffectSequenceOnWindow (TaskOnWindowStructure (List EffectSequenceElement))


type ResponseFromVolatileHost
    = ListGameClientProcessesResponse (List GameClientProcessSummaryStruct)
    | SearchUIRootAddressResponse SearchUIRootAddressResponseStruct
    | ReadFromWindowResult ReadFromWindowResultStructure
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
    }


type alias SearchUIRootAddressStructure =
    { processId : Int
    }


type alias SearchUIRootAddressResponseStruct =
    { processId : Int
    , stage : SearchUIRootAddressStage
    }


type SearchUIRootAddressStage
    = SearchUIRootAddressInProgress SearchUIRootAddressInProgressStruct
    | SearchUIRootAddressCompleted SearchUIRootAddressCompletedStruct


type alias SearchUIRootAddressInProgressStruct =
    { searchBeginTimeMilliseconds : Int
    , currentTimeMilliseconds : Int
    }


type alias SearchUIRootAddressCompletedStruct =
    { uiRootAddress : Maybe String
    }


type ReadFromWindowResultStructure
    = ProcessNotFound
    | Completed MemoryReadingCompletedStructure


type alias MemoryReadingCompletedStructure =
    { processId : Int
    , readingId : String
    , memoryReadingSerialRepresentationJson : Maybe String
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


deserializeResponseFromVolatileHost : String -> Result Json.Decode.Error ResponseFromVolatileHost
deserializeResponseFromVolatileHost =
    Json.Decode.decodeString decodeResponseFromVolatileHost


decodeResponseFromVolatileHost : Json.Decode.Decoder ResponseFromVolatileHost
decodeResponseFromVolatileHost =
    Json.Decode.oneOf
        [ Json.Decode.field "ListGameClientProcessesResponse" (Json.Decode.list jsonDecodeGameClientProcessSummary)
            |> Json.Decode.map ListGameClientProcessesResponse
        , Json.Decode.field "SearchUIRootAddressResponse" decodeSearchUIRootAddressResponse
            |> Json.Decode.map SearchUIRootAddressResponse
        , Json.Decode.field "ReadFromWindowResult" decodeReadFromWindowResult
            |> Json.Decode.map ReadFromWindowResult
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

        EffectSequenceOnWindow taskOnWindow ->
            Json.Encode.object
                [ ( "EffectSequenceOnWindow"
                  , taskOnWindow |> encodeTaskOnWindow (Json.Encode.list encodeEffectSequenceElement)
                  )
                ]


decodeRequestToVolatileHost : Json.Decode.Decoder RequestToVolatileHost
decodeRequestToVolatileHost =
    Json.Decode.oneOf
        [ Json.Decode.field "ListGameClientProcessesRequest" (jsonDecodeSucceedWhenNotNull ListGameClientProcessesRequest)
        , Json.Decode.field "SearchUIRootAddress" (decodeSearchUIRootAddress |> Json.Decode.map SearchUIRootAddress)
        , Json.Decode.field "ReadFromWindow" (decodeReadFromWindow |> Json.Decode.map ReadFromWindow)
        , Json.Decode.field "EffectSequenceOnWindow" (decodeTaskOnWindow (Json.Decode.list decodeEffectSequenceElement) |> Json.Decode.map EffectSequenceOnWindow)
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
        ]


decodeReadFromWindow : Json.Decode.Decoder ReadFromWindowStructure
decodeReadFromWindow =
    Json.Decode.map2 ReadFromWindowStructure
        (Json.Decode.field "windowId" Json.Decode.string)
        (Json.Decode.field "uiRootAddress" Json.Decode.string)


decodeSearchUIRootAddressResponse : Json.Decode.Decoder SearchUIRootAddressResponseStruct
decodeSearchUIRootAddressResponse =
    Json.Decode.map2 SearchUIRootAddressResponseStruct
        (Json.Decode.field "processId" Json.Decode.int)
        (Json.Decode.field "stage" decodeSearchUIRootAddressStage)


decodeSearchUIRootAddressStage : Json.Decode.Decoder SearchUIRootAddressStage
decodeSearchUIRootAddressStage =
    Json.Decode.oneOf
        [ Json.Decode.field "SearchUIRootAddressInProgress"
            decodeSearchUIRootAddressInProgress
            |> Json.Decode.map SearchUIRootAddressInProgress
        , Json.Decode.field "SearchUIRootAddressCompleted"
            decodeSearchUIRootAddressComplete
            |> Json.Decode.map SearchUIRootAddressCompleted
        ]


decodeSearchUIRootAddressInProgress : Json.Decode.Decoder SearchUIRootAddressInProgressStruct
decodeSearchUIRootAddressInProgress =
    Json.Decode.map2 SearchUIRootAddressInProgressStruct
        (Json.Decode.field "searchBeginTimeMilliseconds" Json.Decode.int)
        (Json.Decode.field "currentTimeMilliseconds" Json.Decode.int)


decodeSearchUIRootAddressComplete : Json.Decode.Decoder SearchUIRootAddressCompletedStruct
decodeSearchUIRootAddressComplete =
    Json.Decode.map SearchUIRootAddressCompletedStruct
        (jsonDecode_optionalField "uiRootAddress"
            (Json.Decode.nullable Json.Decode.string)
            |> Json.Decode.map Maybe.Extra.join
        )


decodeReadFromWindowResult : Json.Decode.Decoder ReadFromWindowResultStructure
decodeReadFromWindowResult =
    Json.Decode.oneOf
        [ Json.Decode.field "ProcessNotFound" (Json.Decode.succeed ProcessNotFound)
        , Json.Decode.field "Completed" decodeMemoryReadingCompleted |> Json.Decode.map Completed
        ]


decodeMemoryReadingCompleted : Json.Decode.Decoder MemoryReadingCompletedStructure
decodeMemoryReadingCompleted =
    Json.Decode.map3 MemoryReadingCompletedStructure
        (Json.Decode.field "processId" Json.Decode.int)
        (Json.Decode.field "readingId" Json.Decode.string)
        (jsonDecode_optionalField "memoryReadingSerialRepresentationJson" Json.Decode.string)


buildRequestStringToGetResponseFromVolatileHost : RequestToVolatileHost -> String
buildRequestStringToGetResponseFromVolatileHost =
    encodeRequestToVolatileHost
        >> Json.Encode.encode 0


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


jsonDecode_optionalField : String -> Json.Decode.Decoder a -> Json.Decode.Decoder (Maybe a)
jsonDecode_optionalField fieldName decoder =
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
