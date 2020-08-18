module ParseMemoryReadingTest exposing (allTests)

import Common.EffectOnWindow
import EveOnline.ParseUserInterface
import Expect
import Test


allTests : Test.Test
allTests =
    Test.describe "Parse memory reading"
        [ overview_entry_distance_text_to_meter
        , inventory_capacity_gauge_text
        , parse_module_button_tooltip_shortcut
        ]


overview_entry_distance_text_to_meter : Test.Test
overview_entry_distance_text_to_meter =
    [ ( "2,856 m", Ok 2856 )
    , ( "123 m", Ok 123 )
    , ( "16 km", Ok 16000 )
    , ( "   345 m  ", Ok 345 )

    -- 2020-03-12 from TheRealManiac (https://forum.botengine.org/t/last-version-of-mining-bot/3149)
    , ( "6.621 m  ", Ok 6621 )

    -- 2020-03-22 from istu233 at https://forum.botengine.org/t/mining-bot-problem/3169
    , ( "2 980 m", Ok 2980 )
    ]
        |> List.map
            (\( displayText, expectedResult ) ->
                Test.test displayText <|
                    \_ ->
                        displayText
                            |> EveOnline.ParseUserInterface.parseOverviewEntryDistanceInMetersFromText
                            |> Expect.equal expectedResult
            )
        |> Test.describe "Overview entry distance text"


inventory_capacity_gauge_text : Test.Test
inventory_capacity_gauge_text =
    [ ( "1,211.9/5,000.0 m³", Ok { used = 1211, maximum = Just 5000, selected = Nothing } )
    , ( " 123.4 / 5,000.0 m³ ", Ok { used = 123, maximum = Just 5000, selected = Nothing } )

    -- Example from https://forum.botengine.org/t/standard-mining-bot-problems/2715/14?u=viir
    , ( "4 999,8/5 000,0 m³", Ok { used = 4999, maximum = Just 5000, selected = Nothing } )

    -- 2020-01-31 sample 'process-sample-2FA2DCF580-[In Space with selected Ore Hold].zip' from Leon Bechen.
    , ( "0/5.000,0 m³", Ok { used = 0, maximum = Just 5000, selected = Nothing } )

    -- 2020-02-16-eve-online-sample
    , ( "(33.3) 53.6/450.0 m³", Ok { used = 53, maximum = Just 450, selected = Just 33 } )

    -- 2020-02-23 process-sample-FFE3312944 contributed by ORly (https://forum.botengine.org/t/mining-bot-i-cannot-see-the-ore-hold-capacity-gauge/3101/5?u=viir)
    , ( "0/5\u{00A0}000,0 m³", Ok { used = 0, maximum = Just 5000, selected = Nothing } )

    -- 2020-07-26 scenario shared by neolexo at https://forum.botengine.org/t/issue-with-mining/3469/3?u=viir
    , ( "0/5’000.0 m³", Ok { used = 0, maximum = Just 5000, selected = Nothing } )
    ]
        |> List.map
            (\( text, expectedResult ) ->
                Test.test text <|
                    \_ ->
                        text
                            |> EveOnline.ParseUserInterface.parseInventoryCapacityGaugeText
                            |> Expect.equal expectedResult
            )
        |> Test.describe "Inventory capacity gauge text"


parse_module_button_tooltip_shortcut : Test.Test
parse_module_button_tooltip_shortcut =
    [ ( " F1 ", [ Common.EffectOnWindow.vkey_F1 ] )
    , ( " CTRL-F3 ", [ Common.EffectOnWindow.vkey_LCONTROL, Common.EffectOnWindow.vkey_F3 ] )
    , ( " STRG-F4 ", [ Common.EffectOnWindow.vkey_LCONTROL, Common.EffectOnWindow.vkey_F4 ] )
    , ( " ALT+F4 ", [ Common.EffectOnWindow.vkey_LMENU, Common.EffectOnWindow.vkey_F4 ] )
    , ( " SHIFT - F5 ", [ Common.EffectOnWindow.vkey_LSHIFT, Common.EffectOnWindow.vkey_F5 ] )
    , ( " UMSCH-F6 ", [ Common.EffectOnWindow.vkey_LSHIFT, Common.EffectOnWindow.vkey_F6 ] )
    ]
        |> List.map
            (\( text, expectedResult ) ->
                Test.test text <|
                    \_ ->
                        text
                            |> EveOnline.ParseUserInterface.parseModuleButtonTooltipShortcut
                            |> Expect.equal (Ok expectedResult)
            )
        |> Test.describe "Parse module button tooltip shortcut"
