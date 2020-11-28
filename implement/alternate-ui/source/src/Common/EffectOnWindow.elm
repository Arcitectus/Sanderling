module Common.EffectOnWindow exposing (..)

{-| Names from <https://docs.microsoft.com/en-us/windows/desktop/inputdev/virtual-key-codes>
-}


type EffectOnWindowStructure
    = MouseMoveTo Location2d
    | KeyDown VirtualKeyCode
    | KeyUp VirtualKeyCode


type alias MouseClickAtLocation =
    { location : Location2d
    , mouseButton : MouseButton
    }


type alias Location2d =
    { x : Int, y : Int }


type VirtualKeyCode
    = VirtualKeyCodeFromInt Int


type MouseButton
    = MouseButtonLeft
    | MouseButtonRight


effectsMouseClickAtLocation : MouseButton -> Location2d -> List EffectOnWindowStructure
effectsMouseClickAtLocation mouseButton location =
    [ MouseMoveTo location
    , KeyDown (virtualKeyCodeFromMouseButton mouseButton)
    , KeyUp (virtualKeyCodeFromMouseButton mouseButton)
    ]


effectsForDragAndDrop : { startLocation : Location2d, mouseButton : MouseButton, endLocation : Location2d } -> List EffectOnWindowStructure
effectsForDragAndDrop { startLocation, mouseButton, endLocation } =
    [ MouseMoveTo startLocation
    , KeyDown (virtualKeyCodeFromMouseButton mouseButton)
    , MouseMoveTo endLocation
    , KeyUp (virtualKeyCodeFromMouseButton mouseButton)
    ]


virtualKeyCodeFromMouseButton : MouseButton -> VirtualKeyCode
virtualKeyCodeFromMouseButton mouseButton =
    case mouseButton of
        MouseButtonLeft ->
            vkey_LBUTTON

        MouseButtonRight ->
            vkey_RBUTTON


virtualKeyCodeAsInteger : VirtualKeyCode -> Int
virtualKeyCodeAsInteger keyCode =
    case keyCode of
        VirtualKeyCodeFromInt asInt ->
            asInt


vkey_LBUTTON : VirtualKeyCode
vkey_LBUTTON =
    VirtualKeyCodeFromInt 0x01


vkey_RBUTTON : VirtualKeyCode
vkey_RBUTTON =
    VirtualKeyCodeFromInt 0x02


vkey_CANCEL : VirtualKeyCode
vkey_CANCEL =
    VirtualKeyCodeFromInt 0x03


vkey_MBUTTON : VirtualKeyCode
vkey_MBUTTON =
    VirtualKeyCodeFromInt 0x04


vkey_XBUTTON1 : VirtualKeyCode
vkey_XBUTTON1 =
    VirtualKeyCodeFromInt 0x05


vkey_XBUTTON2 : VirtualKeyCode
vkey_XBUTTON2 =
    VirtualKeyCodeFromInt 0x06


vkey_BACK : VirtualKeyCode
vkey_BACK =
    VirtualKeyCodeFromInt 0x08


vkey_TAB : VirtualKeyCode
vkey_TAB =
    VirtualKeyCodeFromInt 0x09


vkey_CLEAR : VirtualKeyCode
vkey_CLEAR =
    VirtualKeyCodeFromInt 0x0C


vkey_RETURN : VirtualKeyCode
vkey_RETURN =
    VirtualKeyCodeFromInt 0x0D


vkey_SHIFT : VirtualKeyCode
vkey_SHIFT =
    VirtualKeyCodeFromInt 0x10


vkey_CONTROL : VirtualKeyCode
vkey_CONTROL =
    VirtualKeyCodeFromInt 0x11


vkey_MENU : VirtualKeyCode
vkey_MENU =
    VirtualKeyCodeFromInt 0x12


vkey_PAUSE : VirtualKeyCode
vkey_PAUSE =
    VirtualKeyCodeFromInt 0x13


vkey_CAPITAL : VirtualKeyCode
vkey_CAPITAL =
    VirtualKeyCodeFromInt 0x14


vkey_KANA : VirtualKeyCode
vkey_KANA =
    VirtualKeyCodeFromInt 0x15


vkey_HANGUEL : VirtualKeyCode
vkey_HANGUEL =
    VirtualKeyCodeFromInt 0x15


vkey_IME_ON : VirtualKeyCode
vkey_IME_ON =
    VirtualKeyCodeFromInt 0x16


vkey_JUNJA : VirtualKeyCode
vkey_JUNJA =
    VirtualKeyCodeFromInt 0x17


vkey_FINAL : VirtualKeyCode
vkey_FINAL =
    VirtualKeyCodeFromInt 0x18


vkey_HANJA : VirtualKeyCode
vkey_HANJA =
    VirtualKeyCodeFromInt 0x19


vkey_KANJI : VirtualKeyCode
vkey_KANJI =
    VirtualKeyCodeFromInt 0x19


vkey_IME_OFF : VirtualKeyCode
vkey_IME_OFF =
    VirtualKeyCodeFromInt 0x1A


vkey_ESCAPE : VirtualKeyCode
vkey_ESCAPE =
    VirtualKeyCodeFromInt 0x1B


vkey_CONVERT : VirtualKeyCode
vkey_CONVERT =
    VirtualKeyCodeFromInt 0x1C


vkey_NONCONVERT : VirtualKeyCode
vkey_NONCONVERT =
    VirtualKeyCodeFromInt 0x1D


vkey_ACCEPT : VirtualKeyCode
vkey_ACCEPT =
    VirtualKeyCodeFromInt 0x1E


vkey_MODECHANGE : VirtualKeyCode
vkey_MODECHANGE =
    VirtualKeyCodeFromInt 0x1F


vkey_SPACE : VirtualKeyCode
vkey_SPACE =
    VirtualKeyCodeFromInt 0x20


vkey_PRIOR : VirtualKeyCode
vkey_PRIOR =
    VirtualKeyCodeFromInt 0x21


vkey_NEXT : VirtualKeyCode
vkey_NEXT =
    VirtualKeyCodeFromInt 0x22


vkey_END : VirtualKeyCode
vkey_END =
    VirtualKeyCodeFromInt 0x23


vkey_HOME : VirtualKeyCode
vkey_HOME =
    VirtualKeyCodeFromInt 0x24


vkey_LEFT : VirtualKeyCode
vkey_LEFT =
    VirtualKeyCodeFromInt 0x25


vkey_UP : VirtualKeyCode
vkey_UP =
    VirtualKeyCodeFromInt 0x26


vkey_RIGHT : VirtualKeyCode
vkey_RIGHT =
    VirtualKeyCodeFromInt 0x27


vkey_DOWN : VirtualKeyCode
vkey_DOWN =
    VirtualKeyCodeFromInt 0x28


vkey_SELECT : VirtualKeyCode
vkey_SELECT =
    VirtualKeyCodeFromInt 0x29


vkey_PRINT : VirtualKeyCode
vkey_PRINT =
    VirtualKeyCodeFromInt 0x2A


vkey_EXECUTE : VirtualKeyCode
vkey_EXECUTE =
    VirtualKeyCodeFromInt 0x2B


vkey_SNAPSHOT : VirtualKeyCode
vkey_SNAPSHOT =
    VirtualKeyCodeFromInt 0x2C


vkey_INSERT : VirtualKeyCode
vkey_INSERT =
    VirtualKeyCodeFromInt 0x2D


vkey_DELETE : VirtualKeyCode
vkey_DELETE =
    VirtualKeyCodeFromInt 0x2E


vkey_HELP : VirtualKeyCode
vkey_HELP =
    VirtualKeyCodeFromInt 0x2F


vkey_0 : VirtualKeyCode
vkey_0 =
    VirtualKeyCodeFromInt 0x30


vkey_1 : VirtualKeyCode
vkey_1 =
    VirtualKeyCodeFromInt 0x31


vkey_2 : VirtualKeyCode
vkey_2 =
    VirtualKeyCodeFromInt 0x32


vkey_3 : VirtualKeyCode
vkey_3 =
    VirtualKeyCodeFromInt 0x33


vkey_4 : VirtualKeyCode
vkey_4 =
    VirtualKeyCodeFromInt 0x34


vkey_5 : VirtualKeyCode
vkey_5 =
    VirtualKeyCodeFromInt 0x35


vkey_6 : VirtualKeyCode
vkey_6 =
    VirtualKeyCodeFromInt 0x36


vkey_7 : VirtualKeyCode
vkey_7 =
    VirtualKeyCodeFromInt 0x37


vkey_8 : VirtualKeyCode
vkey_8 =
    VirtualKeyCodeFromInt 0x38


vkey_9 : VirtualKeyCode
vkey_9 =
    VirtualKeyCodeFromInt 0x39


vkey_A : VirtualKeyCode
vkey_A =
    VirtualKeyCodeFromInt 0x41


vkey_B : VirtualKeyCode
vkey_B =
    VirtualKeyCodeFromInt 0x42


vkey_C : VirtualKeyCode
vkey_C =
    VirtualKeyCodeFromInt 0x43


vkey_D : VirtualKeyCode
vkey_D =
    VirtualKeyCodeFromInt 0x44


vkey_E : VirtualKeyCode
vkey_E =
    VirtualKeyCodeFromInt 0x45


vkey_F : VirtualKeyCode
vkey_F =
    VirtualKeyCodeFromInt 0x46


vkey_G : VirtualKeyCode
vkey_G =
    VirtualKeyCodeFromInt 0x47


vkey_H : VirtualKeyCode
vkey_H =
    VirtualKeyCodeFromInt 0x48


vkey_I : VirtualKeyCode
vkey_I =
    VirtualKeyCodeFromInt 0x49


vkey_J : VirtualKeyCode
vkey_J =
    VirtualKeyCodeFromInt 0x4A


vkey_K : VirtualKeyCode
vkey_K =
    VirtualKeyCodeFromInt 0x4B


vkey_L : VirtualKeyCode
vkey_L =
    VirtualKeyCodeFromInt 0x4C


vkey_M : VirtualKeyCode
vkey_M =
    VirtualKeyCodeFromInt 0x4D


vkey_N : VirtualKeyCode
vkey_N =
    VirtualKeyCodeFromInt 0x4E


vkey_O : VirtualKeyCode
vkey_O =
    VirtualKeyCodeFromInt 0x4F


vkey_P : VirtualKeyCode
vkey_P =
    VirtualKeyCodeFromInt 0x50


vkey_Q : VirtualKeyCode
vkey_Q =
    VirtualKeyCodeFromInt 0x51


vkey_R : VirtualKeyCode
vkey_R =
    VirtualKeyCodeFromInt 0x52


vkey_S : VirtualKeyCode
vkey_S =
    VirtualKeyCodeFromInt 0x53


vkey_T : VirtualKeyCode
vkey_T =
    VirtualKeyCodeFromInt 0x54


vkey_U : VirtualKeyCode
vkey_U =
    VirtualKeyCodeFromInt 0x55


vkey_V : VirtualKeyCode
vkey_V =
    VirtualKeyCodeFromInt 0x56


vkey_W : VirtualKeyCode
vkey_W =
    VirtualKeyCodeFromInt 0x57


vkey_X : VirtualKeyCode
vkey_X =
    VirtualKeyCodeFromInt 0x58


vkey_Y : VirtualKeyCode
vkey_Y =
    VirtualKeyCodeFromInt 0x59


vkey_Z : VirtualKeyCode
vkey_Z =
    VirtualKeyCodeFromInt 0x5A


vkey_LWIN : VirtualKeyCode
vkey_LWIN =
    VirtualKeyCodeFromInt 0x5B


vkey_RWIN : VirtualKeyCode
vkey_RWIN =
    VirtualKeyCodeFromInt 0x5C


vkey_APPS : VirtualKeyCode
vkey_APPS =
    VirtualKeyCodeFromInt 0x5D


vkey_SLEEP : VirtualKeyCode
vkey_SLEEP =
    VirtualKeyCodeFromInt 0x5F


vkey_NUMPAD0 : VirtualKeyCode
vkey_NUMPAD0 =
    VirtualKeyCodeFromInt 0x60


vkey_NUMPAD1 : VirtualKeyCode
vkey_NUMPAD1 =
    VirtualKeyCodeFromInt 0x61


vkey_NUMPAD2 : VirtualKeyCode
vkey_NUMPAD2 =
    VirtualKeyCodeFromInt 0x62


vkey_NUMPAD3 : VirtualKeyCode
vkey_NUMPAD3 =
    VirtualKeyCodeFromInt 0x63


vkey_NUMPAD4 : VirtualKeyCode
vkey_NUMPAD4 =
    VirtualKeyCodeFromInt 0x64


vkey_NUMPAD5 : VirtualKeyCode
vkey_NUMPAD5 =
    VirtualKeyCodeFromInt 0x65


vkey_NUMPAD6 : VirtualKeyCode
vkey_NUMPAD6 =
    VirtualKeyCodeFromInt 0x66


vkey_NUMPAD7 : VirtualKeyCode
vkey_NUMPAD7 =
    VirtualKeyCodeFromInt 0x67


vkey_NUMPAD8 : VirtualKeyCode
vkey_NUMPAD8 =
    VirtualKeyCodeFromInt 0x68


vkey_NUMPAD9 : VirtualKeyCode
vkey_NUMPAD9 =
    VirtualKeyCodeFromInt 0x69


vkey_MULTIPLY : VirtualKeyCode
vkey_MULTIPLY =
    VirtualKeyCodeFromInt 0x6A


vkey_ADD : VirtualKeyCode
vkey_ADD =
    VirtualKeyCodeFromInt 0x6B


vkey_SEPARATOR : VirtualKeyCode
vkey_SEPARATOR =
    VirtualKeyCodeFromInt 0x6C


vkey_SUBTRACT : VirtualKeyCode
vkey_SUBTRACT =
    VirtualKeyCodeFromInt 0x6D


vkey_DECIMAL : VirtualKeyCode
vkey_DECIMAL =
    VirtualKeyCodeFromInt 0x6E


vkey_DIVIDE : VirtualKeyCode
vkey_DIVIDE =
    VirtualKeyCodeFromInt 0x6F


vkey_F1 : VirtualKeyCode
vkey_F1 =
    VirtualKeyCodeFromInt 0x70


vkey_F2 : VirtualKeyCode
vkey_F2 =
    VirtualKeyCodeFromInt 0x71


vkey_F3 : VirtualKeyCode
vkey_F3 =
    VirtualKeyCodeFromInt 0x72


vkey_F4 : VirtualKeyCode
vkey_F4 =
    VirtualKeyCodeFromInt 0x73


vkey_F5 : VirtualKeyCode
vkey_F5 =
    VirtualKeyCodeFromInt 0x74


vkey_F6 : VirtualKeyCode
vkey_F6 =
    VirtualKeyCodeFromInt 0x75


vkey_F7 : VirtualKeyCode
vkey_F7 =
    VirtualKeyCodeFromInt 0x76


vkey_F8 : VirtualKeyCode
vkey_F8 =
    VirtualKeyCodeFromInt 0x77


vkey_F9 : VirtualKeyCode
vkey_F9 =
    VirtualKeyCodeFromInt 0x78


vkey_F10 : VirtualKeyCode
vkey_F10 =
    VirtualKeyCodeFromInt 0x79


vkey_F11 : VirtualKeyCode
vkey_F11 =
    VirtualKeyCodeFromInt 0x7A


vkey_F12 : VirtualKeyCode
vkey_F12 =
    VirtualKeyCodeFromInt 0x7B


vkey_F13 : VirtualKeyCode
vkey_F13 =
    VirtualKeyCodeFromInt 0x7C


vkey_F14 : VirtualKeyCode
vkey_F14 =
    VirtualKeyCodeFromInt 0x7D


vkey_F15 : VirtualKeyCode
vkey_F15 =
    VirtualKeyCodeFromInt 0x7E


vkey_F16 : VirtualKeyCode
vkey_F16 =
    VirtualKeyCodeFromInt 0x7F


vkey_F17 : VirtualKeyCode
vkey_F17 =
    VirtualKeyCodeFromInt 0x80


vkey_F18 : VirtualKeyCode
vkey_F18 =
    VirtualKeyCodeFromInt 0x81


vkey_F19 : VirtualKeyCode
vkey_F19 =
    VirtualKeyCodeFromInt 0x82


vkey_F20 : VirtualKeyCode
vkey_F20 =
    VirtualKeyCodeFromInt 0x83


vkey_F21 : VirtualKeyCode
vkey_F21 =
    VirtualKeyCodeFromInt 0x84


vkey_F22 : VirtualKeyCode
vkey_F22 =
    VirtualKeyCodeFromInt 0x85


vkey_F23 : VirtualKeyCode
vkey_F23 =
    VirtualKeyCodeFromInt 0x86


vkey_F24 : VirtualKeyCode
vkey_F24 =
    VirtualKeyCodeFromInt 0x87


vkey_NUMLOCK : VirtualKeyCode
vkey_NUMLOCK =
    VirtualKeyCodeFromInt 0x90


vkey_SCROLL : VirtualKeyCode
vkey_SCROLL =
    VirtualKeyCodeFromInt 0x91


vkey_LSHIFT : VirtualKeyCode
vkey_LSHIFT =
    VirtualKeyCodeFromInt 0xA0


vkey_RSHIFT : VirtualKeyCode
vkey_RSHIFT =
    VirtualKeyCodeFromInt 0xA1


vkey_LCONTROL : VirtualKeyCode
vkey_LCONTROL =
    VirtualKeyCodeFromInt 0xA2


vkey_RCONTROL : VirtualKeyCode
vkey_RCONTROL =
    VirtualKeyCodeFromInt 0xA3


vkey_LMENU : VirtualKeyCode
vkey_LMENU =
    VirtualKeyCodeFromInt 0xA4


vkey_RMENU : VirtualKeyCode
vkey_RMENU =
    VirtualKeyCodeFromInt 0xA5


vkey_BROWSER_BACK : VirtualKeyCode
vkey_BROWSER_BACK =
    VirtualKeyCodeFromInt 0xA6


vkey_BROWSER_FORWARD : VirtualKeyCode
vkey_BROWSER_FORWARD =
    VirtualKeyCodeFromInt 0xA7


vkey_BROWSER_REFRESH : VirtualKeyCode
vkey_BROWSER_REFRESH =
    VirtualKeyCodeFromInt 0xA8


vkey_BROWSER_STOP : VirtualKeyCode
vkey_BROWSER_STOP =
    VirtualKeyCodeFromInt 0xA9


vkey_BROWSER_SEARCH : VirtualKeyCode
vkey_BROWSER_SEARCH =
    VirtualKeyCodeFromInt 0xAA


vkey_BROWSER_FAVORITES : VirtualKeyCode
vkey_BROWSER_FAVORITES =
    VirtualKeyCodeFromInt 0xAB


vkey_BROWSER_HOME : VirtualKeyCode
vkey_BROWSER_HOME =
    VirtualKeyCodeFromInt 0xAC


vkey_VOLUME_MUTE : VirtualKeyCode
vkey_VOLUME_MUTE =
    VirtualKeyCodeFromInt 0xAD


vkey_VOLUME_DOWN : VirtualKeyCode
vkey_VOLUME_DOWN =
    VirtualKeyCodeFromInt 0xAE


vkey_VOLUME_UP : VirtualKeyCode
vkey_VOLUME_UP =
    VirtualKeyCodeFromInt 0xAF
