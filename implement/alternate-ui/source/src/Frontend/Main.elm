module Frontend.Main exposing (Event(..), State, init, main, update, view)

import Browser
import Browser.Dom
import Browser.Navigation as Navigation
import Common.App
import Common.EffectOnWindow
import CompilationInterface.GenerateJsonConverters
import Dict
import EveOnline.MemoryReading
import EveOnline.ParseUserInterface exposing (UITreeNodeWithDisplayRegion)
import EveOnline.VolatileProcessInterface
import File
import File.Download
import File.Select
import Frontend.InspectParsedUserInterface
    exposing
        ( ExpandableViewNode
        , InputOnUINode(..)
        , ParsedUITreeViewPathNode(..)
        , TreeViewNode
        , TreeViewNodeChildren(..)
        , maybeInputOfferHtml
        , renderTreeNodeFromParsedUserInterface
        , treeViewNodeFromMemoryReadingUITreeNode
        )
import Html
import Html.Attributes as HA
import Html.Attributes.Aria
import Html.Events as HE
import Http
import InterfaceToFrontendClient
import Json.Decode
import List.Extra
import Process
import Svg
import Svg.Attributes
import Task
import Time
import Url


{-| 2020-01-29 Observation: In this case, I used the alternate UI on the same desktop as the game client. When using a mouse button to click the HTML button, it seemed like sometimes that click interfered with the click on the game client. Using keyboard input on the web page might be sufficient to avoid this issue.
-}
inputDelayDefaultMilliseconds : Int
inputDelayDefaultMilliseconds =
    300


effectSequenceSpacingMilliseconds : Int
effectSequenceSpacingMilliseconds =
    30


main : Program () State Event
main =
    Browser.application
        { init = init
        , update = update
        , subscriptions = subscriptions
        , view = view
        , onUrlRequest = UrlRequest
        , onUrlChange = UrlChange
        }


type alias State =
    { navigationKey : Navigation.Key
    , timeMilli : Int
    , selectedSource : MemoryReadingSource
    , readFromFileResult : Maybe ParseMemoryReadingCompleted
    , readFromLiveProcess : ReadFromLiveProcessState
    , uiTreeView : UITreeViewState
    , selectedViewMode : ViewMode
    , parsedUITreeView : ParsedUITreeViewState
    }


type alias ReadFromLiveProcessState =
    { listEveOnlineClientProcessesResult : Maybe (Result String (List EveOnline.VolatileProcessInterface.GameClientProcessSummaryStruct))
    , searchUIRootAddressResponse :
        Maybe
            ( EveOnline.VolatileProcessInterface.SearchUIRootAddressStructure
            , Result String EveOnline.VolatileProcessInterface.SearchUIRootAddressResponseStruct
            )
    , readMemoryResult : Maybe (Result String ReadFromLiveProcessCompleted)
    , lastPendingRequestToReadFromGameClientTimeMilli : Maybe Int
    }


type alias ReadFromLiveProcessCompleted =
    { windowId : String
    , memoryReading : ParseMemoryReadingCompleted
    }


type alias ParseMemoryReadingCompleted =
    { serialRepresentationJson : String
    , parseResult : Result Json.Decode.Error ParseMemoryReadingSuccess
    }


type alias ParseMemoryReadingSuccess =
    { uiTree : EveOnline.MemoryReading.UITreeNode
    , uiNodesWithDisplayRegion : Dict.Dict String UITreeNodeWithDisplayRegion
    , overviewWindows : List OverviewWindow
    , parsedUserInterface : EveOnline.ParseUserInterface.ParsedUserInterface
    }


type MemoryReadingSource
    = FromLiveProcess
    | FromFile


type ViewMode
    = ViewAlternateUI
    | ViewUITree
    | ViewParsedUI


type Event
    = BackendResponse { request : InterfaceToFrontendClient.RequestFromClient, result : Result HttpRequestErrorStructure ResponseFromServer }
    | UrlRequest Browser.UrlRequest
    | UrlChange Url.Url
    | UserInputSelectMemoryReadingSource MemoryReadingSource
    | UserInputSelectMemoryReadingFile (Maybe File.File)
    | ReadMemoryReadingFile String
    | UserInputSelectViewMode ViewMode
    | UserInputUISetTreeViewNodeIsExpanded (List ExpandableViewNode) Bool
    | UserInputParsedUISetTreeViewNodeIsExpanded (List ParsedUITreeViewPathNode) Bool
    | ContinueReadFromLiveProcess Time.Posix
    | UserInputDownloadJsonFile String
    | UserInputSendInputToUINode UserInputSendInputToUINodeStructure
    | UserInputFocusInUITree (List ExpandableViewNode)
    | UserInputFocusInParsedUI (List ParsedUITreeViewPathNode)
    | UserInputNavigateToElement String
    | DiscardEvent


type alias HttpRequestErrorStructure =
    { error : Http.Error, bodyString : Maybe String }


type alias UserInputSendInputToUINodeStructure =
    { uiNode : EveOnline.ParseUserInterface.UITreeNodeWithDisplayRegion
    , input : InputOnUINode
    , windowId : EveOnline.VolatileProcessInterface.WindowId
    , delayMilliseconds : Maybe Int
    }


type alias ParsedUITreeViewState =
    { expandedNodes : List (List ParsedUITreeViewPathNode)
    , focused : List ParsedUITreeViewPathNode
    }


type alias UITreeViewState =
    { expandedNodes : List (List ExpandableViewNode)
    , focused : List ExpandableViewNode
    }


type ResponseFromServer
    = RunInVolatileProcessResponse InterfaceToFrontendClient.RunInVolatileProcessResponseStructure
    | ReadLogResponse (List String)


type alias OverviewWindow =
    { headers : List String
    , entries : List OverviewEntry
    }


type alias OverviewEntry =
    { uiTreeNode : EveOnline.ParseUserInterface.UITreeNodeWithDisplayRegion
    , cellsContents : Dict.Dict String String
    , iconSpriteColorPercent : Maybe EveOnline.ParseUserInterface.ColorComponents
    , bgColorFillsPercent : List EveOnline.ParseUserInterface.ColorComponents
    }


type alias InputRouteConfig =
    { windowId : EveOnline.VolatileProcessInterface.WindowId
    }


subscriptions : State -> Sub Event
subscriptions state =
    case state.selectedSource of
        FromFile ->
            Sub.none

        FromLiveProcess ->
            Time.every 1000 ContinueReadFromLiveProcess


init : () -> Url.Url -> Navigation.Key -> ( State, Cmd Event )
init _ url navigationKey =
    { navigationKey = navigationKey
    , timeMilli = 0
    , selectedSource = FromFile
    , readFromLiveProcess =
        { listEveOnlineClientProcessesResult = Nothing
        , searchUIRootAddressResponse = Nothing
        , readMemoryResult = Nothing
        , lastPendingRequestToReadFromGameClientTimeMilli = Nothing
        }
    , readFromFileResult = Nothing
    , uiTreeView = { expandedNodes = [], focused = [] }
    , selectedViewMode = ViewAlternateUI
    , parsedUITreeView = { expandedNodes = [], focused = [] }
    }
        |> update (UrlChange url)


apiRequestCmd : InterfaceToFrontendClient.RequestFromClient -> Cmd Event
apiRequestCmd request =
    let
        responseDecoder =
            case request of
                InterfaceToFrontendClient.ReadLogRequest ->
                    -- TODO
                    Json.Decode.succeed (ReadLogResponse [])

                InterfaceToFrontendClient.RunInVolatileProcessRequest _ ->
                    CompilationInterface.GenerateJsonConverters.jsonDecodeRunInVolatileProcessResponseStructure
                        |> Json.Decode.map RunInVolatileProcessResponse
    in
    Http.post
        { url = "/api/"
        , expect = httpExpectJson (\result -> BackendResponse { request = request, result = result }) responseDecoder
        , body = Http.jsonBody (request |> CompilationInterface.GenerateJsonConverters.jsonEncodeRequestFromFrontendClient)
        }


update : Event -> State -> ( State, Cmd Event )
update event stateBefore =
    case event of
        BackendResponse { request, result } ->
            ( stateBefore |> integrateBackendResponse { request = request, result = result }, Cmd.none )

        UrlChange url ->
            ( stateBefore, Cmd.none )

        UrlRequest urlRequest ->
            case urlRequest of
                Browser.Internal url ->
                    ( stateBefore, Navigation.pushUrl stateBefore.navigationKey (Url.toString url) )

                Browser.External url ->
                    ( stateBefore, Navigation.load url )

        UserInputSelectMemoryReadingSource selectedSource ->
            ( { stateBefore | selectedSource = selectedSource }, Cmd.none )

        UserInputSelectViewMode selectedViewMode ->
            ( { stateBefore | selectedViewMode = selectedViewMode }, Cmd.none )

        UserInputSelectMemoryReadingFile Nothing ->
            ( stateBefore, File.Select.file [ "application/json" ] (Just >> UserInputSelectMemoryReadingFile) )

        UserInputSelectMemoryReadingFile (Just file) ->
            ( stateBefore, Task.perform ReadMemoryReadingFile (File.toString file) )

        ReadMemoryReadingFile serialRepresentationJson ->
            let
                memoryReading =
                    { serialRepresentationJson = serialRepresentationJson
                    , parseResult = serialRepresentationJson |> parseMemoryReadingFromJson
                    }
            in
            ( { stateBefore | readFromFileResult = Just memoryReading }, Cmd.none )

        UserInputUISetTreeViewNodeIsExpanded treeViewNode isExpanded ->
            let
                expandedNodes =
                    if isExpanded then
                        treeViewNode :: stateBefore.uiTreeView.expandedNodes

                    else
                        stateBefore.uiTreeView.expandedNodes |> List.filter ((/=) treeViewNode)

                uiTreeViewBefore =
                    stateBefore.uiTreeView
            in
            ( { stateBefore | uiTreeView = { uiTreeViewBefore | expandedNodes = expandedNodes } }, Cmd.none )

        UserInputParsedUISetTreeViewNodeIsExpanded treeViewNode isExpanded ->
            let
                expandedNodes =
                    if isExpanded then
                        treeViewNode :: stateBefore.parsedUITreeView.expandedNodes

                    else
                        stateBefore.parsedUITreeView.expandedNodes |> List.filter ((/=) treeViewNode)

                parsedUITreeViewBefore =
                    stateBefore.parsedUITreeView
            in
            ( { stateBefore | parsedUITreeView = { parsedUITreeViewBefore | expandedNodes = expandedNodes } }, Cmd.none )

        ContinueReadFromLiveProcess time ->
            let
                timeMilli =
                    Time.posixToMillis time

                ( readFromLiveProcessState, cmd ) =
                    (stateBefore.readFromLiveProcess |> decideNextStepToReadFromLiveProcess { timeMilli = timeMilli })
                        |> Tuple.mapSecond .nextCmd
            in
            ( { stateBefore | timeMilli = timeMilli, readFromLiveProcess = readFromLiveProcessState }, cmd )

        UserInputDownloadJsonFile jsonString ->
            ( stateBefore, File.Download.string "memory-reading.json" "application/json" jsonString )

        UserInputSendInputToUINode sendInput ->
            case sendInput.delayMilliseconds of
                Just delayMilliseconds ->
                    let
                        delayedInputCmd =
                            Task.perform
                                (always (UserInputSendInputToUINode { sendInput | delayMilliseconds = Nothing }))
                                (Process.sleep (toFloat delayMilliseconds))
                    in
                    ( stateBefore, delayedInputCmd )

                Nothing ->
                    let
                        uiNodeCenter =
                            sendInput.uiNode.totalDisplayRegionVisible
                                |> EveOnline.ParseUserInterface.centerFromDisplayRegion

                        volatileProcessInterfaceEffects =
                            case sendInput.input of
                                MouseClickLeft ->
                                    Common.EffectOnWindow.effectsMouseClickAtLocation
                                        Common.EffectOnWindow.MouseButtonLeft
                                        uiNodeCenter

                                MouseClickRight ->
                                    Common.EffectOnWindow.effectsMouseClickAtLocation
                                        Common.EffectOnWindow.MouseButtonRight
                                        uiNodeCenter

                        requestSendInputToGameClient =
                            apiRequestCmd
                                (InterfaceToFrontendClient.RunInVolatileProcessRequest
                                    (EveOnline.VolatileProcessInterface.EffectSequenceOnWindow
                                        { windowId = sendInput.windowId
                                        , bringWindowToForeground = True
                                        , task =
                                            volatileProcessInterfaceEffects
                                                |> List.map (effectOnWindowAsVolatileHostEffectOnWindow >> EveOnline.VolatileProcessInterface.Effect)
                                                |> List.intersperse (EveOnline.VolatileProcessInterface.DelayMilliseconds effectSequenceSpacingMilliseconds)
                                        }
                                    )
                                )

                        -- TODO: Remember sending input, to syncronize with get next reading.
                    in
                    ( stateBefore, requestSendInputToGameClient )

        UserInputFocusInUITree focusedPath ->
            let
                uiTreeViewBefore =
                    stateBefore.uiTreeView
            in
            ( { stateBefore | uiTreeView = { uiTreeViewBefore | focused = focusedPath } }, Cmd.none )

        UserInputFocusInParsedUI focusedPath ->
            let
                parsedUITreeViewBefore =
                    stateBefore.parsedUITreeView
            in
            ( { stateBefore | parsedUITreeView = { parsedUITreeViewBefore | focused = focusedPath } }, Cmd.none )

        UserInputNavigateToElement elementId ->
            ( stateBefore, Task.attempt (always DiscardEvent) (Browser.Dom.focus elementId) )

        DiscardEvent ->
            ( stateBefore, Cmd.none )


effectOnWindowAsVolatileHostEffectOnWindow : Common.EffectOnWindow.EffectOnWindowStructure -> EveOnline.VolatileProcessInterface.EffectOnWindowStructure
effectOnWindowAsVolatileHostEffectOnWindow effectOnWindow =
    case effectOnWindow of
        Common.EffectOnWindow.MouseMoveTo mouseMoveTo ->
            EveOnline.VolatileProcessInterface.MouseMoveTo { location = mouseMoveTo }

        Common.EffectOnWindow.KeyDown key ->
            EveOnline.VolatileProcessInterface.KeyDown key

        Common.EffectOnWindow.KeyUp key ->
            EveOnline.VolatileProcessInterface.KeyUp key


integrateBackendResponse : { request : InterfaceToFrontendClient.RequestFromClient, result : Result HttpRequestErrorStructure ResponseFromServer } -> State -> State
integrateBackendResponse { request, result } stateBefore =
    case request of
        -- TODO: Consolidate unpack response common parts.
        InterfaceToFrontendClient.RunInVolatileProcessRequest EveOnline.VolatileProcessInterface.ListGameClientProcessesRequest ->
            let
                listEveOnlineClientProcessesResult =
                    result
                        |> Result.mapError describeHttpError
                        |> Result.andThen
                            (\response ->
                                case response of
                                    ReadLogResponse _ ->
                                        Err "Unexpected response"

                                    RunInVolatileProcessResponse runInVolatileProcessResponse ->
                                        case runInVolatileProcessResponse of
                                            InterfaceToFrontendClient.SetupNotCompleteResponse status ->
                                                Err ("Volatile process setup not complete: " ++ status)

                                            InterfaceToFrontendClient.RunInVolatileProcessCompleteResponse runInVolatileHostCompleteResponse ->
                                                case runInVolatileHostCompleteResponse.exceptionToString of
                                                    Just exception ->
                                                        Err ("Failed with exception: " ++ exception)

                                                    Nothing ->
                                                        runInVolatileHostCompleteResponse.returnValueToString
                                                            |> Maybe.withDefault ""
                                                            |> EveOnline.VolatileProcessInterface.deserializeResponseFromVolatileHost
                                                            |> Result.mapError Json.Decode.errorToString
                                                            |> Result.andThen
                                                                (\responseFromVolatileHost ->
                                                                    case responseFromVolatileHost of
                                                                        EveOnline.VolatileProcessInterface.ListGameClientProcessesResponse gameClientProcesses ->
                                                                            Ok gameClientProcesses

                                                                        EveOnline.VolatileProcessInterface.SearchUIRootAddressResponse _ ->
                                                                            Err "Unexpected response: SearchUIRootAddressResponse"

                                                                        EveOnline.VolatileProcessInterface.ReadFromWindowResult _ ->
                                                                            Err "Unexpected response: ReadFromWindowResult"

                                                                        EveOnline.VolatileProcessInterface.FailedToBringWindowToFront failedToBringWindowToFront ->
                                                                            Err ("Unexpected response: FailedToBringWindowToFront: " ++ failedToBringWindowToFront)

                                                                        EveOnline.VolatileProcessInterface.CompletedEffectSequenceOnWindow ->
                                                                            Err "Unexpected response: CompletedEffectSequenceOnWindow"
                                                                )
                            )

                readFromLiveProcessBefore =
                    stateBefore.readFromLiveProcess
            in
            { stateBefore
                | readFromLiveProcess =
                    { readFromLiveProcessBefore
                        | listEveOnlineClientProcessesResult = Just listEveOnlineClientProcessesResult
                        , searchUIRootAddressResponse = Nothing
                        , readMemoryResult = Nothing
                    }
            }

        InterfaceToFrontendClient.RunInVolatileProcessRequest (EveOnline.VolatileProcessInterface.ReadFromWindow readFromWindow) ->
            let
                readMemoryResult =
                    result
                        |> Result.mapError describeHttpError
                        |> Result.andThen
                            (\response ->
                                case response of
                                    ReadLogResponse _ ->
                                        Err "Unexpected response"

                                    RunInVolatileProcessResponse runInVolatileProcessResponse ->
                                        case runInVolatileProcessResponse of
                                            InterfaceToFrontendClient.SetupNotCompleteResponse status ->
                                                Err ("Volatile process setup not complete: " ++ status)

                                            InterfaceToFrontendClient.RunInVolatileProcessCompleteResponse runInVolatileProcessCompleteResponse ->
                                                case runInVolatileProcessCompleteResponse.exceptionToString of
                                                    Just exception ->
                                                        Err ("Failed with exception: " ++ exception)

                                                    Nothing ->
                                                        runInVolatileProcessCompleteResponse.returnValueToString
                                                            |> Maybe.withDefault ""
                                                            |> EveOnline.VolatileProcessInterface.deserializeResponseFromVolatileHost
                                                            |> Result.mapError Json.Decode.errorToString
                                                            |> Result.andThen
                                                                (\responseFromVolatileProcess ->
                                                                    case responseFromVolatileProcess of
                                                                        EveOnline.VolatileProcessInterface.ListGameClientProcessesResponse _ ->
                                                                            Err "Unexpected response: ListGameClientProcessesResponse"

                                                                        EveOnline.VolatileProcessInterface.SearchUIRootAddressResponse _ ->
                                                                            Err "Unexpected response: SearchUIRootAddressResponse"

                                                                        EveOnline.VolatileProcessInterface.ReadFromWindowResult readFromWindowResult ->
                                                                            case readFromWindowResult of
                                                                                EveOnline.VolatileProcessInterface.ProcessNotFound ->
                                                                                    Err "Process not found"

                                                                                EveOnline.VolatileProcessInterface.Completed memoryReadingCompleted ->
                                                                                    Ok memoryReadingCompleted

                                                                        EveOnline.VolatileProcessInterface.FailedToBringWindowToFront failedToBringWindowToFront ->
                                                                            Err ("Unexpected response: FailedToBringWindowToFront: " ++ failedToBringWindowToFront)

                                                                        EveOnline.VolatileProcessInterface.CompletedEffectSequenceOnWindow ->
                                                                            Err "Unexpected response: CompletedEffectSequenceOnWindow"
                                                                )
                            )
                        |> Result.andThen
                            (\readingCompleted ->
                                case readingCompleted.memoryReadingSerialRepresentationJson of
                                    Nothing ->
                                        Err "Memory reading completed, but 'serialRepresentationJson' is null. Please configure EVE Online client and restart."

                                    Just memoryReadingSerialRepresentationJson ->
                                        Ok
                                            { windowId = readFromWindow.windowId
                                            , memoryReading =
                                                { serialRepresentationJson = memoryReadingSerialRepresentationJson
                                                , parseResult = memoryReadingSerialRepresentationJson |> parseMemoryReadingFromJson
                                                }
                                            }
                            )

                readFromLiveProcessBefore =
                    stateBefore.readFromLiveProcess
            in
            { stateBefore
                | readFromLiveProcess =
                    { readFromLiveProcessBefore
                        | readMemoryResult = Just readMemoryResult
                        , lastPendingRequestToReadFromGameClientTimeMilli = Nothing
                    }
            }

        InterfaceToFrontendClient.RunInVolatileProcessRequest (EveOnline.VolatileProcessInterface.SearchUIRootAddress searchUIRootRequest) ->
            let
                searchUIRootResult =
                    result
                        |> Result.mapError describeHttpError
                        |> Result.andThen
                            (\response ->
                                case response of
                                    ReadLogResponse _ ->
                                        Err "Unexpected response"

                                    RunInVolatileProcessResponse runInVolatileProcessResponse ->
                                        case runInVolatileProcessResponse of
                                            InterfaceToFrontendClient.SetupNotCompleteResponse status ->
                                                Err ("Volatile process setup not complete: " ++ status)

                                            InterfaceToFrontendClient.RunInVolatileProcessCompleteResponse runInVolatileHostCompleteResponse ->
                                                case runInVolatileHostCompleteResponse.exceptionToString of
                                                    Just exception ->
                                                        Err ("Failed with exception: " ++ exception)

                                                    Nothing ->
                                                        runInVolatileHostCompleteResponse.returnValueToString
                                                            |> Maybe.withDefault ""
                                                            |> EveOnline.VolatileProcessInterface.deserializeResponseFromVolatileHost
                                                            |> Result.mapError Json.Decode.errorToString
                                                            |> Result.andThen
                                                                (\responseFromVolatileHost ->
                                                                    case responseFromVolatileHost of
                                                                        EveOnline.VolatileProcessInterface.ListGameClientProcessesResponse _ ->
                                                                            Err "Unexpected response: ListGameClientProcessesResponse"

                                                                        EveOnline.VolatileProcessInterface.SearchUIRootAddressResponse searchUIRootAddressResponse ->
                                                                            Ok searchUIRootAddressResponse

                                                                        EveOnline.VolatileProcessInterface.ReadFromWindowResult _ ->
                                                                            Err "Unexpected response: ReadFromWindowResult"

                                                                        EveOnline.VolatileProcessInterface.FailedToBringWindowToFront failedToBringWindowToFront ->
                                                                            Err ("Unexpected response: FailedToBringWindowToFront: " ++ failedToBringWindowToFront)

                                                                        EveOnline.VolatileProcessInterface.CompletedEffectSequenceOnWindow ->
                                                                            Err "Unexpected response: CompletedEffectSequenceOnWindow"
                                                                )
                            )

                readFromLiveProcessBefore =
                    stateBefore.readFromLiveProcess
            in
            { stateBefore
                | readFromLiveProcess =
                    { readFromLiveProcessBefore
                        | searchUIRootAddressResponse = Just ( searchUIRootRequest, searchUIRootResult )
                        , lastPendingRequestToReadFromGameClientTimeMilli = Nothing
                    }
            }

        _ ->
            stateBefore


decideNextStepToReadFromLiveProcess :
    { timeMilli : Int }
    -> ReadFromLiveProcessState
    ->
        ( ReadFromLiveProcessState
        , { describeState : String
          , lastMemoryReading : Maybe ReadFromLiveProcessCompleted
          , nextCmd : Cmd.Cmd Event
          }
        )
decideNextStepToReadFromLiveProcess { timeMilli } stateBefore =
    let
        requestListGameClientProcesses =
            apiRequestCmd
                (InterfaceToFrontendClient.RunInVolatileProcessRequest
                    EveOnline.VolatileProcessInterface.ListGameClientProcessesRequest
                )

        requestSearchUIRootFrequently config =
            let
                gameClientProcessId =
                    config.selectGameClientResult.selectedProcess.processId

                requestSearchUIRoot =
                    apiRequestCmd
                        (InterfaceToFrontendClient.RunInVolatileProcessRequest
                            (EveOnline.VolatileProcessInterface.SearchUIRootAddress { processId = gameClientProcessId })
                        )

                searchStillPending =
                    stateBefore.lastPendingRequestToReadFromGameClientTimeMilli
                        |> Maybe.map (\pendingReadingTimeMilli -> timeMilli < pendingReadingTimeMilli + 1000)
                        |> Maybe.withDefault False

                ( state, nextCmd ) =
                    if searchStillPending then
                        ( stateBefore, Cmd.none )

                    else
                        ( { stateBefore | lastPendingRequestToReadFromGameClientTimeMilli = Just timeMilli }
                        , requestSearchUIRoot
                        )

                inProgressAddition =
                    case config.searchInProgress of
                        Nothing ->
                            ""

                        Just searchInProgress ->
                            " since "
                                ++ String.fromInt ((searchInProgress.currentTimeMilliseconds - searchInProgress.searchBeginTimeMilliseconds) // 1000)
                                ++ " seconds"

                describeState =
                    (("Searching the address of the UI root in process "
                        ++ String.fromInt gameClientProcessId
                        ++ inProgressAddition
                        ++ "..."
                     )
                        :: config.selectGameClientResult.report
                    )
                        |> String.join "\n"
            in
            ( state
            , { describeState = describeState
              , lastMemoryReading = Nothing
              , nextCmd = nextCmd
              }
            )
    in
    case stateBefore.listEveOnlineClientProcessesResult of
        Nothing ->
            ( stateBefore
            , { describeState = "Did not yet search for the IDs of the EVE Online client processes."
              , lastMemoryReading = Nothing
              , nextCmd = requestListGameClientProcesses
              }
            )

        Just (Err error) ->
            ( stateBefore
            , { describeState = "Failed to get IDs of the EVE Online client processes: " ++ error
              , lastMemoryReading = Nothing
              , nextCmd = requestListGameClientProcesses
              }
            )

        Just (Ok eveOnlineClientProcesses) ->
            case eveOnlineClientProcesses |> selectGameClientProcess of
                Err error ->
                    ( stateBefore
                    , { describeState = error ++ " Looks like there is no EVE Online client process started. I continue looking in case one is started..."
                      , lastMemoryReading = Nothing
                      , nextCmd = requestListGameClientProcesses
                      }
                    )

                Ok selectGameClientResult ->
                    case stateBefore.searchUIRootAddressResponse of
                        Nothing ->
                            requestSearchUIRootFrequently
                                { selectGameClientResult = selectGameClientResult
                                , searchInProgress = Nothing
                                }

                        Just ( searchUIRootRequest, Err error ) ->
                            ( stateBefore
                            , { describeState =
                                    "Failed to search the UI root in process "
                                        ++ (searchUIRootRequest.processId |> String.fromInt)
                                        ++ ": "
                                        ++ error
                              , lastMemoryReading = Nothing
                              , nextCmd = Cmd.none
                              }
                            )

                        Just ( _, Ok searchUIRootAddressResponse ) ->
                            case searchUIRootAddressResponse.stage of
                                EveOnline.VolatileProcessInterface.SearchUIRootAddressInProgress searchInProgress ->
                                    requestSearchUIRootFrequently
                                        { selectGameClientResult = selectGameClientResult
                                        , searchInProgress = Just searchInProgress
                                        }

                                EveOnline.VolatileProcessInterface.SearchUIRootAddressCompleted searchUIRootAddressResult ->
                                    case searchUIRootAddressResult.uiRootAddress of
                                        Nothing ->
                                            ( stateBefore
                                            , { describeState =
                                                    "Did not find the UI root in process "
                                                        ++ String.fromInt searchUIRootAddressResponse.processId
                                              , lastMemoryReading = Nothing
                                              , nextCmd = Cmd.none
                                              }
                                            )

                                        Just uiRootAddress ->
                                            case
                                                stateBefore.listEveOnlineClientProcessesResult
                                                    |> Maybe.andThen Result.toMaybe
                                                    |> Maybe.andThen (List.filter (.processId >> (==) searchUIRootAddressResponse.processId) >> List.head)
                                            of
                                                Nothing ->
                                                    ( stateBefore
                                                    , { describeState = "Did not find a matching entry in the list of the EVE Online client processes."
                                                      , lastMemoryReading = Nothing
                                                      , nextCmd = requestListGameClientProcesses
                                                      }
                                                    )

                                                Just gameClientProcess ->
                                                    let
                                                        requestReadMemory =
                                                            apiRequestCmd
                                                                (InterfaceToFrontendClient.RunInVolatileProcessRequest
                                                                    (EveOnline.VolatileProcessInterface.ReadFromWindow
                                                                        { windowId = gameClientProcess.mainWindowId
                                                                        , uiRootAddress = uiRootAddress
                                                                        }
                                                                    )
                                                                )

                                                        ( describeLastReadResult, lastMemoryReading ) =
                                                            case stateBefore.readMemoryResult of
                                                                Nothing ->
                                                                    ( "", Nothing )

                                                                Just (Err error) ->
                                                                    ( "The last attempt to read from the game client process failed: " ++ error, Nothing )

                                                                Just (Ok lastMemoryReadingCompleted) ->
                                                                    ( "The last attempt to read from the game client process was successful.", Just lastMemoryReadingCompleted )

                                                        memoryReadingStillPending =
                                                            stateBefore.lastPendingRequestToReadFromGameClientTimeMilli
                                                                |> Maybe.map (\pendingReadingTimeMilli -> timeMilli < pendingReadingTimeMilli + 10000)
                                                                |> Maybe.withDefault False

                                                        ( state, nextCmd ) =
                                                            if memoryReadingStillPending then
                                                                ( stateBefore, Cmd.none )

                                                            else
                                                                ( { stateBefore | lastPendingRequestToReadFromGameClientTimeMilli = Just timeMilli }
                                                                , requestReadMemory
                                                                )
                                                    in
                                                    ( state
                                                    , { describeState =
                                                            "I try to read the memory from process "
                                                                ++ (searchUIRootAddressResponse.processId |> String.fromInt)
                                                                ++ " starting from root address "
                                                                ++ uiRootAddress
                                                                ++ ". "
                                                                ++ describeLastReadResult
                                                      , nextCmd = nextCmd
                                                      , lastMemoryReading = lastMemoryReading
                                                      }
                                                    )


selectGameClientProcess :
    List EveOnline.VolatileProcessInterface.GameClientProcessSummaryStruct
    -> Result String { selectedProcess : EveOnline.VolatileProcessInterface.GameClientProcessSummaryStruct, report : List String }
selectGameClientProcess gameClientProcesses =
    case gameClientProcesses |> List.sortBy .mainWindowZIndex |> List.head of
        Nothing ->
            Err "I did not find an EVE Online client process."

        Just selectedProcess ->
            let
                report =
                    if [ selectedProcess ] == gameClientProcesses then
                        []

                    else
                        [ "I found "
                            ++ (gameClientProcesses |> List.length |> String.fromInt)
                            ++ " game client processes. I selected process "
                            ++ (selectedProcess.processId |> String.fromInt)
                            ++ " ('"
                            ++ selectedProcess.mainWindowTitle
                            ++ "') because its main window was the topmost."
                        ]
            in
            Ok { selectedProcess = selectedProcess, report = report }


view : State -> Browser.Document Event
view state =
    let
        sourceSpecificHtml =
            case state.selectedSource of
                FromFile ->
                    viewSourceFromFile state

                FromLiveProcess ->
                    viewSourceFromLiveProcess state

        selectedSourceText =
            case state.selectedSource of
                FromFile ->
                    "file"

                FromLiveProcess ->
                    "live process"

        body =
            [ globalStylesHtmlElement
            , globalGuideHtmlElement
            , verticalSpacerFromHeightInEm 1
            , selectSourceHtml state
            , verticalSpacerFromHeightInEm 1
            , [ ("Reading from " ++ selectedSourceText) |> Html.text ] |> Html.h3 []
            , sourceSpecificHtml
            ]
    in
    { title = "Alternate EVE Online UI version " ++ Common.App.versionId, body = body }


globalGuideHtmlElement : Html.Html a
globalGuideHtmlElement =
    Html.span []
        [ Html.text "For a guide on the structures in the parsed memory reading, see "
        , linkHtmlFromUrl "https://to.botlab.org/guide/parsed-user-interface-of-the-eve-online-game-client"
        ]


viewSourceFromFile : State -> Html.Html Event
viewSourceFromFile state =
    let
        buttonLoadFromFileHtml =
            [ "Click here to load a reading from a JSON file" |> Html.text ]
                |> Html.button [ HE.onClick (UserInputSelectMemoryReadingFile Nothing) ]

        memoryReadingFromFileHtml =
            case state.readFromFileResult of
                Nothing ->
                    [ "No reading loaded" |> Html.text
                    , verticalSpacerFromHeightInEm 0.5
                    , [ "Want to load a memory reading from a bot session? You can use the devtools to export it from the session into a file to load it here. To export readings from a botting session, see the guide at " |> Html.text
                      , linkHtmlFromUrl "https://to.botlab.org/guide/observing-and-inspecting-a-bot"
                      ]
                        |> Html.span []
                    ]
                        |> Html.div []

                Just memoryReadingCompleted ->
                    case memoryReadingCompleted.parseResult of
                        Err error ->
                            ("Failed to decode reading loaded from file: " ++ (error |> Json.Decode.errorToString)) |> Html.text

                        Ok parseSuccess ->
                            [ "Successfully read the reading from the file." |> Html.text
                            , presentParsedMemoryReading Nothing parseSuccess state
                            ]
                                |> List.map (List.singleton >> Html.div [])
                                |> Html.div []
    in
    [ buttonLoadFromFileHtml
    , verticalSpacerFromHeightInEm 1
    , memoryReadingFromFileHtml
    ]
        |> Html.div []


viewSourceFromLiveProcess : State -> Html.Html Event
viewSourceFromLiveProcess state =
    let
        ( _, nextStep ) =
            decideNextStepToReadFromLiveProcess { timeMilli = state.timeMilli } state.readFromLiveProcess

        memoryReadingHtml =
            case nextStep.lastMemoryReading of
                Nothing ->
                    "" |> Html.text

                Just parsedReadMemoryResult ->
                    let
                        downloadButton =
                            [ "Click here to download this reading to a JSON file." |> Html.text ]
                                |> Html.button [ HE.onClick (UserInputDownloadJsonFile parsedReadMemoryResult.memoryReading.serialRepresentationJson) ]

                        inputRoute =
                            { windowId = parsedReadMemoryResult.windowId }

                        parsedHtml =
                            case parsedReadMemoryResult.memoryReading.parseResult of
                                Err parseError ->
                                    ("Failed to parse this reading: " ++ (parseError |> Json.Decode.errorToString)) |> Html.text

                                Ok parseSuccess ->
                                    presentParsedMemoryReading (Just inputRoute) parseSuccess state
                    in
                    [ "Successfully read from the memory of the live process." |> Html.text
                    , downloadButton
                    , verticalSpacerFromHeightInEm 1
                    , parsedHtml
                    ]
                        |> List.map (List.singleton >> Html.div [])
                        |> Html.div []
    in
    [ nextStep.describeState |> Html.text
    , verticalSpacerFromHeightInEm 1
    , memoryReadingHtml
    ]
        |> Html.div []


presentParsedMemoryReading : Maybe InputRouteConfig -> ParseMemoryReadingSuccess -> State -> Html.Html Event
presentParsedMemoryReading maybeInputRoute memoryReading state =
    let
        continueWithTitle title htmlElements =
            ([ title |> Html.text ] |> Html.h3 []) :: htmlElements

        selectedViewHtml =
            case state.selectedViewMode of
                ViewAlternateUI ->
                    continueWithTitle
                        "Using the Alternate UI"
                        [ [ (String.fromInt (List.length memoryReading.overviewWindows) ++ " Overview windows")
                                |> Html.text
                          ]
                            |> Html.h3 []
                        , memoryReading.overviewWindows
                            |> List.map (displayReadOverviewWindow maybeInputRoute)
                            |> List.intersperse (verticalSpacerFromHeightInEm 1)
                            |> Html.div []
                        , verticalSpacerFromHeightInEm 0.5
                        , [ ((memoryReading.parsedUserInterface.contextMenus |> List.length |> String.fromInt) ++ " Context menus") |> Html.text ] |> Html.h3 []
                        , displayParsedContextMenus maybeInputRoute memoryReading.parsedUserInterface.contextMenus
                        ]

                ViewUITree ->
                    continueWithTitle
                        "Inspecting the UI tree"
                        [ "Below is an interactive tree view to explore this reading. You can expand and collapse individual nodes." |> Html.text
                        , viewTreeMemoryReadingUITreeNode maybeInputRoute memoryReading.uiNodesWithDisplayRegion state.uiTreeView memoryReading.uiTree
                        ]

                ViewParsedUI ->
                    continueWithTitle
                        "Inspecting the parsed user interface"
                        [ "Below is an interactive tree view to explore the parsed user interface. You can expand and collapse individual nodes." |> Html.text
                        , viewTreeParsedUserInterface maybeInputRoute memoryReading.uiNodesWithDisplayRegion state.parsedUITreeView memoryReading.parsedUserInterface
                        ]

        uiTreeSvg =
            viewUITreeSvg memoryReading.parsedUserInterface.uiTree

        visualSectionHtml =
            continueWithTitle "Visualization of the UI tree" [ uiTreeSvg ]
    in
    [ selectViewModeHtml state
    , selectedViewHtml
        |> List.map (List.singleton >> Html.div [])
        |> Html.div []
    , visualSectionHtml |> Html.div []
    ]
        |> Html.div []


displayReadOverviewWindow : Maybe InputRouteConfig -> OverviewWindow -> Html.Html Event
displayReadOverviewWindow maybeInputRouteConfig overviewWindow =
    let
        columnsFromHeaders =
            overviewWindow.headers
                |> List.map
                    (\header ->
                        { header = header
                        , cellHtmlFromEntry =
                            \overviewEntry ->
                                overviewEntry.cellsContents
                                    |> Dict.get header
                                    |> Maybe.withDefault ""
                                    |> Html.text
                        }
                    )

        iconColumn =
            { header = ""
            , cellHtmlFromEntry =
                .iconSpriteColorPercent
                    >> Maybe.map
                        (\colorPercent ->
                            Html.div
                                [ HA.style "background-color" (cssColorFromColorPercent colorPercent)
                                , HA.style "width" "10px"
                                , HA.style "height" "10px"
                                ]
                                []
                        )
                    >> Maybe.withDefault (Html.text "")
            }

        columns =
            iconColumn :: columnsFromHeaders

        headersHtml =
            columns
                |> List.map (.header >> Html.text >> List.singleton >> Html.td [])
                |> Html.tr []

        entriesHtml =
            overviewWindow.entries
                |> List.map
                    (\overviewEntry ->
                        let
                            bgFillsColors =
                                overviewEntry.bgColorFillsPercent
                                    |> List.map
                                        (\colorPercent ->
                                            let
                                                colorString =
                                                    ([ colorPercent.r, colorPercent.g, colorPercent.b ]
                                                        |> List.map (\percent -> ((percent * 255) // 100) |> String.fromInt)
                                                    )
                                                        ++ [ (colorPercent.a |> toFloat) / 100 |> String.fromFloat ]
                                                        |> String.join ","
                                            in
                                            "rgba(" ++ colorString ++ ")"
                                        )

                            columnsHtml =
                                columns
                                    |> List.map
                                        (\column -> [ overviewEntry |> column.cellHtmlFromEntry ] |> Html.td [])

                            inputHtml =
                                maybeInputOfferHtml (maybeInputRouteConfig |> Maybe.map inputRouteFromInputConfig) [ MouseClickLeft, MouseClickRight ] overviewEntry.uiTreeNode
                        in
                        (columnsHtml ++ [ inputHtml ])
                            |> Html.tr [ HA.style "background" (bgFillsColors |> String.join " ") ]
                    )
    in
    headersHtml
        :: entriesHtml
        |> Html.table []


cssColorFromColorPercent : EveOnline.ParseUserInterface.ColorComponents -> String
cssColorFromColorPercent colorPercent =
    "rgba("
        ++ (([ colorPercent.r, colorPercent.g, colorPercent.b ]
                |> List.map (\rgbComponent -> String.fromInt ((rgbComponent * 255) // 100))
            )
                ++ [ String.fromFloat ((colorPercent.a |> toFloat) / 100) ]
                |> String.join ","
           )
        ++ ")"


displayParsedContextMenus : Maybe InputRouteConfig -> List EveOnline.ParseUserInterface.ContextMenu -> Html.Html Event
displayParsedContextMenus maybeInputRoute contextMenus =
    contextMenus
        |> List.indexedMap
            (\i contextMenu ->
                [ [ ("Context menu " ++ (i |> String.fromInt)) |> Html.text ] |> Html.h4 []
                , contextMenu |> displayParsedContextMenu maybeInputRoute
                ]
                    |> Html.div []
            )
        |> Html.div []


displayParsedContextMenu : Maybe InputRouteConfig -> EveOnline.ParseUserInterface.ContextMenu -> Html.Html Event
displayParsedContextMenu maybeInputRouteConfig contextMenu =
    contextMenu.entries
        |> List.map
            (\menuEntry ->
                [ menuEntry.text |> Html.text, maybeInputOfferHtml (maybeInputRouteConfig |> Maybe.map inputRouteFromInputConfig) [ MouseClickLeft ] menuEntry.uiNode ]
                    |> Html.div []
            )
        |> Html.div []


selectSourceHtml : State -> Html.Html Event
selectSourceHtml state =
    (([ "Select a source for the reading" |> Html.text ] |> Html.legend [])
        :: ([ ( "From file", FromFile )
            , ( "From live game client process", FromLiveProcess )
            ]
                |> List.map
                    (\( offeredSourceLabel, offeredSource ) ->
                        radioButtonHtml
                            "memoryreadingsource"
                            offeredSourceLabel
                            (state.selectedSource == offeredSource)
                            (UserInputSelectMemoryReadingSource offeredSource)
                    )
           )
    )
        |> Html.fieldset []


selectViewModeHtml : State -> Html.Html Event
selectViewModeHtml state =
    (([ "Select a view mode" |> Html.text ] |> Html.legend [])
        :: ([ ( "View Alternate UI", ViewAlternateUI )
            , ( "View Parsed User Interface", ViewParsedUI )
            , ( "View UI Tree", ViewUITree )
            ]
                |> List.map
                    (\( offeredModeLabel, offeredMode ) ->
                        radioButtonHtml
                            "viewmode"
                            offeredModeLabel
                            (state.selectedViewMode == offeredMode)
                            (UserInputSelectViewMode offeredMode)
                    )
           )
    )
        |> Html.fieldset []


radioButtonHtml : String -> String -> Bool -> event -> Html.Html event
radioButtonHtml groupName labelText isChecked msg =
    [ Html.input [ HA.type_ "radio", HA.name groupName, HE.onClick msg, HA.checked isChecked ] []
    , Html.text labelText
    ]
        |> Html.label [ HA.style "padding" "20px" ]


inputRouteFromInputConfig : InputRouteConfig -> Frontend.InspectParsedUserInterface.InputRoute Event
inputRouteFromInputConfig inputRouteConfig =
    \uiNode inputKind ->
        UserInputSendInputToUINode
            { uiNode = uiNode
            , input = inputKind
            , windowId = inputRouteConfig.windowId
            , delayMilliseconds = Just inputDelayDefaultMilliseconds
            }


viewTreeMemoryReadingUITreeNode :
    Maybe InputRouteConfig
    -> Dict.Dict String UITreeNodeWithDisplayRegion
    -> UITreeViewState
    -> EveOnline.MemoryReading.UITreeNode
    -> Html.Html Event
viewTreeMemoryReadingUITreeNode maybeInputRouteConfig uiNodesWithDisplayRegion viewState treeNode =
    let
        nodeIsExpanded nodeId =
            viewState.expandedNodes |> List.member nodeId
    in
    treeViewNodeFromMemoryReadingUITreeNode (maybeInputRouteConfig |> Maybe.map inputRouteFromInputConfig) uiNodesWithDisplayRegion treeNode
        |> renderInteractiveTreeView
            UserInputUISetTreeViewNodeIsExpanded
            nodeIsExpanded
            { focusedPath = viewState.focused
            , eventForFocus = UserInputFocusInUITree
            , setFocusEvent = UserInputNavigateToElement
            , htmlElementId = htmlElementIdFromUIPathNode
            }
            []


htmlElementIdFromUIPathNode : ExpandableViewNode -> String
htmlElementIdFromUIPathNode pathNode =
    case pathNode of
        Frontend.InspectParsedUserInterface.ExpandableUITreeNode nodeIdentity ->
            "UITreeNode_" ++ nodeIdentity.pythonObjectAddress

        Frontend.InspectParsedUserInterface.ExpandableUITreeNodeChildren ->
            "Children"

        Frontend.InspectParsedUserInterface.ExpandableUITreeNodeDictEntries ->
            "DictEntries"

        Frontend.InspectParsedUserInterface.ExpandableUITreeNodeAllDisplayTexts ->
            "AllDisplayTexts"


viewTreeParsedUserInterface :
    Maybe InputRouteConfig
    -> Dict.Dict String UITreeNodeWithDisplayRegion
    -> ParsedUITreeViewState
    -> EveOnline.ParseUserInterface.ParsedUserInterface
    -> Html.Html Event
viewTreeParsedUserInterface maybeInputRouteConfig uiNodesWithDisplayRegion viewState parsedUserInterface =
    let
        nodeIsExpanded nodeId =
            viewState.expandedNodes |> List.member nodeId
    in
    renderTreeNodeFromParsedUserInterface
        (maybeInputRouteConfig |> Maybe.map inputRouteFromInputConfig)
        uiNodesWithDisplayRegion
        parsedUserInterface
        |> renderInteractiveTreeView
            UserInputParsedUISetTreeViewNodeIsExpanded
            nodeIsExpanded
            { focusedPath = viewState.focused
            , eventForFocus = UserInputFocusInParsedUI
            , setFocusEvent = UserInputNavigateToElement
            , htmlElementId = htmlElementIdFromParsedUIPathNode
            }
            []


htmlElementIdFromParsedUIPathNode : ParsedUITreeViewPathNode -> String
htmlElementIdFromParsedUIPathNode pathNode =
    case pathNode of
        NamedNode name ->
            "NamedNode_" ++ name

        IndexedNode index ->
            "IndexedNode_" ++ (index |> String.fromInt)

        UITreeNode uiTreeNode ->
            "UITreeNode_" ++ htmlElementIdFromUIPathNode uiTreeNode


{-| TODO: Consolidate implementation to get visual tree: Also use `getExpandedPartOfTree`.
-}
renderInteractiveTreeView :
    (List expandPathNode -> Bool -> event)
    -> (List expandPathNode -> Bool)
    ->
        { focusedPath : List expandPathNode
        , eventForFocus : List expandPathNode -> event
        , setFocusEvent : String -> event
        , htmlElementId : expandPathNode -> String
        }
    -> List expandPathNode
    -> TreeViewNode event expandPathNode
    -> Html.Html event
renderInteractiveTreeView eventFromNodeIdAndExpandedState nodeIsExpanded focusConfig parentPath treeNode =
    let
        htmlElementIdFromNodePath =
            List.map focusConfig.htmlElementId >> String.join "-"

        expandIconHtmlFromIsExpanded isExpanded =
            (if isExpanded then
                "ᐯ"

             else
                "ᐳ"
            )
                |> Html.text
                |> List.singleton
                |> Html.span [ HA.style "margin" "0.3em", HA.style "font-weight" "bold" ]

        maybeChildren =
            case treeNode.children of
                NoChildren ->
                    Nothing

                ExpandableChildren pathNodeId getChildren ->
                    case getChildren () of
                        [] ->
                            Nothing

                        notEmptyChildren ->
                            Just { pathNodeId = pathNodeId, children = notEmptyChildren }

        ( maybeExpandIconHtml, childrenHtml, ariaAttributes ) =
            case maybeChildren of
                Nothing ->
                    ( Nothing, Nothing, [ Html.Attributes.Aria.role "none" ] )

                Just childrenInfo ->
                    let
                        currentPath =
                            parentPath ++ [ childrenInfo.pathNodeId ]

                        currentNodeIsExpanded =
                            nodeIsExpanded currentPath

                        expandableButtonHtml =
                            [ expandIconHtmlFromIsExpanded currentNodeIsExpanded ]
                                |> Html.span [ HE.onClick (eventFromNodeIdAndExpandedState currentPath (not currentNodeIsExpanded)), HA.style "cursor" "pointer" ]

                        expandableChildrenHtml =
                            if currentNodeIsExpanded then
                                childrenInfo.children
                                    |> List.map
                                        (renderInteractiveTreeView
                                            eventFromNodeIdAndExpandedState
                                            nodeIsExpanded
                                            focusConfig
                                            currentPath
                                        )
                                    |> Html.ul [ HA.style "padding-inline-start" "1em", HA.style "margin-block-start" "0" ]
                                    |> Just

                            else
                                Nothing

                        ariaExpanded =
                            if currentNodeIsExpanded then
                                "true"

                            else
                                "false"

                        -- https://www.w3.org/TR/wai-aria-practices/#kbd_roving_tabindex
                        tabIndex =
                            if focusConfig.focusedPath == currentPath then
                                0

                            else
                                -1

                        keyEventListeners =
                            if parentPath /= [] then
                                []

                            else
                                let
                                    immediateNeighborsPaths =
                                        searchImmediateNeighborsPaths
                                            focusConfig.focusedPath
                                            (treeNode |> getExpandedPartOfTree nodeIsExpanded [])
                                            { currentPath = [], up = Nothing, down = Nothing, left = Nothing, previousSibling = Nothing }
                                            |> Maybe.withDefault { up = Nothing, down = Nothing, left = Nothing, right = Nothing }

                                    jsonDecodeMapUserInputArrowToEvent inputArrow =
                                        case inputArrow of
                                            ArrowLeft ->
                                                if nodeIsExpanded focusConfig.focusedPath then
                                                    Json.Decode.succeed (eventFromNodeIdAndExpandedState focusConfig.focusedPath False)

                                                else
                                                    case immediateNeighborsPaths.left of
                                                        Nothing ->
                                                            Json.Decode.fail "Path to left not available."

                                                        Just pathToLeft ->
                                                            Json.Decode.succeed (focusConfig.setFocusEvent (htmlElementIdFromNodePath pathToLeft))

                                            ArrowRight ->
                                                if not (nodeIsExpanded focusConfig.focusedPath) then
                                                    Json.Decode.succeed (eventFromNodeIdAndExpandedState focusConfig.focusedPath True)

                                                else
                                                    case immediateNeighborsPaths.right of
                                                        Nothing ->
                                                            Json.Decode.fail "Path to right not available."

                                                        Just pathToRight ->
                                                            Json.Decode.succeed (focusConfig.setFocusEvent (htmlElementIdFromNodePath pathToRight))

                                            ArrowUp ->
                                                case immediateNeighborsPaths.up of
                                                    Nothing ->
                                                        Json.Decode.fail "Path up not available."

                                                    Just pathUp ->
                                                        Json.Decode.succeed (focusConfig.setFocusEvent (htmlElementIdFromNodePath pathUp))

                                            ArrowDown ->
                                                case immediateNeighborsPaths.down of
                                                    Nothing ->
                                                        Json.Decode.fail "Path down not available."

                                                    Just pathDown ->
                                                        Json.Decode.succeed (focusConfig.setFocusEvent (htmlElementIdFromNodePath pathDown))
                                in
                                [ HE.custom "keydown"
                                    (HE.keyCode
                                        |> Json.Decode.andThen
                                            (arrowKeyTypeFromKeyCode
                                                >> Maybe.map Json.Decode.succeed
                                                >> Maybe.withDefault (Json.Decode.fail "No arrow key")
                                            )
                                        |> Json.Decode.andThen jsonDecodeMapUserInputArrowToEvent
                                        |> Json.Decode.map
                                            (\inputEvent ->
                                                { message = inputEvent
                                                , stopPropagation = True
                                                , preventDefault = True
                                                }
                                            )
                                    )
                                ]
                    in
                    ( Just expandableButtonHtml
                    , expandableChildrenHtml
                    , [ HA.id (htmlElementIdFromNodePath currentPath)
                      , Html.Attributes.Aria.role "treeitem"
                      , Html.Attributes.Aria.ariaExpanded ariaExpanded
                      , HA.tabindex tabIndex
                      , HE.onFocus (focusConfig.eventForFocus currentPath)
                      ]
                        ++ keyEventListeners
                    )

        expandIconHtml =
            maybeExpandIconHtml
                |> Maybe.withDefault ([ expandIconHtmlFromIsExpanded False ] |> Html.span [ HA.style "visibility" "hidden" ])

        -- TODO: Implement navigation using keyboard, probably arrow keys.
    in
    [ [ expandIconHtml, treeNode.selfHtml ] |> Html.span []
    , childrenHtml |> Maybe.withDefault (Html.text "")
    ]
        |> Html.li (HA.style "list-style" "none" :: ariaAttributes)


type VisualTreeChild event pathNode
    = VisualWithoutChildren
    | VisualCollapsed
    | VisualExpanded pathNode (List (VisualTreeChild event pathNode))


mapEmptyChildrenToNotExpandable : TreeViewNode event pathNode -> TreeViewNode event pathNode
mapEmptyChildrenToNotExpandable tree =
    let
        children =
            case tree.children of
                NoChildren ->
                    NoChildren

                ExpandableChildren currentNodeId getChildren ->
                    case getChildren () of
                        [] ->
                            NoChildren

                        nonEmptyChildren ->
                            ExpandableChildren currentNodeId
                                (always (nonEmptyChildren |> List.map mapEmptyChildrenToNotExpandable))
    in
    { tree | children = children }


getExpandedPartOfTree : (List pathNode -> Bool) -> List pathNode -> TreeViewNode event pathNode -> TreeViewNode event pathNode
getExpandedPartOfTree nodeIsExpanded fromParentPath tree =
    let
        children =
            case tree.children of
                NoChildren ->
                    NoChildren

                ExpandableChildren currentNodeId getChildren ->
                    let
                        currentPath =
                            fromParentPath ++ [ currentNodeId ]
                    in
                    if nodeIsExpanded currentPath then
                        ExpandableChildren currentNodeId
                            (always (getChildren () |> List.map (getExpandedPartOfTree nodeIsExpanded currentPath)))

                    else
                        ExpandableChildren currentNodeId (always [])
    in
    { tree | children = children }


searchImmediateNeighborsPaths :
    List pathNode
    -> TreeViewNode event pathNode
    -> { currentPath : List pathNode, up : Maybe (List pathNode), down : Maybe (List pathNode), left : Maybe (List pathNode), previousSibling : Maybe (TreeViewNode event pathNode) }
    -> Maybe { up : Maybe (List pathNode), down : Maybe (List pathNode), left : Maybe (List pathNode), right : Maybe (List pathNode) }
searchImmediateNeighborsPaths pathToSearch tree fromParent =
    -- TODO: Check if impl can be simplified by using List of TreeViewNode instead of single one in 'tree'.
    case tree.children of
        NoChildren ->
            Nothing

        ExpandableChildren currentNodeId getVisualChildren ->
            let
                currentPath =
                    fromParent.currentPath ++ [ currentNodeId ]
            in
            case pathToSearch of
                pathToSearchFirstNode :: remainingPathToSearch ->
                    if pathToSearchFirstNode /= currentNodeId then
                        Nothing

                    else if remainingPathToSearch == [] then
                        let
                            pathDownFromExpandedContentNodeId =
                                let
                                    focusableChildren =
                                        case tree.children of
                                            NoChildren ->
                                                []

                                            ExpandableChildren _ getChildren ->
                                                getChildren ()
                                                    |> List.filterMap
                                                        (\candidate ->
                                                            case candidate.children of
                                                                NoChildren ->
                                                                    Nothing

                                                                ExpandableChildren childNodeId _ ->
                                                                    Just childNodeId
                                                        )
                                in
                                focusableChildren |> List.head

                            pathDownFromExpandedContent =
                                pathDownFromExpandedContentNodeId
                                    |> Maybe.map (List.singleton >> (++) currentPath)
                        in
                        Just
                            { up = fromParent.up
                            , down = pathDownFromExpandedContent |> maybeWithMaybeDefault fromParent.down
                            , left = fromParent.left
                            , right = pathDownFromExpandedContent
                            }

                    else
                        let
                            visualChildren =
                                getVisualChildren ()

                            focusableChildren =
                                visualChildren
                                    |> List.filter
                                        (\candidate ->
                                            case candidate.children of
                                                NoChildren ->
                                                    False

                                                ExpandableChildren _ _ ->
                                                    True
                                        )

                            getChildFromFocusableChildIndex childIndex =
                                -- TODO: make each visual children focusable, not only the expandable ones.
                                case focusableChildren |> List.Extra.getAt childIndex of
                                    Nothing ->
                                        Nothing

                                    Just child ->
                                        case child.children of
                                            NoChildren ->
                                                Nothing

                                            ExpandableChildren childPathNodeId _ ->
                                                Just ( child, currentPath ++ [ childPathNodeId ] )
                        in
                        focusableChildren
                            |> List.indexedMap
                                (\childIndex child ->
                                    let
                                        visualNextUpperPath =
                                            getChildFromFocusableChildIndex (childIndex - 1)
                                                |> Maybe.map Tuple.second
                                                |> maybeWithMaybeDefault (Just currentPath)

                                        visualNextLowerPath =
                                            getChildFromFocusableChildIndex (childIndex + 1)
                                                |> Maybe.map Tuple.second
                                                |> maybeWithMaybeDefault fromParent.down
                                    in
                                    searchImmediateNeighborsPaths
                                        remainingPathToSearch
                                        child
                                        { currentPath = currentPath
                                        , up = visualNextUpperPath
                                        , down = visualNextLowerPath
                                        , left = Just currentPath
                                        , previousSibling = getChildFromFocusableChildIndex (childIndex - 1) |> Maybe.map Tuple.first
                                        }
                                )
                            |> List.filterMap identity
                            |> List.head

                _ ->
                    Nothing


viewUITreeSvg : UITreeNodeWithDisplayRegion -> Svg.Svg e
viewUITreeSvg uiTree =
    let
        viewBox =
            [ uiTree.totalDisplayRegion.x
            , uiTree.totalDisplayRegion.y
            , uiTree.totalDisplayRegion.width
            , uiTree.totalDisplayRegion.height
            ]
                |> List.map String.fromInt
                |> String.join " "
    in
    Svg.svg
        [ Svg.Attributes.viewBox viewBox
        , HA.style "background" "#111"
        , HA.style "font-size" "60%"
        ]
        [ uiTree |> svgFromUINodeRecursive ]


svgFromUINodeRecursive : UITreeNodeWithDisplayRegion -> Svg.Svg e
svgFromUINodeRecursive uiNode =
    let
        childrenSvg =
            uiNode.children
                |> Maybe.withDefault []
                |> List.filterMap
                    (\child ->
                        case child of
                            EveOnline.ParseUserInterface.ChildWithRegion childWithRegion ->
                                Just childWithRegion

                            EveOnline.ParseUserInterface.ChildWithoutRegion _ ->
                                Nothing
                    )
                |> List.map svgFromUINodeRecursive

        displayTextSvg =
            case uiNode.uiNode |> EveOnline.ParseUserInterface.getDisplayText of
                Nothing ->
                    Html.text ""

                Just displayText ->
                    Svg.text_
                        [ Svg.Attributes.textLength (uiNode.selfDisplayRegion.width |> String.fromInt)
                        , Svg.Attributes.lengthAdjust "spacing"
                        , HA.style "fill" "grey"
                        , Svg.Attributes.x ((uiNode.selfDisplayRegion.width // 2) |> String.fromInt)
                        , Svg.Attributes.y ((uiNode.selfDisplayRegion.height // 2) |> String.fromInt)
                        , Svg.Attributes.dominantBaseline "middle"
                        , Svg.Attributes.textAnchor "middle"
                        ]
                        [ Svg.text displayText ]

        regionRectPlacementAttributes =
            [ Svg.Attributes.x "0"
            , Svg.Attributes.y "0"
            , Svg.Attributes.width (uiNode.selfDisplayRegion.width |> String.fromInt)
            , Svg.Attributes.height (uiNode.selfDisplayRegion.height |> String.fromInt)
            ]

        regionSvg =
            Svg.rect
                (regionRectPlacementAttributes
                    ++ [ HA.style "fill" "transparent"
                       , HA.style "stroke-width" "1"
                       , HA.style "stroke" "#7AB8FF"
                       , HA.style "stroke-opacity" "0.3"
                       ]
                )
                []

        colorIndicationSvg =
            case uiNode.uiNode |> EveOnline.ParseUserInterface.getColorPercentFromDictEntries of
                Nothing ->
                    Html.text ""

                Just colorPercent ->
                    Svg.rect
                        (regionRectPlacementAttributes
                            ++ [ Svg.Attributes.height (uiNode.selfDisplayRegion.height |> String.fromInt)
                               , HA.style "fill" "transparent"
                               , HA.style "stroke-width" "3"
                               , HA.style "stroke" (cssColorFromColorPercent colorPercent)
                               , HA.style "stroke-opacity" "0.5"
                               ]
                        )
                        []

        transformTranslateText =
            [ uiNode.selfDisplayRegion.x, uiNode.selfDisplayRegion.y ]
                |> List.map String.fromInt
                |> String.join " "
    in
    Svg.g [ Svg.Attributes.transform ("translate(" ++ transformTranslateText ++ ")") ]
        (regionSvg :: colorIndicationSvg :: displayTextSvg :: childrenSvg)


type ArrowKeyType
    = ArrowUp
    | ArrowDown
    | ArrowLeft
    | ArrowRight


arrowKeyTypeFromKeyCode : Int -> Maybe ArrowKeyType
arrowKeyTypeFromKeyCode keyCode =
    [ ( 38, ArrowUp )
    , ( 40, ArrowDown )
    , ( 37, ArrowLeft )
    , ( 39, ArrowRight )
    ]
        |> Dict.fromList
        |> Dict.get keyCode


parseMemoryReadingFromJson : String -> Result Json.Decode.Error ParseMemoryReadingSuccess
parseMemoryReadingFromJson =
    EveOnline.MemoryReading.decodeMemoryReadingFromString
        >> Result.map
            (\uiTree ->
                let
                    uiTreeWithDisplayRegion =
                        uiTree |> EveOnline.ParseUserInterface.parseUITreeWithDisplayRegionFromUITree

                    parsedUserInterface =
                        EveOnline.ParseUserInterface.parseUserInterfaceFromUITree uiTreeWithDisplayRegion
                in
                { uiTree = uiTree
                , uiNodesWithDisplayRegion =
                    uiTreeWithDisplayRegion
                        :: (uiTreeWithDisplayRegion |> EveOnline.ParseUserInterface.listDescendantsWithDisplayRegion)
                        |> List.map (\uiNodeWithRegion -> ( uiNodeWithRegion.uiNode.pythonObjectAddress, uiNodeWithRegion ))
                        |> Dict.fromList
                , overviewWindows = parsedUserInterface.overviewWindows |> List.map parseOverviewWindow
                , parsedUserInterface = parsedUserInterface
                }
            )


parseOverviewWindow : EveOnline.ParseUserInterface.OverviewWindow -> OverviewWindow
parseOverviewWindow overviewWindow =
    let
        mapEntry originalEntry =
            { uiTreeNode = originalEntry.uiNode
            , cellsContents = originalEntry.cellsTexts
            , iconSpriteColorPercent = originalEntry.iconSpriteColorPercent
            , bgColorFillsPercent = originalEntry.bgColorFillsPercent
            }

        headers =
            overviewWindow.entriesHeaders
                |> List.sortBy (Tuple.second >> .totalDisplayRegion >> .x)
                |> List.map Tuple.first

        entries =
            overviewWindow.entries
                |> List.sortBy (.uiNode >> .totalDisplayRegion >> .y)
                |> List.map mapEntry
    in
    { headers = headers
    , entries = entries
    }


globalStylesHtmlElement : Html.Html a
globalStylesHtmlElement =
    """
body {
font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
margin: 1em;
}
"""
        |> Html.text
        |> List.singleton
        |> Html.node "style" []


verticalSpacerFromHeightInEm : Float -> Html.Html a
verticalSpacerFromHeightInEm heightInEm =
    [] |> Html.div [ HA.style "height" ((heightInEm |> String.fromFloat) ++ "em") ]


httpExpectJson : (Result { error : Http.Error, bodyString : Maybe String } a -> msg) -> Json.Decode.Decoder a -> Http.Expect msg
httpExpectJson toMsg decoder =
    Http.expectStringResponse toMsg <|
        \response ->
            case response of
                Http.BadUrl_ url ->
                    Err { error = Http.BadUrl url, bodyString = Nothing }

                Http.Timeout_ ->
                    Err { error = Http.Timeout, bodyString = Nothing }

                Http.NetworkError_ ->
                    Err { error = Http.NetworkError, bodyString = Nothing }

                Http.BadStatus_ metadata body ->
                    Err { error = Http.BadStatus metadata.statusCode, bodyString = Just body }

                Http.GoodStatus_ metadata body ->
                    case Json.Decode.decodeString decoder body of
                        Ok value ->
                            Ok value

                        Err err ->
                            Err { error = Http.BadBody (Json.Decode.errorToString err), bodyString = Just body }


describeHttpError : HttpRequestErrorStructure -> String
describeHttpError { error, bodyString } =
    case error of
        Http.BadUrl errorMessage ->
            "Bad Url: " ++ errorMessage

        Http.Timeout ->
            "Timeout"

        Http.NetworkError ->
            "Network Error"

        Http.BadStatus statusCode ->
            "BadStatus: "
                ++ (statusCode |> String.fromInt)
                ++ " ("
                ++ (bodyString |> Maybe.withDefault "No details in HTTP response body.")
                ++ ")"

        Http.BadBody errorMessage ->
            "BadPayload: " ++ errorMessage


linkHtmlFromUrl : String -> Html.Html a
linkHtmlFromUrl url =
    Html.a [ HA.href url ] [ Html.text url ]


maybeWithMaybeDefault : Maybe a -> Maybe a -> Maybe a
maybeWithMaybeDefault maybeB maybeA =
    case maybeA of
        Just a ->
            Just a

        Nothing ->
            maybeB
