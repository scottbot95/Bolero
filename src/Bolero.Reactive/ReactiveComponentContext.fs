namespace Bolero.Reactive

open System
open System.Collections.Generic
open Bolero.Reactive
open Bolero.Reactive.Utils

type IReactiveComponentContext =
    inherit IDisposable

    /// Schedule a render
    abstract member forceRender: unit -> unit

    /// Adds a disposable to be disposed upon disposal of this context
    abstract member trackDisposable: IDisposable -> unit

    abstract member useStateHook: StateHook<'Value> -> ReactiveValue<'Value>

    abstract member useEffectHook: EffectHook -> unit


type ReactiveComponentContext() =
    let disposables = new DisposableBag()

    // TODO can we use StateHook<?> as the value type here instead of boxing?
    let states = Dictionary<HookIdentity, obj>()

    let effects =
        Dictionary<HookIdentity, EffectHook>()

    let effectQueue = new EffectQueue()
    do disposables.Add effectQueue

    let stateChanged = Event<unit>()

    member _.EffectQueue
        with internal get () = effectQueue

    member _.StateChanged
        with internal get () = stateChanged.Publish

    member _.Hooks
        with internal get () = Map.ofDict states

    interface IReactiveComponentContext with
        member _.forceRender() = stateChanged.Trigger()
        member this.trackDisposable(disposable) = disposables.Add disposable

        member this.useStateHook<'Value>(stateHook: StateHook<'Value>) =
            match states.TryGetValue stateHook.Identity with
            | true, known ->
                let hook = unbox known
                hook.State
            | false, _ ->
                let state = stateHook.State

                states.Add(stateHook.Identity, box stateHook)
                disposables.Add state

                if stateHook.RenderOnChange then
                    disposables.Add(state.Subscribe(fun _ -> (this :> IReactiveComponentContext).forceRender ()))

                state

        member this.useEffectHook(effect: EffectHook) =
            match effects.TryGetValue effect.Identity with
            | true, _ ->
                for trigger in effect.Triggers do
                    match trigger with
                    | AfterRender -> effectQueue.Enqueue effect
                    | _ -> ()
            | false, _ ->
                effects.Add(effect.Identity, effect)

                for trigger in effect.Triggers do
                    match trigger with
                    | AfterChange dep ->
                        (fun _ -> effectQueue.Enqueue effect)
                        |> dep.Subscribe
                        |> disposables.Add
                    | AfterRender -> effectQueue.Enqueue effect
                    | AfterInit -> effectQueue.Enqueue effect


    interface IDisposable with
        member this.Dispose() = (disposables :> IDisposable).Dispose()
