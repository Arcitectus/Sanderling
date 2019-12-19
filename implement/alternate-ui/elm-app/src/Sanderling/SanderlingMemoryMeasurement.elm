module Sanderling.SanderlingMemoryMeasurement exposing
    ( ContextMenu
    , ContextMenuEntry
    , InfoPanelRoute
    , InfoPanelRouteRouteElementMarker
    , MaybeVisible(..)
    , MemoryMeasurementReducedWithNamedNodes
    , OverviewWindowEntry
    , ShipManeuverType(..)
    , ShipUi
    , ShipUiIndication
    , ShipUiModule
    , UIElement
    , UIElementRegion
    , maybeNothingFromCanNotSeeIt
    , maybeVisibleAndThen
    , parseInventoryCapacityGaugeText
    , parseInventoryDecoder
    , parseInventoryWindowCapacityGaugeDecoder
    , parseMemoryMeasurementReducedWithNamedNodesFromJson
    , parseOverviewEntryDistanceInMetersFromText
    , parseOverviewWindowListViewEntryDecoder
    , shipUiDecoder
    , shipUiModuleDecoder
    , targetDecoder
    )

import Json.Decode
import Json.Decode.Extra
import Json.Encode
import Regex
import Result.Extra


type alias MemoryMeasurementReducedWithNamedNodes =
    { contextMenus : List ContextMenu
    , shipUi : MaybeVisible ShipUi
    , targets : List Target
    , infoPanelCurrentSystem : MaybeVisible InfoPanelCurrentSystem
    , infoPanelRoute : MaybeVisible InfoPanelRoute
    , overviewWindow : MaybeVisible OverviewWindow
    , inventoryWindow : MaybeVisible InventoryWindow
    }


type alias ContextMenu =
    { uiElement : UIElement
    , entries : List ContextMenuEntry
    }


type alias ContextMenuEntry =
    TextLabel


type alias InfoPanelCurrentSystem =
    { listSurroundingsButton : UIElement
    , expandedContent : MaybeVisible InfoPanelCurrentSystemExpandedContent
    }


type alias InfoPanelCurrentSystemExpandedContent =
    { currentStationName : Maybe String
    }


type alias InfoPanelRoute =
    { routeElementMarker : List InfoPanelRouteRouteElementMarker }


type alias InfoPanelRouteRouteElementMarker =
    { uiElement : UIElement }


type alias ShipUi =
    { indication : MaybeVisible ShipUiIndication
    , modules : List ShipUiModule
    , shipIsStopped : Maybe Bool
    , hitpointsAndEnergyMilli : HitpointsAndEnergy
    }


type alias HitpointsAndEnergy =
    { struct : Int
    , armor : Int
    , shield : Int
    , capacitor : Int
    }


type alias ShipUiModule =
    { uiElement : UIElement
    , isActive : Maybe Bool
    }


type alias ShipUiIndication =
    { maneuverType : MaybeVisible ShipManeuverType }


type alias Target =
    { uiElement : UIElement
    , textsTopToBottom : List String
    }


type alias OverviewWindow =
    { uiElement : UIElement
    , entries : List OverviewWindowEntry
    }


type alias OverviewWindowEntry =
    { uiElement : UIElement
    , textsLeftToRight : List String
    , distanceInMeters : Result String Int
    }


type alias InventoryWindow =
    { leftTreeEntries : List InventoryWindowLeftTreeEntry
    , selectedContainedCapacityGauge : Maybe InventoryWindowCapacityGauge
    , selectedContainerInventory : Maybe Inventory
    }


type alias Inventory =
    { listViewItems : List UIElement
    }


type alias InventoryWindowLeftTreeEntry =
    { uiElement : UIElement
    , text : String
    }


type alias InventoryWindowCapacityGauge =
    { maximum : Int
    , used : Int
    }


type MaybeVisible feature
    = CanNotSeeIt
    | CanSee feature


type ShipManeuverType
    = Warp
    | Jump
    | Orbit
    | Approach


type alias TextLabel =
    { uiElement : UIElement
    , text : String
    }


type alias UIElement =
    { id : Int
    , region : UIElementRegion
    }


type alias UIElementRegion =
    { left : Int
    , top : Int
    , right : Int
    , bottom : Int
    }


{-| Parse JSON string containing a Sanderling memory measurement.
The string expected here is not the raw measurement, but the stage which after parsing for named nodes.
To get a representation of the EVE Online clients memory contents as expected here, see the example at <https://github.com/Arcitectus/Sanderling/blob/ada11c9f8df2367976a6bcc53efbe9917107bfa7/src/Sanderling/Sanderling.MemoryReading.Test/MemoryReadingDemo.cs>
-}
parseMemoryMeasurementReducedWithNamedNodesFromJson : String -> Result String MemoryMeasurementReducedWithNamedNodes
parseMemoryMeasurementReducedWithNamedNodesFromJson =
    Json.Decode.decodeString memoryMeasurementReducedWithNamedNodesJsonDecoder
        >> Result.mapError Json.Decode.errorToString


memoryMeasurementReducedWithNamedNodesJsonDecoder : Json.Decode.Decoder MemoryMeasurementReducedWithNamedNodes
memoryMeasurementReducedWithNamedNodesJsonDecoder =
    Json.Decode.map7 MemoryMeasurementReducedWithNamedNodes
        -- TODO: Consider treating 'null' value like field is not present, to avoid breakage when server encodes fiels with 'null' values too.
        (Json.Decode.Extra.optionalField "Menu" (Json.Decode.list contextMenuDecoder) |> Json.Decode.map (Maybe.withDefault []))
        (Json.Decode.Extra.optionalField "ShipUi" shipUiDecoder |> Json.Decode.map canNotSeeItFromMaybeNothing)
        (Json.Decode.Extra.optionalField "Target" (Json.Decode.list targetDecoder) |> Json.Decode.map (Maybe.withDefault []))
        (Json.Decode.Extra.optionalField "InfoPanelCurrentSystem" infoPanelCurrentSystemDecoder |> Json.Decode.map canNotSeeItFromMaybeNothing)
        (Json.Decode.Extra.optionalField "InfoPanelRoute" infoPanelRouteDecoder |> Json.Decode.map canNotSeeItFromMaybeNothing)
        (Json.Decode.Extra.optionalField "WindowOverview" (Json.Decode.list parseOverviewWindowDecoder) |> Json.Decode.map (Maybe.andThen List.head >> canNotSeeItFromMaybeNothing))
        (Json.Decode.Extra.optionalField "WindowInventory" (Json.Decode.list parseInventoryWindowDecoder) |> Json.Decode.map (Maybe.andThen List.head >> canNotSeeItFromMaybeNothing))


shipUiDecoder : Json.Decode.Decoder ShipUi
shipUiDecoder =
    Json.Decode.map4 ShipUi
        (Json.Decode.maybe
            (Json.Decode.field "Indication" shipUiIndicationDecoder)
            |> Json.Decode.map canNotSeeItFromMaybeNothing
        )
        (Json.Decode.maybe
            (Json.Decode.field "Module" (Json.Decode.list shipUiModuleDecoder))
            |> Json.Decode.map (Maybe.withDefault [])
        )
        shipUiIsStoppedDecoder
        (Json.Decode.field "HitpointsAndEnergy"
            (Json.Decode.map4 HitpointsAndEnergy
                (Json.Decode.field "Struct" Json.Decode.int)
                (Json.Decode.field "Armor" Json.Decode.int)
                (Json.Decode.field "Shield" Json.Decode.int)
                (Json.Decode.field "Capacitor" Json.Decode.int)
            )
        )


shipUiModuleDecoder : Json.Decode.Decoder ShipUiModule
shipUiModuleDecoder =
    Json.Decode.map2 ShipUiModule
        uiElementDecoder
        (Json.Decode.maybe (Json.Decode.field "RampActive" Json.Decode.bool))


shipUiIndicationDecoder : Json.Decode.Decoder ShipUiIndication
shipUiIndicationDecoder =
    Json.Decode.value |> Json.Decode.map shipUiIndicationFromJsonValue


shipUiIndicationFromJsonValue : Json.Encode.Value -> ShipUiIndication
shipUiIndicationFromJsonValue jsonValue =
    let
        jsonString =
            Json.Encode.encode 0 jsonValue

        maneuverType =
            [ ( "Warp", Warp )
            , ( "Jump", Jump )
            , ( "Orbit", Orbit )
            , ( "Approach", Approach )
            ]
                |> List.filterMap
                    (\( pattern, candidateManeuverType ) ->
                        if jsonString |> String.contains pattern then
                            Just candidateManeuverType

                        else
                            Nothing
                    )
                |> List.head
                |> canNotSeeItFromMaybeNothing
    in
    { maneuverType = maneuverType }


shipUiIsStoppedDecoder : Json.Decode.Decoder (Maybe Bool)
shipUiIsStoppedDecoder =
    case "\\d" |> Regex.fromString of
        Nothing ->
            Json.Decode.fail "Regex code error"

        Just digitRegex ->
            Json.Decode.maybe
                (Json.Decode.field "SpeedLabel" textLabelDecoder)
                |> Json.Decode.map
                    (Maybe.andThen
                        (\speedLabel ->
                            let
                                digitTexts =
                                    speedLabel.text
                                        |> Regex.find digitRegex
                                        |> List.map .match
                            in
                            -- Is stopped means: All decimal digit characters are '0'.
                            if (digitTexts |> List.length) < 1 then
                                Nothing

                            else
                                Just (digitTexts |> List.all ((==) "0"))
                        )
                    )


targetDecoder : Json.Decode.Decoder Target
targetDecoder =
    Json.Decode.map2
        (\uiElement textLabels ->
            let
                textsTopToBottom =
                    textLabels |> List.sortBy (.uiElement >> .region >> .top) |> List.map .text
            in
            { uiElement = uiElement
            , textsTopToBottom = textsTopToBottom
            }
        )
        uiElementDecoder
        (Json.Decode.field "LabelText" (Json.Decode.list textLabelDecoder))


infoPanelCurrentSystemDecoder : Json.Decode.Decoder InfoPanelCurrentSystem
infoPanelCurrentSystemDecoder =
    Json.Decode.map2 InfoPanelCurrentSystem
        (Json.Decode.field "ListSurroundingsButton" uiElementDecoder)
        (fieldMapNotPresentOrNullToMaybe "ExpandedContent" infoPanelCurrentSystemExpandedContentDecoder |> Json.Decode.map canNotSeeItFromMaybeNothing)


infoPanelCurrentSystemExpandedContentDecoder : Json.Decode.Decoder InfoPanelCurrentSystemExpandedContent
infoPanelCurrentSystemExpandedContentDecoder =
    fieldMapNotPresentOrNullToMaybe "LabelText" (Json.Decode.list textLabelDecoder)
        |> Json.Decode.map (Maybe.withDefault [])
        |> Json.Decode.map (List.filterMap (.text >> parseCurrentSystemFromInfoPanelCurrentSystemLabelText) >> List.head)
        |> Json.Decode.map InfoPanelCurrentSystemExpandedContent


parseCurrentSystemFromInfoPanelCurrentSystemLabelText : String -> Maybe String
parseCurrentSystemFromInfoPanelCurrentSystemLabelText labelText =
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


infoPanelRouteDecoder : Json.Decode.Decoder InfoPanelRoute
infoPanelRouteDecoder =
    Json.Decode.map InfoPanelRoute
        (Json.Decode.maybe
            (Json.Decode.field "RouteElementMarker" (Json.Decode.list infoPanelRouteRouteElementMarkerDecoder))
            |> Json.Decode.map (Maybe.withDefault [])
        )


infoPanelRouteRouteElementMarkerDecoder : Json.Decode.Decoder InfoPanelRouteRouteElementMarker
infoPanelRouteRouteElementMarkerDecoder =
    uiElementDecoder
        |> Json.Decode.map (\uiElement -> { uiElement = uiElement })


contextMenuDecoder : Json.Decode.Decoder ContextMenu
contextMenuDecoder =
    Json.Decode.map2 ContextMenu
        uiElementDecoder
        (Json.Decode.field "Entry" (Json.Decode.list contextMenuEntryDecoder))


contextMenuEntryDecoder : Json.Decode.Decoder ContextMenuEntry
contextMenuEntryDecoder =
    textLabelDecoder


parseOverviewWindowDecoder : Json.Decode.Decoder OverviewWindow
parseOverviewWindowDecoder =
    Json.Decode.map2 OverviewWindow
        uiElementDecoder
        (Json.Decode.Extra.optionalField "ListView" parseOverviewWindowListViewDecoder |> Json.Decode.map (Maybe.withDefault []))


parseOverviewWindowListViewDecoder : Json.Decode.Decoder (List OverviewWindowEntry)
parseOverviewWindowListViewDecoder =
    Json.Decode.field "Entry"
        (Json.Decode.list parseOverviewWindowListViewEntryDecoder
            |> Json.Decode.map (List.sortBy (.uiElement >> .region >> .top))
        )


parseOverviewWindowListViewEntryDecoder : Json.Decode.Decoder OverviewWindowEntry
parseOverviewWindowListViewEntryDecoder =
    Json.Decode.map2
        (\uiElement textLabels ->
            let
                textsLeftToRight =
                    textLabels |> List.sortBy (.uiElement >> .region >> .left) |> List.map .text
            in
            { uiElement = uiElement
            , textsLeftToRight = textsLeftToRight
            , distanceInMeters = textsLeftToRight |> parseOverviewEntryDistanceInMetersFromTexts
            }
        )
        uiElementDecoder
        (Json.Decode.field "LabelText" (Json.Decode.list textLabelDecoder))


parseOverviewEntryDistanceInMetersFromTexts : List String -> Result String Int
parseOverviewEntryDistanceInMetersFromTexts texts =
    let
        parseResults =
            texts |> List.map parseOverviewEntryDistanceInMetersFromText
    in
    case parseResults |> List.filterMap Result.toMaybe |> List.head of
        Nothing ->
            Err
                ("Parsing did not succeed for any of the texts: "
                    ++ ((parseResults |> List.filterMap (Result.Extra.unpack Just (always Nothing))) |> String.join ", ")
                )

        Just distanceInMeters ->
            Ok distanceInMeters


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


parseInventoryWindowDecoder : Json.Decode.Decoder InventoryWindow
parseInventoryWindowDecoder =
    Json.Decode.map3 InventoryWindow
        (Json.Decode.field "LeftTreeListEntry" (Json.Decode.list parseInventoryWindowLeftTreeEntry))
        (Json.Decode.Extra.optionalField "SelectedRightInventoryCapacity" parseInventoryWindowCapacityGaugeDecoder)
        (Json.Decode.Extra.optionalField "SelectedRightInventory" parseInventoryDecoder)


parseInventoryWindowLeftTreeEntry : Json.Decode.Decoder InventoryWindowLeftTreeEntry
parseInventoryWindowLeftTreeEntry =
    Json.Decode.map2 InventoryWindowLeftTreeEntry
        uiElementDecoder
        (Json.Decode.field "Text" Json.Decode.string)


parseInventoryWindowCapacityGaugeDecoder : Json.Decode.Decoder InventoryWindowCapacityGauge
parseInventoryWindowCapacityGaugeDecoder =
    Json.Decode.field "Text" Json.Decode.string
        |> Json.Decode.andThen (Json.Decode.Extra.fromResult << parseInventoryCapacityGaugeText)


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


parseNumberTruncatingAfterOptionalDecimalSeparator : String -> Result String Int
parseNumberTruncatingAfterOptionalDecimalSeparator numberDisplayText =
    case "^([\\d\\,\\s]+?)(?=(|[,\\.]\\d)$)" |> Regex.fromString of
        Nothing ->
            Err "Regex code error"

        Just regex ->
            case numberDisplayText |> String.trim |> Regex.find regex |> List.head of
                Nothing ->
                    Err ("Text did not match expected number format: '" ++ numberDisplayText ++ "'")

                Just match ->
                    match.match
                        |> String.replace "," ""
                        |> String.replace " " ""
                        |> String.toInt
                        |> Result.fromMaybe ("Failed to parse to integer: " ++ match.match)


parseInventoryDecoder : Json.Decode.Decoder Inventory
parseInventoryDecoder =
    Json.Decode.map Inventory
        (Json.Decode.field "ListView" (Json.Decode.field "Entry" (Json.Decode.list uiElementDecoder)))


textLabelDecoder : Json.Decode.Decoder TextLabel
textLabelDecoder =
    Json.Decode.map2 TextLabel
        uiElementDecoder
        (Json.Decode.field "Text" Json.Decode.string)


uiElementDecoder : Json.Decode.Decoder UIElement
uiElementDecoder =
    Json.Decode.map2 UIElement
        (Json.Decode.field "Id" Json.Decode.int)
        (Json.Decode.field "Region" uiElementRegionDecoder)


uiElementRegionDecoder : Json.Decode.Decoder UIElementRegion
uiElementRegionDecoder =
    Json.Decode.map4 UIElementRegion
        (Json.Decode.field "Min0" Json.Decode.float |> Json.Decode.map round)
        (Json.Decode.field "Min1" Json.Decode.float |> Json.Decode.map round)
        (Json.Decode.field "Max0" Json.Decode.float |> Json.Decode.map round)
        (Json.Decode.field "Max1" Json.Decode.float |> Json.Decode.map round)


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


fieldMapNotPresentOrNullToMaybe : String -> Json.Decode.Decoder a -> Json.Decode.Decoder (Maybe a)
fieldMapNotPresentOrNullToMaybe fieldName decoderIfPresentAndNotNull =
    Json.Decode.Extra.optionalField fieldName (Json.Decode.nullable decoderIfPresentAndNotNull)
        |> Json.Decode.map (Maybe.andThen identity)
