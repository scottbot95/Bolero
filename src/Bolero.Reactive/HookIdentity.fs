namespace Bolero.Reactive

open System.IO

type HookIdentity =
    | CallerCodeLocation of  lineNum: int
    | StringLocation of string
    
    override this.ToString() =
        match this with
        | CallerCodeLocation(lineNum) -> $"{lineNum}"
        | StringLocation s -> s
