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
    , shipUI : MaybeVisible ShipUI
    , targets : List Target
    , infoPanelContainer : MaybeVisible InfoPanelContainer
    , overviewWindow : MaybeVisible OverviewWindow
    , selectedItemWindow : MaybeVisible SelectedItemWindow
    , dronesWindow : MaybeVisible DronesWindow
    , fittingWindow : MaybeVisible FittingWindow
    , probeScannerWindow : MaybeVisible ProbeScannerWindow
    , stationWindow : MaybeVisible StationWindow
    , inventoryWindows : List InventoryWindow
    , chatWindowStacks : List ChatWindowStack
    , agentConversationWindows : List AgentConversationWindow
    , marketOrdersWindow : MaybeVisible MarketOrdersWindow
    , surveyScanWindow : MaybeVisible SurveyScanWindow
    , moduleButtonTooltip : MaybeVisible ModuleButtonTooltip
    , neocom : MaybeVisible Neocom
    , messageBoxes : List MessageBox
    , layerAbovemain : MaybeVisible UITreeNodeWithDisplayRegion
    }


type alias UITreeNodeWithDisplayRegion =
    { uiNode : EveOnline.MemoryReading.UITreeNode
    , children : Maybe (List ChildOfNodeWithDisplayRegion)
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
    , indication : MaybeVisible ShipUIIndication
    , moduleButtons : List ShipUIModuleButton
    , moduleButtonsRows :
        { top : List ShipUIModuleButton
        , middle : List ShipUIModuleButton
        , bottom : List ShipUIModuleButton
        }
    , offensiveBuffButtonNames : List String
    }


type alias ShipUIIndication =
    { uiNode : UITreeNodeWithDisplayRegion
    , maneuverType : MaybeVisible ShipManeuverType
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


type alias InfoPanelContainer =
    { uiNode : UITreeNodeWithDisplayRegion
    , icons : MaybeVisible InfoPanelIcons
    , infoPanelLocationInfo : MaybeVisible InfoPanelLocationInfo
    , infoPanelRoute : MaybeVisible InfoPanelRoute
    , infoPanelAgentMissions : MaybeVisible InfoPanelAgentMissions
    }


type alias InfoPanelIcons =
    { uiNode : UITreeNodeWithDisplayRegion
    , search : MaybeVisible UITreeNodeWithDisplayRegion
    , locationInfo : MaybeVisible UITreeNodeWithDisplayRegion
    , route : MaybeVisible UITreeNodeWithDisplayRegion
    , agentMissions : MaybeVisible UITreeNodeWithDisplayRegion
    , dailyChallenge : MaybeVisible UITreeNodeWithDisplayRegion
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
    , expandedContent : MaybeVisible InfoPanelLocationInfoExpandedContent
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
    , expander : MaybeVisible Expander
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


type alias StationWindow =
    { uiNode : UITreeNodeWithDisplayRegion
    , undockButton : Maybe { uiNode : UITreeNodeWithDisplayRegion, mainText : String }
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
    , visibleUsers : List ChatUserEntry
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
    , clock : MaybeVisible NeocomClock
    }


type alias NeocomClock =
    { uiNode : UITreeNodeWithDisplayRegion
    , text : String
    , parsedText : Result String { hour : Int, minute : Int }
    }


type alias AgentConversationWindow =
    { uiNode : UITreeNodeWithDisplayRegion
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


type MaybeVisible feature
    = CanNotSeeIt
    | CanSee feature


parseUITreeWithDisplayRegionFromUITree : EveOnline.MemoryReading.UITreeNode -> UITreeNodeWithDisplayRegion
parseUITreeWithDisplayRegionFromUITree uiTree =
    uiTree |> asUITreeNodeWithTotalDisplayRegion (uiTree |> getDisplayRegionFromDictEntries |> Maybe.withDefault { x = 0, y = 0, width = 0, height = 0 })


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
    , stationWindow = parseStationWindowFromUITreeRoot uiTree
    , inventoryWindows = parseInventoryWindowsFromUITreeRoot uiTree
    , moduleButtonTooltip = parseModuleButtonTooltipFromUITreeRoot uiTree
    , chatWindowStacks = parseChatWindowStacksFromUITreeRoot uiTree
    , agentConversationWindows = parseAgentConversationWindowsFromUITreeRoot uiTree
    , marketOrdersWindow = parseMarketOrdersWindowFromUITreeRoot uiTree
    , surveyScanWindow = parseSurveyScanWindowFromUITreeRoot uiTree
    , neocom = parseNeocomFromUITreeRoot uiTree
    , messageBoxes = parseMessageBoxesFromUITreeRoot uiTree
    , layerAbovemain = parseLayerAbovemainFromUITreeRoot uiTree
    }


asUITreeNodeWithTotalDisplayRegion : DisplayRegion -> EveOnline.MemoryReading.UITreeNode -> UITreeNodeWithDisplayRegion
asUITreeNodeWithTotalDisplayRegion totalDisplayRegion uiNode =
    { uiNode = uiNode
    , children = uiNode.children |> Maybe.map (List.map (EveOnline.MemoryReading.unwrapUITreeNodeChild >> asUITreeNodeWithInheritedOffset { x = totalDisplayRegion.x, y = totalDisplayRegion.y }))
    , totalDisplayRegion = totalDisplayRegion
    }


asUITreeNodeWithInheritedOffset : { x : Int, y : Int } -> EveOnline.MemoryReading.UITreeNode -> ChildOfNodeWithDisplayRegion
asUITreeNodeWithInheritedOffset inheritedOffset rawNode =
    case rawNode |> getDisplayRegionFromDictEntries of
        Nothing ->
            ChildWithoutRegion rawNode

        Just selfRegion ->
            ChildWithRegion
                (asUITreeNodeWithTotalDisplayRegion
                    { selfRegion | x = inheritedOffset.x + selfRegion.x, y = inheritedOffset.y + selfRegion.y }
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


parseInfoPanelContainerFromUIRoot : UITreeNodeWithDisplayRegion -> MaybeVisible InfoPanelContainer
parseInfoPanelContainerFromUIRoot uiTreeRoot =
    case
        uiTreeRoot
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "InfoPanelContainer")
            |> List.head
    of
        Nothing ->
            CanNotSeeIt

        Just containerNode ->
            CanSee
                { uiNode = containerNode
                , icons = parseInfoPanelIconsFromInfoPanelContainer containerNode
                , infoPanelLocationInfo = parseInfoPanelLocationInfoFromInfoPanelContainer containerNode
                , infoPanelRoute = parseInfoPanelRouteFromInfoPanelContainer containerNode
                , infoPanelAgentMissions = parseInfoPanelAgentMissionsFromInfoPanelContainer containerNode
                }


parseInfoPanelIconsFromInfoPanelContainer : UITreeNodeWithDisplayRegion -> MaybeVisible InfoPanelIcons
parseInfoPanelIconsFromInfoPanelContainer infoPanelContainerNode =
    case
        infoPanelContainerNode
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> getNameFromDictEntries >> Maybe.map ((==) "iconCont") >> Maybe.withDefault False)
            |> List.sortBy (.totalDisplayRegion >> .y)
            |> List.head
    of
        Nothing ->
            CanNotSeeIt

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
                        |> canNotSeeItFromMaybeNothing
            in
            CanSee
                { uiNode = iconContainerNode
                , search = iconNodeFromTexturePathEnd "search.png"
                , locationInfo = iconNodeFromTexturePathEnd "LocationInfo.png"
                , route = iconNodeFromTexturePathEnd "Route.png"
                , agentMissions = iconNodeFromTexturePathEnd "Missions.png"
                , dailyChallenge = iconNodeFromTexturePathEnd "dailyChallenge.png"
                }


parseInfoPanelLocationInfoFromInfoPanelContainer : UITreeNodeWithDisplayRegion -> MaybeVisible InfoPanelLocationInfo
parseInfoPanelLocationInfoFromInfoPanelContainer infoPanelContainerNode =
    case
        infoPanelContainerNode
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "InfoPanelLocationInfo")
            |> List.head
    of
        Nothing ->
            CanNotSeeIt

        Just infoPanelNode ->
            let
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
                        |> canNotSeeItFromMaybeNothing
            in
            maybeListSurroundingsButton
                |> Maybe.map
                    (\listSurroundingsButton ->
                        { uiNode = infoPanelNode
                        , listSurroundingsButton = listSurroundingsButton
                        , expandedContent = expandedContent
                        }
                    )
                |> canNotSeeItFromMaybeNothing


parseCurrentStationNameFromInfoPanelLocationInfoLabelText : String -> Maybe String
parseCurrentStationNameFromInfoPanelLocationInfoLabelText labelText =
    if labelText |> String.toLower |> String.contains "alt='current station'" |> not then
        Nothing

    else
        {- Note: 2019-12-10 with 'JavaScriptEngineSwitcher.ChakraCore.Native.win-x64', the following regex pattern led to failing 'Regex.fromString': '(?<=\\>).+?(?=\\<)'
              (The same pattern worked in chrome)
           case "(?<=\\>).+?(?=\\<)" |> Regex.fromString of
               Nothing ->
                   Just "Regex code error"

               Just regex ->
                   labelText |> Regex.find regex |> List.map .match |> List.head
        -}
        labelText
            |> String.split ">"
            |> List.drop 1
            |> List.head
            |> Maybe.andThen (String.split "<" >> List.head)
            |> Maybe.map String.trim


parseInfoPanelRouteFromInfoPanelContainer : UITreeNodeWithDisplayRegion -> MaybeVisible InfoPanelRoute
parseInfoPanelRouteFromInfoPanelContainer infoPanelContainerNode =
    case
        infoPanelContainerNode
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "InfoPanelRoute")
            |> List.head
    of
        Nothing ->
            CanNotSeeIt

        Just infoPanelRouteNode ->
            let
                routeElementMarker =
                    infoPanelRouteNode
                        |> listDescendantsWithDisplayRegion
                        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "AutopilotDestinationIcon")
                        |> List.map (\uiNode -> { uiNode = uiNode })
            in
            CanSee { uiNode = infoPanelRouteNode, routeElementMarker = routeElementMarker }


parseInfoPanelAgentMissionsFromInfoPanelContainer : UITreeNodeWithDisplayRegion -> MaybeVisible InfoPanelAgentMissions
parseInfoPanelAgentMissionsFromInfoPanelContainer infoPanelContainerNode =
    case
        infoPanelContainerNode
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "InfoPanelAgentMissions")
            |> List.head
    of
        Nothing ->
            CanNotSeeIt

        Just infoPanelNode ->
            let
                entries =
                    infoPanelNode
                        |> listDescendantsWithDisplayRegion
                        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "MissionEntry")
                        |> List.map (\uiNode -> { uiNode = uiNode })
            in
            CanSee
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


parseShipUIFromUITreeRoot : UITreeNodeWithDisplayRegion -> MaybeVisible ShipUI
parseShipUIFromUITreeRoot uiTreeRoot =
    case
        uiTreeRoot
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "ShipUI")
            |> List.head
    of
        Nothing ->
            CanNotSeeIt

        Just shipUINode ->
            case
                shipUINode
                    |> listDescendantsWithDisplayRegion
                    |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "CapacitorContainer")
                    |> List.head
            of
                Nothing ->
                    CanNotSeeIt

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
                                |> Maybe.map (parseShipUIIndication >> CanSee)
                                |> Maybe.withDefault CanNotSeeIt

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
                                }
                            )
                        |> canNotSeeItFromMaybeNothing


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
                |> canNotSeeItFromMaybeNothing
    in
    { uiNode = indicationUINode, maneuverType = maneuverType }


parseOverviewWindowFromUITreeRoot : UITreeNodeWithDisplayRegion -> MaybeVisible OverviewWindow
parseOverviewWindowFromUITreeRoot uiTreeRoot =
    case
        uiTreeRoot
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "OverView")
            |> List.head
    of
        Nothing ->
            CanNotSeeIt

        Just overviewWindowNode ->
            let
                scrollNode =
                    overviewWindowNode
                        |> listDescendantsWithDisplayRegion
                        |> List.filter (.uiNode >> .pythonObjectTypeName >> String.toLower >> String.contains "scroll")
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
            CanSee { uiNode = overviewWindowNode, entriesHeaders = entriesHeaders, entries = entries }


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


parseSelectedItemWindowFromUITreeRoot : UITreeNodeWithDisplayRegion -> MaybeVisible SelectedItemWindow
parseSelectedItemWindowFromUITreeRoot uiTreeRoot =
    uiTreeRoot
        |> listDescendantsWithDisplayRegion
        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "ActiveItem")
        |> List.head
        |> Maybe.map parseSelectedItemWindow
        |> canNotSeeItFromMaybeNothing


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


parseDronesWindowFromUITreeRoot : UITreeNodeWithDisplayRegion -> MaybeVisible DronesWindow
parseDronesWindowFromUITreeRoot uiTreeRoot =
    case
        uiTreeRoot
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "DroneView")
            |> List.head
    of
        Nothing ->
            CanNotSeeIt

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
            CanSee
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
    , expander = expanderNode |> Maybe.map parseExpander |> canNotSeeItFromMaybeNothing
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


parseProbeScannerWindowFromUITreeRoot : UITreeNodeWithDisplayRegion -> MaybeVisible ProbeScannerWindow
parseProbeScannerWindowFromUITreeRoot uiTreeRoot =
    case
        uiTreeRoot
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "ProbeScannerWindow")
            |> List.head
    of
        Nothing ->
            CanNotSeeIt

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
            CanSee { uiNode = windowNode, scanResults = scanResults }


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


parseStationWindowFromUITreeRoot : UITreeNodeWithDisplayRegion -> MaybeVisible StationWindow
parseStationWindowFromUITreeRoot uiTreeRoot =
    case
        uiTreeRoot
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "LobbyWnd")
            |> List.head
    of
        Nothing ->
            CanNotSeeIt

        Just windowNode ->
            let
                maybeUndockButton =
                    windowNode
                        |> listDescendantsWithDisplayRegion
                        |> List.filter (.uiNode >> getNameFromDictEntries >> Maybe.map (String.contains "undock") >> Maybe.withDefault False)
                        |> List.filterMap
                            (\undockNodeCandidate ->
                                let
                                    maybeMainText =
                                        undockNodeCandidate
                                            |> getAllContainedDisplayTextsWithRegion
                                            |> List.sortBy (Tuple.second >> .totalDisplayRegion >> areaFromDisplayRegion >> Maybe.withDefault 0)
                                            |> List.reverse
                                            |> List.head
                                            |> Maybe.map Tuple.first
                                in
                                maybeMainText
                                    |> Maybe.map (\mainText -> { uiNode = undockNodeCandidate, mainText = mainText })
                            )
                        |> List.head
            in
            CanSee { uiNode = windowNode, undockButton = maybeUndockButton }


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


parseModuleButtonTooltipFromUITreeRoot : UITreeNodeWithDisplayRegion -> MaybeVisible ModuleButtonTooltip
parseModuleButtonTooltipFromUITreeRoot uiTreeRoot =
    case
        uiTreeRoot
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "ModuleButtonTooltip")
            |> List.head
    of
        Nothing ->
            CanNotSeeIt

        Just uiNode ->
            CanSee (parseModuleButtonTooltip uiNode)


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
    [ ( "CTRL", Common.EffectOnWindow.VK_LCONTROL )
    , ( "STRG", Common.EffectOnWindow.VK_LCONTROL )
    , ( "ALT", Common.EffectOnWindow.VK_LMENU )
    , ( "SHIFT", Common.EffectOnWindow.VK_LSHIFT )
    , ( "UMSCH", Common.EffectOnWindow.VK_LSHIFT )
    , ( "F1", Common.EffectOnWindow.key_F1 )
    , ( "F2", Common.EffectOnWindow.key_F2 )
    , ( "F3", Common.EffectOnWindow.key_F3 )
    , ( "F4", Common.EffectOnWindow.key_F4 )
    , ( "F5", Common.EffectOnWindow.key_F5 )
    , ( "F6", Common.EffectOnWindow.key_F6 )
    , ( "F7", Common.EffectOnWindow.key_F7 )
    , ( "F8", Common.EffectOnWindow.key_F8 )
    , ( "F9", Common.EffectOnWindow.key_F9 )
    , ( "F10", Common.EffectOnWindow.key_F10 )
    , ( "F11", Common.EffectOnWindow.key_F11 )
    , ( "F12", Common.EffectOnWindow.key_F12 )
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
        visibleUsers =
            chatWindowUiNode
                |> listDescendantsWithDisplayRegion
                |> List.filter (\uiNode -> [ "XmppChatSimpleUserEntry", "XmppChatUserEntry" ] |> List.member uiNode.uiNode.pythonObjectTypeName)
                |> List.map parseChatUserEntry
    in
    { uiNode = chatWindowUiNode
    , name = getNameFromDictEntries chatWindowUiNode.uiNode
    , visibleUsers = visibleUsers
    }


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


parseMarketOrdersWindowFromUITreeRoot : UITreeNodeWithDisplayRegion -> MaybeVisible MarketOrdersWindow
parseMarketOrdersWindowFromUITreeRoot uiTreeRoot =
    uiTreeRoot
        |> listDescendantsWithDisplayRegion
        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "MarketOrdersWnd")
        |> List.head
        |> Maybe.map parseMarketOrdersWindow
        |> canNotSeeItFromMaybeNothing


parseMarketOrdersWindow : UITreeNodeWithDisplayRegion -> MarketOrdersWindow
parseMarketOrdersWindow windowUINode =
    { uiNode = windowUINode
    }


parseFittingWindowFromUITreeRoot : UITreeNodeWithDisplayRegion -> MaybeVisible FittingWindow
parseFittingWindowFromUITreeRoot uiTreeRoot =
    uiTreeRoot
        |> listDescendantsWithDisplayRegion
        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "FittingWindow")
        |> List.head
        |> Maybe.map parseFittingWindow
        |> canNotSeeItFromMaybeNothing


parseFittingWindow : UITreeNodeWithDisplayRegion -> FittingWindow
parseFittingWindow windowUINode =
    { uiNode = windowUINode
    }


parseSurveyScanWindowFromUITreeRoot : UITreeNodeWithDisplayRegion -> MaybeVisible SurveyScanWindow
parseSurveyScanWindowFromUITreeRoot uiTreeRoot =
    uiTreeRoot
        |> listDescendantsWithDisplayRegion
        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "SurveyScanView")
        |> List.head
        |> Maybe.map parseSurveyScanWindow
        |> canNotSeeItFromMaybeNothing


parseSurveyScanWindow : UITreeNodeWithDisplayRegion -> SurveyScanWindow
parseSurveyScanWindow windowUINode =
    { uiNode = windowUINode
    , scanEntries =
        windowUINode
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "SurveyScanEntry")
    }


parseNeocomFromUITreeRoot : UITreeNodeWithDisplayRegion -> MaybeVisible Neocom
parseNeocomFromUITreeRoot uiTreeRoot =
    case
        uiTreeRoot
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "Neocom")
            |> List.head
    of
        Nothing ->
            CanNotSeeIt

        Just uiNode ->
            CanSee (parseNeocom uiNode)


parseNeocom : UITreeNodeWithDisplayRegion -> Neocom
parseNeocom neocomUiNode =
    let
        maybeClockTextAndNode =
            neocomUiNode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "InGameClock")
                |> List.concatMap getAllContainedDisplayTextsWithRegion
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
                |> canNotSeeItFromMaybeNothing
    in
    { uiNode = neocomUiNode
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


parseLayerAbovemainFromUITreeRoot : UITreeNodeWithDisplayRegion -> MaybeVisible UITreeNodeWithDisplayRegion
parseLayerAbovemainFromUITreeRoot uiTreeRoot =
    uiTreeRoot
        |> listDescendantsWithDisplayRegion
        |> List.filter (.uiNode >> getNameFromDictEntries >> (==) (Just "l_abovemain"))
        |> List.head
        |> canNotSeeItFromMaybeNothing


parseNumberTruncatingAfterOptionalDecimalSeparator : String -> Result String Int
parseNumberTruncatingAfterOptionalDecimalSeparator numberDisplayText =
    case "^(\\d+(\\s*[\\s\\,\\.]\\d{3})*?)(?=(|[,\\.]\\d)$)" |> Regex.fromString of
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


canNotSeeItFromMaybeNothing : Maybe a -> MaybeVisible a
canNotSeeItFromMaybeNothing maybe =
    case maybe of
        Nothing ->
            CanNotSeeIt

        Just feature ->
            CanSee feature


maybeNothingFromCanNotSeeIt : MaybeVisible a -> Maybe a
maybeNothingFromCanNotSeeIt maybeVisible =
    case maybeVisible of
        CanNotSeeIt ->
            Nothing

        CanSee feature ->
            Just feature


maybeVisibleAndThen : (a -> MaybeVisible b) -> MaybeVisible a -> MaybeVisible b
maybeVisibleAndThen map maybeVisible =
    case maybeVisible of
        CanNotSeeIt ->
            CanNotSeeIt

        CanSee visible ->
            map visible


maybeVisibleMap : (a -> b) -> MaybeVisible a -> MaybeVisible b
maybeVisibleMap map maybeVisible =
    case maybeVisible of
        CanNotSeeIt ->
            CanNotSeeIt

        CanSee visible ->
            CanSee (map visible)
