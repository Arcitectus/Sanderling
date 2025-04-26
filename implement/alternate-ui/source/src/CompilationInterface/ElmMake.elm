module CompilationInterface.ElmMake exposing (..)

{-| For documentation of the compilation interface, see <https://github.com/pine-vm/pine/blob/main/guide/how-to-configure-and-deploy-an-elm-backend-app.md#compilationinterfaceelmmake-elm-module>
-}

import Basics


elm_make____src_Frontend_Main_elm : { debug : { utf8 : String }, utf8 : String }
elm_make____src_Frontend_Main_elm =
    { utf8 = "The compiler replaces this declaration."
    , debug = { utf8 = "The compiler replaces this declaration." }
    }
