namespace Bolero.Reactive

open System
open Bolero
open Bolero.Html
open Bolero.Reactive
open Microsoft.AspNetCore.Components

type ReactiveComponent() as this =
    inherit Component()

    let context = new ReactiveComponentContext()

    interface IDisposable with
        member this.Dispose() = (context :> IDisposable).Dispose()

    [<Parameter>]
    member val RenderFn: IReactiveComponentContext -> Node = fun _ -> Node.Empty() with get, set
    
    override _.Render() = this.RenderFn context

    override _.OnInitialized() =
        base.OnInitialized()

        (context :> IReactiveComponentContext)
            .trackDisposable (
                context.StateChanged.Subscribe (fun _ ->
                    context.EffectQueue.Process() |> Async.Start

                    this.ScheduleRerender())
            )

    override _.OnAfterRender(firstRender: bool) =
        base.OnAfterRender(firstRender)

        context.EffectQueue.Process() |> Async.Start

    member _.StateHasChanged() = base.StateHasChanged()

    member _.ScheduleRerender() =
        this.InvokeAsync(this.StateHasChanged) |> ignore
