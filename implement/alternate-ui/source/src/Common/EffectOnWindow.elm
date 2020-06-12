module Common.EffectOnWindow exposing (..)


type VirtualKeyCode
    = VirtualKeyCodeFromInt Int
      -- Names from https://docs.microsoft.com/en-us/windows/desktop/inputdev/virtual-key-codes
    | VK_SHIFT
    | VK_CONTROL
    | VK_MENU
    | VK_ESCAPE
    | VK_SPACE
    | VK_LSHIFT
    | VK_LCONTROL
    | VK_LMENU


type MouseButton
    = MouseButtonLeft
    | MouseButtonRight


virtualKeyCodeFromMouseButton : MouseButton -> VirtualKeyCode
virtualKeyCodeFromMouseButton mouseButton =
    case mouseButton of
        MouseButtonLeft ->
            VirtualKeyCodeFromInt 1

        MouseButtonRight ->
            VirtualKeyCodeFromInt 1


virtualKeyCodeAsInteger : VirtualKeyCode -> Int
virtualKeyCodeAsInteger keyCode =
    -- Mapping from https://docs.microsoft.com/en-us/windows/desktop/inputdev/virtual-key-codes
    case keyCode of
        VirtualKeyCodeFromInt asInt ->
            asInt

        VK_SHIFT ->
            0x10

        VK_CONTROL ->
            0x11

        VK_MENU ->
            0x12

        VK_ESCAPE ->
            0x1B

        VK_SPACE ->
            0x20

        VK_LSHIFT ->
            0xA0

        VK_LCONTROL ->
            0xA2

        VK_LMENU ->
            0xA4


key_F1 : VirtualKeyCode
key_F1 =
    VirtualKeyCodeFromInt 0x70


key_F2 : VirtualKeyCode
key_F2 =
    VirtualKeyCodeFromInt 0x71


key_F3 : VirtualKeyCode
key_F3 =
    VirtualKeyCodeFromInt 0x72


key_F4 : VirtualKeyCode
key_F4 =
    VirtualKeyCodeFromInt 0x73


key_F5 : VirtualKeyCode
key_F5 =
    VirtualKeyCodeFromInt 0x74


key_F6 : VirtualKeyCode
key_F6 =
    VirtualKeyCodeFromInt 0x75


key_F7 : VirtualKeyCode
key_F7 =
    VirtualKeyCodeFromInt 0x76


key_F8 : VirtualKeyCode
key_F8 =
    VirtualKeyCodeFromInt 0x77


key_F9 : VirtualKeyCode
key_F9 =
    VirtualKeyCodeFromInt 0x78


key_F10 : VirtualKeyCode
key_F10 =
    VirtualKeyCodeFromInt 0x79


key_F11 : VirtualKeyCode
key_F11 =
    VirtualKeyCodeFromInt 0x7A


key_F12 : VirtualKeyCode
key_F12 =
    VirtualKeyCodeFromInt 0x7B
