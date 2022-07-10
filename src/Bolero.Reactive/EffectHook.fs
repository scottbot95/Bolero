namespace Bolero.Reactive

open System
open Bolero.Reactive
open Bolero.Reactive.Utils

type EffectTrigger =
    | AfterChange of state: IObservable
    | AfterInit
    | AfterRender

type EffectHook =
    {
        Identity: HookIdentity
        Handler: unit -> IDisposable
        Triggers: EffectTrigger list
    }

    static member Create(identity, effect, triggers) =
        {
            Identity = identity
            Handler = effect
            Triggers = triggers
        }

type internal EffectQueue() =
    let sync = obj ()
    let disposables = new DisposableBag()

    let mutable queue: EffectHook list =
        List.empty

    member _.Enqueue(effect: EffectHook) =
        lock sync (fun _ -> queue <- effect :: queue)

    member _.Process() =
        if List.isEmpty queue then
            async { () }
        else
            async {
                let mutable detached = []

                lock sync (fun _ ->
                    detached <- queue
                    queue <- List.empty)

                let detached' =
                    detached
                    |> List.distinctBy (fun effect -> effect.Identity)

                for effect in detached' do
                    disposables.Add(effect.Handler())
            }

    interface IDisposable with
        member _.Dispose() = (disposables :> IDisposable).Dispose()
