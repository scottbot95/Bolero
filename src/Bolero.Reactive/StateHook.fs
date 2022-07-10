namespace Bolero.Reactive

open Bolero.Reactive

[<Struct>]
type StateHook<'T> =
    {
        Identity: HookIdentity
        State: ReactiveValue<'T>
        RenderOnChange: bool
    }
    static member Create(identity, state, renderOnChange) =
        {
            Identity = identity
            State = state
            RenderOnChange = renderOnChange
        }
