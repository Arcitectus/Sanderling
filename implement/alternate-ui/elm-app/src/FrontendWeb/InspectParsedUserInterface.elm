module FrontendWeb.InspectParsedUserInterface exposing
    ( InputOnUINode(..)
    , InputRoute
    , ParsedUITreeViewPathNode(..)
    , TreeViewNode
    , TreeViewNodeChildren(..)
    , renderTreeNodeFromParsedUserInterface
    , uiNodeCommonSummaryText
    )

import Dict
import EveOnline.MemoryReading
    exposing
        ( MaybeVisible(..)
        , UITreeNodeWithDisplayRegion
        , maybeVisibleMap
        )
import Html
import Html.Events as HE
import Json.Encode
import Set
import String.Extra


type InputOnUINode
    = MouseClickLeft
    | MouseClickRight


type ParsedUITreeViewPathNode
    = NamedNode String
    | IndexedNode Int


type alias InputRoute event =
    EveOnline.MemoryReading.UITreeNodeWithDisplayRegion -> InputOnUINode -> event


maybeInputOfferHtml : Maybe (InputRoute event) -> List InputOnUINode -> EveOnline.MemoryReading.UITreeNodeWithDisplayRegion -> Html.Html event
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
    -> EveOnline.MemoryReading.ParsedUserInterface
    -> TreeViewNode event ParsedUITreeViewPathNode
renderTreeNodeFromParsedUserInterface maybeInputRoute parsedUserInterface =
    let
        commonSummaryHtml =
            [ parsedUserInterface.uiTree.uiNode |> uiNodeCommonSummaryText |> Html.text ] |> Html.span []

        children =
            treeNodeChildrenFromRecord
                [ { fieldName = "uiTree"
                  , fieldValueSummary = "..."
                  , fieldValueChildren =
                        always
                            [ treeNodeFromParsedUserInterfaceUINode
                                maybeInputRoute
                                parsedUserInterface.uiTree
                            ]
                  }
                , parsedUserInterface.contextMenus
                    |> fieldFromListInstance
                        { fieldName = "contextMenus"
                        , fieldValueChildren = treeNodeChildrenFromParsedUserInterfaceContextMenu maybeInputRoute
                        }
                , parsedUserInterface.shipUI
                    |> fieldFromMaybeVisibleInstance
                        { fieldName = "shipUI"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromParsedUserInterfaceShipUI maybeInputRoute
                        }
                , parsedUserInterface.targets
                    |> fieldFromListInstance
                        { fieldName = "targets"
                        , fieldValueChildren = treeNodeChildrenFromParsedUserInterfaceTarget maybeInputRoute
                        }
                , parsedUserInterface.infoPanelLocationInfo
                    |> fieldFromMaybeVisibleInstance
                        { fieldName = "infoPanelLocationInfo"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromInfoPanelLocationInfo maybeInputRoute
                        }
                , parsedUserInterface.infoPanelRoute
                    |> fieldFromMaybeVisibleInstance
                        { fieldName = "infoPanelRoute"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromInfoPanelRoute maybeInputRoute
                        }
                , parsedUserInterface.overviewWindow
                    |> fieldFromMaybeVisibleInstance
                        { fieldName = "overviewWindow"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromOverviewWindow maybeInputRoute
                        }
                , parsedUserInterface.dronesWindow
                    |> fieldFromMaybeVisibleInstance
                        { fieldName = "dronesWindow"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromDronesWindow maybeInputRoute
                        }
                , parsedUserInterface.probeScannerWindow
                    |> fieldFromMaybeVisibleInstance
                        { fieldName = "probeScannerWindow"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromProbeScannerWindow maybeInputRoute
                        }
                , parsedUserInterface.stationWindow
                    |> fieldFromMaybeVisibleInstance
                        { fieldName = "stationWindow"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromStationWindow maybeInputRoute
                        }
                , parsedUserInterface.inventoryWindows
                    |> fieldFromListInstance
                        { fieldName = "inventoryWindows"
                        , fieldValueChildren = treeNodeChildrenFromParsedUserInterfaceInventoryWindow maybeInputRoute
                        }
                , parsedUserInterface.chatWindowStacks
                    |> fieldFromListInstance
                        { fieldName = "chatWindowStacks"
                        , fieldValueChildren = treeNodeChildrenFromChatWindowStack maybeInputRoute
                        }
                , parsedUserInterface.moduleButtonTooltip
                    |> fieldFromMaybeVisibleInstance
                        { fieldName = "moduleButtonTooltip"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromModuleButtonTooltip maybeInputRoute
                        }
                , parsedUserInterface.neocom
                    |> fieldFromMaybeVisibleInstance
                        { fieldName = "neocom"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren = treeNodeChildrenFromNeocom maybeInputRoute
                        }
                , parsedUserInterface.messageBoxes
                    |> fieldFromListInstance
                        { fieldName = "messageBoxes"
                        , fieldValueChildren = treeNodeChildrenFromMessageBox maybeInputRoute
                        }
                ]
    in
    { selfHtml = commonSummaryHtml
    , children = ExpandableChildren (NamedNode "temp-wrapping") (always children)
    }


treeNodeChildrenFromParsedUserInterfaceContextMenu :
    Maybe (InputRoute event)
    -> EveOnline.MemoryReading.ContextMenu
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromParsedUserInterfaceContextMenu maybeInputRoute parsedUserInterfaceContextMenu =
    treeNodeChildrenFromRecordWithUINode
        maybeInputRoute
        parsedUserInterfaceContextMenu.uiNode
        [ parsedUserInterfaceContextMenu.entries
            |> fieldFromListInstance
                { fieldName = "entries"
                , fieldValueChildren =
                    treeNodeChildrenFromParsedUserInterfaceContextMenuEntry maybeInputRoute
                }
        ]


treeNodeChildrenFromParsedUserInterfaceContextMenuEntry :
    Maybe (InputRoute event)
    -> EveOnline.MemoryReading.ContextMenuEntry
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromParsedUserInterfaceContextMenuEntry maybeInputRoute parsedUserInterfaceContextMenuEntry =
    treeNodeChildrenFromRecordWithUINode
        maybeInputRoute
        parsedUserInterfaceContextMenuEntry.uiNode
        [ { fieldName = "text"
          , fieldValueSummary = parsedUserInterfaceContextMenuEntry.text |> Json.Encode.string |> Json.Encode.encode 0
          , fieldValueChildren = always []
          }
        ]


treeNodeChildrenFromParsedUserInterfaceShipUI :
    Maybe (InputRoute event)
    -> EveOnline.MemoryReading.ShipUI
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromParsedUserInterfaceShipUI maybeInputRoute parsedUserInterfaceShipUI =
    treeNodeChildrenFromRecordWithUINode
        maybeInputRoute
        parsedUserInterfaceShipUI.uiNode
        [ parsedUserInterfaceShipUI.modules
            |> fieldFromListInstance
                { fieldName = "modules"
                , fieldValueChildren =
                    treeNodeChildrenFromParsedUserInterfaceShipUIModule maybeInputRoute
                }
        , { fieldName = "hitpointsPercent"
          , fieldValueSummary = "..."
          , fieldValueChildren =
                always (treeNodeChildrenFromParsedUserInterfaceShipUIHitpoints parsedUserInterfaceShipUI.hitpointsPercent)
          }
        ]


treeNodeChildrenFromParsedUserInterfaceShipUIModule :
    Maybe (InputRoute event)
    -> EveOnline.MemoryReading.ShipUIModule
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromParsedUserInterfaceShipUIModule maybeInputRoute parsedUserInterfaceShipUIModule =
    treeNodeChildrenFromRecordWithUINode
        maybeInputRoute
        parsedUserInterfaceShipUIModule.uiNode
        [ parsedUserInterfaceShipUIModule.isActive
            |> fieldFromMaybeInstance
                { fieldName = "isActive"
                , fieldValueSummary =
                    \isActive ->
                        if isActive then
                            "True"

                        else
                            "False"
                , fieldValueChildren = always []
                }
        ]


treeNodeChildrenFromParsedUserInterfaceShipUIHitpoints :
    EveOnline.MemoryReading.Hitpoints
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
    Maybe (InputRoute event)
    -> EveOnline.MemoryReading.Target
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromParsedUserInterfaceTarget maybeInputRoute parsedUserInterfaceTarget =
    treeNodeChildrenFromRecordWithUINode
        maybeInputRoute
        parsedUserInterfaceTarget.uiNode
        [ parsedUserInterfaceTarget.barAndImageCont
            |> fieldFromMaybeInstance
                { fieldName = "barAndImageCont"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeNodeFromParsedUserInterfaceUINode maybeInputRoute >> List.singleton
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
    Maybe (InputRoute event)
    -> EveOnline.MemoryReading.InfoPanelLocationInfo
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromInfoPanelLocationInfo maybeInputRoute infoPanelLocationInfo =
    treeNodeChildrenFromRecordWithUINode
        maybeInputRoute
        infoPanelLocationInfo.uiNode
        [ { fieldName = "listSurroundingsButton"
          , fieldValueSummary = "..."
          , fieldValueChildren = always [ treeNodeFromParsedUserInterfaceUINode maybeInputRoute infoPanelLocationInfo.listSurroundingsButton ]
          }
        ]


treeNodeChildrenFromInfoPanelRoute :
    Maybe (InputRoute event)
    -> EveOnline.MemoryReading.InfoPanelRoute
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromInfoPanelRoute maybeInputRoute infoPanelLocationRoute =
    treeNodeChildrenFromRecordWithUINode
        maybeInputRoute
        infoPanelLocationRoute.uiNode
        [ infoPanelLocationRoute.routeElementMarker
            |> fieldFromListInstance
                { fieldName = "routeElementMarker"
                , fieldValueChildren = .uiNode >> treeNodeFromParsedUserInterfaceUINode maybeInputRoute >> List.singleton
                }
        ]


treeNodeChildrenFromOverviewWindow :
    Maybe (InputRoute event)
    -> EveOnline.MemoryReading.OverviewWindow
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromOverviewWindow maybeInputRoute overviewWindow =
    treeNodeChildrenFromRecordWithUINode
        maybeInputRoute
        overviewWindow.uiNode
        [ overviewWindow.entries
            |> fieldFromListInstance
                { fieldName = "entries"
                , fieldValueChildren = treeNodeChildrenFromOverviewWindowEntry maybeInputRoute
                }
        ]


treeNodeChildrenFromOverviewWindowEntry :
    Maybe (InputRoute event)
    -> EveOnline.MemoryReading.OverviewWindowEntry
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromOverviewWindowEntry maybeInputRoute overviewWindowEntry =
    treeNodeChildrenFromRecordWithUINode
        maybeInputRoute
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
    Maybe (InputRoute event)
    -> EveOnline.MemoryReading.DronesWindow
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromDronesWindow maybeInputRoute dronesWindow =
    treeNodeChildrenFromRecordWithUINode
        maybeInputRoute
        dronesWindow.uiNode
        [ dronesWindow.droneGroups
            |> fieldFromListInstance
                { fieldName = "droneGroups"
                , fieldValueChildren = treeNodeChildrenFromDronesWindowDroneGroup maybeInputRoute
                }
        , dronesWindow.droneGroupInBay
            |> fieldFromMaybeInstance
                { fieldName = "droneGroupInBay"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeNodeChildrenFromDronesWindowDroneGroup maybeInputRoute
                }
        , dronesWindow.droneGroupInLocalSpace
            |> fieldFromMaybeInstance
                { fieldName = "droneGroupInLocalSpace"
                , fieldValueSummary = always "..."
                , fieldValueChildren = treeNodeChildrenFromDronesWindowDroneGroup maybeInputRoute
                }
        ]


treeNodeChildrenFromDronesWindowDroneGroup :
    Maybe (InputRoute event)
    -> EveOnline.MemoryReading.DronesWindowDroneGroup
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromDronesWindowDroneGroup maybeInputRoute dronesWindowDroneGroup =
    treeNodeChildrenFromRecord
        [ { fieldName = "header"
          , fieldValueSummary = "..."
          , fieldValueChildren = always (treeNodeChildrenFromDronesWindowDroneGroupHeader maybeInputRoute dronesWindowDroneGroup.header)
          }
        , dronesWindowDroneGroup.drones
            |> fieldFromListInstance
                { fieldName = "drones"
                , fieldValueChildren = treeNodeChildrenFromDronesWindowEntry maybeInputRoute
                }
        ]


treeNodeChildrenFromDronesWindowEntry :
    Maybe (InputRoute event)
    -> EveOnline.MemoryReading.DronesWindowEntry
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromDronesWindowEntry maybeInputRoute dronesWindowEntry =
    treeNodeChildrenFromRecordWithUINode
        maybeInputRoute
        dronesWindowEntry.uiNode
        [ dronesWindowEntry.mainText
            |> fieldFromMaybeInstance
                { fieldName = "mainText"
                , fieldValueSummary = Json.Encode.string >> Json.Encode.encode 0
                , fieldValueChildren = always []
                }
        ]


treeNodeChildrenFromDronesWindowDroneGroupHeader :
    Maybe (InputRoute event)
    -> EveOnline.MemoryReading.DronesWindowDroneGroupHeader
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromDronesWindowDroneGroupHeader maybeInputRoute dronesWindowDroneGroupHeader =
    treeNodeChildrenFromRecordWithUINode
        maybeInputRoute
        dronesWindowDroneGroupHeader.uiNode
        [ dronesWindowDroneGroupHeader.mainText
            |> fieldFromMaybeInstance
                { fieldName = "mainText"
                , fieldValueSummary = Json.Encode.string >> Json.Encode.encode 0
                , fieldValueChildren = always []
                }
        , dronesWindowDroneGroupHeader.quantityFromTitle
            |> fieldFromMaybeInstance
                { fieldName = "quantityFromTitle"
                , fieldValueSummary = String.fromInt
                , fieldValueChildren = always []
                }
        ]


treeNodeChildrenFromProbeScannerWindow :
    Maybe (InputRoute event)
    -> EveOnline.MemoryReading.ProbeScannerWindow
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromProbeScannerWindow maybeInputRoute probeScannerWindow =
    treeNodeChildrenFromRecordWithUINode
        maybeInputRoute
        probeScannerWindow.uiNode
        [ probeScannerWindow.scanResults
            |> fieldFromListInstance
                { fieldName = "scanResults"
                , fieldValueChildren = treeNodeChildrenFromProbeScanResult maybeInputRoute
                }
        ]


treeNodeChildrenFromProbeScanResult :
    Maybe (InputRoute event)
    -> EveOnline.MemoryReading.ProbeScanResult
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromProbeScanResult maybeInputRoute probeScanResult =
    treeNodeChildrenFromRecordWithUINode
        maybeInputRoute
        probeScanResult.uiNode
        [ probeScanResult.textsLeftToRight
            |> fieldFromPrimitiveListInstance
                { fieldName = "textsLeftToRight"
                , fieldValueDescription = Json.Encode.string >> Json.Encode.encode 0
                }
        ]


treeNodeChildrenFromStationWindow :
    Maybe (InputRoute event)
    -> EveOnline.MemoryReading.StationWindow
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromStationWindow maybeInputRoute stationWindow =
    treeNodeChildrenFromRecordWithUINode
        maybeInputRoute
        stationWindow.uiNode
        []


treeNodeChildrenFromParsedUserInterfaceInventoryWindow :
    Maybe (InputRoute event)
    -> EveOnline.MemoryReading.InventoryWindow
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromParsedUserInterfaceInventoryWindow maybeInputRoute parsedUserInterfaceInventoryWindow =
    treeNodeChildrenFromRecordWithUINode
        maybeInputRoute
        parsedUserInterfaceInventoryWindow.uiNode
        [ parsedUserInterfaceInventoryWindow.selectedContainerCapacityGauge
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
                    treeNodeChildrenFromParsedUserInterfaceInventory maybeInputRoute
                }
        ]


treeNodeChildrenFromParsedUserInterfaceInventory :
    Maybe (InputRoute event)
    -> EveOnline.MemoryReading.Inventory
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromParsedUserInterfaceInventory maybeInputRoute parsedUserInterfaceInventory =
    treeNodeChildrenFromRecordWithUINode
        maybeInputRoute
        parsedUserInterfaceInventory.uiNode
        [ parsedUserInterfaceInventory.itemsView
            |> fieldFromMaybeInstance
                { fieldName = "itemsView"
                , fieldValueSummary = always "..."
                , fieldValueChildren =
                    treeNodeChildrenFromParsedUserInterfaceInventoryItemsView maybeInputRoute
                }
        ]


treeNodeChildrenFromParsedUserInterfaceInventoryCapacityGauge :
    EveOnline.MemoryReading.InventoryWindowCapacityGauge
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromParsedUserInterfaceInventoryCapacityGauge parsedUserInterfaceInventoryCapacityGauge =
    treeNodeChildrenFromRecord
        [ { fieldName = "used"
          , fieldValueSummary = String.fromInt parsedUserInterfaceInventoryCapacityGauge.used
          , fieldValueChildren = always []
          }
        , parsedUserInterfaceInventoryCapacityGauge.maximum
            |> fieldFromMaybeInstance
                { fieldName = "maximum"
                , fieldValueSummary = String.fromInt
                , fieldValueChildren = always []
                }
        , parsedUserInterfaceInventoryCapacityGauge.selected
            |> fieldFromMaybeInstance
                { fieldName = "selected"
                , fieldValueSummary = String.fromInt
                , fieldValueChildren = always []
                }
        ]


treeNodeChildrenFromParsedUserInterfaceInventoryItemsView :
    Maybe (InputRoute event)
    -> EveOnline.MemoryReading.InventoryItemsView
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromParsedUserInterfaceInventoryItemsView maybeInputRoute parsedUserInterfaceInventoryItemsView =
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
                                                [ treeNodeFromParsedUserInterfaceUINode maybeInputRoute inventoryItem ]
                                        }
                                ]
                            )
                  }
                ]
    in
    case parsedUserInterfaceInventoryItemsView of
        EveOnline.MemoryReading.InventoryItemsListView { items } ->
            continueWithTagName "InventoryItemsListView" items

        EveOnline.MemoryReading.InventoryItemsNotListView { items } ->
            continueWithTagName "InventoryItemsNotListView" items


treeNodeChildrenFromChatWindowStack :
    Maybe (InputRoute event)
    -> EveOnline.MemoryReading.ChatWindowStack
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromChatWindowStack maybeInputRoute chatWindowStack =
    treeNodeChildrenFromRecordWithUINode
        maybeInputRoute
        chatWindowStack.uiNode
        []


treeNodeChildrenFromModuleButtonTooltip :
    Maybe (InputRoute event)
    -> EveOnline.MemoryReading.ModuleButtonTooltip
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromModuleButtonTooltip maybeInputRoute moduleButtonTooltip =
    treeNodeChildrenFromRecordWithUINode
        maybeInputRoute
        moduleButtonTooltip.uiNode
        []


treeNodeChildrenFromNeocom :
    Maybe (InputRoute event)
    -> EveOnline.MemoryReading.Neocom
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromNeocom maybeInputRoute neocom =
    treeNodeChildrenFromRecordWithUINode
        maybeInputRoute
        neocom.uiNode
        []


treeNodeChildrenFromMessageBox :
    Maybe (InputRoute event)
    -> EveOnline.MemoryReading.MessageBox
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromMessageBox maybeInputRoute messageBox =
    treeNodeChildrenFromRecordWithUINode
        maybeInputRoute
        messageBox.uiNode
        []


treeNodeFromParsedUserInterfaceUINode :
    Maybe (InputRoute event)
    -> EveOnline.MemoryReading.UITreeNodeWithDisplayRegion
    -> TreeViewNode event ParsedUITreeViewPathNode
treeNodeFromParsedUserInterfaceUINode maybeInputRoute parsedUserInterfaceUINode =
    let
        inputHtml =
            maybeInputOfferHtml maybeInputRoute [ MouseClickLeft, MouseClickRight ] parsedUserInterfaceUINode

        commonSummaryHtml =
            [ parsedUserInterfaceUINode.uiNode |> uiNodeCommonSummaryText |> Html.text, inputHtml ] |> Html.span []
    in
    { selfHtml = commonSummaryHtml
    , children = NoChildren
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


type alias TreeViewNode event expandableId =
    { selfHtml : Html.Html event
    , children : TreeViewNodeChildren event expandableId
    }


type TreeViewNodeChildren event expandableId
    = NoChildren
    | ExpandableChildren expandableId (() -> List (TreeViewNode event expandableId))


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
    Maybe (InputRoute event)
    -> UITreeNodeWithDisplayRegion
    ->
        List
            { fieldName : String
            , fieldValueSummary : String
            , fieldValueChildren : () -> List (TreeViewNode event ParsedUITreeViewPathNode)
            }
    -> List (TreeViewNode event ParsedUITreeViewPathNode)
treeNodeChildrenFromRecordWithUINode maybeInputRoute fieldUINodeValue fields =
    treeNodeChildrenFromRecord
        ({ fieldName = "uiNode"
         , fieldValueSummary = "..."
         , fieldValueChildren =
            always
                [ treeNodeFromParsedUserInterfaceUINode
                    maybeInputRoute
                    fieldUINodeValue
                ]
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
                |> EveOnline.MemoryReading.maybeNothingFromCanNotSeeIt
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
