namespace Bolero.Html

open Bolero
open Bolero.Builders
open Bolero.Reactive
open Microsoft.FSharp.Core

[<AutoOpen>]
module HtmlReactiveExtensions =
    let rcomp (render: IReactiveComponentContext -> Node) =
        ComponentWithAttrsBuilder<ReactiveComponent>(attrs {
            "RenderFn" => render
        })
