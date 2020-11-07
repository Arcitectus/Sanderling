module EveOnline.ParseUserInterface exposing (..)

import Common.EffectOnWindow
import Dict
import EveOnline.MemoryReading
import Json.Decode
import Maybe.Extra
import Regex
import Set


type alias ParsedUserInterface =
    { uiTree : UITreeNodeWithDisplayRegion
    , contextMenus : List ContextMenu
    , shipUI : Maybe ShipUI
    , targets : List Target
    , infoPanelContainer : Maybe InfoPanelContainer
    , overviewWindow : Maybe OverviewWindow
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
    , moduleButtonTooltip : Maybe ModuleButtonTooltip
    , neocom : Maybe Neocom
    , messageBoxes : List MessageBox
    , layerAbovemain : Maybe UITreeNodeWithDisplayRegion
    }


type alias UITreeNodeWithDisplayRegion =
    { uiNode : EveOnline.MemoryReading.UITreeNode
    , children : Maybe (List ChildOfNodeWithDisplayRegion)
    , selfDisplayRegion : DisplayRegion
    , totalDisplayRegion : DisplayRegion
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
    , repairItemButton : Maybe UITreeNodeWithDisplayRegion
    , pickNewItemButton : Maybe UITreeNodeWithDisplayRegion
    , repairAllButton : Maybe UITreeNodeWithDisplayRegion
    }


type alias CharacterSheetWindow =
    { uiNode : UITreeNodeWithDisplayRegion
    , skillGroups : List UITreeNodeWithDisplayRegion
    }


type alias ColorComponents =
    { a : Int, r : Int, g : Int, b : Int }


type alias DronesWindow =
    { uiNode : UITreeNodeWithDisplayRegion
    , droneGroups : List DronesWindowDroneGroup
    , droneGroupInBay : Maybe DronesWindowDroneGroup
    , droneGroupInLocalSpace : Maybe DronesWindowDroneGroup
    }


type alias DronesWindowDroneGroup =
    { header : DronesWindowDroneGroupHeader
    , drones : List DronesWindowEntry
    }


type alias DronesWindowDroneGroupHeader =
    { uiNode : UITreeNodeWithDisplayRegion
    , mainText : Maybe String
    , expander : Maybe Expander
    , quantityFromTitle : Maybe Int
    }


type alias DronesWindowEntry =
    { uiNode : UITreeNodeWithDisplayRegion
    , mainText : Maybe String
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
    }


type alias Inventory =
    { uiNode : UITreeNodeWithDisplayRegion
    , itemsView : Maybe InventoryItemsView
    , scrollControls : Maybe ScrollControls
    }


type InventoryItemsView
    = InventoryItemsListView { items : List UITreeNodeWithDisplayRegion }
    | InventoryItemsNotListView { items : List UITreeNodeWithDisplayRegion }


type alias InventoryWindowLeftTreeEntry =
    { uiNode : UITreeNodeWithDisplayRegion
    , toggleBtn : Maybe UITreeNodeWithDisplayRegion
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


type alias Expander =
    { uiNode : UITreeNodeWithDisplayRegion
    , texturePath : Maybe String
    , isExpanded : Maybe Bool
    }


type alias MessageBox =
    { uiNode : UITreeNodeWithDisplayRegion
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
            }


parseUserInterfaceFromUITree : UITreeNodeWithDisplayRegion -> ParsedUserInterface
parseUserInterfaceFromUITree uiTree =
    { uiTree = uiTree
    , contextMenus = parseContextMenusFromUITreeRoot uiTree
    , shipUI = parseShipUIFromUITreeRoot uiTree
    , targets = parseTargetsFromUITreeRoot uiTree
    , infoPanelContainer = parseInfoPanelContainerFromUIRoot uiTree
    , overviewWindow = parseOverviewWindowFromUITreeRoot uiTree
    , selectedItemWindow = parseSelectedItemWindowFromUITreeRoot uiTree
    , dronesWindow = parseDronesWindowFromUITreeRoot uiTree
    , fittingWindow = parseFittingWindowFromUITreeRoot uiTree
    , probeScannerWindow = parseProbeScannerWindowFromUITreeRoot uiTree
    , directionalScannerWindow = parseDirectionalScannerWindowFromUITreeRoot uiTree
    , stationWindow = parseStationWindowFromUITreeRoot uiTree
    , inventoryWindows = parseInventoryWindowsFromUITreeRoot uiTree
    , moduleButtonTooltip = parseModuleButtonTooltipFromUITreeRoot uiTree
    , chatWindowStacks = parseChatWindowStacksFromUITreeRoot uiTree
    , agentConversationWindows = parseAgentConversationWindowsFromUITreeRoot uiTree
    , marketOrdersWindow = parseMarketOrdersWindowFromUITreeRoot uiTree
    , surveyScanWindow = parseSurveyScanWindowFromUITreeRoot uiTree
    , bookmarkLocationWindow = parseBookmarkLocationWindowFromUITreeRoot uiTree
    , repairShopWindow = parseRepairShopWindowFromUITreeRoot uiTree
    , characterSheetWindow = parseCharacterSheetWindowFromUITreeRoot uiTree
    , fleetWindow = parseFleetWindowFromUITreeRoot uiTree
    , watchListPanel = parseWatchListPanelFromUITreeRoot uiTree
    , neocom = parseNeocomFromUITreeRoot uiTree
    , messageBoxes = parseMessageBoxesFromUITreeRoot uiTree
    , layerAbovemain = parseLayerAbovemainFromUITreeRoot uiTree
    }


asUITreeNodeWithDisplayRegion : { selfDisplayRegion : DisplayRegion, totalDisplayRegion : DisplayRegion } -> EveOnline.MemoryReading.UITreeNode -> UITreeNodeWithDisplayRegion
asUITreeNodeWithDisplayRegion { selfDisplayRegion, totalDisplayRegion } uiNode =
    { uiNode = uiNode
    , children = uiNode.children |> Maybe.map (List.map (EveOnline.MemoryReading.unwrapUITreeNodeChild >> asUITreeNodeWithInheritedOffset { x = totalDisplayRegion.x, y = totalDisplayRegion.y }))
    , selfDisplayRegion = selfDisplayRegion
    , totalDisplayRegion = totalDisplayRegion
    }


asUITreeNodeWithInheritedOffset : { x : Int, y : Int } -> EveOnline.MemoryReading.UITreeNode -> ChildOfNodeWithDisplayRegion
asUITreeNodeWithInheritedOffset inheritedOffset rawNode =
    case rawNode |> getDisplayRegionFromDictEntries of
        Nothing ->
            ChildWithoutRegion rawNode

        Just selfRegion ->
            ChildWithRegion
                (asUITreeNodeWithDisplayRegion
                    { selfDisplayRegion = selfRegion
                    , totalDisplayRegion =
                        { selfRegion | x = inheritedOffset.x + selfRegion.x, y = inheritedOffset.y + selfRegion.y }
                    }
                    rawNode
                )


getDisplayRegionFromDictEntries : EveOnline.MemoryReading.UITreeNode -> Maybe DisplayRegion
getDisplayRegionFromDictEntries uiNode =
    let
        fixedNumberFromJsonValue =
            Json.Decode.decodeValue
                (Json.Decode.oneOf
                    [ jsonDecodeIntFromString
                    , Json.Decode.field "int_low32" jsonDecodeIntFromString
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
                getSecurityStatusPercentFromUINodeText : String -> Maybe Int
                getSecurityStatusPercentFromUINodeText =
                    getSubstringBetweenXmlTagsAfterMarker "hint='Security status'"
                        >> Maybe.andThen (String.trim >> String.toFloat)
                        >> Maybe.map ((*) 100 >> round)

                securityStatusPercent =
                    infoPanelNode.uiNode
                        |> getAllContainedDisplayTexts
                        |> List.filterMap getSecurityStatusPercentFromUINodeText
                        |> List.head

                currentSolarSystemName =
                    infoPanelNode.uiNode
                        |> getAllContainedDisplayTexts
                        |> List.filterMap (getSubstringBetweenXmlTagsAfterMarker "alt='Current Solar System'")
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


parseOverviewWindowFromUITreeRoot : UITreeNodeWithDisplayRegion -> Maybe OverviewWindow
parseOverviewWindowFromUITreeRoot uiTreeRoot =
    case
        uiTreeRoot
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "OverView")
            |> List.head
    of
        Nothing ->
            Nothing

        Just overviewWindowNode ->
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
            Just
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

        cellsTexts =
            overviewEntryNode
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

        objectDistance =
            cellsTexts
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
    in
    { uiNode = overviewEntryNode
    , textsLeftToRight = textsLeftToRight
    , cellsTexts = cellsTexts
    , objectDistance = objectDistance
    , objectDistanceInMeters = objectDistanceInMeters
    , objectName = cellsTexts |> Dict.get "Name"
    , objectType = cellsTexts |> Dict.get "Type"
    , objectAlliance = cellsTexts |> Dict.get "Alliance"
    , iconSpriteColorPercent = iconSpriteColorPercent
    , namesUnderSpaceObjectIcon = namesUnderSpaceObjectIcon
    , bgColorFillsPercent = bgColorFillsPercent
    , rightAlignedIconsHints = rightAlignedIconsHints
    , commonIndications = commonIndications
    }


parseOverviewEntryDistanceInMetersFromText : String -> Result String Int
parseOverviewEntryDistanceInMetersFromText distanceDisplayTextBeforeTrim =
    case "^[\\d\\,\\.\\s]+(?=\\s*m)" |> Regex.fromString of
        Nothing ->
            Err "Regex code error"

        Just regexForUnitMeter ->
            case "^[\\d\\,\\.]+(?=\\s*km)" |> Regex.fromString of
                Nothing ->
                    Err "Regex code error"

                Just regexForUnitKilometer ->
                    let
                        distanceDisplayText =
                            distanceDisplayTextBeforeTrim |> String.trim
                    in
                    case distanceDisplayText |> Regex.find regexForUnitMeter |> List.head of
                        Just match ->
                            match.match
                                |> parseNumberTruncatingAfterOptionalDecimalSeparator

                        Nothing ->
                            case distanceDisplayText |> Regex.find regexForUnitKilometer |> List.head of
                                Just match ->
                                    match.match
                                        |> parseNumberTruncatingAfterOptionalDecimalSeparator
                                        -- unit 'km'
                                        |> Result.map ((*) 1000)

                                Nothing ->
                                    Err ("Text did not match expected number format: '" ++ distanceDisplayText ++ "'")


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
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "DroneView")
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
                        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "DroneMainGroup")
                        |> List.map parseDronesWindowDroneGroupHeader

                droneEntries =
                    windowNode
                        |> listDescendantsWithDisplayRegion
                        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "DroneEntry")
                        |> List.map parseDronesWindowEntry

                headerFromDroneEntry droneEntry =
                    droneGroupHeaders
                        |> List.filter (\header -> header.uiNode.totalDisplayRegion.y < droneEntry.uiNode.totalDisplayRegion.y)
                        |> List.sortBy (.uiNode >> .totalDisplayRegion >> .y)
                        |> List.reverse
                        |> List.head

                droneGroups =
                    droneGroupHeaders
                        |> List.map
                            (\header ->
                                { header = header
                                , drones = droneEntries |> List.filter (headerFromDroneEntry >> (==) (Just header))
                                }
                            )

                droneGroupFromHeaderTextPart headerTextPart =
                    droneGroups
                        |> List.filter (.header >> .mainText >> Maybe.withDefault "" >> String.toLower >> String.contains (headerTextPart |> String.toLower))
                        |> List.sortBy (.header >> .mainText >> Maybe.map String.length >> Maybe.withDefault 999)
                        |> List.head
            in
            Just
                { uiNode = windowNode
                , droneGroups = droneGroups
                , droneGroupInBay = droneGroupFromHeaderTextPart "in Bay"
                , droneGroupInLocalSpace = droneGroupFromHeaderTextPart "in local space"
                }


parseDronesWindowDroneGroupHeader : UITreeNodeWithDisplayRegion -> DronesWindowDroneGroupHeader
parseDronesWindowDroneGroupHeader groupHeaderUiNode =
    let
        mainText =
            groupHeaderUiNode
                |> getAllContainedDisplayTextsWithRegion
                |> List.sortBy (Tuple.second >> .totalDisplayRegion >> areaFromDisplayRegion >> Maybe.withDefault 0)
                |> List.map Tuple.first
                |> List.head

        expanderNode =
            groupHeaderUiNode
                |> listDescendantsWithDisplayRegion
                |> List.filter
                    (.uiNode
                        >> getNameFromDictEntries
                        >> (Maybe.map (String.toLower >> String.contains "expander") >> Maybe.withDefault False)
                    )
                |> List.head

        quantityFromTitle =
            mainText |> Maybe.andThen (parseQuantityFromDroneGroupTitleText >> Result.withDefault Nothing)
    in
    { uiNode = groupHeaderUiNode
    , mainText = mainText
    , expander = expanderNode |> Maybe.map parseExpander
    , quantityFromTitle = quantityFromTitle
    }


parseQuantityFromDroneGroupTitleText : String -> Result String (Maybe Int)
parseQuantityFromDroneGroupTitleText droneGroupTitleText =
    case "\\(\\s*(\\d+)\\s*\\)*$" |> Regex.fromString of
        Nothing ->
            Err "Regex code error"

        Just regex ->
            case droneGroupTitleText |> String.trim |> Regex.find regex |> List.head of
                Nothing ->
                    Ok Nothing

                Just match ->
                    case match.submatches of
                        [ quantityText ] ->
                            quantityText
                                |> Maybe.withDefault ""
                                |> String.trim
                                |> String.toInt
                                |> Maybe.map (Just >> Ok)
                                |> Maybe.withDefault (Err ("Failed to parse to integer: " ++ match.match))

                        _ ->
                            Err "Unexpected number of text elements."


parseDronesWindowEntry : UITreeNodeWithDisplayRegion -> DronesWindowEntry
parseDronesWindowEntry droneEntryNode =
    let
        mainText =
            droneEntryNode
                |> getAllContainedDisplayTextsWithRegion
                |> List.sortBy (Tuple.second >> .totalDisplayRegion >> areaFromDisplayRegion >> Maybe.withDefault 0)
                |> List.map Tuple.first
                |> List.head
    in
    { uiNode = droneEntryNode
    , mainText = mainText
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
                        |> Maybe.map listDescendantsWithDisplayRegion
                        |> Maybe.withDefault []
                        |> List.filter (.uiNode >> .pythonObjectTypeName >> String.toLower >> String.contains "header")
                        |> List.head

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
                buttons =
                    windowNode
                        |> listDescendantsWithDisplayRegion
                        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "Button")

                buttonFromDisplayText textToSearch =
                    let
                        textToSearchLowercase =
                            String.toLower textToSearch

                        textMatches text =
                            text == textToSearchLowercase || (text |> String.contains (">" ++ textToSearchLowercase ++ "<"))
                    in
                    buttons
                        |> List.filter (.uiNode >> getAllContainedDisplayTexts >> List.map (String.toLower >> String.trim) >> List.any textMatches)
                        |> List.head
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
                        >> List.filter (\uiNode -> [ "ShipCargo", "ShipDroneBay", "ShipOreHold", "StationItems" ] |> List.member uiNode.uiNode.pythonObjectTypeName)
                        >> List.head
                    )

        selectedContainerInventory =
            maybeSelectedContainerInventoryNode
                |> Maybe.map
                    (\selectedContainerInventoryNode ->
                        let
                            listViewItemNodes =
                                selectedContainerInventoryNode
                                    |> listDescendantsWithDisplayRegion
                                    |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "Item")

                            scrollControlsNode =
                                selectedContainerInventoryNode
                                    |> listDescendantsWithDisplayRegion
                                    |> List.filter (.uiNode >> .pythonObjectTypeName >> String.contains "ScrollControls")
                                    |> List.head

                            notListViewItemNodes =
                                selectedContainerInventoryNode
                                    |> listDescendantsWithDisplayRegion
                                    |> List.filter (.uiNode >> .pythonObjectTypeName >> String.contains "InvItem")

                            itemsView =
                                if 0 < (listViewItemNodes |> List.length) then
                                    Just (InventoryItemsListView { items = listViewItemNodes })

                                else if 0 < (notListViewItemNodes |> List.length) then
                                    Just (InventoryItemsNotListView { items = notListViewItemNodes })

                                else
                                    Nothing
                        in
                        { uiNode = selectedContainerInventoryNode
                        , itemsView = itemsView
                        , scrollControls = scrollControlsNode |> Maybe.map parseScrollControls
                        }
                    )

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
    in
    { uiNode = windowUiNode
    , leftTreeEntries = leftTreeEntries
    , subCaptionLabelText = subCaptionLabelText
    , selectedContainerCapacityGauge = selectedContainerCapacityGauge
    , selectedContainerInventory = selectedContainerInventory
    , buttonToSwitchToListView = buttonToSwitchToListView
    }


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
    let
        buttonFromLabelText labelText =
            windowUINode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> .pythonObjectTypeName >> String.contains "Button")
                |> List.filter (.uiNode >> getAllContainedDisplayTexts >> List.map (String.trim >> String.toLower) >> List.member (labelText |> String.toLower))
                |> List.sortBy (.totalDisplayRegion >> areaFromDisplayRegion >> Maybe.withDefault 0)
                |> List.head
    in
    { uiNode = windowUINode
    , submitButton = buttonFromLabelText "submit"
    , cancelButton = buttonFromLabelText "cancel"
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
        buttonFromLabelText labelText =
            windowUINode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> .pythonObjectTypeName >> String.contains "Button")
                |> List.filter (.uiNode >> getAllContainedDisplayTexts >> List.map (String.trim >> String.toLower) >> List.member (labelText |> String.toLower))
                |> List.sortBy (.totalDisplayRegion >> areaFromDisplayRegion >> Maybe.withDefault 0)
                |> List.head
    in
    { uiNode = windowUINode
    , items =
        windowUINode
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "Item")
    , repairItemButton = buttonFromLabelText "repair item"
    , pickNewItemButton = buttonFromLabelText "pick new item"
    , repairAllButton = buttonFromLabelText "repair all"
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
    case "(\\d+)\\:(\\d+)" |> Regex.fromString of
        Nothing ->
            Err "Regex code error"

        Just regex ->
            case clockText |> Regex.find regex |> List.head of
                Nothing ->
                    Err ("Text did not match expected format: '" ++ clockText ++ "'")

                Just match ->
                    case match.submatches of
                        [ Just hourText, Just minuteText ] ->
                            case hourText |> String.toInt of
                                Nothing ->
                                    Err ("Failed to parse hour: '" ++ hourText ++ "'")

                                Just hour ->
                                    case minuteText |> String.toInt of
                                        Nothing ->
                                            Err ("Failed to parse minute: '" ++ minuteText ++ "'")

                                        Just minute ->
                                            Ok { hour = hour, minute = minute }

                        _ ->
                            Err "Unexpected number of text elements."


parseExpander : UITreeNodeWithDisplayRegion -> Expander
parseExpander uiNode =
    let
        maybeTexturePath =
            getTexturePathFromDictEntries uiNode.uiNode

        isExpanded =
            maybeTexturePath
                |> Maybe.andThen
                    (\texturePath ->
                        [ ( "38_16_228.png", False ), ( "38_16_229.png", True ) ]
                            |> List.filter (\( pathEnd, _ ) -> texturePath |> String.endsWith pathEnd)
                            |> List.map Tuple.second
                            |> List.head
                    )
    in
    { uiNode = uiNode
    , texturePath = maybeTexturePath
    , isExpanded = isExpanded
    }


parseMessageBoxesFromUITreeRoot : UITreeNodeWithDisplayRegion -> List MessageBox
parseMessageBoxesFromUITreeRoot uiTreeRoot =
    uiTreeRoot
        |> listDescendantsWithDisplayRegion
        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "MessageBox")
        |> List.map parseMessageBox


parseMessageBox : UITreeNodeWithDisplayRegion -> MessageBox
parseMessageBox uiNode =
    let
        buttons =
            uiNode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "Button")
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
    { buttons = buttons
    , uiNode = uiNode
    }


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


parseLayerAbovemainFromUITreeRoot : UITreeNodeWithDisplayRegion -> Maybe UITreeNodeWithDisplayRegion
parseLayerAbovemainFromUITreeRoot uiTreeRoot =
    uiTreeRoot
        |> listDescendantsWithDisplayRegion
        |> List.filter (.uiNode >> getNameFromDictEntries >> (==) (Just "l_abovemain"))
        |> List.head


getSubstringBetweenXmlTagsAfterMarker : String -> String -> Maybe String
getSubstringBetweenXmlTagsAfterMarker marker =
    String.split marker
        >> List.drop 1
        >> List.head
        >> Maybe.andThen (String.split ">" >> List.drop 1 >> List.head)
        >> Maybe.andThen (String.split "<" >> List.head)


parseNumberTruncatingAfterOptionalDecimalSeparator : String -> Result String Int
parseNumberTruncatingAfterOptionalDecimalSeparator numberDisplayText =
    case "^(\\d+(\\s*[\\s\\,\\.’]\\d{3})*?)(?=(|[,\\.]\\d)$)" |> Regex.fromString of
        Nothing ->
            Err "Regex code error"

        Just regex ->
            case numberDisplayText |> String.trim |> Regex.find regex |> List.head of
                Nothing ->
                    Err ("Text did not match expected number format: '" ++ numberDisplayText ++ "'")

                Just match ->
                    match.match
                        |> String.replace "," ""
                        |> String.replace "." ""
                        |> String.replace "’" ""
                        |> String.replace " " ""
                        |> String.replace "\u{00A0}" ""
                        |> String.replace "\u{202F}" ""
                        |> String.toInt
                        |> Result.fromMaybe ("Failed to parse to integer: " ++ match.match)


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
getTexturePathFromDictEntries =
    getStringPropertyFromDictEntries "texturePath"


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
        (Json.Decode.field "aPercent" jsonDecodeIntFromString)
        (Json.Decode.field "rPercent" jsonDecodeIntFromString)
        (Json.Decode.field "gPercent" jsonDecodeIntFromString)
        (Json.Decode.field "bPercent" jsonDecodeIntFromString)


getRotationFloatFromDictEntries : EveOnline.MemoryReading.UITreeNode -> Maybe Float
getRotationFloatFromDictEntries =
    .dictEntriesOfInterest
        >> Dict.get "_rotation"
        >> Maybe.andThen (Json.Decode.decodeValue Json.Decode.float >> Result.toMaybe)


jsonDecodeIntFromString : Json.Decode.Decoder Int
jsonDecodeIntFromString =
    Json.Decode.string
        |> Json.Decode.andThen
            (\asString ->
                case asString |> String.toInt of
                    Just asInt ->
                        Json.Decode.succeed asInt

                    Nothing ->
                        Json.Decode.fail ("Failed to parse integer from string '" ++ asString ++ "'")
            )


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


getMostPopulousDescendantMatchingPredicate : (EveOnline.MemoryReading.UITreeNode -> Bool) -> EveOnline.MemoryReading.UITreeNode -> Maybe EveOnline.MemoryReading.UITreeNode
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
        |> List.filterMap
            (\child ->
                case child of
                    ChildWithoutRegion _ ->
                        Nothing

                    ChildWithRegion childWithRegion ->
                        Just childWithRegion
            )
