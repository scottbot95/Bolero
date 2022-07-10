module Bolero.Reactive.Utils

open System
open System.Collections.Generic

[<RequireQualifiedAccess>]
module internal Map =
    let ofDict(items: IDictionary<'key, 'value>): Map<'key, 'value> =
        items
        |> Seq.map (|KeyValue|)
        |> Map.ofSeq
    
type internal DisposableBag () =
    let items = ResizeArray<IDisposable>()
    member this.Add (item: IDisposable) =
        if item <> null then
            items.Add item

    interface IDisposable with
        member this.Dispose () =
            for item in items do
                if item <> null then
                    item.Dispose ()