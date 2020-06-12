module ElmFullstackCompilerInterface.ElmMakeFrontendWeb exposing (..)

import Bytes
import Bytes.Encode

elm_make_frontendWeb_html : Bytes.Bytes
elm_make_frontendWeb_html =
    "The Elm-fullstack compiler replaces this function."
        |> Bytes.Encode.string
        |> Bytes.Encode.encode

elm_make_frontendWeb_html_debug : Bytes.Bytes
elm_make_frontendWeb_html_debug =
    "The Elm-fullstack compiler replaces this function."
        |> Bytes.Encode.string
        |> Bytes.Encode.encode
