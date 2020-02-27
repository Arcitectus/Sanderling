module FrontendWeb.Main exposing (Event(..), State, init, main, update, view)

import Browser
import Browser.Dom
import Browser.Navigation as Navigation
import Dict
import EveOnline.MemoryReading
    exposing
        ( MaybeVisible(..)
        , UITreeNodeWithDisplayRegion
        , maybeVisibleMap
        )
import EveOnline.VolatileHostInterface
import File
import File.Download
import File.Select
import FrontendWeb.InspectParsedUserInterface
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
import Set
import Task
import Time
import Url


versionId : String
versionId =
    "2020-02-27"


{-| 2020-01-29 Observation: In this case, I used the alternate UI on the same desktop as the game client. When using a mouse button to click the HTML button, it seemed like sometimes that click interfered with the click on the game client. Using keyboard input on the web page might be sufficient to avoid this issue.
-}
inputDelayDefaultMilliseconds : Int
inputDelayDefaultMilliseconds =
    300


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
    { readEveOnlineClientProcessesIdsResult : Maybe (Result String (Set.Set Int))
    , searchUIRootAddressResult : Maybe ( EveOnline.VolatileHostInterface.SearchUIRootAddressStructure, Result String EveOnline.VolatileHostInterface.SearchUIRootAddressResultStructure )
    , readMemoryResult : Maybe (Result String ReadFromLiveProcessCompleted)
    , lastPendingRequestToReadMemoryTimeMilli : Maybe Int
    }


type alias ReadFromLiveProcessCompleted =
    { mainWindowId : String
    , memoryReading : ParseMemoryReadingCompleted
    }


type alias ParseMemoryReadingCompleted =
    { serialRepresentationJson : String
    , parseResult : Result Json.Decode.Error ParseMemoryReadingSuccess
    }


type alias ParseMemoryReadingSuccess =
    { uiTree : EveOnline.MemoryReading.UITreeNode
    , uiNodesWithDisplayRegion : Dict.Dict String UITreeNodeWithDisplayRegion
    , overviewWindow : MaybeVisible OverviewWindow
    , parsedUserInterface : EveOnline.MemoryReading.ParsedUserInterface
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
    { uiNode : EveOnline.MemoryReading.UITreeNodeWithDisplayRegion
    , input : InputOnUINode
    , windowId : EveOnline.VolatileHostInterface.WindowId
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
    = RunInVolatileHostResponse InterfaceToFrontendClient.RunInVolatileHostResponseStructure
    | ReadLogResponse (List String)


type alias OverviewWindow =
    { headers : List String
    , entries : List OverviewEntry
    }


type alias OverviewEntry =
    { uiTreeNode : EveOnline.MemoryReading.UITreeNodeWithDisplayRegion
    , cellsContents : Dict.Dict String String
    , iconSpriteColorPercent : Maybe EveOnline.MemoryReading.ColorComponents
    }


type alias InputRouteConfig =
    { windowId : EveOnline.VolatileHostInterface.WindowId
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
        { readEveOnlineClientProcessesIdsResult = Nothing
        , searchUIRootAddressResult = Nothing
        , readMemoryResult = Nothing
        , lastPendingRequestToReadMemoryTimeMilli = Nothing
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

                InterfaceToFrontendClient.RunInVolatileHostRequest _ ->
                    InterfaceToFrontendClient.jsonDecodeRunInVolatileHostResponseStructure
                        |> Json.Decode.map RunInVolatileHostResponse
    in
    Http.post
        { url = "/api/"
        , expect = httpExpectJson (\result -> BackendResponse { request = request, result = result }) responseDecoder
        , body = Http.jsonBody (request |> InterfaceToFrontendClient.jsonEncodeRequestFromClient)
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
                            sendInput.uiNode.totalDisplayRegion |> EveOnline.MemoryReading.centerFromDisplayRegion

                        volatileHostInterfaceTaskOnWindow =
                            case sendInput.input of
                                MouseClickLeft ->
                                    EveOnline.VolatileHostInterface.SimpleMouseClickAtLocation
                                        { location = uiNodeCenter, mouseButton = EveOnline.VolatileHostInterface.MouseButtonLeft }

                                MouseClickRight ->
                                    EveOnline.VolatileHostInterface.SimpleMouseClickAtLocation
                                        { location = uiNodeCenter, mouseButton = EveOnline.VolatileHostInterface.MouseButtonRight }

                        requestSendInputToGameClient =
                            apiRequestCmd
                                (InterfaceToFrontendClient.RunInVolatileHostRequest
                                    (EveOnline.VolatileHostInterface.EffectOnWindow
                                        { windowId = sendInput.windowId
                                        , bringWindowToForeground = True
                                        , task = volatileHostInterfaceTaskOnWindow
                                        }
                                    )
                                )

                        -- TODO: Remember sending input, to syncronize with get next memory reading.
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


integrateBackendResponse : { request : InterfaceToFrontendClient.RequestFromClient, result : Result HttpRequestErrorStructure ResponseFromServer } -> State -> State
integrateBackendResponse { request, result } stateBefore =
    case request of
        -- TODO: Consolidate unpack response common parts.
        InterfaceToFrontendClient.RunInVolatileHostRequest EveOnline.VolatileHostInterface.GetEveOnlineProcessesIds ->
            let
                getEveOnlineClientProcessesIdsResult =
                    result
                        |> Result.mapError describeHttpError
                        |> Result.andThen
                            (\response ->
                                case response of
                                    ReadLogResponse _ ->
                                        Err "Unexpected response"

                                    RunInVolatileHostResponse runInVolatileHostResponse ->
                                        case runInVolatileHostResponse of
                                            InterfaceToFrontendClient.SetupNotCompleteResponse status ->
                                                Err ("Volatile host setup not complete: " ++ status)

                                            InterfaceToFrontendClient.RunInVolatileHostCompleteResponse runInVolatileHostCompleteResponse ->
                                                case runInVolatileHostCompleteResponse.exceptionToString of
                                                    Just exception ->
                                                        Err ("Failed with exception: " ++ exception)

                                                    Nothing ->
                                                        runInVolatileHostCompleteResponse.returnValueToString
                                                            |> Maybe.withDefault ""
                                                            |> EveOnline.VolatileHostInterface.deserializeResponseFromVolatileHost
                                                            |> Result.mapError Json.Decode.errorToString
                                                            |> Result.andThen
                                                                (\responseFromVolatileHost ->
                                                                    case responseFromVolatileHost of
                                                                        EveOnline.VolatileHostInterface.EveOnlineProcessesIds processIds ->
                                                                            Ok (processIds |> Set.fromList)

                                                                        EveOnline.VolatileHostInterface.SearchUIRootAddressResult _ ->
                                                                            Err "Unexpected response: SearchUIRootAddressResult"

                                                                        EveOnline.VolatileHostInterface.GetMemoryReadingResult _ ->
                                                                            Err "Unexpected response: GetMemoryReadingResult"
                                                                )
                            )

                readFromLiveProcessBefore =
                    stateBefore.readFromLiveProcess
            in
            { stateBefore
                | readFromLiveProcess =
                    { readFromLiveProcessBefore
                        | readEveOnlineClientProcessesIdsResult = Just getEveOnlineClientProcessesIdsResult
                    }
            }

        InterfaceToFrontendClient.RunInVolatileHostRequest (EveOnline.VolatileHostInterface.GetMemoryReading _) ->
            let
                readMemoryResult =
                    result
                        |> Result.mapError describeHttpError
                        |> Result.andThen
                            (\response ->
                                case response of
                                    ReadLogResponse _ ->
                                        Err "Unexpected response"

                                    RunInVolatileHostResponse runInVolatileHostResponse ->
                                        case runInVolatileHostResponse of
                                            InterfaceToFrontendClient.SetupNotCompleteResponse status ->
                                                Err ("Volatile host setup not complete: " ++ status)

                                            InterfaceToFrontendClient.RunInVolatileHostCompleteResponse runInVolatileHostCompleteResponse ->
                                                case runInVolatileHostCompleteResponse.exceptionToString of
                                                    Just exception ->
                                                        Err ("Failed with exception: " ++ exception)

                                                    Nothing ->
                                                        runInVolatileHostCompleteResponse.returnValueToString
                                                            |> Maybe.withDefault ""
                                                            |> EveOnline.VolatileHostInterface.deserializeResponseFromVolatileHost
                                                            |> Result.mapError Json.Decode.errorToString
                                                            |> Result.andThen
                                                                (\responseFromVolatileHost ->
                                                                    case responseFromVolatileHost of
                                                                        EveOnline.VolatileHostInterface.EveOnlineProcessesIds _ ->
                                                                            Err "Unexpected response: EveOnlineProcessesIds"

                                                                        EveOnline.VolatileHostInterface.SearchUIRootAddressResult _ ->
                                                                            Err "Unexpected response: SearchUIRootAddressResult"

                                                                        EveOnline.VolatileHostInterface.GetMemoryReadingResult getMemoryReadingResult ->
                                                                            case getMemoryReadingResult of
                                                                                EveOnline.VolatileHostInterface.ProcessNotFound ->
                                                                                    Err "Process not found"

                                                                                EveOnline.VolatileHostInterface.Completed memoryReadingCompleted ->
                                                                                    Ok memoryReadingCompleted
                                                                )
                            )
                        |> Result.andThen
                            (\memoryReadingCompleted ->
                                case memoryReadingCompleted.serialRepresentationJson of
                                    Nothing ->
                                        Err "Memory reading completed, but 'serialRepresentationJson' is null. Please configure EVE Online client and restart."

                                    Just serialRepresentationJson ->
                                        Ok
                                            { mainWindowId = memoryReadingCompleted.mainWindowId
                                            , memoryReading =
                                                { serialRepresentationJson = serialRepresentationJson
                                                , parseResult = serialRepresentationJson |> parseMemoryReadingFromJson
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
                        , lastPendingRequestToReadMemoryTimeMilli = Nothing
                    }
            }

        InterfaceToFrontendClient.RunInVolatileHostRequest (EveOnline.VolatileHostInterface.SearchUIRootAddress searchUIRootRequest) ->
            let
                searchUIRootResult =
                    result
                        |> Result.mapError describeHttpError
                        |> Result.andThen
                            (\response ->
                                case response of
                                    ReadLogResponse _ ->
                                        Err "Unexpected response"

                                    RunInVolatileHostResponse runInVolatileHostResponse ->
                                        case runInVolatileHostResponse of
                                            InterfaceToFrontendClient.SetupNotCompleteResponse status ->
                                                Err ("Volatile host setup not complete: " ++ status)

                                            InterfaceToFrontendClient.RunInVolatileHostCompleteResponse runInVolatileHostCompleteResponse ->
                                                case runInVolatileHostCompleteResponse.exceptionToString of
                                                    Just exception ->
                                                        Err ("Failed with exception: " ++ exception)

                                                    Nothing ->
                                                        runInVolatileHostCompleteResponse.returnValueToString
                                                            |> Maybe.withDefault ""
                                                            |> EveOnline.VolatileHostInterface.deserializeResponseFromVolatileHost
                                                            |> Result.mapError Json.Decode.errorToString
                                                            |> Result.andThen
                                                                (\responseFromVolatileHost ->
                                                                    case responseFromVolatileHost of
                                                                        EveOnline.VolatileHostInterface.EveOnlineProcessesIds _ ->
                                                                            Err "Unexpected response: EveOnlineProcessesIds"

                                                                        EveOnline.VolatileHostInterface.SearchUIRootAddressResult searchUIRootAddressResult ->
                                                                            Ok searchUIRootAddressResult

                                                                        EveOnline.VolatileHostInterface.GetMemoryReadingResult _ ->
                                                                            Err "Unexpected response: GetMemoryReadingResult"
                                                                )
                            )

                readFromLiveProcessBefore =
                    stateBefore.readFromLiveProcess
            in
            { stateBefore
                | readFromLiveProcess =
                    { readFromLiveProcessBefore
                        | searchUIRootAddressResult = Just ( searchUIRootRequest, searchUIRootResult )
                        , lastPendingRequestToReadMemoryTimeMilli = Nothing
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
        requestGetProcessesIds =
            apiRequestCmd (InterfaceToFrontendClient.RunInVolatileHostRequest EveOnline.VolatileHostInterface.GetEveOnlineProcessesIds)
    in
    case stateBefore.searchUIRootAddressResult of
        Nothing ->
            case stateBefore.readEveOnlineClientProcessesIdsResult of
                Nothing ->
                    ( stateBefore
                    , { describeState = "Did not yet search for the IDs of the EVE Online client processes."
                      , lastMemoryReading = Nothing
                      , nextCmd = requestGetProcessesIds
                      }
                    )

                Just (Err error) ->
                    ( stateBefore
                    , { describeState = "Failed to get IDs of the EVE Online client processes: " ++ error
                      , lastMemoryReading = Nothing
                      , nextCmd = requestGetProcessesIds
                      }
                    )

                Just (Ok eveOnlineClientProcessesIds) ->
                    case eveOnlineClientProcessesIds |> Set.toList of
                        [] ->
                            ( stateBefore
                            , { describeState = "Looks like there is no EVE Online client process started. I continue looking in case one is started..."
                              , lastMemoryReading = Nothing
                              , nextCmd = requestGetProcessesIds
                              }
                            )

                        eveOnlineProcessId :: _ ->
                            let
                                requestSearchUIRoot =
                                    apiRequestCmd
                                        (InterfaceToFrontendClient.RunInVolatileHostRequest
                                            (EveOnline.VolatileHostInterface.SearchUIRootAddress { processId = eveOnlineProcessId })
                                        )

                                searchStillPending =
                                    stateBefore.lastPendingRequestToReadMemoryTimeMilli
                                        |> Maybe.map (\pendingReadingTimeMilli -> timeMilli < pendingReadingTimeMilli + 10000)
                                        |> Maybe.withDefault False

                                ( state, nextCmd ) =
                                    if searchStillPending then
                                        ( stateBefore, Cmd.none )

                                    else
                                        ( { stateBefore | lastPendingRequestToReadMemoryTimeMilli = Just timeMilli }, requestSearchUIRoot )
                            in
                            ( state
                            , { describeState = "Search the address of the UI root in process " ++ (eveOnlineProcessId |> String.fromInt)
                              , lastMemoryReading = Nothing
                              , nextCmd = nextCmd
                              }
                            )

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

        Just ( _, Ok searchUIRootAddressResult ) ->
            case searchUIRootAddressResult.uiRootAddress of
                Nothing ->
                    ( stateBefore
                    , { describeState = "Did not find the UI root in process " ++ (searchUIRootAddressResult.processId |> String.fromInt)
                      , lastMemoryReading = Nothing
                      , nextCmd = Cmd.none
                      }
                    )

                Just uiRootAddress ->
                    let
                        requestReadMemory =
                            apiRequestCmd
                                (InterfaceToFrontendClient.RunInVolatileHostRequest
                                    (EveOnline.VolatileHostInterface.GetMemoryReading
                                        { processId = searchUIRootAddressResult.processId, uiRootAddress = uiRootAddress }
                                    )
                                )

                        ( describeLastReadResult, lastMemoryReading ) =
                            case stateBefore.readMemoryResult of
                                Nothing ->
                                    ( "", Nothing )

                                Just (Err error) ->
                                    ( "The last attempt to read from the process memory failed: " ++ error, Nothing )

                                Just (Ok lastMemoryReadingCompleted) ->
                                    ( "The last attempt to read from the process memory was successful.", Just lastMemoryReadingCompleted )

                        memoryReadingStillPending =
                            stateBefore.lastPendingRequestToReadMemoryTimeMilli
                                |> Maybe.map (\pendingReadingTimeMilli -> timeMilli < pendingReadingTimeMilli + 10000)
                                |> Maybe.withDefault False

                        ( state, nextCmd ) =
                            if memoryReadingStillPending then
                                ( stateBefore, Cmd.none )

                            else
                                ( { stateBefore | lastPendingRequestToReadMemoryTimeMilli = Just timeMilli }, requestReadMemory )
                    in
                    ( state
                    , { describeState =
                            "I try to read the memory from process "
                                ++ (searchUIRootAddressResult.processId |> String.fromInt)
                                ++ " starting from root address "
                                ++ uiRootAddress
                                ++ ". "
                                ++ describeLastReadResult
                      , nextCmd = nextCmd
                      , lastMemoryReading = lastMemoryReading
                      }
                    )


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
            , selectSourceHtml state
            , verticalSpacerFromHeightInEm 1
            , [ ("Reading from " ++ selectedSourceText) |> Html.text ] |> Html.h3 []
            , sourceSpecificHtml
            ]
    in
    { title = "Alternate EVE Online UI version " ++ versionId, body = body }


viewSourceFromFile : State -> Html.Html Event
viewSourceFromFile state =
    let
        buttonLoadFromFileHtml =
            [ "Click here to load a memory reading from a JSON file" |> Html.text ]
                |> Html.button [ HE.onClick (UserInputSelectMemoryReadingFile Nothing) ]

        memoryReadingFromFileHtml =
            case state.readFromFileResult of
                Nothing ->
                    "No memory reading loaded" |> Html.text

                Just memoryReadingCompleted ->
                    case memoryReadingCompleted.parseResult of
                        Err error ->
                            ("Failed to decode memory reading loaded from file: " ++ (error |> Json.Decode.errorToString)) |> Html.text

                        Ok parseSuccess ->
                            [ "Successfully read the memory reading from the file." |> Html.text
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
                            [ "Click here to download this memory reading to a JSON file." |> Html.text ]
                                |> Html.button [ HE.onClick (UserInputDownloadJsonFile parsedReadMemoryResult.memoryReading.serialRepresentationJson) ]

                        inputRoute =
                            { windowId = parsedReadMemoryResult.mainWindowId }

                        parsedHtml =
                            case parsedReadMemoryResult.memoryReading.parseResult of
                                Err parseError ->
                                    ("Failed to parse this memory reading: " ++ (parseError |> Json.Decode.errorToString)) |> Html.text

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
                        [ [ "Overview" |> Html.text ] |> Html.h3 []
                        , displayReadOverviewWindowResult maybeInputRoute memoryReading.overviewWindow
                        , verticalSpacerFromHeightInEm 0.5
                        , [ ((memoryReading.parsedUserInterface.contextMenus |> List.length |> String.fromInt) ++ " Context menus") |> Html.text ] |> Html.h3 []
                        , displayParsedContextMenus maybeInputRoute memoryReading.parsedUserInterface.contextMenus
                        ]

                ViewUITree ->
                    continueWithTitle
                        "Inspecting the UI tree"
                        [ "Below is an interactive tree view to explore this memory reading. You can expand and collapse individual nodes." |> Html.text
                        , viewTreeMemoryReadingUITreeNode maybeInputRoute memoryReading.uiNodesWithDisplayRegion state.uiTreeView memoryReading.uiTree
                        ]

                ViewParsedUI ->
                    continueWithTitle
                        "Inspecting the parsed user interface"
                        [ "Below is an interactive tree view to explore the parsed user interface. You can expand and collapse individual nodes." |> Html.text
                        , viewTreeParsedUserInterface maybeInputRoute memoryReading.uiNodesWithDisplayRegion state.parsedUITreeView memoryReading.parsedUserInterface
                        ]
    in
    [ selectViewModeHtml state
    , selectedViewHtml
        |> List.map (List.singleton >> Html.div [])
        |> Html.div []
    ]
        |> Html.div []


displayReadOverviewWindowResult : Maybe InputRouteConfig -> MaybeVisible OverviewWindow -> Html.Html Event
displayReadOverviewWindowResult maybeInputRouteConfig maybeOverviewWindow =
    case maybeOverviewWindow of
        CanNotSeeIt ->
            "Can not see the overview window" |> Html.text

        CanSee overviewWindow ->
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

                cssColorFromColorPercent colorPercent =
                    "rgba("
                        ++ (([ colorPercent.r, colorPercent.g, colorPercent.b ]
                                |> List.map (\rgbComponent -> String.fromInt ((rgbComponent * 255) // 100))
                            )
                                ++ [ String.fromFloat ((colorPercent.a |> toFloat) / 100) ]
                                |> String.join ","
                           )
                        ++ ")"

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
                                    columnsHtml =
                                        columns
                                            |> List.map
                                                (\column -> [ overviewEntry |> column.cellHtmlFromEntry ] |> Html.td [])

                                    inputHtml =
                                        maybeInputOfferHtml (maybeInputRouteConfig |> Maybe.map inputRouteFromInputConfig) [ MouseClickLeft, MouseClickRight ] overviewEntry.uiTreeNode
                                in
                                (columnsHtml ++ [ inputHtml ]) |> Html.tr []
                            )
            in
            headersHtml
                :: entriesHtml
                |> Html.table []


displayParsedContextMenus : Maybe InputRouteConfig -> List EveOnline.MemoryReading.ContextMenu -> Html.Html Event
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


displayParsedContextMenu : Maybe InputRouteConfig -> EveOnline.MemoryReading.ContextMenu -> Html.Html Event
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
    (([ "Select a source for the memory reading" |> Html.text ] |> Html.legend [])
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


inputRouteFromInputConfig : InputRouteConfig -> FrontendWeb.InspectParsedUserInterface.InputRoute Event
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
        FrontendWeb.InspectParsedUserInterface.ExpandableUITreeNode nodeIdentity ->
            "UITreeNode_" ++ nodeIdentity.pythonObjectAddress

        FrontendWeb.InspectParsedUserInterface.ExpandableUITreeNodeChildren ->
            "Children"

        FrontendWeb.InspectParsedUserInterface.ExpandableUITreeNodeDictEntries ->
            "DictEntries"

        FrontendWeb.InspectParsedUserInterface.ExpandableUITreeNodeAllDisplayTexts ->
            "AllDisplayTexts"


viewTreeParsedUserInterface :
    Maybe InputRouteConfig
    -> Dict.Dict String UITreeNodeWithDisplayRegion
    -> ParsedUITreeViewState
    -> EveOnline.MemoryReading.ParsedUserInterface
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
                        uiTree |> EveOnline.MemoryReading.parseUITreeWithDisplayRegionFromUITree

                    parsedUserInterface =
                        EveOnline.MemoryReading.parseUserInterfaceFromUITree uiTreeWithDisplayRegion
                in
                { uiTree = uiTree
                , uiNodesWithDisplayRegion =
                    uiTreeWithDisplayRegion
                        :: (uiTreeWithDisplayRegion |> EveOnline.MemoryReading.listDescendantsWithDisplayRegion)
                        |> List.map (\uiNodeWithRegion -> ( uiNodeWithRegion.uiNode.pythonObjectAddress, uiNodeWithRegion ))
                        |> Dict.fromList
                , overviewWindow = parsedUserInterface.overviewWindow |> maybeVisibleMap parseOverviewWindow
                , parsedUserInterface = parsedUserInterface
                }
            )


parseOverviewWindow : EveOnline.MemoryReading.OverviewWindow -> OverviewWindow
parseOverviewWindow overviewWindow =
    let
        mapEntry originalEntry =
            { uiTreeNode = originalEntry.uiNode
            , cellsContents = originalEntry.cellsTexts
            , iconSpriteColorPercent = originalEntry.iconSpriteColorPercent
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


maybeWithMaybeDefault : Maybe a -> Maybe a -> Maybe a
maybeWithMaybeDefault maybeB maybeA =
    case maybeA of
        Just a ->
            Just a

        Nothing ->
            maybeB
