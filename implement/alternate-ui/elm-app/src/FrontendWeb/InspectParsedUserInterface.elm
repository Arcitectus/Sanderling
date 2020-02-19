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
                        , fieldValueChildren =
                            treeNodeChildrenFromParsedUserInterfaceContextMenu maybeInputRoute
                        }
                , parsedUserInterface.shipUI
                    |> fieldFromMaybeVisibleInstance
                        { fieldName = "shipUI"
                        , fieldValueSummary = always "..."
                        , fieldValueChildren =
                            treeNodeChildrenFromParsedUserInterfaceShipUI maybeInputRoute
                        }
                , parsedUserInterface.targets
                    |> fieldFromListInstance
                        { fieldName = "targets"
                        , fieldValueChildren =
                            treeNodeChildrenFromParsedUserInterfaceTarget maybeInputRoute
                        }
                , parsedUserInterface.inventoryWindows
                    |> fieldFromListInstance
                        { fieldName = "inventoryWindows"
                        , fieldValueChildren =
                            treeNodeChildrenFromParsedUserInterfaceInventoryWindow
                                maybeInputRoute
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
