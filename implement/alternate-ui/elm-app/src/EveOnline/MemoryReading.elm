module EveOnline.MemoryReading exposing
    ( ChatUserEntry
    , ChatWindow
    , ChatWindowStack
    , ChildOfNodeWithDisplayRegion(..)
    , ColorComponents
    , ContextMenu
    , ContextMenuEntry
    , DisplayRegion
    , DronesWindow
    , DronesWindowDroneGroup
    , DronesWindowDroneGroupHeader
    , DronesWindowEntry
    , Expander
    , Hitpoints
    , InfoPanelLocationInfo
    , InfoPanelLocationInfoExpandedContent
    , InfoPanelRoute
    , InfoPanelRouteRouteElementMarker
    , InventoryWindow
    , InventoryWindowCapacityGauge
    , MaybeVisible(..)
    , ModuleButtonTooltip
    , Neocom
    , NeocomClock
    , OverviewWindow
    , OverviewWindowEntry
    , ParsedUserInterface
    , ProbeScanResult
    , ProbeScannerWindow
    , ShipManeuverType(..)
    , ShipUI
    , ShipUIIndication
    , ShipUIModule
    , StationWindow
    , Target
    , UITreeNode
    , UITreeNodeWithDisplayRegion
    , asUITreeNodeWithTotalDisplayRegion
    , canNotSeeItFromMaybeNothing
    , centerFromDisplayRegion
    , countDescendantsInUITreeNode
    , decodeMemoryReadingFromString
    , getDisplayText
    , getHorizontalOffsetFromParentAndWidth
    , getMostPopulousDescendantMatchingPredicate
    , getNameFromDictEntries
    , getVerticalOffsetFromParent
    , listDescendantsInUITreeNode
    , listDescendantsWithDisplayRegion
    , maybeNothingFromCanNotSeeIt
    , maybeVisibleAndThen
    , maybeVisibleMap
    , parseContextMenusFromUITreeRoot
    , parseInventoryCapacityGaugeText
    , parseOverviewEntryDistanceInMetersFromText
    , parseShipUIFromUITreeRoot
    , parseUITreeWithDisplayRegionFromUITree
    , parseUserInterfaceFromUITree
    , uiTreeNodeDecoder
    , unwrapUITreeNodeChild
    )

import Dict
import Json.Decode
import Json.Encode
import Regex
import Result.Extra


type alias ParsedUserInterface =
    { uiTree : UITreeNodeWithDisplayRegion
    , contextMenus : List ContextMenu
    , shipUI : MaybeVisible ShipUI
    , targets : List Target
    , infoPanelLocationInfo : MaybeVisible InfoPanelLocationInfo
    , infoPanelRoute : MaybeVisible InfoPanelRoute
    , overviewWindow : MaybeVisible OverviewWindow
    , dronesWindow : MaybeVisible DronesWindow
    , probeScannerWindow : MaybeVisible ProbeScannerWindow
    , stationWindow : MaybeVisible StationWindow
    , inventoryWindows : List InventoryWindow
    , chatWindowStacks : List ChatWindowStack
    , moduleButtonTooltip : MaybeVisible ModuleButtonTooltip
    , neocom : MaybeVisible Neocom
    }


type alias UITreeNodeWithDisplayRegion =
    { uiNode : UITreeNode
    , children : Maybe (List ChildOfNodeWithDisplayRegion)
    , totalDisplayRegion : DisplayRegion
    }


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


type ChildOfNodeWithDisplayRegion
    = ChildWithRegion UITreeNodeWithDisplayRegion
    | ChildWithoutRegion UITreeNode


type alias UITreeNode =
    { originalJson : Json.Encode.Value
    , pythonObjectAddress : String
    , pythonObjectTypeName : String
    , dictEntriesOfInterest : Dict.Dict String Json.Encode.Value
    , children : Maybe (List UITreeNodeChild)
    }


type UITreeNodeChild
    = UITreeNodeChild UITreeNode


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
    , indication : MaybeVisible ShipUIIndication
    , modules : List ShipUIModule
    , hitpointsPercent : Hitpoints
    }


type alias ShipUIIndication =
    { maneuverType : MaybeVisible ShipManeuverType
    }


type alias ShipUIModule =
    { uiNode : UITreeNodeWithDisplayRegion
    , isActive : Maybe Bool
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


type alias InfoPanelRoute =
    { routeElementMarker : List InfoPanelRouteRouteElementMarker
    }


type alias InfoPanelRouteRouteElementMarker =
    { uiNode : UITreeNodeWithDisplayRegion
    }


type alias InfoPanelLocationInfo =
    { listSurroundingsButton : UITreeNodeWithDisplayRegion
    , expandedContent : MaybeVisible InfoPanelLocationInfoExpandedContent
    }


type alias InfoPanelLocationInfoExpandedContent =
    { currentStationName : Maybe String
    }


type alias Target =
    { uiNode : UITreeNodeWithDisplayRegion
    , textsTopToBottom : List String
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
    , distanceInMeters : Result String Int
    , iconSpriteColorPercent : Maybe ColorComponents
    }


type alias ColorComponents =
    { a : Int, r : Int, g : Int, b : Int }


type alias DronesWindow =
    { uiNode : UITreeNodeWithDisplayRegion
    , droneGroups : List DronesWindowDroneGroup
    }


type alias DronesWindowDroneGroup =
    { header : DronesWindowDroneGroupHeader
    , drones : List DronesWindowEntry
    }


type alias DronesWindowDroneGroupHeader =
    { uiNode : UITreeNodeWithDisplayRegion
    , mainText : Maybe String
    , expander : MaybeVisible Expander
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
    , warpButton : Maybe UITreeNodeWithDisplayRegion
    }


type alias StationWindow =
    { uiNode : UITreeNodeWithDisplayRegion
    , undockButton : Maybe { uiNode : UITreeNodeWithDisplayRegion, mainText : String }
    }


type alias InventoryWindow =
    { uiNode : UITreeNodeWithDisplayRegion
    , leftTreeEntries : List InventoryWindowLeftTreeEntry
    , selectedContainerCapacityGauge : Maybe InventoryWindowCapacityGauge
    , selectedContainerInventory : Maybe Inventory
    }


type alias Inventory =
    { uiNode : UITreeNodeWithDisplayRegion
    , listViewItems : List UITreeNodeWithDisplayRegion
    }


type alias InventoryWindowLeftTreeEntry =
    { uiNode : UITreeNodeWithDisplayRegion
    , text : String
    }


type alias InventoryWindowCapacityGauge =
    { maximum : Int
    , used : Int
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


type alias Expander =
    { uiNode : UITreeNodeWithDisplayRegion
    , texturePath : Maybe String
    , isExpanded : Maybe Bool
    }


type MaybeVisible feature
    = CanNotSeeIt
    | CanSee feature


parseUITreeWithDisplayRegionFromUITree : UITreeNode -> UITreeNodeWithDisplayRegion
parseUITreeWithDisplayRegionFromUITree uiTree =
    uiTree |> asUITreeNodeWithTotalDisplayRegion (uiTree |> getDisplayRegionFromDictEntries |> Maybe.withDefault { x = 0, y = 0, width = 0, height = 0 })


parseUserInterfaceFromUITree : UITreeNodeWithDisplayRegion -> ParsedUserInterface
parseUserInterfaceFromUITree uiTree =
    { uiTree = uiTree
    , contextMenus = parseContextMenusFromUITreeRoot uiTree
    , shipUI = parseShipUIFromUITreeRoot uiTree
    , targets = parseTargetsFromUITreeRoot uiTree
    , infoPanelLocationInfo = parseInfoPanelLocationInfoFromUITreeRoot uiTree
    , infoPanelRoute = parseInfoPanelRouteFromUITreeRoot uiTree
    , overviewWindow = parseOverviewWindowFromUITreeRoot uiTree
    , dronesWindow = parseDronesWindowFromUITreeRoot uiTree
    , probeScannerWindow = parseProbeScannerWindowFromUITreeRoot uiTree
    , stationWindow = parseStationWindowFromUITreeRoot uiTree
    , inventoryWindows = parseInventoryWindowsFromUITreeRoot uiTree
    , moduleButtonTooltip = parseModuleButtonTooltipFromUITreeRoot uiTree
    , chatWindowStacks = parseChatWindowStacksFromUITreeRoot uiTree
    , neocom = parseNeocomFromUITreeRoot uiTree
    }


asUITreeNodeWithTotalDisplayRegion : DisplayRegion -> UITreeNode -> UITreeNodeWithDisplayRegion
asUITreeNodeWithTotalDisplayRegion totalDisplayRegion uiNode =
    { uiNode = uiNode
    , children = uiNode.children |> Maybe.map (List.map (unwrapUITreeNodeChild >> asUITreeNodeWithInheritedOffset { x = totalDisplayRegion.x, y = totalDisplayRegion.y }))
    , totalDisplayRegion = totalDisplayRegion
    }


asUITreeNodeWithInheritedOffset : { x : Int, y : Int } -> UITreeNode -> ChildOfNodeWithDisplayRegion
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


getDisplayRegionFromDictEntries : UITreeNode -> Maybe DisplayRegion
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


parseInfoPanelLocationInfoFromUITreeRoot : UITreeNodeWithDisplayRegion -> MaybeVisible InfoPanelLocationInfo
parseInfoPanelLocationInfoFromUITreeRoot uiTreeRoot =
    case
        uiTreeRoot
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
                        { listSurroundingsButton = listSurroundingsButton
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


parseInfoPanelRouteFromUITreeRoot : UITreeNodeWithDisplayRegion -> MaybeVisible InfoPanelRoute
parseInfoPanelRouteFromUITreeRoot uiTreeRoot =
    case
        uiTreeRoot
            |> listDescendantsWithDisplayRegion
            |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "InfoPanelRoute")
            |> List.head
    of
        Nothing ->
            CanNotSeeIt

        Just infoPanelRouteElement ->
            let
                routeElementMarker =
                    infoPanelRouteElement
                        |> listDescendantsWithDisplayRegion
                        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "AutopilotDestinationIcon")
                        |> List.map (\uiNode -> { uiNode = uiNode })
            in
            CanSee { routeElementMarker = routeElementMarker }


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
            let
                speedGaugeElement =
                    shipUINode
                        |> listDescendantsWithDisplayRegion
                        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "SpeedGauge")
                        |> List.head

                maybeIndicationElement =
                    shipUINode.uiNode
                        |> listDescendantsInUITreeNode
                        |> List.filter (getNameFromDictEntries >> Maybe.map (String.toLower >> String.contains "indicationcontainer") >> Maybe.withDefault False)
                        |> List.head

                indication =
                    maybeIndicationElement
                        |> Maybe.map (parseShipUIIndication >> CanSee)
                        |> Maybe.withDefault CanNotSeeIt

                modules =
                    shipUINode
                        |> listDescendantsWithDisplayRegion
                        |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "ModuleButton")
                        |> List.map
                            (\moduleNode ->
                                { uiNode = moduleNode
                                , isActive =
                                    moduleNode.uiNode.dictEntriesOfInterest
                                        |> Dict.get "ramp_active"
                                        |> Maybe.andThen (Json.Decode.decodeValue Json.Decode.bool >> Result.toMaybe)
                                }
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
            in
            maybeHitpointsPercent
                |> Maybe.map
                    (\hitpointsPercent ->
                        { uiNode = shipUINode
                        , indication = indication
                        , modules = modules
                        , hitpointsPercent = hitpointsPercent
                        }
                    )
                |> canNotSeeItFromMaybeNothing


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
    in
    { uiNode = targetNode
    , textsTopToBottom = textsTopToBottom
    }


parseShipUIIndication : UITreeNode -> ShipUIIndication
parseShipUIIndication indicationUINode =
    let
        displayTexts =
            indicationUINode |> getAllContainedDisplayTexts

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
    { maneuverType = maneuverType }


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

        distanceInMeters =
            cellsTexts
                |> Dict.get "Distance"
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
    in
    { uiNode = overviewEntryNode
    , textsLeftToRight = textsLeftToRight
    , cellsTexts = cellsTexts
    , distanceInMeters = distanceInMeters
    , iconSpriteColorPercent = iconSpriteColorPercent
    }


parseOverviewEntryDistanceInMetersFromText : String -> Result String Int
parseOverviewEntryDistanceInMetersFromText distanceDisplayTextBeforeTrim =
    case "^[\\d\\,]+(?=\\s*m)" |> Regex.fromString of
        Nothing ->
            Err "Regex code error"

        Just regexForUnitMeter ->
            case "^[\\d\\,]+(?=\\s*km)" |> Regex.fromString of
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
                                |> String.replace "," ""
                                |> String.toInt
                                |> Result.fromMaybe ("Failed to parse to integer: " ++ match.match)

                        Nothing ->
                            case distanceDisplayText |> Regex.find regexForUnitKilometer |> List.head of
                                Just match ->
                                    match.match
                                        |> String.replace "," ""
                                        |> String.toInt
                                        -- unit 'km'
                                        |> Maybe.map ((*) 1000)
                                        |> Result.fromMaybe ("Failed to parse to integer: " ++ match.match)

                                Nothing ->
                                    Err ("Text did not match expected number format: '" ++ distanceDisplayText ++ "'")


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
                scrollNode =
                    windowNode
                        |> listDescendantsWithDisplayRegion
                        |> List.filter (.uiNode >> .pythonObjectTypeName >> String.toLower >> String.contains "scroll")
                        |> List.head

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
            in
            CanSee { uiNode = windowNode, droneGroups = droneGroups }


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
    in
    { uiNode = groupHeaderUiNode
    , mainText = mainText
    , expander = expanderNode |> Maybe.map parseExpander |> canNotSeeItFromMaybeNothing
    }


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

                scanResults =
                    scanResultsNodes
                        |> List.map parseProbeScanResult
            in
            CanSee { uiNode = windowNode, scanResults = scanResults }


parseProbeScanResult : UITreeNodeWithDisplayRegion -> ProbeScanResult
parseProbeScanResult scanResultUiNode =
    let
        textsLeftToRight =
            scanResultUiNode
                |> getAllContainedDisplayTextsWithRegion
                |> List.sortBy (Tuple.second >> .totalDisplayRegion >> .x)
                |> List.map Tuple.first

        warpButton =
            scanResultUiNode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> getTexturePathFromDictEntries >> Maybe.map (String.endsWith "44_32_18.png") >> Maybe.withDefault False)
                |> List.head
    in
    { uiNode = scanResultUiNode
    , textsLeftToRight = textsLeftToRight
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
                |> Maybe.map (.uiNode >> listDescendantsInUITreeNode)
                |> Maybe.withDefault []
                |> List.filterMap getDisplayText
                |> List.sortBy (String.length >> negate)
                |> List.head
                |> Maybe.andThen (parseInventoryCapacityGaugeText >> Result.toMaybe)

        leftTreeEntries =
            windowUiNode
                |> listDescendantsWithDisplayRegion
                |> List.filter (.uiNode >> .pythonObjectTypeName >> String.startsWith "TreeViewEntry")
                |> List.map
                    (\treeEntryNode ->
                        let
                            displayTextsWithRegion =
                                treeEntryNode
                                    |> getAllContainedDisplayTextsWithRegion

                            text =
                                displayTextsWithRegion
                                    |> List.sortBy (\( _, textNode ) -> textNode.totalDisplayRegion.x + textNode.totalDisplayRegion.y)
                                    |> List.head
                                    |> Maybe.map Tuple.first
                                    |> Maybe.withDefault ""
                        in
                        { uiNode = treeEntryNode
                        , text = text
                        }
                    )

        rightContainerNode =
            windowUiNode
                |> listDescendantsWithDisplayRegion
                |> List.filter
                    (\uiNode ->
                        uiNode.uiNode.pythonObjectTypeName
                            == "Container"
                            && (uiNode.uiNode |> getNameFromDictEntries |> Maybe.map (String.contains "right") |> Maybe.withDefault False)
                    )
                |> List.head

        selectedContainerInventory =
            rightContainerNode
                |> Maybe.andThen
                    (listDescendantsWithDisplayRegion
                        >> List.filter (\uiNode -> [ "ShipCargo", "ShipDroneBay", "ShipOreHold", "StationItems" ] |> List.member uiNode.uiNode.pythonObjectTypeName)
                        >> List.head
                    )
                |> Maybe.map
                    (\selectedContainerInventoryNode ->
                        { uiNode = selectedContainerInventoryNode
                        , listViewItems =
                            selectedContainerInventoryNode
                                |> listDescendantsWithDisplayRegion
                                |> List.filter (.uiNode >> .pythonObjectTypeName >> (==) "Item")
                        }
                    )
    in
    { uiNode = windowUiNode
    , leftTreeEntries = leftTreeEntries
    , selectedContainerCapacityGauge = selectedContainerCapacityGauge
    , selectedContainerInventory = selectedContainerInventory
    }


parseInventoryCapacityGaugeText : String -> Result String InventoryWindowCapacityGauge
parseInventoryCapacityGaugeText capacityText =
    let
        numbersParseResults =
            capacityText
                |> String.replace "m³" ""
                |> String.split "/"
                |> List.map (String.trim >> parseNumberTruncatingAfterOptionalDecimalSeparator)
    in
    case numbersParseResults |> Result.Extra.combine of
        Err parseError ->
            Err ("Failed to parse numbers: " ++ parseError)

        Ok numbers ->
            case numbers of
                [ leftNumber, rightNumber ] ->
                    Ok { used = leftNumber, maximum = rightNumber }

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
            CanSee { uiNode = uiNode }


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
                            Err "Unexpected numer of text elements."


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
                        |> String.toInt
                        |> Result.fromMaybe ("Failed to parse to integer: " ++ match.match)


centerFromDisplayRegion : DisplayRegion -> Location2d
centerFromDisplayRegion region =
    { x = region.x + region.width // 2, y = region.y + region.height // 2 }


getDisplayText : UITreeNode -> Maybe String
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


getAllContainedDisplayTexts : UITreeNode -> List String
getAllContainedDisplayTexts uiNode =
    uiNode
        :: (uiNode |> listDescendantsInUITreeNode)
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


getNameFromDictEntries : UITreeNode -> Maybe String
getNameFromDictEntries =
    getStringPropertyFromDictEntries "_name"


getHintTextFromDictEntries : UITreeNode -> Maybe String
getHintTextFromDictEntries =
    getStringPropertyFromDictEntries "_hint"


getTexturePathFromDictEntries : UITreeNode -> Maybe String
getTexturePathFromDictEntries =
    getStringPropertyFromDictEntries "texturePath"


getStringPropertyFromDictEntries : String -> UITreeNode -> Maybe String
getStringPropertyFromDictEntries dictEntryKey uiNode =
    uiNode.dictEntriesOfInterest
        |> Dict.get dictEntryKey
        |> Maybe.andThen (Json.Decode.decodeValue Json.Decode.string >> Result.toMaybe)


getColorPercentFromDictEntries : UITreeNode -> Maybe ColorComponents
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


getHorizontalOffsetFromParentAndWidth : UITreeNode -> Maybe { offset : Int, width : Int }
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


getVerticalOffsetFromParent : UITreeNode -> Maybe Int
getVerticalOffsetFromParent =
    .dictEntriesOfInterest
        >> Dict.get "_displayY"
        >> Maybe.andThen (Json.Decode.decodeValue Json.Decode.float >> Result.toMaybe)
        >> Maybe.map round


getMostPopulousDescendantMatchingPredicate : (UITreeNode -> Bool) -> UITreeNode -> Maybe UITreeNode
getMostPopulousDescendantMatchingPredicate predicate parent =
    listDescendantsInUITreeNode parent
        |> List.filter predicate
        |> List.sortBy countDescendantsInUITreeNode
        |> List.reverse
        |> List.head


unwrapUITreeNodeChild : UITreeNodeChild -> UITreeNode
unwrapUITreeNodeChild child =
    case child of
        UITreeNodeChild node ->
            node


countDescendantsInUITreeNode : UITreeNode -> Int
countDescendantsInUITreeNode parent =
    parent.children
        |> Maybe.withDefault []
        |> List.map unwrapUITreeNodeChild
        |> List.map (countDescendantsInUITreeNode >> (+) 1)
        |> List.sum


listDescendantsInUITreeNode : UITreeNode -> List UITreeNode
listDescendantsInUITreeNode parent =
    parent.children
        |> Maybe.withDefault []
        |> List.map unwrapUITreeNodeChild
        |> List.concatMap (\child -> child :: listDescendantsInUITreeNode child)


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


decodeMemoryReadingFromString : String -> Result Json.Decode.Error UITreeNode
decodeMemoryReadingFromString =
    Json.Decode.decodeString uiTreeNodeDecoder


uiTreeNodeDecoder : Json.Decode.Decoder UITreeNode
uiTreeNodeDecoder =
    Json.Decode.map5
        (\originalJson pythonObjectAddress pythonObjectTypeName dictEntriesOfInterest children ->
            { originalJson = originalJson
            , pythonObjectAddress = pythonObjectAddress
            , pythonObjectTypeName = pythonObjectTypeName
            , dictEntriesOfInterest = dictEntriesOfInterest |> Dict.fromList
            , children = children |> Maybe.map (List.map UITreeNodeChild)
            }
        )
        Json.Decode.value
        (Json.Decode.field "pythonObjectAddress" Json.Decode.string)
        (decodeOptionalField "pythonObjectTypeName" Json.Decode.string |> Json.Decode.map (Maybe.withDefault ""))
        (Json.Decode.field "dictEntriesOfInterest" (Json.Decode.keyValuePairs Json.Decode.value))
        (decodeOptionalOrNullField "children" (Json.Decode.list (Json.Decode.lazy (\_ -> uiTreeNodeDecoder))))


decodeOptionalOrNullField : String -> Json.Decode.Decoder a -> Json.Decode.Decoder (Maybe a)
decodeOptionalOrNullField fieldName decoder =
    decodeOptionalField fieldName (Json.Decode.nullable decoder)
        |> Json.Decode.map (Maybe.andThen identity)


decodeOptionalField : String -> Json.Decode.Decoder a -> Json.Decode.Decoder (Maybe a)
decodeOptionalField fieldName decoder =
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
