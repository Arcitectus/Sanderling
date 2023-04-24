module EveOnline.ParseUserInterface exposing (..)

{-| A library of building blocks to build programs that read from the EVE Online game client.

The EVE Online client's UI tree can contain thousands of nodes and tens of thousands of individual properties. Because of this large amount of data, navigating in there can be time-consuming.

This library helps us navigate the UI tree with functions to filter out redundant data and extract the interesting bits.

The types in this module provide names more closely related to players' experience, such as the overview window or ship modules.

To learn about the user interface structures in the EVE Online game client, see the guide at <https://to.botlab.org/guide/parsed-user-interface-of-the-eve-online-game-client>

-}

import Common.EffectOnWindow
import Dict
import EveOnline.MemoryReading
import Json.Decode
import List.Extra
import Maybe.Extra
import Regex
import Result.Extra
import Set


type alias ParsedUserInterface =
    { uiTree : UITreeNodeWithDisplayRegion
    , contextMenus : List ContextMenu
    , shipUI : Maybe ShipUI
    , targets : List Target
    , infoPanelContainer : Maybe InfoPanelContainer
    , overviewWindows : List OverviewWindow
    , selectedItemWindow : Maybe SelectedItemWindow
    , dronesWindow : Maybe DronesWindow
    , fittingWindow : Maybe FittingWindow
    , probeScannerWindow : Maybe ProbeScannerWindow
    , directionalScannerWindow : Maybe DirectionalScannerWindow
    , stationWindow : Maybe StationWindow
    , inventoryWindows : List InventoryWindow
    , chatWindowStacks : List ChatWindowStack
    , agentConversationWindows : List AgentConversationWindow
    , marketOrdersWindow : Maybe MarketOrdersWindow
    , surveyScanWindow : Maybe SurveyScanWindow
    , bookmarkLocationWindow : Maybe BookmarkLocationWindow
    , repairShopWindow : Maybe RepairShopWindow
    , characterSheetWindow : Maybe CharacterSheetWindow
    , fleetWindow : Maybe FleetWindow
    , watchListPanel : Maybe WatchListPanel
    , standaloneBookmarkWindow : Maybe StandaloneBookmarkWindow
    , moduleButtonTooltip : Maybe ModuleButtonTooltip
    , heatStatusTooltip : Maybe HeatStatusTooltip
    , neocom : Maybe Neocom
    , messageBoxes : List MessageBox
    , layerAbovemain : Maybe LayerAbovemain
    , keyActivationWindow : Maybe KeyActivationWindow
    , compressionWindow : Maybe CompressionWindow
    }


type alias UITreeNodeWithDisplayRegion =
    { uiNode : EveOnline.MemoryReading.UITreeNode
    , children : Maybe (List ChildOfNodeWithDisplayRegion)
    , selfDisplayRegion : DisplayRegion
    , totalDisplayRegion : DisplayRegion
    , totalDisplayRegionVisible : DisplayRegion
    }


type ChildOfNodeWithDisplayRegion
    = ChildWithRegion UITreeNodeWithDisplayRegion
    | ChildWithoutRegion EveOnline.MemoryReading.UITreeNode


type alias DisplayRegion =
    { x : Int
    , y : Int
    , width : Int
    , height : Int
    }


type alias Location2d =
    { x : Int
    , y : Int
    }


type alias ContextMenu =
    { uiNode : UITreeNodeWithDisplayRegion
    , entries : List ContextMenuEntry
    }


type alias ContextMenuEntry =
    { uiNode : UITreeNodeWithDisplayRegion
    , text : String
    }


type alias ShipUI =
    { uiNode : UITreeNodeWithDisplayRegion
    , capacitor : ShipUICapacitor
    , hitpointsPercent : Hitpoints
    , indication : Maybe ShipUIIndication
    , moduleButtons : List ShipUIModuleButton
    , moduleButtonsRows :
        { top : List ShipUIModuleButton
        , middle : List ShipUIModuleButton
        , bottom : List ShipUIModuleButton
        }
    , offensiveBuffButtonNames : List String
    , squadronsUI : Maybe SquadronsUI
    , stopButton : Maybe UITreeNodeWithDisplayRegion
    , maxSpeedButton : Maybe UITreeNodeWithDisplayRegion
    , heatGauges : Maybe ShipUIHeatGauges
    }


type alias ShipUIIndication =
    { uiNode : UITreeNodeWithDisplayRegion
    , maneuverType : Maybe ShipManeuverType
    }


type alias ShipUIModuleButton =
    { uiNode : UITreeNodeWithDisplayRegion
    , slotUINode : UITreeNodeWithDisplayRegion
    , isActive : Maybe Bool
    , isHiliteVisible : Bool
    , isBusy : Bool
    , rampRotationMilli : Maybe Int
    }


type alias ShipUICapacitor =
    { uiNode : UITreeNodeWithDisplayRegion
    , pmarks : List ShipUICapacitorPmark
    , levelFromPmarksPercent : Maybe Int
    }


type alias ShipUICapacitorPmark =
    { uiNode : UITreeNodeWithDisplayRegion
    , colorPercent : Maybe ColorComponents
    }


type alias ShipUIHeatGauges =
    { uiNode : UITreeNodeWithDisplayRegion
    , gauges : List ShipUIHeatGauge
    }


type alias ShipUIHeatGauge =
    { uiNode : UITreeNodeWithDisplayRegion
    , rotationPercent : Maybe Int
    , heatPercent : Maybe Int
    }


type alias Hitpoints =
    { structure : Int
    , armor : Int
    , shield : Int
    }


type ShipManeuverType
    = ManeuverWarp
    | ManeuverJump
    | ManeuverOrbit
    | ManeuverApproach


type alias SquadronsUI =
    { uiNode : UITreeNodeWithDisplayRegion
    , squadrons : List SquadronUI
    }


type alias SquadronUI =
    { uiNode : UITreeNodeWithDisplayRegion
    , abilities : List SquadronAbilityIcon
    , actionLabel : Maybe UITreeNodeWithDisplayRegion
    }


type alias SquadronAbilityIcon =
    { uiNode : UITreeNodeWithDisplayRegion
    , quantity : Maybe Int
    , ramp_active : Maybe Bool
    }


type alias InfoPanelContainer =
    { uiNode : UITreeNodeWithDisplayRegion
    , icons : Maybe InfoPanelIcons
    , infoPanelLocationInfo : Maybe InfoPanelLocationInfo
    , infoPanelRoute : Maybe InfoPanelRoute
    , infoPanelAgentMissions : Maybe InfoPanelAgentMissions
    }


type alias InfoPanelIcons =
    { uiNode : UITreeNodeWithDisplayRegion
    , search : Maybe UITreeNodeWithDisplayRegion
    , locationInfo : Maybe UITreeNodeWithDisplayRegion
    , route : Maybe UITreeNodeWithDisplayRegion
    , agentMissions : Maybe UITreeNodeWithDisplayRegion
    , dailyChallenge : Maybe UITreeNodeWithDisplayRegion
    }


type alias InfoPanelRoute =
    { uiNode : UITreeNodeWithDisplayRegion
    , routeElementMarker : List InfoPanelRouteRouteElementMarker
    }


type alias InfoPanelRouteRouteElementMarker =
    { uiNode : UITreeNodeWithDisplayRegion
    }


type alias InfoPanelLocationInfo =
    { uiNode : UITreeNodeWithDisplayRegion
    , listSurroundingsButton : UITreeNodeWithDisplayRegion
    , currentSolarSystemName : Maybe String
    , securityStatusPercent : Maybe Int
    , expandedContent : Maybe InfoPanelLocationInfoExpandedContent
    }


type alias InfoPanelLocationInfoExpandedContent =
    { currentStationName : Maybe String
    }


type alias InfoPanelAgentMissions =
    { uiNode : UITreeNodeWithDisplayRegion
    , entries : List InfoPanelAgentMissionsEntry
    }


type alias InfoPanelAgentMissionsEntry =
    { uiNode : UITreeNodeWithDisplayRegion
    }


type alias Target =
    { uiNode : UITreeNodeWithDisplayRegion
    , barAndImageCont : Maybe UITreeNodeWithDisplayRegion
    , textsTopToBottom : List String
    , isActiveTarget : Bool
    , assignedContainerNode : Maybe UITreeNodeWithDisplayRegion
    , assignedIcons : List UITreeNodeWithDisplayRegion
    }


type alias OverviewWindow =
    { uiNode : UITreeNodeWithDisplayRegion
    , entriesHeaders : List ( String, UITreeNodeWithDisplayRegion )
    , entries : List OverviewWindowEntry
    , scrollControls : Maybe ScrollControls
    }


type alias OverviewWindowEntry =
    { uiNode : UITreeNodeWithDisplayRegion
    , textsLeftToRight : List String
    , cellsTexts : Dict.Dict String String
    , objectDistance : Maybe String
    , objectDistanceInMeters : Result String Int
    , objectName : Maybe String
    , objectType : Maybe String
    , objectAlliance : Maybe String
    , iconSpriteColorPercent : Maybe ColorComponents
    , namesUnderSpaceObjectIcon : Set.Set String
    , bgColorFillsPercent : List ColorComponents
    , rightAlignedIconsHints : List String
    , commonIndications : OverviewWindowEntryCommonIndications
    , opacityPercent : Maybe Int
    }


type alias OverviewWindowEntryCommonIndications =
    { targeting : Bool
    , targetedByMe : Bool
    , isJammingMe : Bool
    , isWarpDisruptingMe : Bool
    }


type alias SelectedItemWindow =
    { uiNode : UITreeNodeWithDisplayRegion
    , orbitButton : Maybe UITreeNodeWithDisplayRegion
    }


type alias FittingWindow =
    { uiNode : UITreeNodeWithDisplayRegion
    }


type alias MarketOrdersWindow =
    { uiNode : UITreeNodeWithDisplayRegion
    }


type alias SurveyScanWindow =
    { uiNode : UITreeNodeWithDisplayRegion
    , scanEntries : List UITreeNodeWithDisplayRegion
    }


type alias RepairShopWindow =
    { uiNode : UITreeNodeWithDisplayRegion
    , items : List UITreeNodeWithDisplayRegion
    , buttonGroup : Maybe UITreeNodeWithDisplayRegion
    , buttons : List { uiNode : UITreeNodeWithDisplayRegion, mainText : Maybe String }
    }


type alias CharacterSheetWindow =
    { uiNode : UITreeNodeWithDisplayRegion
    , skillGroups : List UITreeNodeWithDisplayRegion
    }


type alias ColorComponents =
    { a : Int, r : Int, g : Int, b : Int }


type alias DronesWindow =
    { uiNode : UITreeNodeWithDisplayRegion
    , droneGroups : List DronesWindowEntryGroupStructure
    , droneGroupInBay : Maybe DronesWindowEntryGroupStructure
    , droneGroupInSpace : Maybe DronesWindowEntryGroupStructure
    }


type alias DronesWindowEntryGroupStructure =
    { header : DronesWindowDroneGroupHeader
    , children : List DronesWindowEntry
    }


type DronesWindowEntry
    = DronesWindowEntryGroup DronesWindowEntryGroupStructure
    | DronesWindowEntryDrone DronesWindowEntryDroneStructure


type alias DronesWindowDroneGroupHeader =
    { uiNode : UITreeNodeWithDisplayRegion
    , mainText : Maybe String
    , quantityFromTitle : Maybe DronesWindowDroneGroupHeaderQuantity
    }


type alias DronesWindowDroneGroupHeaderQuantity =
    { current : Int
    , maximum : Maybe Int
    }


type alias DronesWindowEntryDroneStructure =
    { uiNode : UITreeNodeWithDisplayRegion
    , mainText : Maybe String
    , hitpointsPercent : Maybe Hitpoints
    }


type alias ProbeScannerWindow =
    { uiNode : UITreeNodeWithDisplayRegion
    , scanResults : List ProbeScanResult
    }


type alias ProbeScanResult =
    { uiNode : UITreeNodeWithDisplayRegion
    , textsLeftToRight : List String
    , cellsTexts : Dict.Dict String String
    , warpButton : Maybe UITreeNodeWithDisplayRegion
    }


type alias DirectionalScannerWindow =
    { uiNode : UITreeNodeWithDisplayRegion
    , scrollNode : Maybe UITreeNodeWithDisplayRegion
    , scanResults : List UITreeNodeWithDisplayRegion
    }


type alias StationWindow =
    { uiNode : UITreeNodeWithDisplayRegion
    , undockButton : Maybe UITreeNodeWithDisplayRegion
    , abortUndockButton : Maybe UITreeNodeWithDisplayRegion
    }


type alias InventoryWindow =
    { uiNode : UITreeNodeWithDisplayRegion
    , leftTreeEntries : List InventoryWindowLeftTreeEntry
    , subCaptionLabelText : Maybe String
    , selectedContainerCapacityGauge : Maybe (Result String InventoryWindowCapacityGauge)
    , selectedContainerInventory : Maybe Inventory
    , buttonToSwitchToListView : Maybe UITreeNodeWithDisplayRegion
    , buttonToStackAll : Maybe UITreeNodeWithDisplayRegion
    }


type alias Inventory =
    { uiNode : UITreeNodeWithDisplayRegion
    , itemsView : Maybe InventoryItemsView
    , scrollControls : Maybe ScrollControls
    }


type InventoryItemsView
    = InventoryItemsListView { items : List InventoryItemsListViewEntry }
    | InventoryItemsNotListView { items : List UITreeNodeWithDisplayRegion }


type alias InventoryWindowLeftTreeEntry =
    { uiNode : UITreeNodeWithDisplayRegion
    , toggleBtn : Maybe UITreeNodeWithDisplayRegion
    , selectRegion : Maybe UITreeNodeWithDisplayRegion
    , text : String
    , children : List InventoryWindowLeftTreeEntryChild
    }


type InventoryWindowLeftTreeEntryChild
    = InventoryWindowLeftTreeEntryChild InventoryWindowLeftTreeEntry


type alias InventoryWindowCapacityGauge =
    { used : Int
    , maximum : Maybe Int
    , selected : Maybe Int
    }


type alias InventoryItemsListViewEntry =
    { uiNode : UITreeNodeWithDisplayRegion
    , cellsTexts : Dict.Dict String String
    }


type alias ChatWindowStack =
    { uiNode : UITreeNodeWithDisplayRegion
    , chatWindow : Maybe ChatWindow
    }


type alias ChatWindow =
    { uiNode : UITreeNodeWithDisplayRegion
    , name : Maybe String
    , userlist : Maybe ChatWindowUserlist
    }


type alias ChatWindowUserlist =
    { uiNode : UITreeNodeWithDisplayRegion
    , visibleUsers : List ChatUserEntry
    , scrollControls : Maybe ScrollControls
    }


type alias ChatUserEntry =
    { uiNode : UITreeNodeWithDisplayRegion
    , name : Maybe String
    , standingIconHint : Maybe String
    }


type alias ModuleButtonTooltip =
    { uiNode : UITreeNodeWithDisplayRegion
    , shortcut : Maybe { text : String, parseResult : Result String (List Common.EffectOnWindow.VirtualKeyCode) }
    , optimalRange : Maybe { asString : String, inMeters : Result String Int }
    }


type alias HeatStatusTooltip =
    { uiNode : UITreeNodeWithDisplayRegion
    , lowPercent : Maybe Int
    , mediumPercent : Maybe Int
    , highPercent : Maybe Int
    }


type alias Neocom =
    { uiNode : UITreeNodeWithDisplayRegion
    , iconInventory : Maybe UITreeNodeWithDisplayRegion
    , clock : Maybe NeocomClock
    }


type alias NeocomClock =
    { uiNode : UITreeNodeWithDisplayRegion
    , text : String
    , parsedText : Result String { hour : Int, minute : Int }
    }


type alias AgentConversationWindow =
    { uiNode : UITreeNodeWithDisplayRegion
    }


type alias BookmarkLocationWindow =
    { uiNode : UITreeNodeWithDisplayRegion
    , submitButton : Maybe UITreeNodeWithDisplayRegion
    , cancelButton : Maybe UITreeNodeWithDisplayRegion
    }


type alias MessageBox =
    { uiNode : UITreeNodeWithDisplayRegion
    , buttonGroup : Maybe UITreeNodeWithDisplayRegion
    , buttons : List { uiNode : UITreeNodeWithDisplayRegion, mainText : Maybe String }
    }


type alias ScrollControls =
    { uiNode : UITreeNodeWithDisplayRegion
    , scrollHandle : Maybe UITreeNodeWithDisplayRegion
    }


type alias FleetWindow =
    { uiNode : UITreeNodeWithDisplayRegion
    , fleetMembers : List UITreeNodeWithDisplayRegion
    }


type alias WatchListPanel =
    { uiNode : UITreeNodeWithDisplayRegion
    , entries : List UITreeNodeWithDisplayRegion
    }


type alias StandaloneBookmarkWindow =
    { uiNode : UITreeNodeWithDisplayRegion
    , entries : List UITreeNodeWithDisplayRegion
    }


type alias LayerAbovemain =
    { uiNode : UITreeNodeWithDisplayRegion
    , quickMessage : Maybe QuickMessage
    }


type alias QuickMessage =
    { uiNode : UITreeNodeWithDisplayRegion
    , text : String
    }


type alias KeyActivationWindow =
    { uiNode : UITreeNodeWithDisplayRegion
    , activateButton : Maybe UITreeNodeWithDisplayRegion
    }


type alias CompressionWindow =
    { uiNode : UITreeNodeWithDisplayRegion
    , compressButton : Maybe UITreeNodeWithDisplayRegion
    , windowControls : Maybe WindowControls
    }


type alias WindowControls =
    { uiNode : UITreeNodeWithDisplayRegion
    , minimizeButton : Maybe UITreeNodeWithDisplayRegion
    , closeButton : Maybe UITreeNodeWithDisplayRegion
    }


parseUITreeWithDisplayRegionFromUITree : EveOnline.MemoryReading.UITreeNode -> UITreeNodeWithDisplayRegion
parseUITreeWithDisplayRegionFromUITree uiTree =
    let
        selfDisplayRegion =
            uiTree |> getDisplayRegionFromDictEntries |> Maybe.withDefault { x = 0, y = 0, width = 0, height = 0 }
    in
    uiTree
        |> asUITreeNodeWithDisplayRegion
            { selfDisplayRegion = selfDisplayRegion
            , totalDisplayRegion = selfDisplayRegion
            , occludedRegions = []
            }


parseUserInterfaceFromUITree : UITreeNodeWithDisplayRegion -> ParsedUserInterface
parseUserInterfaceFromUITree uiTree =
    { uiTree = uiTree
    , contextMenus = parseContextMenusFromUITreeRoot uiTree
    , shipUI = parseShipUIFromUITreeRoot uiTree
    , targets = parseTargetsFromUITreeRoot uiTree
    , infoPanelContainer = parseInfoPanelContainerFromUIRoot uiTree
    , overviewWindows = parseOverviewWindowsFromUITreeRoot uiTree
    , selectedItemWindow = parseSelectedItemWindowFromUITreeRoot uiTree
    , dronesWindow = parseDronesWindowFromUITreeRoot uiTree
    , fittingWindow = parseFittingWindowFromUITreeRoot uiTree
    , probeScannerWindow = parseProbeScannerWindowFromUITreeRoot uiTree
    , directionalScannerWindow = parseDirectionalScannerWindowFromUITreeRoot uiTree
    , stationWindow = parseStationWindowFromUITreeRoot uiTree
    , inventoryWindows = parseInventoryWindowsFromUITreeRoot uiTree
    , moduleButtonTooltip = parseModuleButtonTooltipFromUITreeRoot uiTree
    , heatStatusTooltip = parseHeatStatusTooltipFromUITreeRoot uiTree
    , chatWindowStacks = parseChatWindowStacksFromUITreeRoot uiTree
    , agentConversationWindows = parseAgentConversationWindowsFromUITreeRoot uiTree
    , marketOrdersWindow = parseMarketOrdersWindowFromUITreeRoot uiTree
    , surveyScanWindow = parseSurveyScanWindowFromUITreeRoot uiTree
    , bookmarkLocationWindow = parseBookmarkLocationWindowFromUITreeRoot uiTree
    , repairShopWindow = parseRepairShopWindowFromUITreeRoot uiTree
    , characterSheetWindow = parseCharacterSheetWindowFromUITreeRoot uiTree
    , fleetWindow = parseFleetWindowFromUITreeRoot uiTree
    , watchListPanel = parseWatchListPanelFromUITreeRoot uiTree
    , standaloneBookmarkWindow = parseStandaloneBookmarkWindowFromUITreeRoot uiTree
    , neocom = parseNeocomFromUITreeRoot uiTree
    , messageBoxes = parseMessageBoxesFromUITreeRoot uiTree
    , layerAbovemain = parseLayerAbovemainFromUITreeRoot uiTree
    , keyActivationWindow = parseKeyActivationWindowFromUITreeRoot uiTree
    , compressionWindow = parseCompressionWindowFromUITreeRoot uiTree
    }


asUITreeNodeWithDisplayRegion :
    { selfDisplayRegion : DisplayRegion, totalDisplayRegion : DisplayRegion, occludedRegions : List DisplayRegion }
    -> EveOnline.MemoryReading.UITreeNode
    -> UITreeNodeWithDisplayRegion
asUITreeNodeWithDisplayRegion { selfDisplayRegion, totalDisplayRegion, occludedRegions } uiNode =
    { uiNode = uiNode
    , children =
        uiNode.children
            |> Maybe.map
                (List.foldl
                    (\currentChild ( mappedSiblings, occludedRegionsFromSiblings ) ->
                        let
                            currentChildResult =
                                currentChild
                                    |> EveOnline.MemoryReading.unwrapUITreeNodeChild
                                    |> asUITreeNodeWithInheritedOffset
                                        { x = totalDisplayRegion.x, y = totalDisplayRegion.y }
                                        { occludedRegions = occludedRegionsFromSiblings ++ occludedRegions }

                            newOccludedRegionsFromSiblings =
                                currentChildResult
                                    |> justCaseWithDisplayRegion
                                    |> Maybe.map listDescendantsWithDisplayRegion
                                    |> Maybe.withDefault []
                                    |> List.filter (.uiNode >> nodeOccludesFollowingNodes)
                                    |> List.map .totalDisplayRegion
                        in
                        ( currentChildResult :: mappedSiblings
                        , newOccludedRegionsFromSiblings ++ occludedRegionsFromSiblings
                        )
                    )
                    ( [], [] )
                    >> Tuple.first
                    >> List.reverse
                )
    , selfDisplayRegion = selfDisplayRegion
    , totalDisplayRegion = totalDisplayRegion
    , totalDisplayRegionVisible =
        subtractRegionsFromRegion { minuend = totalDisplayRegion, subtrahend = occludedRegions }
            |> List.sortBy (areaFromDisplayRegion >> Maybe.withDefault -1 >> negate)
            |> List.head
            |> Maybe.withDefault { x = -1, y = -1, width = 0, height = 0 }
    }


asUITreeNodeWithInheritedOffset :
    { x : Int, y : Int }
    -> { occludedRegions : List DisplayRegion }
    -> EveOnline.MemoryReading.UITreeNode
    -> ChildOfNodeWithDisplayRegion
asUITreeNodeWithInheritedOffset inheritedOffset { occludedRegions } rawNode =
    case rawNode |> getDisplayRegionFromDictEntries of
        Nothing ->
            ChildWithoutRegion rawNode

        Just selfRegion ->
            ChildWithRegion
                (asUITreeNodeWithDisplayRegion
                    { selfDisplayRegion = selfRegion
                    , totalDisplayRegion =
                        { selfRegion | x = inheritedOffset.x + selfRegion.x, y = inheritedOffset.y + selfRegion.y }
                    , occludedRegions = occludedRegions
                    }
                    rawNode
                )


getDisplayRegionFromDictEntries : EveOnline.MemoryReading.UITreeNode -> Maybe DisplayRegion
getDisplayRegionFromDictEntries uiNode =
    let
        fixedNumberFromJsonValue =
            Json.Decode.decodeValue
                (Json.Decode.oneOf
                    [ jsonDecodeIntFromIntOrString
                    , Json.Decode.field "int_low32" jsonDecodeIntFromIntOrString
                    ]
                )

        fixedNumberFromPropertyName propertyName =
            uiNode.dictEntriesOfInterest
                |> Dict.get propertyName
                |> Maybe.andThen (fixedNumberFromJsonValue >> Result.toMaybe)
    in
    case
        ( ( fixedNumberFromPropertyName "_displayX", fixedNumberFromPropertyName "_displayY" )
        , ( fixedNumberFromPropertyName "_displayWidth", fixedNumberFromPropertyName "_displayHeight" )
        )
    of
        ( ( Just displayX, Just displayY ), ( Just displayWidth, Just displayHeight ) ) ->
            Just { x = displayX, y = displayY, width = displayWidth, height = displayHeight }

        _ ->
            Nothing


parseContextMenusFromUITreeRoot : UITreeNodeWithDisplayRegion -> List ContextMenu
parseContextMenusFromUITreeRoot uiTreeRoot =
    case
        uiTreeRoot
            |> listChildrenWithDisplayRegion
            |> List.filter (.uiNode >> getNameFromDictEntries >> Maybe.map String.toLower >> (==) (Just "l_menu"))
            |> List.head
    of
        Nothing ->
            []

        Just layerMenu ->
            layerMenu
                |> listChildrenWithDisplayRegion
                |> List.filter (.uiNode >> .pythonObjectTypeName >> String.toLower >> String.contains "menu")
                |> List.map parseContextMenu


parseInfoPanelContainerFromUIRoot : UITreeNodeWithDisplayRegion -> Maybe InfoPanelContainer
parseInfoPanelContainerFromUIRoot uiTreeRoot =
    case
        uiTreeRoot
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "InfoPanelContainer")
            |> List.sortBy (.uiNode >> EveOnline.MemoryReading.countDescendantsInUITreeNode >> negate)
            |> List.head
    of
        Nothing ->
            Nothing

        Just containerNode ->
            Just
                { uiNode = containerNode
                , icons = parseInfoPanelIconsFromInfoPanelContainer containerNode
                , infoPanelLocationInfo = parseInfoPanelLocationInfoFromInfoPanelContainer containerNode
                , infoPanelRoute = parseInfoPanelRouteFromInfoPanelContainer containerNode
                , infoPanelAgentMissions = parseInfoPanelAgentMissionsFromInfoPanelContainer containerNode
                }


parseInfoPanelIconsFromInfoPanelContainer : UITreeNodeWithDisplayRegion -> Maybe InfoPanelIcons
parseInfoPanelIconsFromInfoPanelContainer infoPanelContainerNode =
    case
        infoPanelContainerNode
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> getNameFromDictEntries >> Maybe.map ((==) "iconCont") >> Maybe.withDefault False)
            |> List.sortBy (.totalDisplayRegion >> .y)
            |> List.head
    of
        Nothing ->
            Nothing

        Just iconContainerNode ->
            let
                iconNodeFromTexturePathEnd texturePathEnd =
                    iconContainerNode
                        |> listDescendantsWithDisplayRegion
                        |> List.filter
                            (.uiNode
                                >> getTexturePathFromDictEntries
                                >> Maybe.map (String.endsWith texturePathEnd)
                                >> Maybe.withDefault False
                            )
                        |> List.head
            in
            Just
                { uiNode = iconContainerNode
                , search = iconNodeFromTexturePathEnd "search.png"
                , locationInfo = iconNodeFromTexturePathEnd "LocationInfo.png"
                , route = iconNodeFromTexturePathEnd "Route.png"
                , agentMissions = iconNodeFromTexturePathEnd "Missions.png"
                , dailyChallenge = iconNodeFromTexturePathEnd "dailyChallenge.png"
                }


parseInfoPanelLocationInfoFromInfoPanelContainer : UITreeNodeWithDisplayRegion -> Maybe InfoPanelLocationInfo
parseInfoPanelLocationInfoFromInfoPanelContainer infoPanelContainerNode =
    case
        infoPanelContainerNode
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "InfoPanelLocationInfo")
            |> List.head
    of
        Nothing ->
            Nothing

        Just infoPanelNode ->
            let
                securityStatusPercent =
                    infoPanelNode.uiNode
                        |> getAllContainedDisplayTexts
                        |> List.filterMap parseSecurityStatusPercentFromUINodeText
                        |> List.head

                currentSolarSystemName =
                    infoPanelNode.uiNode
                        |> getAllContainedDisplayTexts
                        |> List.filterMap parseCurrentSolarSystemFromUINodeText
                        |> List.head
                        |> Maybe.map String.trim

                maybeListSurroundingsButton =
                    infoPanelNode
                        |> listDescendantsWithDisplayRegion
                        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "ListSurroundingsBtn")
                        |> List.head

                expandedContent =
                    infoPanelNode
                        |> listDescendantsWithDisplayRegion
                        |> List.filter
                            (\uiNode ->
                                (uiNode.uiNode.pythonObjectTypeName |> String.contains "Container")
                                    && (uiNode.uiNode |> getNameFromDictEntries |> Maybe.withDefault "" |> String.contains "mainCont")
                            )
                        |> List.head
                        |> Maybe.map
                            (\expandedContainer ->
                                { currentStationName =
                                    expandedContainer.uiNode
                                        |> getAllContainedDisplayTexts
                                        |> List.filterMap parseCurrentStationNameFromInfoPanelLocationInfoLabelText
                                        |> List.head
                                }
                            )
            in
            maybeListSurroundingsButton
                |> Maybe.map
                    (\listSurroundingsButton ->
                        { uiNode = infoPanelNode
                        , listSurroundingsButton = listSurroundingsButton
                        , currentSolarSystemName = currentSolarSystemName
                        , securityStatusPercent = securityStatusPercent
                        , expandedContent = expandedContent
                        }
                    )


parseSecurityStatusPercentFromUINodeText : String -> Maybe Int
parseSecurityStatusPercentFromUINodeText =
    Maybe.Extra.oneOf
        [ getSubstringBetweenXmlTagsAfterMarker "hint='Security status'"
        , getSubstringBetweenXmlTagsAfterMarker "hint=\"Security status\"><color="
        ]
        >> Maybe.andThen (String.trim >> String.toFloat)
        >> Maybe.map ((*) 100 >> round)


parseCurrentSolarSystemFromUINodeText : String -> Maybe String
parseCurrentSolarSystemFromUINodeText =
    Maybe.Extra.oneOf
        [ getSubstringBetweenXmlTagsAfterMarker "alt='Current Solar System'"
        , getSubstringBetweenXmlTagsAfterMarker "alt=\"Current Solar System\""
        ]


parseCurrentStationNameFromInfoPanelLocationInfoLabelText : String -> Maybe String
parseCurrentStationNameFromInfoPanelLocationInfoLabelText =
    getSubstringBetweenXmlTagsAfterMarker "alt='Current Station'"
        >> Maybe.map String.trim


parseInfoPanelRouteFromInfoPanelContainer : UITreeNodeWithDisplayRegion -> Maybe InfoPanelRoute
parseInfoPanelRouteFromInfoPanelContainer infoPanelContainerNode =
    case
        infoPanelContainerNode
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "InfoPanelRoute")
            |> List.head
    of
        Nothing ->
            Nothing

        Just infoPanelRouteNode ->
            let
                routeElementMarker =
                    infoPanelRouteNode
                        |> listDescendantsWithDisplayRegion
                        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "AutopilotDestinationIcon")
                        |> List.map (\uiNode -> { uiNode = uiNode })
            in
            Just { uiNode = infoPanelRouteNode, routeElementMarker = routeElementMarker }


parseInfoPanelAgentMissionsFromInfoPanelContainer : UITreeNodeWithDisplayRegion -> Maybe InfoPanelAgentMissions
parseInfoPanelAgentMissionsFromInfoPanelContainer infoPanelContainerNode =
    case
        infoPanelContainerNode
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "InfoPanelAgentMissions")
            |> List.head
    of
        Nothing ->
            Nothing

        Just infoPanelNode ->
            let
                entries =
                    infoPanelNode
                        |> listDescendantsWithDisplayRegion
                        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "MissionEntry")
                        |> List.map (\uiNode -> { uiNode = uiNode })
            in
            Just
                { uiNode = infoPanelNode
                , entries = entries
                }


parseContextMenu : UITreeNodeWithDisplayRegion -> ContextMenu
parseContextMenu contextMenuUINode =
    let
        entriesUINodes =
            contextMenuUINode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> .pythonObjectTypeName >> String.toLower >> String.contains "menuentry")

        entries =
            entriesUINodes
                |> List.map
                    (\entryUINode ->
                        let
                            text =
                                entryUINode
                                    |> listDescendantsWithDisplayRegion
                                    |> List.filterMap (.uiNode >> getDisplayText)
                                    |> List.sortBy (String.length >> negate)
                                    |> List.head
                                    |> Maybe.withDefault ""
                        in
                        { text = text
                        , uiNode = entryUINode
                        }
                    )
                |> List.sortBy (.uiNode >> .totalDisplayRegion >> .y)
    in
    { uiNode = contextMenuUINode
    , entries = entries
    }


parseShipUIFromUITreeRoot : UITreeNodeWithDisplayRegion -> Maybe ShipUI
parseShipUIFromUITreeRoot uiTreeRoot =
    case
        uiTreeRoot
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "ShipUI")
            |> List.head
    of
        Nothing ->
            Nothing

        Just shipUINode ->
            case
                shipUINode
                    |> listDescendantsWithDisplayRegion
                    |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "CapacitorContainer")
                    |> List.head
            of
                Nothing ->
                    Nothing

                Just capacitorUINode ->
                    let
                        descendantNodesFromPythonObjectTypeNameEqual pythonObjectTypeName =
                            shipUINode
                                |> listDescendantsWithDisplayRegion
                                |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) pythonObjectTypeName)

                        capacitor =
                            capacitorUINode |> parseShipUICapacitorFromUINode

                        {-
                           speedGaugeElement =
                               shipUINode
                                   |> listDescendantsWithDisplayRegion
                                   |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "SpeedGauge")
                                   |> List.head
                        -}
                        maybeIndicationNode =
                            shipUINode
                                |> listDescendantsWithDisplayRegion
                                |> List.filter (.uiNode >> getNameFromDictEntries >> Maybe.map (String.toLower >> String.contains "indicationcontainer") >> Maybe.withDefault False)
                                |> List.head

                        indication =
                            maybeIndicationNode
                                |> Maybe.map (parseShipUIIndication >> Just)
                                |> Maybe.withDefault Nothing

                        moduleButtons =
                            shipUINode
                                |> listDescendantsWithDisplayRegion
                                |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "ShipSlot")
                                |> List.filterMap
                                    (\slotNode ->
                                        slotNode
                                            |> listDescendantsWithDisplayRegion
                                            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "ModuleButton")
                                            |> List.head
                                            |> Maybe.map
                                                (\moduleButtonNode ->
                                                    parseShipUIModuleButton { slotNode = slotNode, moduleButtonNode = moduleButtonNode }
                                                )
                                    )

                        getLastValuePercentFromGaugeName gaugeName =
                            shipUINode
                                |> listDescendantsWithDisplayRegion
                                |> List.filter (.uiNode >> getNameFromDictEntries >> Maybe.map ((==) gaugeName) >> Maybe.withDefault False)
                                |> List.head
                                |> Maybe.andThen (.uiNode >> .dictEntriesOfInterest >> Dict.get "_lastValue")
                                |> Maybe.andThen (Json.Decode.decodeValue Json.Decode.float >> Result.toMaybe)
                                |> Maybe.map ((*) 100 >> round)

                        maybeHitpointsPercent =
                            case ( getLastValuePercentFromGaugeName "structureGauge", getLastValuePercentFromGaugeName "armorGauge", getLastValuePercentFromGaugeName "shieldGauge" ) of
                                ( Just structure, Just armor, Just shield ) ->
                                    Just { structure = structure, armor = armor, shield = shield }

                                _ ->
                                    Nothing

                        offensiveBuffButtonNames =
                            shipUINode
                                |> listDescendantsWithDisplayRegion
                                |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "OffensiveBuffButton")
                                |> List.filterMap (.uiNode >> getNameFromDictEntries)

                        squadronsUI =
                            shipUINode
                                |> listDescendantsWithDisplayRegion
                                |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "SquadronsUI")
                                |> List.head
                                |> Maybe.map parseSquadronsUI

                        heatGauges =
                            shipUINode
                                |> listDescendantsWithDisplayRegion
                                |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "HeatGauges")
                                |> List.head
                                |> Maybe.map parseShipUIHeatGaugesFromUINode
                    in
                    maybeHitpointsPercent
                        |> Maybe.map
                            (\hitpointsPercent ->
                                { uiNode = shipUINode
                                , capacitor = capacitor
                                , hitpointsPercent = hitpointsPercent
                                , indication = indication
                                , moduleButtons = moduleButtons
                                , moduleButtonsRows = groupShipUIModulesIntoRows capacitor moduleButtons
                                , offensiveBuffButtonNames = offensiveBuffButtonNames
                                , squadronsUI = squadronsUI
                                , stopButton = descendantNodesFromPythonObjectTypeNameEqual "StopButton" |> List.head
                                , maxSpeedButton = descendantNodesFromPythonObjectTypeNameEqual "MaxSpeedButton" |> List.head
                                , heatGauges = heatGauges
                                }
                            )


parseShipUIModuleButton : { slotNode : UITreeNodeWithDisplayRegion, moduleButtonNode : UITreeNodeWithDisplayRegion } -> ShipUIModuleButton
parseShipUIModuleButton { slotNode, moduleButtonNode } =
    let
        rotationFloatFromRampName rampName =
            slotNode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> getNameFromDictEntries >> (==) (Just rampName))
                |> List.filterMap (.uiNode >> getRotationFloatFromDictEntries)
                |> List.head

        rampRotationMilli =
            case ( rotationFloatFromRampName "leftRamp", rotationFloatFromRampName "rightRamp" ) of
                ( Just leftRampRotationFloat, Just rightRampRotationFloat ) ->
                    if
                        (leftRampRotationFloat < 0 || pi * 2.01 < leftRampRotationFloat)
                            || (rightRampRotationFloat < 0 || pi * 2.01 < rightRampRotationFloat)
                    then
                        Nothing

                    else
                        Just (max 0 (min 1000 (round (1000 - ((leftRampRotationFloat + rightRampRotationFloat) * 500) / pi))))

                _ ->
                    Nothing
    in
    { uiNode = moduleButtonNode
    , slotUINode = slotNode
    , isActive =
        moduleButtonNode.uiNode.dictEntriesOfInterest
            |> Dict.get "ramp_active"
            |> Maybe.andThen (Json.Decode.decodeValue Json.Decode.bool >> Result.toMaybe)
    , isHiliteVisible =
        slotNode
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "Sprite")
            |> List.filter (.uiNode >> getNameFromDictEntries >> (==) (Just "hilite"))
            |> List.isEmpty
            |> not
    , isBusy =
        slotNode
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "Sprite")
            |> List.filter (.uiNode >> getNameFromDictEntries >> (==) (Just "busy"))
            |> List.isEmpty
            |> not
    , rampRotationMilli = rampRotationMilli
    }


parseShipUICapacitorFromUINode : UITreeNodeWithDisplayRegion -> ShipUICapacitor
parseShipUICapacitorFromUINode capacitorUINode =
    let
        pmarks =
            capacitorUINode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> getNameFromDictEntries >> Maybe.map ((==) "pmark") >> Maybe.withDefault False)
                |> List.map
                    (\pmarkUINode ->
                        { uiNode = pmarkUINode
                        , colorPercent = pmarkUINode.uiNode |> getColorPercentFromDictEntries
                        }
                    )

        maybePmarksFills =
            pmarks
                |> List.map (.colorPercent >> Maybe.map (\colorPercent -> colorPercent.a < 20))
                |> Maybe.Extra.combine

        levelFromPmarksPercent =
            maybePmarksFills
                |> Maybe.andThen
                    (\pmarksFills ->
                        if (pmarksFills |> List.length) < 1 then
                            Nothing

                        else
                            Just (((pmarksFills |> List.filter identity |> List.length) * 100) // (pmarksFills |> List.length))
                    )
    in
    { uiNode = capacitorUINode
    , pmarks = pmarks
    , levelFromPmarksPercent = levelFromPmarksPercent
    }


parseShipUIHeatGaugesFromUINode : UITreeNodeWithDisplayRegion -> ShipUIHeatGauges
parseShipUIHeatGaugesFromUINode gaugesUINode =
    let
        heatGaugesRotationZeroValues =
            [ -213, -108, -3 ]

        heatValuePercentFromRotationPercent rotationPercent =
            heatGaugesRotationZeroValues
                |> List.map
                    (\gaugeRotationZero ->
                        if rotationPercent <= gaugeRotationZero && gaugeRotationZero - 100 <= rotationPercent then
                            Just -(rotationPercent - gaugeRotationZero)

                        else
                            Nothing
                    )
                |> List.filterMap identity
                |> List.head

        gauges =
            gaugesUINode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> getNameFromDictEntries >> Maybe.map ((==) "heatGauge") >> Maybe.withDefault False)
                |> List.map
                    (\gaugeUiNode ->
                        let
                            rotationPercent =
                                gaugeUiNode.uiNode
                                    |> getRotationFloatFromDictEntries
                                    |> Maybe.map ((*) 100 >> round)
                        in
                        { uiNode = gaugeUiNode
                        , rotationPercent = rotationPercent
                        , heatPercent = rotationPercent |> Maybe.andThen heatValuePercentFromRotationPercent
                        }
                    )
    in
    { uiNode = gaugesUINode
    , gauges = gauges
    }


groupShipUIModulesIntoRows :
    ShipUICapacitor
    -> List ShipUIModuleButton
    -> { top : List ShipUIModuleButton, middle : List ShipUIModuleButton, bottom : List ShipUIModuleButton }
groupShipUIModulesIntoRows capacitor modules =
    let
        verticalDistanceThreshold =
            20

        verticalCenterOfUINode uiNode =
            uiNode.totalDisplayRegion.y + uiNode.totalDisplayRegion.height // 2

        capacitorVerticalCenter =
            verticalCenterOfUINode capacitor.uiNode
    in
    modules
        |> List.foldr
            (\shipModule previousRows ->
                if verticalCenterOfUINode shipModule.uiNode < capacitorVerticalCenter - verticalDistanceThreshold then
                    { previousRows | top = shipModule :: previousRows.top }

                else if verticalCenterOfUINode shipModule.uiNode > capacitorVerticalCenter + verticalDistanceThreshold then
                    { previousRows | bottom = shipModule :: previousRows.bottom }

                else
                    { previousRows | middle = shipModule :: previousRows.middle }
            )
            { top = [], middle = [], bottom = [] }


parseShipUIIndication : UITreeNodeWithDisplayRegion -> ShipUIIndication
parseShipUIIndication indicationUINode =
    let
        displayTexts =
            indicationUINode.uiNode |> getAllContainedDisplayTexts

        maneuverType =
            [ ( "Warp", ManeuverWarp )
            , ( "Jump", ManeuverJump )
            , ( "Orbit", ManeuverOrbit )
            , ( "Approach", ManeuverApproach )

            -- Sample `session-2022-05-23T23-00-32-87ba97.zip` shared by Abaddon at https://forum.botlab.org/t/i-want-to-add-korean-support-on-eve-online-bot-what-should-i-do/4370/9
            , ( "워프 드라이브 가동", ManeuverWarp )

            -- Sample `session-2022-05-26T03-13-42-83df2b.zip` shared by Abaddon at https://forum.botlab.org/t/i-want-to-add-korean-support-on-eve-online-bot-what-should-i-do/4370/14
            , ( "점프 중", ManeuverJump )
            ]
                |> List.filterMap
                    (\( pattern, candidateManeuverType ) ->
                        if displayTexts |> List.any (String.contains pattern) then
                            Just candidateManeuverType

                        else
                            Nothing
                    )
                |> List.head
    in
    { uiNode = indicationUINode, maneuverType = maneuverType }


parseSquadronsUI : UITreeNodeWithDisplayRegion -> SquadronsUI
parseSquadronsUI squadronsUINode =
    { uiNode = squadronsUINode
    , squadrons =
        squadronsUINode
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "SquadronUI")
            |> List.map parseSquadronUI
    }


parseSquadronUI : UITreeNodeWithDisplayRegion -> SquadronUI
parseSquadronUI squadronUINode =
    { uiNode = squadronUINode
    , abilities =
        squadronUINode
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "AbilityIcon")
            |> List.map parseSquadronAbilityIcon
    , actionLabel =
        squadronUINode
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "SquadronActionLabel")
            |> List.head
    }


parseSquadronAbilityIcon : UITreeNodeWithDisplayRegion -> SquadronAbilityIcon
parseSquadronAbilityIcon abilityIconUINode =
    { uiNode = abilityIconUINode
    , quantity =
        abilityIconUINode
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> getNameFromDictEntries >> Maybe.map (String.toLower >> String.contains "quantity") >> Maybe.withDefault False)
            |> List.concatMap (.uiNode >> getAllContainedDisplayTexts)
            |> List.head
            |> Maybe.andThen (String.trim >> String.toInt)
    , ramp_active =
        abilityIconUINode.uiNode.dictEntriesOfInterest
            |> Dict.get "ramp_active"
            |> Maybe.andThen (Json.Decode.decodeValue Json.Decode.bool >> Result.toMaybe)
    }


parseTargetsFromUITreeRoot : UITreeNodeWithDisplayRegion -> List Target
parseTargetsFromUITreeRoot =
    listDescendantsWithDisplayRegion
        >> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "TargetInBar")
        >> List.map parseTarget


parseTarget : UITreeNodeWithDisplayRegion -> Target
parseTarget targetNode =
    let
        textsTopToBottom =
            targetNode
                |> getAllContainedDisplayTextsWithRegion
                |> List.sortBy (Tuple.second >> .totalDisplayRegion >> .y)
                |> List.map Tuple.first

        barAndImageCont =
            targetNode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> getNameFromDictEntries >> (==) (Just "barAndImageCont"))
                |> List.head

        isActiveTarget =
            targetNode.uiNode
                |> EveOnline.MemoryReading.listDescendantsInUITreeNode
                |> List.any (.pythonObjectTypeName >> (==) "ActiveTargetOnBracket")

        assignedContainerNode =
            targetNode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> getNameFromDictEntries >> Maybe.map (String.toLower >> String.contains "assigned") >> Maybe.withDefault False)
                |> List.sortBy (.totalDisplayRegion >> .width)
                |> List.head

        assignedIcons =
            assignedContainerNode
                |> Maybe.map listDescendantsWithDisplayRegion
                |> Maybe.withDefault []
                |> List.filter (\uiNode -> [ "Sprite", "Icon" ] |> List.member uiNode.uiNode.pythonObjectTypeName)
    in
    { uiNode = targetNode
    , barAndImageCont = barAndImageCont
    , textsTopToBottom = textsTopToBottom
    , isActiveTarget = isActiveTarget
    , assignedContainerNode = assignedContainerNode
    , assignedIcons = assignedIcons
    }


parseOverviewWindowsFromUITreeRoot : UITreeNodeWithDisplayRegion -> List OverviewWindow
parseOverviewWindowsFromUITreeRoot uiTreeRoot =
    uiTreeRoot
        |> listDescendantsWithDisplayRegion
        |> List.filter
            (.uiNode
                >> .pythonObjectTypeName
                >> (List.member >> (|>) [ "OverView", "OverviewWindow", "OverviewWindowOld" ])
            )
        |> List.map parseOverviewWindow


parseOverviewWindow : UITreeNodeWithDisplayRegion -> OverviewWindow
parseOverviewWindow overviewWindowNode =
    let
        scrollNode =
            overviewWindowNode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> .pythonObjectTypeName >> String.toLower >> String.contains "scroll")
                |> List.head

        scrollControlsNode =
            scrollNode
                |> Maybe.map listDescendantsWithDisplayRegion
                |> Maybe.withDefault []
                |> List.filter (.uiNode >> .pythonObjectTypeName >> String.contains "ScrollControls")
                |> List.head

        headersContainerNode =
            scrollNode
                |> Maybe.map listDescendantsWithDisplayRegion
                |> Maybe.withDefault []
                |> List.filter (.uiNode >> .pythonObjectTypeName >> String.toLower >> String.contains "headers")
                |> List.head

        entriesHeaders =
            headersContainerNode
                |> Maybe.map getAllContainedDisplayTextsWithRegion
                |> Maybe.withDefault []

        entries =
            overviewWindowNode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "OverviewScrollEntry")
                |> List.map (parseOverviewWindowEntry entriesHeaders)
    in
    { uiNode = overviewWindowNode
    , entriesHeaders = entriesHeaders
    , entries = entries
    , scrollControls = scrollControlsNode |> Maybe.map parseScrollControls
    }


parseOverviewWindowEntry : List ( String, UITreeNodeWithDisplayRegion ) -> UITreeNodeWithDisplayRegion -> OverviewWindowEntry
parseOverviewWindowEntry entriesHeaders overviewEntryNode =
    let
        textsLeftToRight =
            overviewEntryNode
                |> getAllContainedDisplayTextsWithRegion
                |> List.sortBy (Tuple.second >> .totalDisplayRegion >> .x)
                |> List.map Tuple.first

        listViewEntry =
            parseListViewEntry entriesHeaders overviewEntryNode

        objectDistance =
            listViewEntry.cellsTexts
                |> Dict.get "Distance"

        objectDistanceInMeters =
            objectDistance
                |> Maybe.map parseOverviewEntryDistanceInMetersFromText
                |> Maybe.withDefault (Err "Did not find the 'Distance' cell text.")

        spaceObjectIconNode =
            overviewEntryNode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "SpaceObjectIcon")
                |> List.head

        iconSpriteColorPercent =
            spaceObjectIconNode
                |> Maybe.map listDescendantsWithDisplayRegion
                |> Maybe.withDefault []
                |> List.filter (.uiNode >> getNameFromDictEntries >> (==) (Just "iconSprite"))
                |> List.head
                |> Maybe.andThen (.uiNode >> getColorPercentFromDictEntries)

        namesUnderSpaceObjectIcon =
            spaceObjectIconNode
                |> Maybe.map (.uiNode >> EveOnline.MemoryReading.listDescendantsInUITreeNode)
                |> Maybe.withDefault []
                |> List.filterMap getNameFromDictEntries
                |> Set.fromList

        bgColorFillsPercent =
            overviewEntryNode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "Fill")
                |> List.filter (.uiNode >> getNameFromDictEntries >> Maybe.map ((==) "bgColor") >> Maybe.withDefault False)
                |> List.filterMap (\fillUiNode -> fillUiNode.uiNode |> getColorPercentFromDictEntries)

        rightAlignedIconsHints =
            overviewEntryNode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> getNameFromDictEntries >> Maybe.map ((==) "rightAlignedIconContainer") >> Maybe.withDefault False)
                |> List.concatMap listDescendantsWithDisplayRegion
                |> List.filterMap (.uiNode >> getHintTextFromDictEntries)

        rightAlignedIconsHintsContainsTextIgnoringCase textToSearch =
            rightAlignedIconsHints |> List.any (String.toLower >> String.contains (textToSearch |> String.toLower))

        commonIndications =
            { targeting = namesUnderSpaceObjectIcon |> Set.member "targeting"
            , targetedByMe = namesUnderSpaceObjectIcon |> Set.member "targetedByMeIndicator"
            , isJammingMe = rightAlignedIconsHintsContainsTextIgnoringCase "is jamming me"
            , isWarpDisruptingMe = rightAlignedIconsHintsContainsTextIgnoringCase "is warp disrupting me"
            }

        opacityPercent =
            overviewEntryNode.uiNode
                |> getOpacityFloatFromDictEntries
                |> Maybe.map ((*) 100 >> round)
    in
    { uiNode = overviewEntryNode
    , textsLeftToRight = textsLeftToRight
    , cellsTexts = listViewEntry.cellsTexts
    , objectDistance = objectDistance
    , objectDistanceInMeters = objectDistanceInMeters
    , objectName = listViewEntry.cellsTexts |> Dict.get "Name"
    , objectType = listViewEntry.cellsTexts |> Dict.get "Type"
    , objectAlliance = listViewEntry.cellsTexts |> Dict.get "Alliance"
    , iconSpriteColorPercent = iconSpriteColorPercent
    , namesUnderSpaceObjectIcon = namesUnderSpaceObjectIcon
    , bgColorFillsPercent = bgColorFillsPercent
    , rightAlignedIconsHints = rightAlignedIconsHints
    , commonIndications = commonIndications
    , opacityPercent = opacityPercent
    }


parseOverviewEntryDistanceInMetersFromText : String -> Result String Int
parseOverviewEntryDistanceInMetersFromText distanceDisplayTextBeforeTrim =
    case distanceDisplayTextBeforeTrim |> String.trim |> String.split " " |> List.reverse of
        unitText :: reversedNumberTexts ->
            case parseDistanceUnitInMeters unitText of
                Nothing ->
                    Err ("Failed to parse distance unit text of '" ++ unitText ++ "'")

                Just unitInMeters ->
                    case
                        reversedNumberTexts |> List.reverse |> String.join " " |> parseNumberTruncatingAfterOptionalDecimalSeparator
                    of
                        Err parseNumberError ->
                            Err ("Failed to parse number: " ++ parseNumberError)

                        Ok parsedNumber ->
                            Ok (parsedNumber * unitInMeters)

        _ ->
            Err "Expecting at least one whitespace character separating number and unit."


parseDistanceUnitInMeters : String -> Maybe Int
parseDistanceUnitInMeters unitText =
    case String.trim unitText of
        "m" ->
            Just 1

        "km" ->
            Just 1000

        _ ->
            Nothing


parseSelectedItemWindowFromUITreeRoot : UITreeNodeWithDisplayRegion -> Maybe SelectedItemWindow
parseSelectedItemWindowFromUITreeRoot uiTreeRoot =
    uiTreeRoot
        |> listDescendantsWithDisplayRegion
        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "ActiveItem")
        |> List.head
        |> Maybe.map parseSelectedItemWindow


parseSelectedItemWindow : UITreeNodeWithDisplayRegion -> SelectedItemWindow
parseSelectedItemWindow windowNode =
    let
        actionButtonFromTexturePathEnding texturePathEnding =
            windowNode
                |> listDescendantsWithDisplayRegion
                |> List.filter
                    (.uiNode
                        >> getTexturePathFromDictEntries
                        >> Maybe.map (String.toLower >> String.endsWith (String.toLower texturePathEnding))
                        >> Maybe.withDefault False
                    )
                |> List.head

        orbitButton =
            actionButtonFromTexturePathEnding "44_32_21.png"
    in
    { uiNode = windowNode, orbitButton = orbitButton }


parseDronesWindowFromUITreeRoot : UITreeNodeWithDisplayRegion -> Maybe DronesWindow
parseDronesWindowFromUITreeRoot uiTreeRoot =
    case
        uiTreeRoot
            |> listDescendantsWithDisplayRegion
            |> List.filter
                (.uiNode
                    >> .pythonObjectTypeName
                    >> (List.member >> (|>) [ "DroneView", "DronesWindow" ])
                )
            |> List.head
    of
        Nothing ->
            Nothing

        Just windowNode ->
            let
                {-
                   scrollNode =
                       windowNode
                           |> listDescendantsWithDisplayRegion
                           |> List.filter (.uiNode >> .pythonObjectTypeName >> String.toLower >> String.contains "scroll")
                           |> List.head
                -}
                droneGroupHeaders =
                    windowNode
                        |> listDescendantsWithDisplayRegion
                        |> List.filter (.uiNode >> .pythonObjectTypeName >> String.contains "DroneGroupHeader")
                        |> List.filterMap parseDronesWindowDroneGroupHeader

                droneEntries =
                    windowNode
                        |> listDescendantsWithDisplayRegion
                        |> List.filter
                            (.uiNode
                                >> .pythonObjectTypeName
                                >> (\pythonTypeName ->
                                        {-
                                           2023-01-02 Observed: 'DroneInBayEntry'
                                        -}
                                        String.startsWith "Drone" pythonTypeName
                                            && String.endsWith "Entry" pythonTypeName
                                   )
                            )
                        |> List.map parseDronesWindowDroneEntry

                droneGroups =
                    [ droneEntries |> List.map DronesWindowEntryDrone
                    , droneGroupHeaders
                        |> List.map (\header -> { header = header, children = [] })
                        |> List.map DronesWindowEntryGroup
                    ]
                        |> List.concat
                        |> dronesGroupTreesFromFlatListOfEntries

                droneGroupFromHeaderTextPart headerTextPart =
                    droneGroups
                        |> List.filter (.header >> .mainText >> Maybe.withDefault "" >> String.toLower >> String.contains (headerTextPart |> String.toLower))
                        |> List.sortBy (.header >> .mainText >> Maybe.map String.length >> Maybe.withDefault 999)
                        |> List.head
            in
            Just
                { uiNode = windowNode
                , droneGroups = droneGroups
                , droneGroupInBay = droneGroupFromHeaderTextPart "in bay"
                , droneGroupInSpace = droneGroupFromHeaderTextPart "in space"
                }


dronesGroupTreesFromFlatListOfEntries : List DronesWindowEntry -> List DronesWindowEntryGroupStructure
dronesGroupTreesFromFlatListOfEntries entriesBeforeOrdering =
    let
        verticalOffsetFromEntry entry =
            case entry of
                DronesWindowEntryDrone droneEntry ->
                    droneEntry.uiNode.totalDisplayRegion.y

                DronesWindowEntryGroup groupEntry ->
                    groupEntry.header.uiNode.totalDisplayRegion.y

        entriesOrderedVertically =
            entriesBeforeOrdering
                |> List.sortBy verticalOffsetFromEntry
    in
    entriesOrderedVertically
        |> List.filterMap
            (\entry ->
                case entry of
                    DronesWindowEntryDrone _ ->
                        Nothing

                    DronesWindowEntryGroup group ->
                        Just group
            )
        |> List.head
        |> Maybe.map
            (\topmostGroupEntry ->
                let
                    entriesUpToSibling =
                        entriesOrderedVertically
                            |> List.Extra.dropWhile
                                (verticalOffsetFromEntry
                                    >> (\offset -> offset <= verticalOffsetFromEntry (DronesWindowEntryGroup topmostGroupEntry))
                                )
                            |> List.Extra.takeWhile
                                (\entry ->
                                    case entry of
                                        DronesWindowEntryDrone _ ->
                                            True

                                        DronesWindowEntryGroup group ->
                                            topmostGroupEntry.header.uiNode.totalDisplayRegion.x
                                                < (group.header.uiNode.totalDisplayRegion.x - 3)
                                )

                    childGroupTrees =
                        dronesGroupTreesFromFlatListOfEntries entriesUpToSibling

                    childDrones =
                        entriesUpToSibling
                            |> List.Extra.takeWhile
                                (\entry ->
                                    case entry of
                                        DronesWindowEntryDrone _ ->
                                            True

                                        DronesWindowEntryGroup _ ->
                                            False
                                )

                    children =
                        [ childDrones, childGroupTrees |> List.map DronesWindowEntryGroup ]
                            |> List.concat
                            |> List.sortBy verticalOffsetFromEntry

                    topmostGroupTree =
                        { header = topmostGroupEntry.header
                        , children = children
                        }

                    bottommostDescendantOffset =
                        enumerateDescendantsOfDronesGroup topmostGroupTree
                            |> List.map verticalOffsetFromEntry
                            |> List.maximum
                            |> Maybe.withDefault (verticalOffsetFromEntry (DronesWindowEntryGroup topmostGroupTree))

                    entriesBelow =
                        entriesOrderedVertically
                            |> List.Extra.dropWhile (verticalOffsetFromEntry >> (\offset -> offset <= bottommostDescendantOffset))
                in
                topmostGroupTree :: dronesGroupTreesFromFlatListOfEntries entriesBelow
            )
        |> Maybe.withDefault []


enumerateAllDronesFromDronesGroup : DronesWindowEntryGroupStructure -> List DronesWindowEntryDroneStructure
enumerateAllDronesFromDronesGroup =
    enumerateDescendantsOfDronesGroup
        >> List.filterMap
            (\entry ->
                case entry of
                    DronesWindowEntryDrone drone ->
                        Just drone

                    DronesWindowEntryGroup _ ->
                        Nothing
            )


enumerateDescendantsOfDronesGroup : DronesWindowEntryGroupStructure -> List DronesWindowEntry
enumerateDescendantsOfDronesGroup group =
    group.children
        |> List.concatMap
            (\child ->
                case child of
                    DronesWindowEntryDrone _ ->
                        [ child ]

                    DronesWindowEntryGroup childGroup ->
                        child :: enumerateDescendantsOfDronesGroup childGroup
            )


parseDronesWindowDroneGroupHeader : UITreeNodeWithDisplayRegion -> Maybe DronesWindowDroneGroupHeader
parseDronesWindowDroneGroupHeader groupHeaderUiNode =
    case
        groupHeaderUiNode
            |> getAllContainedDisplayTextsWithRegion
            |> List.sortBy (Tuple.second >> .totalDisplayRegion >> areaFromDisplayRegion >> Maybe.withDefault 0)
            |> List.map Tuple.first
            |> List.head
    of
        Nothing ->
            Nothing

        Just mainText ->
            let
                quantityFromTitle =
                    mainText
                        |> parseQuantityFromDroneGroupTitleText
                        |> Result.withDefault Nothing
            in
            Just
                { uiNode = groupHeaderUiNode
                , mainText = Just mainText
                , quantityFromTitle = quantityFromTitle
                }


parseQuantityFromDroneGroupTitleText : String -> Result String (Maybe DronesWindowDroneGroupHeaderQuantity)
parseQuantityFromDroneGroupTitleText droneGroupTitleText =
    case droneGroupTitleText |> String.split "(" |> List.drop 1 of
        [] ->
            Ok Nothing

        [ textAfterOpeningParenthesis ] ->
            case textAfterOpeningParenthesis |> String.split ")" |> List.head of
                Nothing ->
                    Err "Missing closing parens"

                Just textInParens ->
                    case
                        textInParens
                            |> String.split "/"
                            |> List.map String.trim
                            |> List.map
                                (\numberText ->
                                    numberText
                                        |> String.toInt
                                        |> Result.fromMaybe ("Failed to parse to integer from '" ++ numberText ++ "'")
                                )
                            |> Result.Extra.combine
                    of
                        Err err ->
                            Err ("Failed to parse numbers in parentheses: " ++ err)

                        Ok integersInParens ->
                            case integersInParens of
                                [ singleNumber ] ->
                                    Ok (Just { current = singleNumber, maximum = Nothing })

                                [ firstNumber, secondNumber ] ->
                                    Ok (Just { current = firstNumber, maximum = Just secondNumber })

                                _ ->
                                    Err "Found unexpected number of numbers in parentheses."

        _ ->
            Err "Found unexpected number of parentheses."


parseDronesWindowDroneEntry : UITreeNodeWithDisplayRegion -> DronesWindowEntryDroneStructure
parseDronesWindowDroneEntry droneEntryNode =
    let
        mainText =
            droneEntryNode
                |> getAllContainedDisplayTextsWithRegion
                |> List.sortBy (Tuple.second >> .totalDisplayRegion >> areaFromDisplayRegion >> Maybe.withDefault 0)
                |> List.map Tuple.first
                |> List.head

        gaugeValuePercentFromContainerName containerName =
            droneEntryNode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> getNameFromDictEntries >> (==) (Just containerName))
                |> List.head
                |> Maybe.andThen
                    (\gaugeNode ->
                        let
                            gaudeDescendantFromName gaugeDescendantName =
                                gaugeNode
                                    |> listDescendantsWithDisplayRegion
                                    |> List.filter (.uiNode >> getNameFromDictEntries >> (==) (Just gaugeDescendantName))
                                    |> List.head
                        in
                        gaudeDescendantFromName "droneGaugeBar"
                            |> Maybe.andThen
                                (\gaugeBar ->
                                    gaudeDescendantFromName "droneGaugeBarDmg"
                                        |> Maybe.map
                                            (\droneGaugeBarDmg ->
                                                ((gaugeBar.totalDisplayRegion.width - droneGaugeBarDmg.totalDisplayRegion.width) * 100)
                                                    // gaugeBar.totalDisplayRegion.width
                                            )
                                )
                    )

        hitpointsPercent =
            gaugeValuePercentFromContainerName "gauge_shield"
                |> Maybe.andThen
                    (\shieldPercent ->
                        gaugeValuePercentFromContainerName "gauge_armor"
                            |> Maybe.andThen
                                (\armorPercent ->
                                    gaugeValuePercentFromContainerName "gauge_struct"
                                        |> Maybe.map
                                            (\structPercent ->
                                                { shield = shieldPercent
                                                , armor = armorPercent
                                                , structure = structPercent
                                                }
                                            )
                                )
                    )
    in
    { uiNode = droneEntryNode
    , mainText = mainText
    , hitpointsPercent = hitpointsPercent
    }


parseProbeScannerWindowFromUITreeRoot : UITreeNodeWithDisplayRegion -> Maybe ProbeScannerWindow
parseProbeScannerWindowFromUITreeRoot uiTreeRoot =
    case
        uiTreeRoot
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "ProbeScannerWindow")
            |> List.head
    of
        Nothing ->
            Nothing

        Just windowNode ->
            let
                scanResultsNodes =
                    windowNode
                        |> listDescendantsWithDisplayRegion
                        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "ScanResultNew")

                scrollNode =
                    windowNode
                        |> listDescendantsWithDisplayRegion
                        |> List.filter (.uiNode >> getNameFromDictEntries >> Maybe.map (String.contains "ResultsContainer") >> Maybe.withDefault False)
                        |> List.concatMap listDescendantsWithDisplayRegion
                        |> List.filter (.uiNode >> .pythonObjectTypeName >> String.toLower >> String.contains "scroll")
                        |> List.head

                headersContainerNode =
                    scrollNode

                entriesHeaders =
                    headersContainerNode
                        |> Maybe.map getAllContainedDisplayTextsWithRegion
                        |> Maybe.withDefault []

                scanResults =
                    scanResultsNodes
                        |> List.map (parseProbeScanResult entriesHeaders)
            in
            Just { uiNode = windowNode, scanResults = scanResults }


parseProbeScanResult : List ( String, UITreeNodeWithDisplayRegion ) -> UITreeNodeWithDisplayRegion -> ProbeScanResult
parseProbeScanResult entriesHeaders scanResultNode =
    let
        textsLeftToRight =
            scanResultNode
                |> getAllContainedDisplayTextsWithRegion
                |> List.sortBy (Tuple.second >> .totalDisplayRegion >> .x)
                |> List.map Tuple.first

        cellsTexts =
            scanResultNode
                |> getAllContainedDisplayTextsWithRegion
                |> List.filterMap
                    (\( cellText, cell ) ->
                        let
                            cellMiddle =
                                cell.totalDisplayRegion.x + (cell.totalDisplayRegion.width // 2)

                            maybeHeader =
                                entriesHeaders
                                    |> List.filter
                                        (\( _, header ) ->
                                            header.totalDisplayRegion.x
                                                < cellMiddle
                                                + 1
                                                && cellMiddle
                                                < header.totalDisplayRegion.x
                                                + header.totalDisplayRegion.width
                                                - 1
                                        )
                                    |> List.head
                        in
                        maybeHeader
                            |> Maybe.map (\( headerText, _ ) -> ( headerText, cellText ))
                    )
                |> Dict.fromList

        warpButton =
            scanResultNode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> getTexturePathFromDictEntries >> Maybe.map (String.endsWith "44_32_18.png") >> Maybe.withDefault False)
                |> List.head
    in
    { uiNode = scanResultNode
    , textsLeftToRight = textsLeftToRight
    , cellsTexts = cellsTexts
    , warpButton = warpButton
    }


parseDirectionalScannerWindowFromUITreeRoot : UITreeNodeWithDisplayRegion -> Maybe DirectionalScannerWindow
parseDirectionalScannerWindowFromUITreeRoot uiTreeRoot =
    case
        uiTreeRoot
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "DirectionalScanner")
            |> List.head
    of
        Nothing ->
            Nothing

        Just windowNode ->
            let
                scrollNode =
                    windowNode
                        |> listDescendantsWithDisplayRegion
                        |> List.filter (.uiNode >> .pythonObjectTypeName >> String.toLower >> String.contains "scroll")
                        |> List.sortBy (.totalDisplayRegion >> areaFromDisplayRegion >> Maybe.withDefault 0 >> negate)
                        |> List.head

                scanResultsNodes =
                    scrollNode
                        |> Maybe.map listDescendantsWithDisplayRegion
                        |> Maybe.withDefault []
                        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "DirectionalScanResultEntry")
            in
            Just
                { uiNode = windowNode
                , scrollNode = scrollNode
                , scanResults = scanResultsNodes
                }


parseStationWindowFromUITreeRoot : UITreeNodeWithDisplayRegion -> Maybe StationWindow
parseStationWindowFromUITreeRoot uiTreeRoot =
    case
        uiTreeRoot
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "LobbyWnd")
            |> List.head
    of
        Nothing ->
            Nothing

        Just windowNode ->
            let
                buttonFromDisplayText textToSearch =
                    let
                        textToSearchLowercase =
                            String.toLower textToSearch

                        textMatches text =
                            text == textToSearchLowercase || (text |> String.contains (">" ++ textToSearchLowercase ++ "<"))
                    in
                    findButtonInDescendantsByDisplayTextsPredicate
                        (List.any (String.toLower >> textMatches))
                        windowNode
            in
            Just
                { uiNode = windowNode
                , undockButton = buttonFromDisplayText "undock"
                , abortUndockButton = buttonFromDisplayText "undocking"
                }


parseInventoryWindowsFromUITreeRoot : UITreeNodeWithDisplayRegion -> List InventoryWindow
parseInventoryWindowsFromUITreeRoot uiTreeRoot =
    uiTreeRoot
        |> listDescendantsWithDisplayRegion
        |> List.filter (\uiNode -> [ "InventoryPrimary", "ActiveShipCargo" ] |> List.member uiNode.uiNode.pythonObjectTypeName)
        |> List.map parseInventoryWindow


parseInventoryWindow : UITreeNodeWithDisplayRegion -> InventoryWindow
parseInventoryWindow windowUiNode =
    let
        selectedContainerCapacityGaugeNode =
            windowUiNode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> .pythonObjectTypeName >> String.contains "CapacityGauge")
                |> List.head

        selectedContainerCapacityGauge =
            selectedContainerCapacityGaugeNode
                |> Maybe.map (.uiNode >> EveOnline.MemoryReading.listDescendantsInUITreeNode)
                |> Maybe.withDefault []
                |> List.filterMap getDisplayText
                |> List.sortBy (String.length >> negate)
                |> List.head
                |> Maybe.map parseInventoryCapacityGaugeText

        leftTreeEntriesRootNodes =
            windowUiNode |> getContainedTreeViewEntryRootNodes

        leftTreeEntries =
            leftTreeEntriesRootNodes |> List.map parseInventoryWindowTreeViewEntry

        rightContainerNode =
            windowUiNode
                |> listDescendantsWithDisplayRegion
                |> List.filter
                    (\uiNode ->
                        (uiNode.uiNode.pythonObjectTypeName == "Container")
                            && (uiNode.uiNode |> getNameFromDictEntries |> Maybe.map (String.contains "right") |> Maybe.withDefault False)
                    )
                |> List.head

        subCaptionLabelText =
            rightContainerNode
                |> Maybe.map listDescendantsWithDisplayRegion
                |> Maybe.withDefault []
                |> List.filter (.uiNode >> getNameFromDictEntries >> Maybe.map (String.startsWith "subCaptionLabel") >> Maybe.withDefault False)
                |> List.concatMap (.uiNode >> getAllContainedDisplayTexts)
                |> List.head

        maybeSelectedContainerInventoryNode =
            rightContainerNode
                |> Maybe.andThen
                    (listDescendantsWithDisplayRegion
                        >> List.filter
                            (\uiNode ->
                                [ "ShipCargo", "ShipDroneBay", "ShipGeneralMiningHold", "StationItems", "ShipFleetHangar", "StructureItemHangar" ]
                                    |> List.member uiNode.uiNode.pythonObjectTypeName
                            )
                        >> List.head
                    )

        selectedContainerInventory =
            maybeSelectedContainerInventoryNode
                |> Maybe.map parseInventory

        buttonToSwitchToListView =
            rightContainerNode
                |> Maybe.map listDescendantsWithDisplayRegion
                |> Maybe.withDefault []
                |> List.filter
                    (\uiNode ->
                        (uiNode.uiNode.pythonObjectTypeName |> String.contains "ButtonIcon")
                            && ((uiNode.uiNode |> getTexturePathFromDictEntries |> Maybe.withDefault "") |> String.endsWith "38_16_190.png")
                    )
                |> List.head

        buttonToStackAll =
            rightContainerNode
                |> Maybe.map listDescendantsWithDisplayRegion
                |> Maybe.withDefault []
                |> List.filter
                    (\uiNode ->
                        (uiNode.uiNode.pythonObjectTypeName |> String.contains "ButtonIcon")
                            && (uiNode.uiNode |> getHintTextFromDictEntries |> Maybe.map (String.contains "Stack All") |> Maybe.withDefault False)
                    )
                |> List.head
    in
    { uiNode = windowUiNode
    , leftTreeEntries = leftTreeEntries
    , subCaptionLabelText = subCaptionLabelText
    , selectedContainerCapacityGauge = selectedContainerCapacityGauge
    , selectedContainerInventory = selectedContainerInventory
    , buttonToSwitchToListView = buttonToSwitchToListView
    , buttonToStackAll = buttonToStackAll
    }


parseInventory : UITreeNodeWithDisplayRegion -> Inventory
parseInventory inventoryNode =
    let
        listViewItemNodes =
            inventoryNode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "Item")
                |> subsequenceNotContainedInAnyOtherWithDisplayRegion

        scrollNode =
            inventoryNode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> .pythonObjectTypeName >> String.toLower >> String.contains "scroll")
                |> List.head

        scrollControlsNode =
            scrollNode
                |> Maybe.map listDescendantsWithDisplayRegion
                |> Maybe.withDefault []
                |> List.filter (.uiNode >> .pythonObjectTypeName >> String.contains "ScrollControls")
                |> List.head

        headersContainerNode =
            scrollNode
                |> Maybe.andThen
                    (getMostPopulousDescendantWithDisplayRegionMatchingPredicate
                        (predicateAny
                            [ .uiNode >> .pythonObjectTypeName >> String.toLower >> String.contains "headers"
                            , .uiNode
                                >> getNameFromDictEntries
                                >> Maybe.map (String.toLower >> String.contains "headers")
                                >> Maybe.withDefault False
                            ]
                        )
                    )

        entriesHeaders =
            headersContainerNode
                |> Maybe.map getAllContainedDisplayTextsWithRegion
                |> Maybe.withDefault []
                |> List.Extra.uniqueBy Tuple.first

        notListViewItemNodes =
            inventoryNode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> .pythonObjectTypeName >> String.contains "InvItem")
                |> subsequenceNotContainedInAnyOtherWithDisplayRegion

        itemsView =
            if 0 < (listViewItemNodes |> List.length) then
                Just
                    (InventoryItemsListView
                        { items =
                            listViewItemNodes
                                |> List.map (parseInventoryItemsListViewEntry entriesHeaders)
                        }
                    )

            else if 0 < (notListViewItemNodes |> List.length) then
                Just (InventoryItemsNotListView { items = notListViewItemNodes })

            else
                Nothing
    in
    { uiNode = inventoryNode
    , itemsView = itemsView
    , scrollControls = scrollControlsNode |> Maybe.map parseScrollControls
    }


parseInventoryItemsListViewEntry :
    List ( String, UITreeNodeWithDisplayRegion )
    -> UITreeNodeWithDisplayRegion
    -> InventoryItemsListViewEntry
parseInventoryItemsListViewEntry entriesHeaders inventoryEntryNode =
    let
        listViewEntry =
            parseListViewEntry entriesHeaders inventoryEntryNode
    in
    { uiNode = inventoryEntryNode
    , cellsTexts = listViewEntry.cellsTexts
    }


parseListViewEntry :
    List ( String, UITreeNodeWithDisplayRegion )
    -> UITreeNodeWithDisplayRegion
    -> { cellsTexts : Dict.Dict String String }
parseListViewEntry entriesHeaders listViewEntryNode =
    {-
       Observations show two different kinds of representations of the texts in the cells in a list view:

       + Each cell text in a dedicated UI element. (Overview entry)
       + All cell texts in a single UI element, separated by a tab-tag (<t>) (Inventory item)

       Following is an example of the latter case:
       Condensed Scordite<t><right>200<t>Scordite<t><t><t><right>30 m3<t><right>2.290,00 ISK
    -}
    case entriesHeaders |> List.head of
        Nothing ->
            { cellsTexts = Dict.empty }

        Just leftmostHeader ->
            let
                headerRegionMatchesCellRegion headerRegion cellRegion =
                    (headerRegion.x < cellRegion.x + 3)
                        && (headerRegion.x + headerRegion.width > cellRegion.x + cellRegion.width - 3)

                cellsTexts =
                    listViewEntryNode
                        |> getAllContainedDisplayTextsWithRegion
                        |> List.concatMap
                            (\( cellText, cell ) ->
                                let
                                    distanceFromLeftmostHeader =
                                        cell.totalDisplayRegion.x - (Tuple.second leftmostHeader).totalDisplayRegion.x

                                    maybeHeaderByCellRegion =
                                        entriesHeaders
                                            |> List.filter
                                                (\( _, header ) ->
                                                    headerRegionMatchesCellRegion
                                                        header.totalDisplayRegion
                                                        cell.totalDisplayRegion
                                                )
                                            |> List.head
                                in
                                case maybeHeaderByCellRegion of
                                    Just ( headerText, _ ) ->
                                        [ ( headerText, cellText ) ]

                                    Nothing ->
                                        if abs distanceFromLeftmostHeader < 4 then
                                            []

                                        else
                                            cellText
                                                |> String.split "<t>"
                                                |> List.map String.trim
                                                |> List.indexedMap Tuple.pair
                                                |> List.filterMap
                                                    (\( cellIndex, cellSubText ) ->
                                                        entriesHeaders
                                                            |> List.drop cellIndex
                                                            |> List.head
                                                            |> Maybe.map
                                                                (\( headerText, _ ) ->
                                                                    ( headerText, cellSubText )
                                                                )
                                                    )
                            )
                        |> Dict.fromList
            in
            { cellsTexts = cellsTexts }


getContainedTreeViewEntryRootNodes : UITreeNodeWithDisplayRegion -> List UITreeNodeWithDisplayRegion
getContainedTreeViewEntryRootNodes parentNode =
    let
        leftTreeEntriesAllNodes =
            parentNode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> .pythonObjectTypeName >> String.startsWith "TreeViewEntry")

        isContainedInTreeEntry candidate =
            leftTreeEntriesAllNodes
                |> List.concatMap listDescendantsWithDisplayRegion
                |> List.member candidate
    in
    leftTreeEntriesAllNodes
        |> List.filter (isContainedInTreeEntry >> not)


parseInventoryWindowTreeViewEntry : UITreeNodeWithDisplayRegion -> InventoryWindowLeftTreeEntry
parseInventoryWindowTreeViewEntry treeEntryNode =
    let
        topContNode =
            treeEntryNode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> getNameFromDictEntries >> Maybe.map (String.startsWith "topCont_") >> Maybe.withDefault False)
                |> List.sortBy (.totalDisplayRegion >> .y)
                |> List.head

        toggleBtn =
            topContNode
                |> Maybe.map listDescendantsWithDisplayRegion
                |> Maybe.withDefault []
                |> List.filter (.uiNode >> getNameFromDictEntries >> Maybe.map ((==) "toggleBtn") >> Maybe.withDefault False)
                |> List.head

        text =
            topContNode
                |> Maybe.map getAllContainedDisplayTextsWithRegion
                |> Maybe.withDefault []
                |> List.sortBy (Tuple.second >> .totalDisplayRegion >> .y)
                |> List.head
                |> Maybe.map Tuple.first
                |> Maybe.withDefault ""

        childrenNodes =
            treeEntryNode |> getContainedTreeViewEntryRootNodes

        children =
            childrenNodes |> List.map (parseInventoryWindowTreeViewEntry >> InventoryWindowLeftTreeEntryChild)
    in
    { uiNode = treeEntryNode
    , toggleBtn = toggleBtn
    , selectRegion = topContNode
    , text = text
    , children = children
    }


unwrapInventoryWindowLeftTreeEntryChild : InventoryWindowLeftTreeEntryChild -> InventoryWindowLeftTreeEntry
unwrapInventoryWindowLeftTreeEntryChild child =
    case child of
        InventoryWindowLeftTreeEntryChild unpacked ->
            unpacked


parseInventoryCapacityGaugeText : String -> Result String InventoryWindowCapacityGauge
parseInventoryCapacityGaugeText capacityText =
    let
        parseMaybeNumber =
            Maybe.map (String.trim >> parseNumberTruncatingAfterOptionalDecimalSeparator >> Result.map Just)
                >> Maybe.withDefault (Ok Nothing)

        continueWithTexts { usedText, maybeMaximumText, maybeSelectedText } =
            case usedText |> parseNumberTruncatingAfterOptionalDecimalSeparator of
                Err parseNumberError ->
                    Err ("Failed to parse used number: " ++ parseNumberError)

                Ok used ->
                    case maybeMaximumText |> parseMaybeNumber of
                        Err parseNumberError ->
                            Err ("Failed to parse maximum number: " ++ parseNumberError)

                        Ok maximum ->
                            case maybeSelectedText |> parseMaybeNumber of
                                Err parseNumberError ->
                                    Err ("Failed to parse selected number: " ++ parseNumberError)

                                Ok selected ->
                                    Ok { used = used, maximum = maximum, selected = selected }

        continueAfterSeparatingBySlash { beforeSlashText, afterSlashMaybeText } =
            case beforeSlashText |> String.trim |> String.split ")" of
                [ onlyUsedText ] ->
                    continueWithTexts { usedText = onlyUsedText, maybeMaximumText = afterSlashMaybeText, maybeSelectedText = Nothing }

                [ firstPart, secondPart ] ->
                    continueWithTexts { usedText = secondPart, maybeMaximumText = afterSlashMaybeText, maybeSelectedText = Just (firstPart |> String.replace "(" "") }

                _ ->
                    Err ("Unexpected number of components in text before slash '" ++ beforeSlashText ++ "'")
    in
    case capacityText |> String.replace "m³" "" |> String.split "/" of
        [ withoutSlash ] ->
            continueAfterSeparatingBySlash { beforeSlashText = withoutSlash, afterSlashMaybeText = Nothing }

        [ partBeforeSlash, partAfterSlash ] ->
            continueAfterSeparatingBySlash { beforeSlashText = partBeforeSlash, afterSlashMaybeText = Just partAfterSlash }

        _ ->
            Err ("Unexpected number of components in capacityText '" ++ capacityText ++ "'")


parseModuleButtonTooltipFromUITreeRoot : UITreeNodeWithDisplayRegion -> Maybe ModuleButtonTooltip
parseModuleButtonTooltipFromUITreeRoot uiTreeRoot =
    case
        uiTreeRoot
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "ModuleButtonTooltip")
            |> List.head
    of
        Nothing ->
            Nothing

        Just uiNode ->
            Just (parseModuleButtonTooltip uiNode)


parseModuleButtonTooltip : UITreeNodeWithDisplayRegion -> ModuleButtonTooltip
parseModuleButtonTooltip tooltipUINode =
    let
        upperRightCornerFromDisplayRegion region =
            { x = region.x + region.width, y = region.y }

        distanceSquared a b =
            let
                distanceX =
                    a.x - b.x

                distanceY =
                    a.y - b.y
            in
            distanceX * distanceX + distanceY * distanceY

        shortcutCandidates =
            tooltipUINode
                |> getAllContainedDisplayTextsWithRegion
                |> List.map
                    (\( text, textUINode ) ->
                        { text = text
                        , distanceUpperRightCornerSquared =
                            distanceSquared
                                (textUINode.totalDisplayRegion |> upperRightCornerFromDisplayRegion)
                                (tooltipUINode.totalDisplayRegion |> upperRightCornerFromDisplayRegion)
                        }
                    )
                |> List.sortBy .distanceUpperRightCornerSquared

        shortcut =
            shortcutCandidates
                |> List.filter (\textAndDistance -> textAndDistance.distanceUpperRightCornerSquared < 1000)
                |> List.head
                |> Maybe.map (\{ text } -> { text = text, parseResult = text |> parseModuleButtonTooltipShortcut })

        optimalRangeString =
            tooltipUINode.uiNode
                |> getAllContainedDisplayTexts
                |> List.filterMap
                    (\text ->
                        "Optimal range (|within)\\s*([\\d\\.]+\\s*[km]+)"
                            |> Regex.fromString
                            |> Maybe.andThen (\regex -> text |> Regex.find regex |> List.head)
                            |> Maybe.andThen (.submatches >> List.drop 1 >> List.head)
                            |> Maybe.andThen identity
                            |> Maybe.map String.trim
                    )
                |> List.head

        optimalRange =
            optimalRangeString
                |> Maybe.map (\asString -> { asString = asString, inMeters = asString |> parseOverviewEntryDistanceInMetersFromText })
    in
    { uiNode = tooltipUINode
    , shortcut = shortcut
    , optimalRange = optimalRange
    }


parseModuleButtonTooltipShortcut : String -> Result String (List Common.EffectOnWindow.VirtualKeyCode)
parseModuleButtonTooltipShortcut shortcutText =
    shortcutText
        |> String.split "-"
        |> List.concatMap (String.split "+")
        |> List.map String.trim
        |> List.filter (String.length >> (<) 0)
        |> List.foldl
            (\nextKeyText previousResult ->
                previousResult
                    |> Result.andThen
                        (\previousKeys ->
                            case nextKeyText |> parseKeyShortcutText of
                                Just nextKey ->
                                    Ok (nextKey :: previousKeys)

                                Nothing ->
                                    Err ("Unknown key text: '" ++ nextKeyText ++ "'")
                        )
            )
            (Ok [])
        |> Result.map List.reverse


parseHeatStatusTooltipFromUITreeRoot : UITreeNodeWithDisplayRegion -> Maybe HeatStatusTooltip
parseHeatStatusTooltipFromUITreeRoot uiTreeRoot =
    uiTreeRoot
        |> listDescendantsWithDisplayRegion
        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "TooltipPanel")
        |> List.filter
            (getAllContainedDisplayTextsWithRegion
                >> List.sortBy (Tuple.second >> .totalDisplayRegion >> .y)
                >> List.head
                >> Maybe.map (Tuple.first >> String.contains "Heat Status")
                >> Maybe.withDefault False
            )
        |> List.head
        |> Maybe.map parseHeatStatusTooltip


parseHeatStatusTooltip : UITreeNodeWithDisplayRegion -> HeatStatusTooltip
parseHeatStatusTooltip tooltipNode =
    let
        parsePercentFromPrefix prefix =
            tooltipNode.uiNode
                |> getAllContainedDisplayTexts
                |> List.map String.trim
                |> List.filter (String.toLower >> String.startsWith prefix)
                |> List.head
                |> Maybe.map (String.split " " >> List.filter (String.isEmpty >> not) >> List.drop 1 >> String.join "")
                |> Maybe.andThen (String.split "%" >> List.head)
                |> Maybe.andThen String.toInt
    in
    { uiNode = tooltipNode
    , lowPercent = parsePercentFromPrefix "low"
    , mediumPercent = parsePercentFromPrefix "medium"
    , highPercent = parsePercentFromPrefix "high"
    }


parseKeyShortcutText : String -> Maybe Common.EffectOnWindow.VirtualKeyCode
parseKeyShortcutText keyText =
    [ ( "CTRL", Common.EffectOnWindow.vkey_LCONTROL )
    , ( "STRG", Common.EffectOnWindow.vkey_LCONTROL )
    , ( "ALT", Common.EffectOnWindow.vkey_LMENU )
    , ( "SHIFT", Common.EffectOnWindow.vkey_LSHIFT )
    , ( "UMSCH", Common.EffectOnWindow.vkey_LSHIFT )
    , ( "F1", Common.EffectOnWindow.vkey_F1 )
    , ( "F2", Common.EffectOnWindow.vkey_F2 )
    , ( "F3", Common.EffectOnWindow.vkey_F3 )
    , ( "F4", Common.EffectOnWindow.vkey_F4 )
    , ( "F5", Common.EffectOnWindow.vkey_F5 )
    , ( "F6", Common.EffectOnWindow.vkey_F6 )
    , ( "F7", Common.EffectOnWindow.vkey_F7 )
    , ( "F8", Common.EffectOnWindow.vkey_F8 )
    , ( "F9", Common.EffectOnWindow.vkey_F9 )
    , ( "F10", Common.EffectOnWindow.vkey_F10 )
    , ( "F11", Common.EffectOnWindow.vkey_F11 )
    , ( "F12", Common.EffectOnWindow.vkey_F12 )
    ]
        |> Dict.fromList
        |> Dict.get (keyText |> String.toUpper)


parseChatWindowStacksFromUITreeRoot : UITreeNodeWithDisplayRegion -> List ChatWindowStack
parseChatWindowStacksFromUITreeRoot uiTreeRoot =
    uiTreeRoot
        |> listDescendantsWithDisplayRegion
        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "ChatWindowStack")
        |> List.map parseChatWindowStack


parseChatWindowStack : UITreeNodeWithDisplayRegion -> ChatWindowStack
parseChatWindowStack chatWindowStackUiNode =
    let
        chatWindowNode =
            chatWindowStackUiNode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "XmppChatWindow")
                |> List.head
    in
    { uiNode = chatWindowStackUiNode
    , chatWindow = chatWindowNode |> Maybe.map parseChatWindow
    }


parseChatWindow : UITreeNodeWithDisplayRegion -> ChatWindow
parseChatWindow chatWindowUiNode =
    let
        userlistNode =
            chatWindowUiNode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> getNameFromDictEntries >> Maybe.map (String.toLower >> String.contains "userlist") >> Maybe.withDefault False)
                |> List.head
    in
    { uiNode = chatWindowUiNode
    , name = getNameFromDictEntries chatWindowUiNode.uiNode
    , userlist = userlistNode |> Maybe.map parseChatWindowUserlist
    }


parseChatWindowUserlist : UITreeNodeWithDisplayRegion -> ChatWindowUserlist
parseChatWindowUserlist userlistNode =
    let
        visibleUsers =
            userlistNode
                |> listDescendantsWithDisplayRegion
                |> List.filter (\uiNode -> [ "XmppChatSimpleUserEntry", "XmppChatUserEntry" ] |> List.member uiNode.uiNode.pythonObjectTypeName)
                |> List.map parseChatUserEntry

        scrollControls =
            userlistNode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> .pythonObjectTypeName >> String.contains "ScrollControls")
                |> List.head
                |> Maybe.map parseScrollControls
    in
    { uiNode = userlistNode, visibleUsers = visibleUsers, scrollControls = scrollControls }


parseChatUserEntry : UITreeNodeWithDisplayRegion -> ChatUserEntry
parseChatUserEntry chatUserUiNode =
    let
        standingIconNode =
            chatUserUiNode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "FlagIconWithState")
                |> List.head

        name =
            chatUserUiNode.uiNode
                |> getAllContainedDisplayTexts
                |> List.sortBy String.length
                |> List.reverse
                |> List.head

        standingIconHint =
            standingIconNode
                |> Maybe.andThen (.uiNode >> getHintTextFromDictEntries)
    in
    { uiNode = chatUserUiNode
    , name = name
    , standingIconHint = standingIconHint
    }


parseAgentConversationWindowsFromUITreeRoot : UITreeNodeWithDisplayRegion -> List AgentConversationWindow
parseAgentConversationWindowsFromUITreeRoot uiTreeRoot =
    uiTreeRoot
        |> listDescendantsWithDisplayRegion
        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "AgentDialogueWindow")
        |> List.map parseAgentConversationWindow


parseAgentConversationWindow : UITreeNodeWithDisplayRegion -> AgentConversationWindow
parseAgentConversationWindow windowUINode =
    { uiNode = windowUINode
    }


parseMarketOrdersWindowFromUITreeRoot : UITreeNodeWithDisplayRegion -> Maybe MarketOrdersWindow
parseMarketOrdersWindowFromUITreeRoot uiTreeRoot =
    uiTreeRoot
        |> listDescendantsWithDisplayRegion
        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "MarketOrdersWnd")
        |> List.head
        |> Maybe.map parseMarketOrdersWindow


parseMarketOrdersWindow : UITreeNodeWithDisplayRegion -> MarketOrdersWindow
parseMarketOrdersWindow windowUINode =
    { uiNode = windowUINode
    }


parseFittingWindowFromUITreeRoot : UITreeNodeWithDisplayRegion -> Maybe FittingWindow
parseFittingWindowFromUITreeRoot uiTreeRoot =
    uiTreeRoot
        |> listDescendantsWithDisplayRegion
        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "FittingWindow")
        |> List.head
        |> Maybe.map parseFittingWindow


parseFittingWindow : UITreeNodeWithDisplayRegion -> FittingWindow
parseFittingWindow windowUINode =
    { uiNode = windowUINode
    }


parseSurveyScanWindowFromUITreeRoot : UITreeNodeWithDisplayRegion -> Maybe SurveyScanWindow
parseSurveyScanWindowFromUITreeRoot uiTreeRoot =
    uiTreeRoot
        |> listDescendantsWithDisplayRegion
        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "SurveyScanView")
        |> List.head
        |> Maybe.map parseSurveyScanWindow


parseSurveyScanWindow : UITreeNodeWithDisplayRegion -> SurveyScanWindow
parseSurveyScanWindow windowUINode =
    { uiNode = windowUINode
    , scanEntries =
        windowUINode
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "SurveyScanEntry")
    }


parseBookmarkLocationWindowFromUITreeRoot : UITreeNodeWithDisplayRegion -> Maybe BookmarkLocationWindow
parseBookmarkLocationWindowFromUITreeRoot uiTreeRoot =
    uiTreeRoot
        |> listDescendantsWithDisplayRegion
        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "BookmarkLocationWindow")
        |> List.head
        |> Maybe.map parseBookmarkLocationWindow


parseBookmarkLocationWindow : UITreeNodeWithDisplayRegion -> BookmarkLocationWindow
parseBookmarkLocationWindow windowUINode =
    { uiNode = windowUINode
    , submitButton = findButtonInDescendantsContainingDisplayText "submit" windowUINode
    , cancelButton = findButtonInDescendantsContainingDisplayText "cancel" windowUINode
    }


parseRepairShopWindowFromUITreeRoot : UITreeNodeWithDisplayRegion -> Maybe RepairShopWindow
parseRepairShopWindowFromUITreeRoot uiTreeRoot =
    uiTreeRoot
        |> listDescendantsWithDisplayRegion
        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "RepairShopWindow")
        |> List.head
        |> Maybe.map parseRepairShopWindow


parseRepairShopWindow : UITreeNodeWithDisplayRegion -> RepairShopWindow
parseRepairShopWindow windowUINode =
    let
        buttonGroup =
            windowUINode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> .pythonObjectTypeName >> String.contains "ButtonGroup")
                |> List.head

        buttons =
            buttonGroup
                |> Maybe.map listDescendantsWithDisplayRegion
                |> Maybe.withDefault []
                |> List.filter (.uiNode >> .pythonObjectTypeName >> String.contains "Button")
                |> List.map
                    (\buttonNode ->
                        { uiNode = buttonNode
                        , mainText =
                            buttonNode
                                |> getAllContainedDisplayTextsWithRegion
                                |> List.sortBy (Tuple.second >> .totalDisplayRegion >> areaFromDisplayRegion >> Maybe.withDefault 0)
                                |> List.map Tuple.first
                                |> List.head
                        }
                    )
    in
    { uiNode = windowUINode
    , items =
        windowUINode
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "Item")
    , buttonGroup = buttonGroup
    , buttons = buttons
    }


parseCharacterSheetWindowFromUITreeRoot : UITreeNodeWithDisplayRegion -> Maybe CharacterSheetWindow
parseCharacterSheetWindowFromUITreeRoot uiTreeRoot =
    uiTreeRoot
        |> listDescendantsWithDisplayRegion
        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "CharacterSheetWindow")
        |> List.head
        |> Maybe.map parseCharacterSheetWindow


parseCharacterSheetWindow : UITreeNodeWithDisplayRegion -> CharacterSheetWindow
parseCharacterSheetWindow windowUINode =
    let
        skillGroups =
            windowUINode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> .pythonObjectTypeName >> String.contains "SkillGroupGauge")
    in
    { uiNode = windowUINode
    , skillGroups = skillGroups
    }


parseFleetWindowFromUITreeRoot : UITreeNodeWithDisplayRegion -> Maybe FleetWindow
parseFleetWindowFromUITreeRoot uiTreeRoot =
    uiTreeRoot
        |> listDescendantsWithDisplayRegion
        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "FleetWindow")
        |> List.head
        |> Maybe.map parseFleetWindow


parseFleetWindow : UITreeNodeWithDisplayRegion -> FleetWindow
parseFleetWindow windowUINode =
    let
        fleetMembers =
            windowUINode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "FleetMember")
    in
    { uiNode = windowUINode
    , fleetMembers = fleetMembers
    }


parseWatchListPanelFromUITreeRoot : UITreeNodeWithDisplayRegion -> Maybe WatchListPanel
parseWatchListPanelFromUITreeRoot uiTreeRoot =
    uiTreeRoot
        |> listDescendantsWithDisplayRegion
        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "WatchListPanel")
        |> List.head
        |> Maybe.map parseWatchListPanel


parseWatchListPanel : UITreeNodeWithDisplayRegion -> WatchListPanel
parseWatchListPanel windowUINode =
    let
        entries =
            windowUINode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "WatchListEntry")
    in
    { uiNode = windowUINode
    , entries = entries
    }


parseStandaloneBookmarkWindowFromUITreeRoot : UITreeNodeWithDisplayRegion -> Maybe StandaloneBookmarkWindow
parseStandaloneBookmarkWindowFromUITreeRoot uiTreeRoot =
    uiTreeRoot
        |> listDescendantsWithDisplayRegion
        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "StandaloneBookmarkWnd")
        |> List.head
        |> Maybe.map parseStandaloneBookmarkWindow


parseStandaloneBookmarkWindow : UITreeNodeWithDisplayRegion -> StandaloneBookmarkWindow
parseStandaloneBookmarkWindow windowUINode =
    let
        entries =
            windowUINode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "PlaceEntry")
    in
    { uiNode = windowUINode
    , entries = entries
    }


parseNeocomFromUITreeRoot : UITreeNodeWithDisplayRegion -> Maybe Neocom
parseNeocomFromUITreeRoot uiTreeRoot =
    case
        uiTreeRoot
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "Neocom")
            |> List.head
    of
        Nothing ->
            Nothing

        Just uiNode ->
            Just (parseNeocom uiNode)


parseNeocom : UITreeNodeWithDisplayRegion -> Neocom
parseNeocom neocomUiNode =
    let
        maybeClockTextAndNode =
            neocomUiNode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "InGameClock")
                |> List.concatMap getAllContainedDisplayTextsWithRegion
                |> List.head

        nodeFromTexturePathEnd texturePathEnd =
            neocomUiNode
                |> listDescendantsWithDisplayRegion
                |> List.filter
                    (.uiNode
                        >> getTexturePathFromDictEntries
                        >> Maybe.map (String.endsWith texturePathEnd)
                        >> Maybe.withDefault False
                    )
                |> List.head

        clock =
            maybeClockTextAndNode
                |> Maybe.map
                    (\( clockText, clockNode ) ->
                        { uiNode = clockNode
                        , text = clockText
                        , parsedText = parseNeocomClockText clockText
                        }
                    )
    in
    { uiNode = neocomUiNode
    , iconInventory = nodeFromTexturePathEnd "items.png"
    , clock = clock
    }


parseNeocomClockText : String -> Result String { hour : Int, minute : Int }
parseNeocomClockText clockText =
    case clockText |> String.split ":" of
        [ hourText, minuteText ] ->
            case hourText |> String.trim |> String.toInt of
                Nothing ->
                    Err ("Failed to parse hour: '" ++ hourText ++ "'")

                Just hour ->
                    case minuteText |> String.trim |> String.toInt of
                        Nothing ->
                            Err ("Failed to parse minute: '" ++ minuteText ++ "'")

                        Just minute ->
                            Ok { hour = hour, minute = minute }

        _ ->
            Err "Expecting exactly two substrings separated by a colon (:)."


parseKeyActivationWindowFromUITreeRoot : UITreeNodeWithDisplayRegion -> Maybe KeyActivationWindow
parseKeyActivationWindowFromUITreeRoot uiTreeRoot =
    uiTreeRoot
        |> listDescendantsWithDisplayRegion
        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "KeyActivationWindow")
        |> List.head
        |> Maybe.map parseKeyActivationWindow


parseKeyActivationWindow : UITreeNodeWithDisplayRegion -> KeyActivationWindow
parseKeyActivationWindow windowUiNode =
    let
        activateButton =
            windowUiNode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "ActivateButton")
                |> List.head
    in
    { uiNode = windowUiNode
    , activateButton = activateButton
    }


parseCompressionWindowFromUITreeRoot : UITreeNodeWithDisplayRegion -> Maybe CompressionWindow
parseCompressionWindowFromUITreeRoot uiTreeRoot =
    uiTreeRoot
        |> listDescendantsWithDisplayRegion
        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "CompressionWindow")
        |> List.head
        |> Maybe.map parseCompressionWindow


parseCompressionWindow : UITreeNodeWithDisplayRegion -> CompressionWindow
parseCompressionWindow windowUiNode =
    let
        compressButton =
            findButtonInDescendantsContainingDisplayText "Compress" windowUiNode
    in
    { uiNode = windowUiNode
    , windowControls = parseWindowControlsFromWindow windowUiNode
    , compressButton = compressButton
    }


parseWindowControlsFromWindow : UITreeNodeWithDisplayRegion -> Maybe WindowControls
parseWindowControlsFromWindow =
    listDescendantsWithDisplayRegion
        >> List.filter (.uiNode >> .pythonObjectTypeName >> String.contains "WindowControls")
        >> List.head
        >> Maybe.map parseWindowControls


parseWindowControls : UITreeNodeWithDisplayRegion -> WindowControls
parseWindowControls controlsNode =
    let
        nodeFromTexturePathContains texturePathSubstring =
            controlsNode
                |> listDescendantsWithDisplayRegion
                |> List.filter
                    (.uiNode
                        >> getTexturePathFromDictEntries
                        >> Maybe.map (String.toLower >> String.contains (String.toLower texturePathSubstring))
                        >> Maybe.withDefault False
                    )
                |> List.head

        minimizeButton =
            nodeFromTexturePathContains "eveicon/window/minimize"

        closeButton =
            nodeFromTexturePathContains "eveicon/window/close"
    in
    { uiNode = controlsNode
    , minimizeButton = minimizeButton
    , closeButton = closeButton
    }


parseMessageBoxesFromUITreeRoot : UITreeNodeWithDisplayRegion -> List MessageBox
parseMessageBoxesFromUITreeRoot uiTreeRoot =
    let
        messageBoxNodes =
            uiTreeRoot
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "MessageBox")

        modalLayers =
            uiTreeRoot
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "LayerCore")
                |> List.filter
                    (.uiNode
                        >> getNameFromDictEntries
                        >> Maybe.map (String.toLower >> String.contains "modal")
                        >> Maybe.withDefault False
                    )

        modalHybridWindowNodes =
            modalLayers
                |> List.concatMap listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "HybridWindow")
    in
    [ messageBoxNodes
    , modalHybridWindowNodes
    ]
        |> List.concat
        |> List.map parseMessageBox


parseMessageBox : UITreeNodeWithDisplayRegion -> MessageBox
parseMessageBox uiNode =
    let
        buttonGroup =
            uiNode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> .pythonObjectTypeName >> String.contains "ButtonGroup")
                |> List.head

        buttons =
            buttonGroup
                |> Maybe.map listDescendantsWithDisplayRegion
                |> Maybe.withDefault []
                |> List.filter (.uiNode >> .pythonObjectTypeName >> String.contains "Button")
                |> List.map
                    (\buttonNode ->
                        { uiNode = buttonNode
                        , mainText =
                            buttonNode
                                |> getAllContainedDisplayTextsWithRegion
                                |> List.sortBy (Tuple.second >> .totalDisplayRegion >> areaFromDisplayRegion >> Maybe.withDefault 0)
                                |> List.map Tuple.first
                                |> List.head
                        }
                    )
    in
    { buttonGroup = buttonGroup
    , buttons = buttons
    , uiNode = uiNode
    }


findButtonInDescendantsContainingDisplayText : String -> UITreeNodeWithDisplayRegion -> Maybe UITreeNodeWithDisplayRegion
findButtonInDescendantsContainingDisplayText displayText =
    findButtonInDescendantsByDisplayTextsPredicate
        (List.any (String.toLower >> String.contains (String.toLower displayText)))


findButtonInDescendantsByDisplayTextsPredicate : (List String -> Bool) -> UITreeNodeWithDisplayRegion -> Maybe UITreeNodeWithDisplayRegion
findButtonInDescendantsByDisplayTextsPredicate displayTextsPredicate =
    listDescendantsWithDisplayRegion
        {-
           2023-01-12 discovered name: UndockButton
        -}
        >> List.filter (.uiNode >> .pythonObjectTypeName >> String.contains "Button")
        >> List.filter (.uiNode >> getAllContainedDisplayTexts >> displayTextsPredicate)
        >> List.sortBy (.totalDisplayRegion >> areaFromDisplayRegion >> Maybe.withDefault 0)
        >> List.head


parseScrollControls : UITreeNodeWithDisplayRegion -> ScrollControls
parseScrollControls scrollControlsNode =
    let
        scrollHandle =
            scrollControlsNode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "ScrollHandle")
                |> List.head
    in
    { uiNode = scrollControlsNode
    , scrollHandle = scrollHandle
    }


parseLayerAbovemainFromUITreeRoot : UITreeNodeWithDisplayRegion -> Maybe LayerAbovemain
parseLayerAbovemainFromUITreeRoot uiTreeRoot =
    case
        uiTreeRoot
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> getNameFromDictEntries >> (==) (Just "l_abovemain"))
            |> List.head
    of
        Nothing ->
            Nothing

        Just layerAboveMainUINode ->
            Just
                { uiNode = layerAboveMainUINode
                , quickMessage = parseQuickMessage layerAboveMainUINode
                }


parseQuickMessage : UITreeNodeWithDisplayRegion -> Maybe QuickMessage
parseQuickMessage layerAboveMainUINode =
    case
        layerAboveMainUINode
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "QuickMessage")
            |> List.head
    of
        Nothing ->
            Nothing

        Just quickMessageUINode ->
            let
                text =
                    quickMessageUINode.uiNode
                        |> getAllContainedDisplayTexts
                        |> List.head
                        |> Maybe.withDefault ""
            in
            Just
                { uiNode = quickMessageUINode
                , text = text
                }


getSubstringBetweenXmlTagsAfterMarker : String -> String -> Maybe String
getSubstringBetweenXmlTagsAfterMarker marker =
    String.split marker
        >> List.drop 1
        >> List.head
        >> Maybe.andThen (String.split ">" >> List.drop 1 >> List.head)
        >> Maybe.andThen (String.split "<" >> List.head)


parseNumberTruncatingAfterOptionalDecimalSeparator : String -> Result String Int
parseNumberTruncatingAfterOptionalDecimalSeparator numberDisplayText =
    let
        expectedSeparators =
            [ ",", ".", "’", " ", "\u{00A0}", "\u{202F}" ]

        groupsTexts =
            expectedSeparators
                |> List.foldl (\separator -> List.concatMap (String.split separator))
                    [ String.trim numberDisplayText ]

        lastGroupIsFraction =
            case List.reverse groupsTexts of
                lastGroupText :: _ :: _ ->
                    String.length lastGroupText < 3

                _ ->
                    False

        integerText =
            String.join ""
                (if lastGroupIsFraction then
                    groupsTexts |> List.reverse |> List.drop 1 |> List.reverse

                 else
                    groupsTexts
                )
    in
    integerText
        |> String.toInt
        |> Result.fromMaybe ("Failed to parse to integer: " ++ integerText)


centerFromDisplayRegion : DisplayRegion -> Location2d
centerFromDisplayRegion region =
    { x = region.x + region.width // 2, y = region.y + region.height // 2 }


getDisplayText : EveOnline.MemoryReading.UITreeNode -> Maybe String
getDisplayText uiNode =
    [ "_setText", "_text" ]
        |> List.filterMap
            (\displayTextPropertyName ->
                uiNode.dictEntriesOfInterest
                    |> Dict.get displayTextPropertyName
                    |> Maybe.andThen (Json.Decode.decodeValue Json.Decode.string >> Result.toMaybe)
            )
        |> List.sortBy (String.length >> negate)
        |> List.head


getAllContainedDisplayTexts : EveOnline.MemoryReading.UITreeNode -> List String
getAllContainedDisplayTexts uiNode =
    uiNode
        :: (uiNode |> EveOnline.MemoryReading.listDescendantsInUITreeNode)
        |> List.filterMap getDisplayText


getAllContainedDisplayTextsWithRegion : UITreeNodeWithDisplayRegion -> List ( String, UITreeNodeWithDisplayRegion )
getAllContainedDisplayTextsWithRegion uiNode =
    uiNode
        :: (uiNode |> listDescendantsWithDisplayRegion)
        |> List.filterMap
            (\descendant ->
                let
                    displayText =
                        descendant.uiNode |> getDisplayText |> Maybe.withDefault ""
                in
                if 0 < (displayText |> String.length) then
                    Just ( displayText, descendant )

                else
                    Nothing
            )


getNameFromDictEntries : EveOnline.MemoryReading.UITreeNode -> Maybe String
getNameFromDictEntries =
    getStringPropertyFromDictEntries "_name"


getHintTextFromDictEntries : EveOnline.MemoryReading.UITreeNode -> Maybe String
getHintTextFromDictEntries =
    getStringPropertyFromDictEntries "_hint"


getTexturePathFromDictEntries : EveOnline.MemoryReading.UITreeNode -> Maybe String
getTexturePathFromDictEntries node =
    getStringPropertyFromDictEntries "texturePath" node
        |> Maybe.Extra.or (getStringPropertyFromDictEntries "_texturePath" node)


getStringPropertyFromDictEntries : String -> EveOnline.MemoryReading.UITreeNode -> Maybe String
getStringPropertyFromDictEntries dictEntryKey uiNode =
    uiNode.dictEntriesOfInterest
        |> Dict.get dictEntryKey
        |> Maybe.andThen (Json.Decode.decodeValue Json.Decode.string >> Result.toMaybe)


getColorPercentFromDictEntries : EveOnline.MemoryReading.UITreeNode -> Maybe ColorComponents
getColorPercentFromDictEntries =
    .dictEntriesOfInterest
        >> Dict.get "_color"
        >> Maybe.andThen (Json.Decode.decodeValue jsonDecodeColorPercent >> Result.toMaybe)


jsonDecodeColorPercent : Json.Decode.Decoder ColorComponents
jsonDecodeColorPercent =
    Json.Decode.map4 ColorComponents
        (Json.Decode.field "aPercent" jsonDecodeIntFromIntOrString)
        (Json.Decode.field "rPercent" jsonDecodeIntFromIntOrString)
        (Json.Decode.field "gPercent" jsonDecodeIntFromIntOrString)
        (Json.Decode.field "bPercent" jsonDecodeIntFromIntOrString)


getRotationFloatFromDictEntries : EveOnline.MemoryReading.UITreeNode -> Maybe Float
getRotationFloatFromDictEntries =
    .dictEntriesOfInterest
        >> Dict.get "_rotation"
        >> Maybe.andThen (Json.Decode.decodeValue Json.Decode.float >> Result.toMaybe)


getOpacityFloatFromDictEntries : EveOnline.MemoryReading.UITreeNode -> Maybe Float
getOpacityFloatFromDictEntries =
    .dictEntriesOfInterest
        >> Dict.get "_opacity"
        >> Maybe.andThen (Json.Decode.decodeValue Json.Decode.float >> Result.toMaybe)


jsonDecodeIntFromIntOrString : Json.Decode.Decoder Int
jsonDecodeIntFromIntOrString =
    Json.Decode.oneOf
        [ Json.Decode.int
        , Json.Decode.string
            |> Json.Decode.andThen
                (\asString ->
                    case asString |> String.toInt of
                        Just asInt ->
                            Json.Decode.succeed asInt

                        Nothing ->
                            Json.Decode.fail ("Failed to parse integer from string '" ++ asString ++ "'")
                )
        ]


getHorizontalOffsetFromParentAndWidth : EveOnline.MemoryReading.UITreeNode -> Maybe { offset : Int, width : Int }
getHorizontalOffsetFromParentAndWidth uiNode =
    let
        roundedNumberFromPropertyName propertyName =
            uiNode.dictEntriesOfInterest
                |> Dict.get propertyName
                |> Maybe.andThen (Json.Decode.decodeValue Json.Decode.float >> Result.toMaybe)
                |> Maybe.map round
    in
    case ( roundedNumberFromPropertyName "_displayX", roundedNumberFromPropertyName "_width" ) of
        ( Just offset, Just width ) ->
            Just { offset = offset, width = width }

        _ ->
            Nothing


areaFromDisplayRegion : DisplayRegion -> Maybe Int
areaFromDisplayRegion region =
    if region.width < 0 || region.height < 0 then
        Nothing

    else
        Just (region.width * region.height)


getVerticalOffsetFromParent : EveOnline.MemoryReading.UITreeNode -> Maybe Int
getVerticalOffsetFromParent =
    .dictEntriesOfInterest
        >> Dict.get "_displayY"
        >> Maybe.andThen (Json.Decode.decodeValue Json.Decode.float >> Result.toMaybe)
        >> Maybe.map round


{-| Returns the subsequence of items not contained in any of the other ones
-}
subsequenceNotContainedInAnyOtherWithDisplayRegion :
    List UITreeNodeWithDisplayRegion
    -> List UITreeNodeWithDisplayRegion
subsequenceNotContainedInAnyOtherWithDisplayRegion original =
    original
        |> List.filter
            (\item ->
                original
                    |> List.any (\other -> item /= other && nodeDescendantsContainWithDisplayRegion item other)
                    |> not
            )


nodeDescendantsContainWithDisplayRegion : UITreeNodeWithDisplayRegion -> UITreeNodeWithDisplayRegion -> Bool
nodeDescendantsContainWithDisplayRegion =
    treeNodeDescendantsContain listChildrenWithDisplayRegion


{-| Returns True if the set of descendants of the second node contains the first node.
(Order of arguments is same as in `List.member`)
To learn more about these kinds of trees, see <https://en.wikipedia.org/wiki/Tree_(graph_theory)>
-}
treeNodeDescendantsContain : (node -> List node) -> node -> node -> Bool
treeNodeDescendantsContain childrenFromNode contained containing =
    let
        children =
            childrenFromNode containing
    in
    List.member contained children
        || List.any (treeNodeDescendantsContain childrenFromNode contained) children


getMostPopulousDescendantWithDisplayRegionMatchingPredicate :
    (UITreeNodeWithDisplayRegion -> Bool)
    -> UITreeNodeWithDisplayRegion
    -> Maybe UITreeNodeWithDisplayRegion
getMostPopulousDescendantWithDisplayRegionMatchingPredicate predicate parent =
    listDescendantsWithDisplayRegion parent
        |> List.filter predicate
        |> List.sortBy countDescendantsInUITreeNodeWithDisplayRegion
        |> List.reverse
        |> List.head


countDescendantsInUITreeNodeWithDisplayRegion : UITreeNodeWithDisplayRegion -> Int
countDescendantsInUITreeNodeWithDisplayRegion parent =
    parent.children
        |> Maybe.withDefault []
        |> List.filterMap unwrapUITreeNodeWithDisplayRegionChild
        |> List.map (countDescendantsInUITreeNodeWithDisplayRegion >> (+) 1)
        |> List.sum


unwrapUITreeNodeWithDisplayRegionChild : ChildOfNodeWithDisplayRegion -> Maybe UITreeNodeWithDisplayRegion
unwrapUITreeNodeWithDisplayRegionChild child =
    case child of
        ChildWithRegion node ->
            Just node

        ChildWithoutRegion _ ->
            Nothing


getMostPopulousDescendantMatchingPredicate :
    (EveOnline.MemoryReading.UITreeNode -> Bool)
    -> EveOnline.MemoryReading.UITreeNode
    -> Maybe EveOnline.MemoryReading.UITreeNode
getMostPopulousDescendantMatchingPredicate predicate parent =
    EveOnline.MemoryReading.listDescendantsInUITreeNode parent
        |> List.filter predicate
        |> List.sortBy EveOnline.MemoryReading.countDescendantsInUITreeNode
        |> List.reverse
        |> List.head


listDescendantsWithDisplayRegion : UITreeNodeWithDisplayRegion -> List UITreeNodeWithDisplayRegion
listDescendantsWithDisplayRegion parent =
    parent
        |> listChildrenWithDisplayRegion
        |> List.concatMap (\child -> child :: listDescendantsWithDisplayRegion child)


listChildrenWithDisplayRegion : UITreeNodeWithDisplayRegion -> List UITreeNodeWithDisplayRegion
listChildrenWithDisplayRegion parent =
    parent.children
        |> Maybe.withDefault []
        |> List.filterMap justCaseWithDisplayRegion


justCaseWithDisplayRegion : ChildOfNodeWithDisplayRegion -> Maybe UITreeNodeWithDisplayRegion
justCaseWithDisplayRegion child =
    case child of
        ChildWithoutRegion _ ->
            Nothing

        ChildWithRegion childWithRegion ->
            Just childWithRegion


nodeOccludesFollowingNodes : EveOnline.MemoryReading.UITreeNode -> Bool
nodeOccludesFollowingNodes node =
    Set.member node.pythonObjectTypeName pythonObjectTypesKnownToOccludeFollowingElements


pythonObjectTypesKnownToOccludeFollowingElements : Set.Set String
pythonObjectTypesKnownToOccludeFollowingElements =
    Set.fromList
        [ -- session-recording-2022-12-09T12-32-56.zip: In Overview window: "SortHeaders"
          "SortHeaders"
        , "ContextMenu"
        , "OverviewWindow"
        , "DronesWindow"
        , "SelectedItemWnd"
        , "InventoryPrimary"
        , "ChatWindowStack"
        ]


subtractRegionsFromRegion :
    { minuend : DisplayRegion
    , subtrahend : List DisplayRegion
    }
    -> List DisplayRegion
subtractRegionsFromRegion { minuend, subtrahend } =
    subtrahend
        |> List.foldl
            (\subtrahendPart previousResults ->
                previousResults
                    |> List.concatMap
                        (\minuendPart ->
                            subtractRegionFromRegion { subtrahend = subtrahendPart, minuend = minuendPart }
                        )
            )
            [ minuend ]


subtractRegionFromRegion :
    { minuend : DisplayRegion
    , subtrahend : DisplayRegion
    }
    -> List DisplayRegion
subtractRegionFromRegion { minuend, subtrahend } =
    let
        minuendRight =
            minuend.x + minuend.width

        minuendBottom =
            minuend.y + minuend.height

        subtrahendRight =
            subtrahend.x + subtrahend.width

        subtrahendBottom =
            subtrahend.y + subtrahend.height
    in
    {-
       Similar to approach from https://stackoverflow.com/questions/3765283/how-to-subtract-a-rectangle-from-another/15228510#15228510
       We want to support finding the largest rectangle, so we let them overlap here.

       ----------------------------
       |  A  |       A      |  A  |
       |  B  |              |  C  |
       |--------------------------|
       |  B  |  subtrahend  |  C  |
       |--------------------------|
       |  B  |              |  C  |
       |  D  |      D       |  D  |
       ----------------------------
    -}
    [ { left = minuend.x
      , top = minuend.y
      , right = minuendRight
      , bottom = minuendBottom |> min subtrahend.y
      }
    , { left = minuend.x
      , top = minuend.y
      , right = minuendRight |> min subtrahend.x
      , bottom = minuendBottom
      }
    , { left = minuend.x |> max subtrahendRight
      , top = minuend.y
      , right = minuendRight
      , bottom = minuendBottom
      }
    , { left = minuend.x
      , top = minuend.y |> max subtrahendBottom
      , right = minuendRight
      , bottom = minuendBottom
      }
    ]
        |> List.map
            (\rect ->
                { x = rect.left
                , y = rect.top
                , width = rect.right - rect.left
                , height = rect.bottom - rect.top
                }
            )
        |> List.filter (\rect -> 0 < rect.width && 0 < rect.height)
        |> listUnique


regionsOverlap : DisplayRegion -> DisplayRegion -> Bool
regionsOverlap regionA regionB =
    subtractRegionFromRegion
        { minuend = regionA
        , subtrahend = regionB
        }
        /= [ regionA ]


predicateAny : List (a -> Bool) -> a -> Bool
predicateAny predicates candidate =
    predicates
        |> List.any (\predicate -> predicate candidate)


{-| Remove duplicate values, keeping the first instance of each element which appears more than once.
-}
listUnique : List element -> List element
listUnique =
    List.foldr
        (\nextElement elements ->
            if elements |> List.member nextElement then
                elements

            else
                nextElement :: elements
        )
        []
