module Frontend.InspectParsedUserInterface exposing
    ( ExpandableViewNode(..)
    , InputOnUINode(..)
    , InputRoute
    , ParsedUITreeViewPathNode(..)
    , TreeViewNode
    , TreeViewNodeChildren(..)
    , maybeInputOfferHtml
    , renderTreeNodeFromParsedUserInterface
    , treeViewNodeFromMemoryReadingUITreeNode
    , uiNodeCommonSummaryText
    )

import Dict
import EveOnline.MemoryReading
import EveOnline.ParseUserInterface exposing (DisplayRegion, UITreeNodeWithDisplayRegion)
import Html
import Html.Events as HE
import Json.Encode
import Set
import String.Extra


type alias TreeViewNode event expandableId =
    { selfHtml : Html.Html event
    , children : TreeViewNodeChildren event expandableId
    }


type TreeViewNodeChildren event expandableId
    = NoChildren
    | ExpandableChildren expandableId (() -> List (TreeViewNode event expandableId))


type InputOnUINode
    = MouseClickLeft
    | MouseClickRight


type ParsedUITreeViewPathNode
    = NamedNode String
    | IndexedNode Int
    | UITreeNode ExpandableViewNode


type alias InputRoute event =
    EveOnline.ParseUserInterface.UITreeNodeWithDisplayRegion -> InputOnUINode -> event


type ExpandableViewNode
    = ExpandableUITreeNode UITreeNodeIdentity
    | ExpandableUITreeNodeChildren
    | ExpandableUITreeNodeDictEntries
    | ExpandableUITreeNodeAllDisplayTexts


type alias UITreeNodeIdentity =
    { pythonObjectAddress : String }


type alias ViewConfig event =
    { inputRoute : Maybe (InputRoute event)
    , uiNodesWithDisplayRegion : Dict.Dict String UITreeNodeWithDisplayRegion
    }


maybeInputOfferHtml : Maybe (InputRoute event) -> List InputOnUINode -> EveOnline.ParseUserInterface.UITreeNodeWithDisplayRegion -> Html.Html event
maybeInputOfferHtml maybeInputRoute enabledInputKinds uiNode =
    maybeInputRoute
        |> Maybe.map
            (\inputRoute ->
                enabledInputKinds
                    |> List.map
                        (\inputKind ->
                            let
                                inputCmd =
                                    inputRoute uiNode inputKind
                            in
                            [ displayTextForInputKind inputKind |> Html.text ]
                                |> Html.button [ HE.onClick inputCmd ]
                        )
                    |> Html.span []
            )
        |> Maybe.withDefault (Html.text "")


displayTextForInputKind : InputOnUINode -> String
displayTextForInputKind inputKind =
    case inputKind of
        MouseClickLeft ->
            "leftclick"

        MouseClickRight ->
            "rightclick"


renderTreeNodeFromParsedUserInterface :
    Maybe (InputRoute event)
    -> Dict.Dict String UITreeNodeWithDisplayRegion
    -> EveOnline.ParseUserInterface.ParsedUserInterface
    -> TreeViewNode event ParsedUITreeViewPathNode
renderTreeNodeFromParsedUserInterface maybeInputRoute uiNodesWithDisplayRegion parsedUserInterface =
    let
        commonSummaryHtml =
            [ parsedUserInterface.uiTree.uiNode |> uiNodeCommonSummaryText |> Html.text ] |> Html.span []

        viewConfig =
            { inputRoute = maybeInputRoute, uiNodesWithDisplayRegion = uiNodesWithDisplayRegion }

        children =
            treeNodeChildrenFromRecord
                [ { fieldName = "uiTree"
                  , fieldValueSummary = "..."
                  , fieldValueChildren = always [ treeViewNodeFromUINode viewConfig parsedUserInterface.uiTree ]
                  }
                , parsedUserInterface.contextMenus
                    |> fieldFromListInstance
                        { fieldName = "contextMenus"
                        , fieldValueChildren = treeNodeChildrenFromParsedUserInterfaceContextMenu viewConfig
                        }
                , parsedUserInterface.shipUI
                    |> fieldFromMaybeInstance
                        { fieldName = "shipUI"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromParsedUserInterfaceShipUI viewConfig
                        }
                , parsedUserInterface.targets
                    |> fieldFromListInstance
                        { fieldName = "targets"
                        , fieldValueChildren = treeNodeChildrenFromParsedUserInterfaceTarget viewConfig
                        }
                , parsedUserInterface.infoPanelContainer
                    |> fieldFromMaybeInstance
                        { fieldName = "infoPanelContainer"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromInfoPanelContainer viewConfig
                        }
                , parsedUserInterface.overviewWindows
                    |> fieldFromListInstance
                        { fieldName = "overviewWindows"
                        , fieldValueChildren = treeNodeChildrenFromOverviewWindow viewConfig
                        }
                , parsedUserInterface.selectedItemWindow
                    |> fieldFromMaybeInstance
                        { fieldName = "selectedItemWindow"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromSelectedItemWindow viewConfig
                        }
                , parsedUserInterface.dronesWindow
                    |> fieldFromMaybeInstance
                        { fieldName = "dronesWindow"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromDronesWindow viewConfig
                        }
                , parsedUserInterface.fittingWindow
                    |> fieldFromMaybeInstance
                        { fieldName = "fittingWindow"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromMarketOrdersWindow viewConfig
                        }
                , parsedUserInterface.probeScannerWindow
                    |> fieldFromMaybeInstance
                        { fieldName = "probeScannerWindow"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromProbeScannerWindow viewConfig
                        }
                , parsedUserInterface.directionalScannerWindow
                    |> fieldFromMaybeInstance
                        { fieldName = "directionalScannerWindow"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromDirectionalScannerWindow viewConfig
                        }
                , parsedUserInterface.stationWindow
                    |> fieldFromMaybeInstance
                        { fieldName = "stationWindow"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromStationWindow viewConfig
                        }
                , parsedUserInterface.inventoryWindows
                    |> fieldFromListInstance
                        { fieldName = "inventoryWindows"
                        , fieldValueChildren = treeNodeChildrenFromParsedUserInterfaceInventoryWindow viewConfig
                        }
                , parsedUserInterface.chatWindowStacks
                    |> fieldFromListInstance
                        { fieldName = "chatWindowStacks"
                        , fieldValueChildren = treeNodeChildrenFromChatWindowStack viewConfig
                        }
                , parsedUserInterface.agentConversationWindows
                    |> fieldFromListInstance
                        { fieldName = "agentConversationWindows"
                        , fieldValueChildren = treeNodeChildrenFromAgentConversationWindow viewConfig
                        }
                , parsedUserInterface.marketOrdersWindow
                    |> fieldFromMaybeInstance
                        { fieldName = "marketOrdersWindow"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromMarketOrdersWindow viewConfig
                        }
                , parsedUserInterface.surveyScanWindow
                    |> fieldFromMaybeInstance
                        { fieldName = "surveyScanWindow"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromSurveyScanWindow viewConfig
                        }
                , parsedUserInterface.bookmarkLocationWindow
                    |> fieldFromMaybeInstance
                        { fieldName = "bookmarkLocationWindow"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromBookmarkLocationWindow viewConfig
                        }
                , parsedUserInterface.repairShopWindow
                    |> fieldFromMaybeInstance
                        { fieldName = "repairShopWindow"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromRepairShopWindow viewConfig
                        }
                , parsedUserInterface.characterSheetWindow
                    |> fieldFromMaybeInstance
                        { fieldName = "characterSheetWindow"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromCharacterSheetWindow viewConfig
                        }
                , parsedUserInterface.fleetWindow
                    |> fieldFromMaybeInstance
                        { fieldName = "fleetWindow"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromFleetWindow viewConfig
                        }
                , parsedUserInterface.watchListPanel
                    |> fieldFromMaybeInstance
                        { fieldName = "watchListPanel"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromWatchListPanel viewConfig
                        }
                , parsedUserInterface.standaloneBookmarkWindow
                    |> fieldFromMaybeInstance
                        { fieldName = "standaloneBookmarkWindow"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromStandaloneBookmarkWindow viewConfig
                        }
                , parsedUserInterface.moduleButtonTooltip
                    |> fieldFromMaybeInstance
                        { fieldName = "moduleButtonTooltip"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromModuleButtonTooltip viewConfig
                        }
                , parsedUserInterface.heatStatusTooltip
                    |> fieldFromMaybeInstance
                        { fieldName = "heatStatusTooltip"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromHeatStatusTooltip viewConfig
                        }
                , parsedUserInterface.neocom
                    |> fieldFromMaybeInstance
                        { fieldName = "neocom"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromNeocom viewConfig
                        }
                , parsedUserInterface.messageBoxes
                    |> fieldFromListInstance
                        { fieldName = "messageBoxes"
                        , fieldValueChildren = treeNodeChildrenFromMessageBox viewConfig
                        }
                , parsedUserInterface.layerAbovemain
                    |> fieldFromMaybeInstance
                        { fieldName = "layerAbovemain"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromLayerAbovemain viewConfig
                        }
                , parsedUserInterface.keyActivationWindow
                    |> fieldFromMaybeInstance
                        { fieldName = "keyActivationWindow"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromKeyActivationWindow viewConfig
                        }
                , parsedUserInterface.compressionWindow
                    |> fieldFromMaybeInstance
                        { fieldName = "compressionWindow"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromCompressionWindow viewConfig
                        }
                ]
    in
    { selfHtml = commonSummaryHtml
    , children = ExpandableChildren (NamedNode "temp-wrapping") (always children)
    }


treeNodeChildrenFromParsedUserInterfaceContextMenu :
    ViewConfig event
    -> EveOnline.ParseUserInterface.ContextMenu
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromParsedUserInterfaceContextMenu viewConfig parsedUserInterfaceContextMenu =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        parsedUserInterfaceContextMenu.uiNode
        [ parsedUserInterfaceContextMenu.entries
            |> fieldFromListInstance
                { fieldName = "entries"
                , fieldValueChildren =
                    treeNodeChildrenFromParsedUserInterfaceContextMenuEntry viewConfig
                }
        ]


treeNodeChildrenFromParsedUserInterfaceContextMenuEntry :
    ViewConfig event
    -> EveOnline.ParseUserInterface.ContextMenuEntry
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromParsedUserInterfaceContextMenuEntry viewConfig parsedUserInterfaceContextMenuEntry =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        parsedUserInterfaceContextMenuEntry.uiNode
        [ { fieldName = "text"
          , fieldValueSummary = parsedUserInterfaceContextMenuEntry.text |> Json.Encode.string |> Json.Encode.encode 0
          , fieldValueChildren = always []
          }
        ]


treeNodeChildrenFromParsedUserInterfaceShipUI :
    ViewConfig event
    -> EveOnline.ParseUserInterface.ShipUI
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromParsedUserInterfaceShipUI viewConfig parsedUserInterfaceShipUI =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        parsedUserInterfaceShipUI.uiNode
        [ { fieldName = "capacitor"
          , fieldValueSummary = "..."
          , fieldValueChildren = always (parsedUserInterfaceShipUI.capacitor |> treeNodeChildrenFromParsedUserInterfaceShipUICapacitor viewConfig)
          }
        , { fieldName = "hitpointsPercent"
          , fieldValueSummary = "..."
          , fieldValueChildren =
                always (treeNodeChildrenFromHitpoints parsedUserInterfaceShipUI.hitpointsPercent)
          }
        , parsedUserInterfaceShipUI.moduleButtons
            |> fieldFromListInstance
                { fieldName = "moduleButtons"
                , fieldValueChildren =
                    treeNodeChildrenFromShipUIModuleButton viewConfig
                }
        , { fieldName = "moduleButtonsRows"
          , fieldValueSummary = "..."
          , fieldValueChildren =
                always (parsedUserInterfaceShipUI.moduleButtonsRows |> treeNodeChildrenFromShipUIModuleButtonsRows viewConfig)
          }
        , parsedUserInterfaceShipUI.offensiveBuffButtonNames
            |> fieldFromPrimitiveListInstance
                { fieldName = "offensiveBuffButtonNames"
                , fieldValueDescription = Json.Encode.string >> Json.Encode.encode 0
                }
        , parsedUserInterfaceShipUI.squadronsUI
            |> fieldFromMaybeInstance
                { fieldName = "squadronsUI"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeNodeChildrenFromShipUISquadronsUI viewConfig
                }
        , parsedUserInterfaceShipUI.stopButton
            |> fieldFromMaybeInstance
                { fieldName = "stopButton"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        , parsedUserInterfaceShipUI.maxSpeedButton
            |> fieldFromMaybeInstance
                { fieldName = "maxSpeedButton"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        , parsedUserInterfaceShipUI.heatGauges
            |> fieldFromMaybeInstance
                { fieldName = "heatGauges"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeNodeChildrenFromShipUIHeatGauges viewConfig
                }
        ]


treeNodeChildrenFromShipUIModuleButtonsRows :
    ViewConfig event
    ->
        { top : List EveOnline.ParseUserInterface.ShipUIModuleButton
        , middle : List EveOnline.ParseUserInterface.ShipUIModuleButton
        , bottom : List EveOnline.ParseUserInterface.ShipUIModuleButton
        }
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromShipUIModuleButtonsRows viewConfig shipUIModulesRows =
    treeNodeChildrenFromRecord
        [ shipUIModulesRows.top
            |> fieldFromListInstance
                { fieldName = "top"
                , fieldValueChildren =
                    treeNodeChildrenFromShipUIModuleButton viewConfig
                }
        , shipUIModulesRows.middle
            |> fieldFromListInstance
                { fieldName = "middle"
                , fieldValueChildren =
                    treeNodeChildrenFromShipUIModuleButton viewConfig
                }
        , shipUIModulesRows.bottom
            |> fieldFromListInstance
                { fieldName = "bottom"
                , fieldValueChildren =
                    treeNodeChildrenFromShipUIModuleButton viewConfig
                }
        ]


treeNodeChildrenFromShipUIModuleButton :
    ViewConfig event
    -> EveOnline.ParseUserInterface.ShipUIModuleButton
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromShipUIModuleButton viewConfig shipUIModuleButton =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        shipUIModuleButton.uiNode
        [ { fieldName = "slotUINode"
          , fieldValueSummary = "..."
          , fieldValueChildren =
                always [ treeViewNodeFromUINode viewConfig shipUIModuleButton.slotUINode ]
          }
        , shipUIModuleButton.isActive |> fieldFromMaybeBool "isActive"
        , shipUIModuleButton.isHiliteVisible |> fieldFromBool "isHiliteVisible"
        , shipUIModuleButton.isBusy |> fieldFromBool "isBusy"
        , shipUIModuleButton.rampRotationMilli |> fieldFromMaybeInt "rampRotationMilli"
        ]


treeNodeChildrenFromHitpoints :
    EveOnline.ParseUserInterface.Hitpoints
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromHitpoints hitpoints =
    treeNodeChildrenFromRecord
        [ { fieldName = "structure"
          , fieldValueSummary = String.fromInt hitpoints.structure
          , fieldValueChildren = always []
          }
        , { fieldName = "armor"
          , fieldValueSummary = String.fromInt hitpoints.armor
          , fieldValueChildren = always []
          }
        , { fieldName = "shield"
          , fieldValueSummary = String.fromInt hitpoints.shield
          , fieldValueChildren = always []
          }
        ]


treeNodeChildrenFromParsedUserInterfaceShipUICapacitor :
    ViewConfig event
    -> EveOnline.ParseUserInterface.ShipUICapacitor
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromParsedUserInterfaceShipUICapacitor viewConfig parsedUserInterfaceShipUICapacitor =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        parsedUserInterfaceShipUICapacitor.uiNode
        [ parsedUserInterfaceShipUICapacitor.levelFromPmarksPercent |> fieldFromMaybeInt "levelFromPmarksPercent"
        ]


treeNodeChildrenFromShipUIHeatGauges :
    ViewConfig event
    -> EveOnline.ParseUserInterface.ShipUIHeatGauges
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromShipUIHeatGauges viewConfig heatGauges =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        heatGauges.uiNode
        [ heatGauges.gauges
            |> fieldFromListInstance
                { fieldName = "gauges"
                , fieldValueChildren = treeNodeChildrenFromShipUIHeatGauge viewConfig
                }
        ]


treeNodeChildrenFromShipUIHeatGauge :
    ViewConfig event
    -> EveOnline.ParseUserInterface.ShipUIHeatGauge
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromShipUIHeatGauge viewConfig heatGauge =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        heatGauge.uiNode
        [ heatGauge.rotationPercent |> fieldFromMaybeInt "rotationPercent"
        , heatGauge.heatPercent |> fieldFromMaybeInt "heatPercent"
        ]


treeNodeChildrenFromShipUISquadronsUI :
    ViewConfig event
    -> EveOnline.ParseUserInterface.SquadronsUI
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromShipUISquadronsUI viewConfig squadronsUI =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        squadronsUI.uiNode
        [ squadronsUI.squadrons
            |> fieldFromListInstance
                { fieldName = "squadrons"
                , fieldValueChildren =
                    treeNodeChildrenFromShipUISquadronUI viewConfig
                }
        ]


treeNodeChildrenFromShipUISquadronUI :
    ViewConfig event
    -> EveOnline.ParseUserInterface.SquadronUI
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromShipUISquadronUI viewConfig squadronUI =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        squadronUI.uiNode
        [ squadronUI.abilities
            |> fieldFromListInstance
                { fieldName = "abilities"
                , fieldValueChildren = treeNodeChildrenFromShipUISquadronAbilityIcon viewConfig
                }
        , squadronUI.actionLabel
            |> fieldFromMaybeInstance
                { fieldName = "actionLabel"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        ]


treeNodeChildrenFromShipUISquadronAbilityIcon :
    ViewConfig event
    -> EveOnline.ParseUserInterface.SquadronAbilityIcon
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromShipUISquadronAbilityIcon viewConfig abilityIcon =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        abilityIcon.uiNode
        [ abilityIcon.quantity |> fieldFromMaybeInt "quantity"
        , abilityIcon.ramp_active |> fieldFromMaybeBool "ramp_active"
        ]


treeNodeChildrenFromParsedUserInterfaceTarget :
    ViewConfig event
    -> EveOnline.ParseUserInterface.Target
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromParsedUserInterfaceTarget viewConfig parsedUserInterfaceTarget =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        parsedUserInterfaceTarget.uiNode
        [ parsedUserInterfaceTarget.barAndImageCont
            |> fieldFromMaybeInstance
                { fieldName = "barAndImageCont"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        , parsedUserInterfaceTarget.textsTopToBottom
            |> fieldFromPrimitiveListInstance
                { fieldName = "textsTopToBottom"
                , fieldValueDescription = Json.Encode.string >> Json.Encode.encode 0
                }
        , { fieldName = "isActiveTarget"
          , fieldValueSummary =
                if parsedUserInterfaceTarget.isActiveTarget then
                    "True"

                else
                    "False"
          , fieldValueChildren = always []
          }
        , parsedUserInterfaceTarget.assignedContainerNode
            |> fieldFromMaybeInstance
                { fieldName = "assignedContainerNode"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        , parsedUserInterfaceTarget.assignedIcons
            |> fieldFromListInstance
                { fieldName = "assignedIcons"
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        ]


treeNodeChildrenFromInfoPanelContainer :
    ViewConfig event
    -> EveOnline.ParseUserInterface.InfoPanelContainer
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromInfoPanelContainer viewConfig infoPanelContainer =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        infoPanelContainer.uiNode
        [ infoPanelContainer.icons
            |> fieldFromMaybeInstance
                { fieldName = "icons"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeNodeChildrenFromInfoPanelIcons viewConfig
                }
        , infoPanelContainer.infoPanelLocationInfo
            |> fieldFromMaybeInstance
                { fieldName = "infoPanelLocationInfo"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeNodeChildrenFromInfoPanelLocationInfo viewConfig
                }
        , infoPanelContainer.infoPanelRoute
            |> fieldFromMaybeInstance
                { fieldName = "infoPanelRoute"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeNodeChildrenFromInfoPanelRoute viewConfig
                }
        , infoPanelContainer.infoPanelAgentMissions
            |> fieldFromMaybeInstance
                { fieldName = "infoPanelAgentMissions"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeNodeChildrenFromInfoPanelAgentMissions viewConfig
                }
        ]


treeNodeChildrenFromInfoPanelIcons :
    ViewConfig event
    -> EveOnline.ParseUserInterface.InfoPanelIcons
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromInfoPanelIcons viewConfig infoPanelIcons =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        infoPanelIcons.uiNode
        [ infoPanelIcons.search
            |> fieldFromMaybeInstance
                { fieldName = "search"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        , infoPanelIcons.locationInfo
            |> fieldFromMaybeInstance
                { fieldName = "locationInfo"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        , infoPanelIcons.route
            |> fieldFromMaybeInstance
                { fieldName = "route"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        , infoPanelIcons.agentMissions
            |> fieldFromMaybeInstance
                { fieldName = "agentMissions"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        , infoPanelIcons.dailyChallenge
            |> fieldFromMaybeInstance
                { fieldName = "dailyChallenge"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        ]


treeNodeChildrenFromInfoPanelLocationInfo :
    ViewConfig event
    -> EveOnline.ParseUserInterface.InfoPanelLocationInfo
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromInfoPanelLocationInfo viewConfig infoPanelLocationInfo =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        infoPanelLocationInfo.uiNode
        [ { fieldName = "listSurroundingsButton"
          , fieldValueSummary = "..."
          , fieldValueChildren = always [ treeViewNodeFromUINode viewConfig infoPanelLocationInfo.listSurroundingsButton ]
          }
        , infoPanelLocationInfo.currentSolarSystemName |> fieldFromMaybeString "currentSolarSystemName"
        , infoPanelLocationInfo.securityStatusPercent |> fieldFromMaybeInt "securityStatusPercent"
        , infoPanelLocationInfo.expandedContent
            |> fieldFromMaybeInstance
                { fieldName = "expandedContent"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeNodeChildrenFromInfoPanelLocationInfoExpandedContent
                }
        ]


treeNodeChildrenFromInfoPanelLocationInfoExpandedContent :
    EveOnline.ParseUserInterface.InfoPanelLocationInfoExpandedContent
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromInfoPanelLocationInfoExpandedContent expandedContent =
    treeNodeChildrenFromRecord
        [ expandedContent.currentStationName |> fieldFromMaybeString "currentStationName"
        ]


treeNodeChildrenFromInfoPanelRoute :
    ViewConfig event
    -> EveOnline.ParseUserInterface.InfoPanelRoute
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromInfoPanelRoute viewConfig infoPanelRoute =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        infoPanelRoute.uiNode
        [ infoPanelRoute.routeElementMarker
            |> fieldFromListInstance
                { fieldName = "routeElementMarker"
                , fieldValueChildren = .uiNode >> treeViewNodeFromUINode viewConfig >> List.singleton
                }
        ]


treeNodeChildrenFromInfoPanelAgentMissions :
    ViewConfig event
    -> EveOnline.ParseUserInterface.InfoPanelAgentMissions
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromInfoPanelAgentMissions viewConfig infoPanelAgentMissions =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        infoPanelAgentMissions.uiNode
        [ infoPanelAgentMissions.entries
            |> fieldFromListInstance
                { fieldName = "entries"
                , fieldValueChildren = .uiNode >> treeViewNodeFromUINode viewConfig >> List.singleton
                }
        ]


treeNodeChildrenFromOverviewWindow :
    ViewConfig event
    -> EveOnline.ParseUserInterface.OverviewWindow
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromOverviewWindow viewConfig overviewWindow =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        overviewWindow.uiNode
        [ overviewWindow.entries
            |> fieldFromListInstance
                { fieldName = "entries"
                , fieldValueChildren = treeNodeChildrenFromOverviewWindowEntry viewConfig
                }
        , overviewWindow.scrollControls
            |> fieldFromMaybeInstance
                { fieldName = "scrollControls"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeNodeChildrenFromScrollControls viewConfig
                }
        ]


treeNodeChildrenFromOverviewWindowEntry :
    ViewConfig event
    -> EveOnline.ParseUserInterface.OverviewWindowEntry
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromOverviewWindowEntry viewConfig overviewWindowEntry =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        overviewWindowEntry.uiNode
        [ overviewWindowEntry.namesUnderSpaceObjectIcon
            |> Set.toList
            |> fieldFromPrimitiveListInstance
                { fieldName = "namesUnderSpaceObjectIcon"
                , fieldValueDescription = Json.Encode.string >> Json.Encode.encode 0
                }
        , overviewWindowEntry.cellsTexts
            |> fieldFromPrimitiveStringDictInstance
                { fieldName = "cellsTexts"
                , fieldValueDescription = Json.Encode.string >> Json.Encode.encode 0
                }
        , overviewWindowEntry.objectDistance |> fieldFromMaybeString "objectDistance"
        , overviewWindowEntry.objectDistanceInMeters
            |> fieldFromResultPrimitive
                { fieldName = "objectDistanceInMeters", errValueSummary = Json.Encode.string, okValueSummary = Json.Encode.int }
        , overviewWindowEntry.objectName |> fieldFromMaybeString "objectName"
        , overviewWindowEntry.objectType |> fieldFromMaybeString "objectType"
        , overviewWindowEntry.objectAlliance |> fieldFromMaybeString "objectAlliance"
        , overviewWindowEntry.iconSpriteColorPercent
            |> fieldFromMaybeInstance
                { fieldName = "iconSpriteColorPercent"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeNodeChildrenFromColorComponents
                }
        , overviewWindowEntry.rightAlignedIconsHints
            |> fieldFromPrimitiveListInstance
                { fieldName = "rightAlignedIconsHints"
                , fieldValueDescription = Json.Encode.string >> Json.Encode.encode 0
                }
        , { fieldName = "commonIndications"
          , fieldValueSummary = "..."
          , fieldValueChildren = always (treeNodeChildrenFromOverviewWindowEntryCommonIndications overviewWindowEntry.commonIndications)
          }
        , overviewWindowEntry.opacityPercent |> fieldFromMaybeInt "opacityPercent"
        ]


treeNodeChildrenFromOverviewWindowEntryCommonIndications :
    EveOnline.ParseUserInterface.OverviewWindowEntryCommonIndications
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromOverviewWindowEntryCommonIndications indications =
    treeNodeChildrenFromRecord
        [ indications.targeting |> fieldFromBool "targeting"
        , indications.targetedByMe |> fieldFromBool "targetedByMe"
        , indications.isJammingMe |> fieldFromBool "isJammingMe"
        , indications.isWarpDisruptingMe |> fieldFromBool "isWarpDisruptingMe"
        ]


treeNodeChildrenFromDronesWindow :
    ViewConfig event
    -> EveOnline.ParseUserInterface.DronesWindow
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromDronesWindow viewConfig dronesWindow =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        dronesWindow.uiNode
        [ dronesWindow.droneGroups
            |> fieldFromListInstance
                { fieldName = "droneGroups"
                , fieldValueChildren = treeNodeChildrenFromDronesWindowDroneGroup viewConfig
                }
        , dronesWindow.droneGroupInBay
            |> fieldFromMaybeInstance
                { fieldName = "droneGroupInBay"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeNodeChildrenFromDronesWindowDroneGroup viewConfig
                }
        , dronesWindow.droneGroupInSpace
            |> fieldFromMaybeInstance
                { fieldName = "droneGroupInSpace"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeNodeChildrenFromDronesWindowDroneGroup viewConfig
                }
        ]


treeNodeChildrenFromDronesWindowDroneGroup :
    ViewConfig event
    -> EveOnline.ParseUserInterface.DronesWindowEntryGroupStructure
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromDronesWindowDroneGroup viewConfig dronesWindowDroneGroup =
    treeNodeChildrenFromRecord
        [ { fieldName = "header"
          , fieldValueSummary = "..."
          , fieldValueChildren = always (treeNodeChildrenFromDronesWindowDroneGroupHeader viewConfig dronesWindowDroneGroup.header)
          }
        , dronesWindowDroneGroup.children
            |> fieldFromListInstance
                { fieldName = "children"
                , fieldValueChildren = treeNodeChildrenFromDronesWindowDroneGroupEntry viewConfig
                }
        ]


treeNodeChildrenFromDronesWindowDroneGroupEntry :
    ViewConfig event
    -> EveOnline.ParseUserInterface.DronesWindowEntry
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromDronesWindowDroneGroupEntry viewConfig droneGroupEntry =
    let
        continueWithTagName tagName items =
            treeNodeChildrenFromRecord
                [ { fieldName = tagName
                  , fieldValueSummary = ""
                  , fieldValueChildren = always items
                  }
                ]
    in
    case droneGroupEntry of
        EveOnline.ParseUserInterface.DronesWindowEntryDrone drone ->
            continueWithTagName "Drone" (treeNodeChildrenFromDronesWindowEntryDrone viewConfig drone)

        EveOnline.ParseUserInterface.DronesWindowEntryGroup group ->
            continueWithTagName "Group" (treeNodeChildrenFromDronesWindowDroneGroup viewConfig group)


treeNodeChildrenFromDronesWindowEntryDrone :
    ViewConfig event
    -> EveOnline.ParseUserInterface.DronesWindowEntryDroneStructure
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromDronesWindowEntryDrone viewConfig dronesWindowEntry =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        dronesWindowEntry.uiNode
        [ dronesWindowEntry.mainText |> fieldFromMaybeString "mainText"
        , dronesWindowEntry.hitpointsPercent
            |> fieldFromMaybeInstance
                { fieldName = "hitpointsPercent"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeNodeChildrenFromHitpoints
                }
        ]


treeNodeChildrenFromDronesWindowDroneGroupHeader :
    ViewConfig event
    -> EveOnline.ParseUserInterface.DronesWindowDroneGroupHeader
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromDronesWindowDroneGroupHeader viewConfig dronesWindowDroneGroupHeader =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        dronesWindowDroneGroupHeader.uiNode
        [ dronesWindowDroneGroupHeader.mainText |> fieldFromMaybeString "mainText"
        , dronesWindowDroneGroupHeader.quantityFromTitle
            |> fieldFromMaybeInstance
                { fieldName = "quantityFromTitle"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeNodeChildrenFromDronesWindowDroneGroupHeaderQuantity
                }
        ]


treeNodeChildrenFromDronesWindowDroneGroupHeaderQuantity :
    EveOnline.ParseUserInterface.DronesWindowDroneGroupHeaderQuantity
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromDronesWindowDroneGroupHeaderQuantity dronesWindowDroneGroupHeaderQuantity =
    treeNodeChildrenFromRecord
        [ dronesWindowDroneGroupHeaderQuantity.current |> fieldFromInt "current"
        , dronesWindowDroneGroupHeaderQuantity.maximum |> fieldFromMaybeInt "maximum"
        ]


treeNodeChildrenFromProbeScannerWindow :
    ViewConfig event
    -> EveOnline.ParseUserInterface.ProbeScannerWindow
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromProbeScannerWindow viewConfig probeScannerWindow =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        probeScannerWindow.uiNode
        [ probeScannerWindow.scanResults
            |> fieldFromListInstance
                { fieldName = "scanResults"
                , fieldValueChildren = treeNodeChildrenFromProbeScanResult viewConfig
                }
        ]


treeNodeChildrenFromProbeScanResult :
    ViewConfig event
    -> EveOnline.ParseUserInterface.ProbeScanResult
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromProbeScanResult viewConfig probeScanResult =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        probeScanResult.uiNode
        [ probeScanResult.textsLeftToRight
            |> fieldFromPrimitiveListInstance
                { fieldName = "textsLeftToRight"
                , fieldValueDescription = Json.Encode.string >> Json.Encode.encode 0
                }
        , probeScanResult.cellsTexts
            |> fieldFromPrimitiveStringDictInstance
                { fieldName = "cellsTexts"
                , fieldValueDescription = Json.Encode.string >> Json.Encode.encode 0
                }
        ]


treeNodeChildrenFromDirectionalScannerWindow :
    ViewConfig event
    -> EveOnline.ParseUserInterface.DirectionalScannerWindow
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromDirectionalScannerWindow viewConfig directionalScannerWindow =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        directionalScannerWindow.uiNode
        [ directionalScannerWindow.scrollNode
            |> fieldFromMaybeInstance
                { fieldName = "scrollNode"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        , directionalScannerWindow.scanResults
            |> fieldFromListInstance
                { fieldName = "scanResults"
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        ]


treeNodeChildrenFromStationWindow :
    ViewConfig event
    -> EveOnline.ParseUserInterface.StationWindow
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromStationWindow viewConfig stationWindow =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        stationWindow.uiNode
        [ stationWindow.undockButton
            |> fieldFromMaybeInstance
                { fieldName = "undockButton"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        , stationWindow.abortUndockButton
            |> fieldFromMaybeInstance
                { fieldName = "abortUndockButton"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        ]


treeNodeChildrenFromParsedUserInterfaceInventoryWindow :
    ViewConfig event
    -> EveOnline.ParseUserInterface.InventoryWindow
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromParsedUserInterfaceInventoryWindow viewConfig inventoryWindow =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        inventoryWindow.uiNode
        [ inventoryWindow.subCaptionLabelText |> fieldFromMaybeString "subCaptionLabelText"
        , inventoryWindow.leftTreeEntries
            |> fieldFromListInstance
                { fieldName = "leftTreeEntries "
                , fieldValueChildren =
                    treeNodeChildrenFromInventoryWindowLeftTreeEntry viewConfig
                }
        , inventoryWindow.selectedContainerCapacityGauge
            |> Maybe.andThen Result.toMaybe
            |> fieldFromMaybeInstance
                { fieldName = "selectedContainerCapacityGauge"
                , fieldValueSummary = always "..."
                , fieldValueChildren =
                    treeNodeChildrenFromParsedUserInterfaceInventoryCapacityGauge
                }
        , inventoryWindow.selectedContainerInventory
            |> fieldFromMaybeInstance
                { fieldName = "selectedContainerInventory"
                , fieldValueSummary = always "..."
                , fieldValueChildren =
                    treeNodeChildrenFromParsedUserInterfaceInventory viewConfig
                }
        , inventoryWindow.buttonToStackAll
            |> fieldFromMaybeInstance
                { fieldName = "buttonToStackAll"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        , inventoryWindow.buttonToSwitchToListView
            |> fieldFromMaybeInstance
                { fieldName = "buttonToSwitchToListView"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        ]


treeNodeChildrenFromInventoryWindowLeftTreeEntry :
    ViewConfig event
    -> EveOnline.ParseUserInterface.InventoryWindowLeftTreeEntry
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromInventoryWindowLeftTreeEntry viewConfig inventoryWindowLeftTreeEntry =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        inventoryWindowLeftTreeEntry.uiNode
        [ inventoryWindowLeftTreeEntry.toggleBtn
            |> fieldFromMaybeInstance
                { fieldName = "toggleBtn"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        , inventoryWindowLeftTreeEntry.selectRegion
            |> fieldFromMaybeInstance
                { fieldName = "selectRegion"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        , inventoryWindowLeftTreeEntry.text |> fieldFromString "text"
        , inventoryWindowLeftTreeEntry.children
            |> List.map EveOnline.ParseUserInterface.unwrapInventoryWindowLeftTreeEntryChild
            |> fieldFromListInstance
                { fieldName = "children"
                , fieldValueChildren =
                    treeNodeChildrenFromInventoryWindowLeftTreeEntry viewConfig
                }
        ]


treeNodeChildrenFromParsedUserInterfaceInventory :
    ViewConfig event
    -> EveOnline.ParseUserInterface.Inventory
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromParsedUserInterfaceInventory viewConfig parsedUserInterfaceInventory =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        parsedUserInterfaceInventory.uiNode
        [ parsedUserInterfaceInventory.itemsView
            |> fieldFromMaybeInstance
                { fieldName = "itemsView"
                , fieldValueSummary = always "..."
                , fieldValueChildren =
                    treeNodeChildrenFromParsedUserInterfaceInventoryItemsView viewConfig
                }
        , parsedUserInterfaceInventory.scrollControls
            |> fieldFromMaybeInstance
                { fieldName = "scrollControls"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeNodeChildrenFromScrollControls viewConfig
                }
        ]


treeNodeChildrenFromParsedUserInterfaceInventoryCapacityGauge :
    EveOnline.ParseUserInterface.InventoryWindowCapacityGauge
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromParsedUserInterfaceInventoryCapacityGauge parsedUserInterfaceInventoryCapacityGauge =
    treeNodeChildrenFromRecord
        [ { fieldName = "used"
          , fieldValueSummary = String.fromInt parsedUserInterfaceInventoryCapacityGauge.used
          , fieldValueChildren = always []
          }
        , parsedUserInterfaceInventoryCapacityGauge.maximum |> fieldFromMaybeInt "maximum"
        , parsedUserInterfaceInventoryCapacityGauge.selected |> fieldFromMaybeInt "selected"
        ]


treeNodeChildrenFromParsedUserInterfaceInventoryItemsView :
    ViewConfig event
    -> EveOnline.ParseUserInterface.InventoryItemsView
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromParsedUserInterfaceInventoryItemsView viewConfig parsedUserInterfaceInventoryItemsView =
    let
        continueWithTagName tagName items itemView =
            treeNodeChildrenFromRecord
                [ { fieldName = tagName
                  , fieldValueSummary = ""
                  , fieldValueChildren =
                        always
                            (treeNodeChildrenFromRecord
                                [ items
                                    |> fieldFromListInstance
                                        { fieldName = "items"
                                        , fieldValueChildren = itemView
                                        }
                                ]
                            )
                  }
                ]
    in
    case parsedUserInterfaceInventoryItemsView of
        EveOnline.ParseUserInterface.InventoryItemsListView { items } ->
            continueWithTagName "InventoryItemsListView"
                items
                (treeNodeChildrenFromParsedUserInterfaceInventoryItemsListViewEntry viewConfig)

        EveOnline.ParseUserInterface.InventoryItemsNotListView { items } ->
            continueWithTagName "InventoryItemsNotListView"
                items
                (treeViewNodeFromUINode viewConfig >> List.singleton)


treeNodeChildrenFromParsedUserInterfaceInventoryItemsListViewEntry :
    ViewConfig event
    -> EveOnline.ParseUserInterface.InventoryItemsListViewEntry
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromParsedUserInterfaceInventoryItemsListViewEntry viewConfig inventoryEntry =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        inventoryEntry.uiNode
        [ inventoryEntry.cellsTexts
            |> fieldFromPrimitiveStringDictInstance
                { fieldName = "cellsTexts"
                , fieldValueDescription = Json.Encode.string >> Json.Encode.encode 0
                }
        ]


treeNodeChildrenFromChatWindowStack :
    ViewConfig event
    -> EveOnline.ParseUserInterface.ChatWindowStack
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromChatWindowStack viewConfig chatWindowStack =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        chatWindowStack.uiNode
        [ chatWindowStack.chatWindow
            |> fieldFromMaybeInstance
                { fieldName = "chatWindow"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeNodeChildrenFromChatWindow viewConfig
                }
        ]


treeNodeChildrenFromChatWindow :
    ViewConfig event
    -> EveOnline.ParseUserInterface.ChatWindow
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromChatWindow viewConfig chatWindow =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        chatWindow.uiNode
        [ chatWindow.name |> fieldFromMaybeString "name"
        , chatWindow.userlist
            |> fieldFromMaybeInstance
                { fieldName = "userlist"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeNodeChildrenFromChatWindowUserlist viewConfig
                }
        ]


treeNodeChildrenFromChatWindowUserlist :
    ViewConfig event
    -> EveOnline.ParseUserInterface.ChatWindowUserlist
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromChatWindowUserlist viewConfig userlist =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        userlist.uiNode
        [ userlist.visibleUsers
            |> fieldFromListInstance
                { fieldName = "visibleUsers "
                , fieldValueChildren = treeNodeChildrenFromChatUserEntry viewConfig
                }
        , userlist.scrollControls
            |> fieldFromMaybeInstance
                { fieldName = "scrollControls"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeNodeChildrenFromScrollControls viewConfig
                }
        ]


treeNodeChildrenFromChatUserEntry :
    ViewConfig event
    -> EveOnline.ParseUserInterface.ChatUserEntry
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromChatUserEntry viewConfig chatUserEntry =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        chatUserEntry.uiNode
        [ chatUserEntry.name |> fieldFromMaybeString "name"
        , chatUserEntry.standingIconHint |> fieldFromMaybeString "standingIconHint"
        ]


treeNodeChildrenFromScrollControls :
    ViewConfig event
    -> EveOnline.ParseUserInterface.ScrollControls
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromScrollControls viewConfig scrollControls =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        scrollControls.uiNode
        [ scrollControls.scrollHandle
            |> fieldFromMaybeInstance
                { fieldName = "scrollHandle"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        ]


treeNodeChildrenFromModuleButtonTooltip :
    ViewConfig event
    -> EveOnline.ParseUserInterface.ModuleButtonTooltip
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromModuleButtonTooltip viewConfig moduleButtonTooltip =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        moduleButtonTooltip.uiNode
        [ moduleButtonTooltip.optimalRange
            |> fieldFromMaybeInstance
                { fieldName = "optimalRange"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeNodeChildrenFromOptimalRange
                }
        ]


treeNodeChildrenFromOptimalRange :
    { asString : String, inMeters : Result String Int }
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromOptimalRange optimalRange =
    treeNodeChildrenFromRecord
        [ { fieldName = "asString"
          , fieldValueSummary = optimalRange.asString
          , fieldValueChildren = always []
          }
        , optimalRange.inMeters
            |> fieldFromResultPrimitive
                { fieldName = "inMeters", errValueSummary = Json.Encode.string, okValueSummary = Json.Encode.int }
        ]


treeNodeChildrenFromHeatStatusTooltip :
    ViewConfig event
    -> EveOnline.ParseUserInterface.HeatStatusTooltip
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromHeatStatusTooltip viewConfig heatStatusTooltip =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        heatStatusTooltip.uiNode
        [ heatStatusTooltip.lowPercent |> fieldFromMaybeInt "lowPercent"
        , heatStatusTooltip.mediumPercent |> fieldFromMaybeInt "mediumPercent"
        , heatStatusTooltip.highPercent |> fieldFromMaybeInt "highPercent"
        ]


treeNodeChildrenFromAgentConversationWindow :
    ViewConfig event
    -> EveOnline.ParseUserInterface.AgentConversationWindow
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromAgentConversationWindow viewConfig agentConversationWindow =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        agentConversationWindow.uiNode
        []


treeNodeChildrenFromSelectedItemWindow :
    ViewConfig event
    -> EveOnline.ParseUserInterface.SelectedItemWindow
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromSelectedItemWindow viewConfig selectedItemWindow =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        selectedItemWindow.uiNode
        []


treeNodeChildrenFromMarketOrdersWindow :
    ViewConfig event
    -> EveOnline.ParseUserInterface.MarketOrdersWindow
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromMarketOrdersWindow viewConfig marketOrdersWindow =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        marketOrdersWindow.uiNode
        []


treeNodeChildrenFromSurveyScanWindow :
    ViewConfig event
    -> EveOnline.ParseUserInterface.SurveyScanWindow
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromSurveyScanWindow viewConfig surveyScanWindow =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        surveyScanWindow.uiNode
        [ surveyScanWindow.scanEntries
            |> fieldFromListInstance
                { fieldName = "scanEntries"
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        ]


treeNodeChildrenFromBookmarkLocationWindow :
    ViewConfig event
    -> EveOnline.ParseUserInterface.BookmarkLocationWindow
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromBookmarkLocationWindow viewConfig bookmarkLocationWindow =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        bookmarkLocationWindow.uiNode
        [ bookmarkLocationWindow.submitButton
            |> fieldFromMaybeInstance
                { fieldName = "submitButton"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        , bookmarkLocationWindow.cancelButton
            |> fieldFromMaybeInstance
                { fieldName = "cancelButton"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        ]


treeNodeChildrenFromRepairShopWindow :
    ViewConfig event
    -> EveOnline.ParseUserInterface.RepairShopWindow
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromRepairShopWindow viewConfig repairShopWindow =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        repairShopWindow.uiNode
        [ repairShopWindow.items
            |> fieldFromListInstance
                { fieldName = "items"
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        , repairShopWindow.buttonGroup
            |> fieldFromMaybeInstance
                { fieldName = "buttonGroup"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        , repairShopWindow.buttons
            |> fieldFromListInstance
                { fieldName = "buttons"
                , fieldValueChildren = treeNodeChildrenFromUINodeWithMainText viewConfig
                }
        ]


treeNodeChildrenFromCharacterSheetWindow :
    ViewConfig event
    -> EveOnline.ParseUserInterface.CharacterSheetWindow
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromCharacterSheetWindow viewConfig characterSheetWindow =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        characterSheetWindow.uiNode
        [ characterSheetWindow.skillGroups
            |> fieldFromListInstance
                { fieldName = "skillGroups"
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        ]


treeNodeChildrenFromFleetWindow :
    ViewConfig event
    -> EveOnline.ParseUserInterface.FleetWindow
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromFleetWindow viewConfig fleetWindow =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        fleetWindow.uiNode
        [ fleetWindow.fleetMembers
            |> fieldFromListInstance
                { fieldName = "fleetMembers"
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        ]


treeNodeChildrenFromWatchListPanel :
    ViewConfig event
    -> EveOnline.ParseUserInterface.WatchListPanel
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromWatchListPanel viewConfig watchListPanel =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        watchListPanel.uiNode
        [ watchListPanel.entries
            |> fieldFromListInstance
                { fieldName = "entries"
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        ]


treeNodeChildrenFromStandaloneBookmarkWindow :
    ViewConfig event
    -> EveOnline.ParseUserInterface.StandaloneBookmarkWindow
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromStandaloneBookmarkWindow viewConfig standaloneBookmarkWindow =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        standaloneBookmarkWindow.uiNode
        [ standaloneBookmarkWindow.entries
            |> fieldFromListInstance
                { fieldName = "entries"
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        ]


treeNodeChildrenFromNeocom :
    ViewConfig event
    -> EveOnline.ParseUserInterface.Neocom
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromNeocom viewConfig neocom =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        neocom.uiNode
        [ neocom.inventoryButton
            |> fieldFromMaybeInstance
                { fieldName = "inventoryButton"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        , neocom.clock
            |> fieldFromMaybeInstance
                { fieldName = "clock"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeNodeChildrenFromNeocomClock viewConfig
                }
        ]


treeNodeChildrenFromNeocomClock :
    ViewConfig event
    -> EveOnline.ParseUserInterface.NeocomClock
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromNeocomClock viewConfig neocomClock =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        neocomClock.uiNode
        [ neocomClock.text |> fieldFromString "text"
        ]


treeNodeChildrenFromMessageBox :
    ViewConfig event
    -> EveOnline.ParseUserInterface.MessageBox
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromMessageBox viewConfig messageBox =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        messageBox.uiNode
        [ messageBox.buttons
            |> fieldFromListInstance
                { fieldName = "buttons"
                , fieldValueChildren = treeNodeChildrenFromUINodeWithMainText viewConfig
                }
        , messageBox.buttonGroup
            |> fieldFromMaybeInstance
                { fieldName = "buttonGroup"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        ]


treeNodeChildrenFromLayerAbovemain :
    ViewConfig event
    -> EveOnline.ParseUserInterface.LayerAbovemain
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromLayerAbovemain viewConfig layerAbovemain =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        layerAbovemain.uiNode
        [ layerAbovemain.quickMessage
            |> fieldFromMaybeInstance
                { fieldName = "quickMessage"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeNodeChildrenFromQuickMessage viewConfig
                }
        ]


treeNodeChildrenFromQuickMessage :
    ViewConfig event
    -> EveOnline.ParseUserInterface.QuickMessage
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromQuickMessage viewConfig layerAboveMainUINode =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        layerAboveMainUINode.uiNode
        [ layerAboveMainUINode.text |> fieldFromString "text" ]


treeNodeChildrenFromKeyActivationWindow :
    ViewConfig event
    -> EveOnline.ParseUserInterface.KeyActivationWindow
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromKeyActivationWindow viewConfig keyActivationWindow =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        keyActivationWindow.uiNode
        [ keyActivationWindow.activateButton
            |> fieldFromMaybeInstance
                { fieldName = "activateButton"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        ]


treeNodeChildrenFromCompressionWindow :
    ViewConfig event
    -> EveOnline.ParseUserInterface.CompressionWindow
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromCompressionWindow viewConfig compressionWindow =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        compressionWindow.uiNode
        [ compressionWindow.windowControls
            |> fieldFromMaybeInstance
                { fieldName = "windowControls"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeNodeChildrenFromWindowControls viewConfig
                }
        , compressionWindow.compressButton
            |> fieldFromMaybeInstance
                { fieldName = "compressButton"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        ]


treeNodeChildrenFromWindowControls :
    ViewConfig event
    -> EveOnline.ParseUserInterface.WindowControls
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromWindowControls viewConfig windowControls =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        windowControls.uiNode
        [ windowControls.minimizeButton
            |> fieldFromMaybeInstance
                { fieldName = "minimizeButton"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        , windowControls.closeButton
            |> fieldFromMaybeInstance
                { fieldName = "closeButton"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
                }
        ]


treeNodeChildrenFromUINodeWithMainText :
    ViewConfig event
    -> { uiNode : UITreeNodeWithDisplayRegion, mainText : Maybe String }
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromUINodeWithMainText viewConfig node =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        node.uiNode
        [ node.mainText |> fieldFromMaybeString "mainText"
        ]


treeViewNodeFromUINode :
    ViewConfig event
    -> EveOnline.ParseUserInterface.UITreeNodeWithDisplayRegion
    -> TreeViewNode event ParsedUITreeViewPathNode
treeViewNodeFromUINode viewConfig parsedUserInterfaceUINode =
    treeViewNodeFromMemoryReadingUITreeNode viewConfig.inputRoute viewConfig.uiNodesWithDisplayRegion parsedUserInterfaceUINode.uiNode
        |> mapTreeViewNode UITreeNode identity


treeViewNodeFromMemoryReadingUITreeNode :
    Maybe (InputRoute event)
    -> Dict.Dict String UITreeNodeWithDisplayRegion
    -> EveOnline.MemoryReading.UITreeNode
    -> TreeViewNode event ExpandableViewNode
treeViewNodeFromMemoryReadingUITreeNode maybeInputRoute uiNodesWithDisplayRegion treeNode =
    let
        nodeIdentityInView =
            { pythonObjectAddress = treeNode.pythonObjectAddress }

        maybeNodeWithDisplayRegion =
            uiNodesWithDisplayRegion |> Dict.get treeNode.pythonObjectAddress

        inputHtml =
            maybeNodeWithDisplayRegion
                |> Maybe.map
                    (\nodeWithDisplayRegion ->
                        maybeInputOfferHtml maybeInputRoute [ MouseClickLeft, MouseClickRight ] nodeWithDisplayRegion
                    )
                |> Maybe.withDefault (Html.text "")

        commonSummaryHtml =
            [ uiNodeCommonSummaryText treeNode |> Html.text, inputHtml ] |> Html.span []

        getDisplayChildren _ =
            let
                totalDisplayRegionJson =
                    maybeNodeWithDisplayRegion
                        |> Maybe.map .totalDisplayRegion
                        |> Maybe.map displayRegionAsJson
                        |> Maybe.withDefault Json.Encode.null

                totalDisplayRegionVisibleJson =
                    maybeNodeWithDisplayRegion
                        |> Maybe.map .totalDisplayRegionVisible
                        |> Maybe.map displayRegionAsJson
                        |> Maybe.withDefault Json.Encode.null

                ( childrenNodeChildren, childrenNodeText ) =
                    case treeNode.children |> Maybe.withDefault [] of
                        [] ->
                            ( NoChildren, "No children" )

                        children ->
                            ( ExpandableChildren
                                ExpandableUITreeNodeChildren
                                (\() -> children |> List.map (EveOnline.MemoryReading.unwrapUITreeNodeChild >> treeViewNodeFromMemoryReadingUITreeNode maybeInputRoute uiNodesWithDisplayRegion))
                            , (children |> List.length |> String.fromInt) ++ " children"
                            )

                displayEntryChildren =
                    { selfHtml = childrenNodeText |> Html.text, children = childrenNodeChildren }

                allContainedDisplayTexts =
                    EveOnline.ParseUserInterface.getAllContainedDisplayTexts treeNode

                allContainedDisplayTextsChildren =
                    ExpandableChildren
                        ExpandableUITreeNodeAllDisplayTexts
                        (\() ->
                            allContainedDisplayTexts
                                |> List.map
                                    (\displayText ->
                                        { selfHtml = displayText |> Json.Encode.string |> Json.Encode.encode 0 |> Html.text
                                        , children = NoChildren
                                        }
                                    )
                        )

                displayEntryGetAllContainedDisplayTexts =
                    { selfHtml =
                        ("getAllContainedDisplayTexts = List (" ++ (allContainedDisplayTexts |> List.length |> String.fromInt) ++ ")") |> Html.text
                    , children = allContainedDisplayTextsChildren
                    }

                propertyDisplayChild ( propertyName, propertyValue ) =
                    { selfHtml = (propertyName ++ " = " ++ (propertyValue |> Json.Encode.encode 0)) |> Html.text
                    , children = NoChildren
                    }

                displayChildrenOtherProperties =
                    treeNode.dictEntriesOfInterest
                        |> Dict.toList
                        |> List.map propertyDisplayChild

                displayEntryOtherProperties =
                    { selfHtml = (treeNode.dictEntriesOfInterest |> Dict.size |> String.fromInt) ++ " dictEntriesOfInterest" |> Html.text
                    , children = ExpandableChildren ExpandableUITreeNodeDictEntries (always displayChildrenOtherProperties)
                    }

                properties =
                    [ ( "pythonObjectAddress", Json.Encode.string treeNode.pythonObjectAddress )
                    , ( "pythonObjectTypeName", Json.Encode.string treeNode.pythonObjectTypeName )
                    , ( "totalDisplayRegion", totalDisplayRegionJson )
                    , ( "totalDisplayRegionVisible", totalDisplayRegionVisibleJson )
                    ]

                allDisplayChildren =
                    (properties |> List.map propertyDisplayChild) ++ [ displayEntryOtherProperties, displayEntryChildren, displayEntryGetAllContainedDisplayTexts ]
            in
            allDisplayChildren
    in
    { selfHtml = commonSummaryHtml
    , children = ExpandableChildren (ExpandableUITreeNode nodeIdentityInView) getDisplayChildren
    }


displayRegionAsJson : DisplayRegion -> Json.Encode.Value
displayRegionAsJson displayRegion =
    [ ( "x", .x ), ( "y", .y ), ( "width", .width ), ( "height", .height ) ]
        |> List.map (Tuple.mapSecond (\regionProperty -> displayRegion |> regionProperty |> Json.Encode.int))
        |> Json.Encode.object


uiNodeCommonSummaryText : EveOnline.MemoryReading.UITreeNode -> String
uiNodeCommonSummaryText uiNode =
    let
        popularPropertiesDescription =
            uiNode.pythonObjectTypeName
                :: ([ "_name" ]
                        |> List.filterMap
                            (\popularProperty -> uiNode.dictEntriesOfInterest |> Dict.get popularProperty)
                        |> List.map (Json.Encode.encode 0)
                   )
                |> List.map (String.Extra.ellipsis 25)

        commonSummaryText =
            ((EveOnline.MemoryReading.countDescendantsInUITreeNode uiNode |> String.fromInt)
                ++ " descendants"
            )
                :: popularPropertiesDescription
                |> String.join ", "
    in
    commonSummaryText


mapTreeViewNode : (expandableIdA -> expandableIdB) -> (eventA -> eventB) -> TreeViewNode eventA expandableIdA -> TreeViewNode eventB expandableIdB
mapTreeViewNode mapNodeId mapEvent originalTreeViewNode =
    let
        children =
            case originalTreeViewNode.children of
                NoChildren ->
                    NoChildren

                ExpandableChildren expandableId getChildren ->
                    ExpandableChildren
                        (mapNodeId expandableId)
                        (always (getChildren () |> List.map (mapTreeViewNode mapNodeId mapEvent)))
    in
    { selfHtml = originalTreeViewNode.selfHtml |> Html.map mapEvent
    , children = children
    }


treeViewNodeForField :
    { fieldName : String }
    -> String
    -> (() -> List (TreeViewNode event ParsedUITreeViewPathNode))
    -> TreeViewNode event ParsedUITreeViewPathNode
treeViewNodeForField field fieldValueDescription children =
    { selfHtml = (field.fieldName ++ " = " ++ fieldValueDescription) |> Html.text
    , children = ExpandableChildren (NamedNode field.fieldName) children
    }


treeNodeChildrenFromRecordWithUINode :
    ViewConfig event
    -> UITreeNodeWithDisplayRegion
    ->
        List
            { fieldName : String
            , fieldValueSummary : String
            , fieldValueChildren : () -> List (TreeViewNode event ParsedUITreeViewPathNode)
            }
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromRecordWithUINode viewConfig fieldUINodeValue fields =
    treeNodeChildrenFromRecord
        ({ fieldName = "uiNode"
         , fieldValueSummary = "..."
         , fieldValueChildren =
            always [ treeViewNodeFromUINode viewConfig fieldUINodeValue ]
         }
            :: fields
        )


treeNodeChildrenFromRecord :
    List
        { fieldName : String
        , fieldValueSummary : String
        , fieldValueChildren : () -> List (TreeViewNode event ParsedUITreeViewPathNode)
        }
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromRecord fields =
    fields
        |> List.map
            (\{ fieldName, fieldValueSummary, fieldValueChildren } ->
                treeViewNodeForField
                    { fieldName = fieldName }
                    fieldValueSummary
                    fieldValueChildren
            )


treeNodeChildrenFromColorComponents :
    EveOnline.ParseUserInterface.ColorComponents
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromColorComponents color =
    treeNodeChildrenFromRecord
        [ color.r |> fieldFromInt "r"
        , color.g |> fieldFromInt "g"
        , color.b |> fieldFromInt "b"
        ]


fieldFromPrimitiveListInstance :
    { fieldName : String, fieldValueDescription : element -> String }
    -> List element
    -> { fieldName : String, fieldValueSummary : String, fieldValueChildren : () -> List (TreeViewNode event ParsedUITreeViewPathNode) }
fieldFromPrimitiveListInstance listField list =
    { fieldName = listField.fieldName
    , fieldValueSummary = "List (" ++ (list |> List.length |> String.fromInt) ++ ")"
    , fieldValueChildren =
        always
            (list
                |> List.indexedMap
                    (\index elem ->
                        { selfHtml = ((index |> String.fromInt) ++ " = " ++ listField.fieldValueDescription elem) |> Html.text
                        , children = NoChildren
                        }
                    )
            )
    }


fieldFromPrimitiveStringDictInstance :
    { fieldName : String, fieldValueDescription : value -> String }
    -> Dict.Dict String value
    -> { fieldName : String, fieldValueSummary : String, fieldValueChildren : () -> List (TreeViewNode event ParsedUITreeViewPathNode) }
fieldFromPrimitiveStringDictInstance listField dict =
    { fieldName = listField.fieldName
    , fieldValueSummary = "Dict (" ++ (dict |> Dict.size |> String.fromInt) ++ ")"
    , fieldValueChildren =
        always
            (dict
                |> Dict.toList
                |> List.map
                    (\( key, value ) ->
                        { selfHtml = "\"" ++ key ++ " = " ++ listField.fieldValueDescription value |> Html.text
                        , children = NoChildren
                        }
                    )
            )
    }


fieldFromListInstance :
    { fieldName : String, fieldValueChildren : element -> List (TreeViewNode event ParsedUITreeViewPathNode) }
    -> List element
    -> { fieldName : String, fieldValueSummary : String, fieldValueChildren : () -> List (TreeViewNode event ParsedUITreeViewPathNode) }
fieldFromListInstance listField list =
    { fieldName = listField.fieldName
    , fieldValueSummary = "List (" ++ (list |> List.length |> String.fromInt) ++ ")"
    , fieldValueChildren =
        always
            (list
                |> List.indexedMap
                    (\index elem ->
                        { selfHtml = index |> String.fromInt |> Html.text
                        , children = ExpandableChildren (IndexedNode index) (always (listField.fieldValueChildren elem))
                        }
                    )
            )
    }


fieldFromBool :
    String
    -> Bool
    -> { fieldName : String, fieldValueSummary : String, fieldValueChildren : () -> List (TreeViewNode event ParsedUITreeViewPathNode) }
fieldFromBool fieldName fieldValue =
    { fieldName = fieldName
    , fieldValueSummary = Json.Encode.bool fieldValue |> Json.Encode.encode 0
    , fieldValueChildren = always []
    }


fieldFromInt :
    String
    -> Int
    -> { fieldName : String, fieldValueSummary : String, fieldValueChildren : () -> List (TreeViewNode event ParsedUITreeViewPathNode) }
fieldFromInt fieldName fieldValue =
    { fieldName = fieldName
    , fieldValueSummary = Json.Encode.int fieldValue |> Json.Encode.encode 0
    , fieldValueChildren = always []
    }


fieldFromString :
    String
    -> String
    -> { fieldName : String, fieldValueSummary : String, fieldValueChildren : () -> List (TreeViewNode event ParsedUITreeViewPathNode) }
fieldFromString fieldName fieldValue =
    { fieldName = fieldName
    , fieldValueSummary = Json.Encode.string fieldValue |> Json.Encode.encode 0
    , fieldValueChildren = always []
    }


fieldFromMaybeBool :
    String
    -> Maybe Bool
    -> { fieldName : String, fieldValueSummary : String, fieldValueChildren : () -> List (TreeViewNode event ParsedUITreeViewPathNode) }
fieldFromMaybeBool fieldName =
    fieldFromMaybePrimitive { fieldName = fieldName, fieldValueSummary = Json.Encode.bool }


fieldFromMaybeInt :
    String
    -> Maybe Int
    -> { fieldName : String, fieldValueSummary : String, fieldValueChildren : () -> List (TreeViewNode event ParsedUITreeViewPathNode) }
fieldFromMaybeInt fieldName =
    fieldFromMaybePrimitive { fieldName = fieldName, fieldValueSummary = Json.Encode.int }


fieldFromMaybeString :
    String
    -> Maybe String
    -> { fieldName : String, fieldValueSummary : String, fieldValueChildren : () -> List (TreeViewNode event ParsedUITreeViewPathNode) }
fieldFromMaybeString fieldName =
    fieldFromMaybePrimitive { fieldName = fieldName, fieldValueSummary = Json.Encode.string }


fieldFromMaybePrimitive :
    { fieldName : String, fieldValueSummary : element -> Json.Encode.Value }
    -> Maybe element
    -> { fieldName : String, fieldValueSummary : String, fieldValueChildren : () -> List (TreeViewNode event ParsedUITreeViewPathNode) }
fieldFromMaybePrimitive { fieldName, fieldValueSummary } =
    fieldFromMaybeInstance
        { fieldName = fieldName
        , fieldValueSummary = fieldValueSummary >> Json.Encode.encode 0
        , fieldValueChildren = always []
        }


fieldFromResultPrimitive :
    { fieldName : String, errValueSummary : err -> Json.Encode.Value, okValueSummary : ok -> Json.Encode.Value }
    -> Result err ok
    -> { fieldName : String, fieldValueSummary : String, fieldValueChildren : () -> List (TreeViewNode event ParsedUITreeViewPathNode) }
fieldFromResultPrimitive maybeField maybeValue =
    let
        valueSummary =
            case maybeValue of
                Err err ->
                    "Err " ++ Json.Encode.encode 0 (maybeField.errValueSummary err)

                Ok ok ->
                    "Ok " ++ Json.Encode.encode 0 (maybeField.okValueSummary ok)
    in
    { fieldName = maybeField.fieldName
    , fieldValueSummary = valueSummary
    , fieldValueChildren = always []
    }


fieldFromMaybeInstance :
    { fieldName : String, fieldValueSummary : element -> String, fieldValueChildren : element -> List (TreeViewNode event ParsedUITreeViewPathNode) }
    -> Maybe element
    -> { fieldName : String, fieldValueSummary : String, fieldValueChildren : () -> List (TreeViewNode event ParsedUITreeViewPathNode) }
fieldFromMaybeInstance maybeField maybeValue =
    let
        ( valueSummary, children ) =
            maybeValue
                |> Maybe.map
                    (\just ->
                        ( "Just " ++ maybeField.fieldValueSummary just
                        , maybeField.fieldValueChildren just
                        )
                    )
                |> Maybe.withDefault ( "Nothing", [] )
    in
    { fieldName = maybeField.fieldName
    , fieldValueSummary = valueSummary
    , fieldValueChildren = always children
    }
