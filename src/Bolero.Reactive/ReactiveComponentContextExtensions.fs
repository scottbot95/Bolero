namespace Bolero.Reactive

open System
open System.Runtime.CompilerServices
open Bolero.Reactive

[<AutoOpen>]
module ReactiveComponentContextExtensions =
    type IReactiveComponentContext with

        member this.useState<'Value>
            (
                initialValue: 'Value,
                ?renderOnChange: bool,
                [<CallerLineNumber>] ?lineNum: int
            ) : ReactiveValue<'Value> =
            let state =
                this.useStateHook<'Value> (
                    StateHook<_>.Create
                        (identity = HookIdentity.CallerCodeLocation lineNum.Value,
                         state = new ReactiveValue<'Value>(initialValue),
                         renderOnChange = defaultArg renderOnChange true)
                )

            state

        member this.useEffect
            (
                handler: unit -> IDisposable,
                ?triggers: EffectTrigger list,
                [<CallerLineNumber>] ?callerLineNumber: int
            ) =
            this.useEffectHook (
                EffectHook.Create(CallerCodeLocation callerLineNumber.Value, handler, defaultArg triggers [ AfterInit ])
            )

        member this.useEffect
            (
                handler: unit -> unit,
                ?triggers: EffectTrigger list,
                [<CallerLineNumber>] ?callerLineNumber: int
            ) =
            this.useEffectHook (
                EffectHook.Create(
                    CallerCodeLocation callerLineNumber.Value,
                    (fun _ ->
                        handler ()
                        null),
                    defaultArg triggers [ AfterInit ]
                )
            )
