module FrontendWeb.InspectParsedUserInterface exposing
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
import EveOnline.ParseUserInterface
    exposing
        ( MaybeVisible(..)
        , UITreeNodeWithDisplayRegion
        )
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
                    |> fieldFromMaybeVisibleInstance
                        { fieldName = "shipUI"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromParsedUserInterfaceShipUI viewConfig
                        }
                , parsedUserInterface.targets
                    |> fieldFromListInstance
                        { fieldName = "targets"
                        , fieldValueChildren = treeNodeChildrenFromParsedUserInterfaceTarget viewConfig
                        }
                , parsedUserInterface.infoPanelLocationInfo
                    |> fieldFromMaybeVisibleInstance
                        { fieldName = "infoPanelLocationInfo"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromInfoPanelLocationInfo viewConfig
                        }
                , parsedUserInterface.infoPanelRoute
                    |> fieldFromMaybeVisibleInstance
                        { fieldName = "infoPanelRoute"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromInfoPanelRoute viewConfig
                        }
                , parsedUserInterface.overviewWindow
                    |> fieldFromMaybeVisibleInstance
                        { fieldName = "overviewWindow"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromOverviewWindow viewConfig
                        }
                , parsedUserInterface.selectedItemWindow
                    |> fieldFromMaybeVisibleInstance
                        { fieldName = "selectedItemWindow"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromSelectedItemWindow viewConfig
                        }
                , parsedUserInterface.dronesWindow
                    |> fieldFromMaybeVisibleInstance
                        { fieldName = "dronesWindow"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromDronesWindow viewConfig
                        }
                , parsedUserInterface.fittingWindow
                    |> fieldFromMaybeVisibleInstance
                        { fieldName = "fittingWindow"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromMarketOrdersWindow viewConfig
                        }
                , parsedUserInterface.probeScannerWindow
                    |> fieldFromMaybeVisibleInstance
                        { fieldName = "probeScannerWindow"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromProbeScannerWindow viewConfig
                        }
                , parsedUserInterface.stationWindow
                    |> fieldFromMaybeVisibleInstance
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
                    |> fieldFromMaybeVisibleInstance
                        { fieldName = "marketOrdersWindow"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromMarketOrdersWindow viewConfig
                        }
                , parsedUserInterface.moduleButtonTooltip
                    |> fieldFromMaybeVisibleInstance
                        { fieldName = "moduleButtonTooltip"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromModuleButtonTooltip viewConfig
                        }
                , parsedUserInterface.neocom
                    |> fieldFromMaybeVisibleInstance
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
                    |> fieldFromMaybeVisibleInstance
                        { fieldName = "layerAbovemain"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeViewNodeFromUINode viewConfig >> List.singleton
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
        [ parsedUserInterfaceShipUI.modules
            |> fieldFromListInstance
                { fieldName = "modules"
                , fieldValueChildren =
                    treeNodeChildrenFromParsedUserInterfaceShipUIModule viewConfig
                }
        , { fieldName = "hitpointsPercent"
          , fieldValueSummary = "..."
          , fieldValueChildren =
                always (treeNodeChildrenFromParsedUserInterfaceShipUIHitpoints parsedUserInterfaceShipUI.hitpointsPercent)
          }
        ]


treeNodeChildrenFromParsedUserInterfaceShipUIModule :
    ViewConfig event
    -> EveOnline.ParseUserInterface.ShipUIModule
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromParsedUserInterfaceShipUIModule viewConfig parsedUserInterfaceShipUIModule =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        parsedUserInterfaceShipUIModule.uiNode
        [ { fieldName = "slotUINode"
          , fieldValueSummary = "..."
          , fieldValueChildren =
                always [ treeViewNodeFromUINode viewConfig parsedUserInterfaceShipUIModule.slotUINode ]
          }
        , parsedUserInterfaceShipUIModule.isActive |> fieldFromMaybeBool "isActive"
        , parsedUserInterfaceShipUIModule.isHiliteVisible |> fieldFromBool "isHiliteVisible"
        ]


treeNodeChildrenFromParsedUserInterfaceShipUIHitpoints :
    EveOnline.ParseUserInterface.Hitpoints
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromParsedUserInterfaceShipUIHitpoints hitpoints =
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
        ]


treeNodeChildrenFromInfoPanelRoute :
    ViewConfig event
    -> EveOnline.ParseUserInterface.InfoPanelRoute
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromInfoPanelRoute viewConfig infoPanelLocationRoute =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        infoPanelLocationRoute.uiNode
        [ infoPanelLocationRoute.routeElementMarker
            |> fieldFromListInstance
                { fieldName = "routeElementMarker"
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
        , dronesWindow.droneGroupInLocalSpace
            |> fieldFromMaybeInstance
                { fieldName = "droneGroupInLocalSpace"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeNodeChildrenFromDronesWindowDroneGroup viewConfig
                }
        ]


treeNodeChildrenFromDronesWindowDroneGroup :
    ViewConfig event
    -> EveOnline.ParseUserInterface.DronesWindowDroneGroup
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromDronesWindowDroneGroup viewConfig dronesWindowDroneGroup =
    treeNodeChildrenFromRecord
        [ { fieldName = "header"
          , fieldValueSummary = "..."
          , fieldValueChildren = always (treeNodeChildrenFromDronesWindowDroneGroupHeader viewConfig dronesWindowDroneGroup.header)
          }
        , dronesWindowDroneGroup.drones
            |> fieldFromListInstance
                { fieldName = "drones"
                , fieldValueChildren = treeNodeChildrenFromDronesWindowEntry viewConfig
                }
        ]


treeNodeChildrenFromDronesWindowEntry :
    ViewConfig event
    -> EveOnline.ParseUserInterface.DronesWindowEntry
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromDronesWindowEntry viewConfig dronesWindowEntry =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        dronesWindowEntry.uiNode
        [ dronesWindowEntry.mainText |> fieldFromMaybeString "mainText"
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
        , dronesWindowDroneGroupHeader.quantityFromTitle |> fieldFromMaybeInt "quantityFromTitle"
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
        ]


treeNodeChildrenFromStationWindow :
    ViewConfig event
    -> EveOnline.ParseUserInterface.StationWindow
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromStationWindow viewConfig stationWindow =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        stationWindow.uiNode
        []


treeNodeChildrenFromParsedUserInterfaceInventoryWindow :
    ViewConfig event
    -> EveOnline.ParseUserInterface.InventoryWindow
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromParsedUserInterfaceInventoryWindow viewConfig parsedUserInterfaceInventoryWindow =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        parsedUserInterfaceInventoryWindow.uiNode
        [ parsedUserInterfaceInventoryWindow.selectedContainerCapacityGauge
            |> Maybe.andThen Result.toMaybe
            |> fieldFromMaybeInstance
                { fieldName = "selectedContainerCapacityGauge"
                , fieldValueSummary = always "..."
                , fieldValueChildren =
                    treeNodeChildrenFromParsedUserInterfaceInventoryCapacityGauge
                }
        , parsedUserInterfaceInventoryWindow.selectedContainerInventory
            |> fieldFromMaybeInstance
                { fieldName = "selectedContainerInventory"
                , fieldValueSummary = always "..."
                , fieldValueChildren =
                    treeNodeChildrenFromParsedUserInterfaceInventory viewConfig
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
        continueWithTagName tagName items =
            treeNodeChildrenFromRecord
                [ { fieldName = tagName
                  , fieldValueSummary = ""
                  , fieldValueChildren =
                        always
                            (treeNodeChildrenFromRecord
                                [ items
                                    |> fieldFromListInstance
                                        { fieldName = "items"
                                        , fieldValueChildren =
                                            \inventoryItem ->
                                                [ treeViewNodeFromUINode viewConfig inventoryItem ]
                                        }
                                ]
                            )
                  }
                ]
    in
    case parsedUserInterfaceInventoryItemsView of
        EveOnline.ParseUserInterface.InventoryItemsListView { items } ->
            continueWithTagName "InventoryItemsListView" items

        EveOnline.ParseUserInterface.InventoryItemsNotListView { items } ->
            continueWithTagName "InventoryItemsNotListView" items


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
        , chatWindow.visibleUsers
            |> fieldFromListInstance
                { fieldName = "visibleUsers "
                , fieldValueChildren = treeNodeChildrenFromChatUserEntry viewConfig
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


treeNodeChildrenFromModuleButtonTooltip :
    ViewConfig event
    -> EveOnline.ParseUserInterface.ModuleButtonTooltip
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromModuleButtonTooltip viewConfig moduleButtonTooltip =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        moduleButtonTooltip.uiNode
        []


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


treeNodeChildrenFromNeocom :
    ViewConfig event
    -> EveOnline.ParseUserInterface.Neocom
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromNeocom viewConfig neocom =
    treeNodeChildrenFromRecordWithUINode
        viewConfig
        neocom.uiNode
        []


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
                , fieldValueChildren = treeNodeChildrenFromDronesWindowEntry viewConfig
                }
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
                        |> Maybe.map
                            (\totalDisplayRegion ->
                                [ ( "x", .x ), ( "y", .y ), ( "width", .width ), ( "height", .height ) ]
                                    |> List.map
                                        (\( regionPropertyName, regionProperty ) ->
                                            ( regionPropertyName, totalDisplayRegion |> regionProperty |> Json.Encode.int )
                                        )
                                    |> Json.Encode.object
                            )
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
                    ]

                allDisplayChildren =
                    (properties |> List.map propertyDisplayChild) ++ [ displayEntryOtherProperties, displayEntryChildren, displayEntryGetAllContainedDisplayTexts ]
            in
            allDisplayChildren
    in
    { selfHtml = commonSummaryHtml
    , children = ExpandableChildren (ExpandableUITreeNode nodeIdentityInView) getDisplayChildren
    }


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


fieldFromMaybeVisibleInstance :
    { fieldName : String, fieldValueSummary : element -> String, fieldValueChildren : element -> List (TreeViewNode event ParsedUITreeViewPathNode) }
    -> MaybeVisible element
    -> { fieldName : String, fieldValueSummary : String, fieldValueChildren : () -> List (TreeViewNode event ParsedUITreeViewPathNode) }
fieldFromMaybeVisibleInstance maybeField maybeValue =
    let
        ( valueSummary, children ) =
            maybeValue
                |> EveOnline.ParseUserInterface.maybeNothingFromCanNotSeeIt
                |> Maybe.map
                    (\just ->
                        ( "CanSee " ++ maybeField.fieldValueSummary just
                        , maybeField.fieldValueChildren just
                        )
                    )
                |> Maybe.withDefault ( "CanNotSeeIt", [] )
    in
    { fieldName = maybeField.fieldName
    , fieldValueSummary = valueSummary
    , fieldValueChildren = always children
    }
