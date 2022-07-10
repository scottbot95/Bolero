namespace Bolero.Reactive

open System
open Microsoft.FSharp.Core

type IObservable =
    abstract member Subscribe: handler: IObserver<obj> -> IDisposable

[<AutoOpen>]
module IObservableExtensions =
    type IObservable with
        member this.Subscribe callback =
            this.Subscribe
                { new IObserver<obj> with
                    member x.OnNext args = callback args
                    member x.OnError e = ()
                    member x.OnCompleted() = ()
                }


type ReactiveValue<'T>(initialValue: 'T) =
    let mutable observers: IObserver<'T> list =
        List.empty

    let mutable genericObservers: IObserver<obj> list =
        List.empty

    let mutable _value: 'T = initialValue

    member this.Value
        with get () = _value
        and private set value = _value <- value

    member this.Set(newValue: 'T) =
        _value <- newValue

        for observer in observers do
            observer.OnNext newValue

        for observer in genericObservers do
            observer.OnNext newValue

    interface IObservable<'T> with
        member this.Subscribe observer =
            if not (List.contains observer observers) then
                observers <- observer :: observers

            { new IDisposable with
                member _.Dispose() =
                    observers <- List.except (seq { observer }) observers
            }

    interface IObservable with
        member this.Subscribe observer =
            if not (List.contains observer genericObservers) then
                genericObservers <- observer :: genericObservers

            { new IDisposable with
                member _.Dispose() =
                    genericObservers <- List.except (seq { observer }) genericObservers
            }


    interface IDisposable with
        member this.Dispose() = () // TODO do we actually need to do anything here?
