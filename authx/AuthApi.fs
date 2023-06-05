namespace authx

open System.Threading.Tasks

[<RequireQualifiedAccess>]
type AuthResult =
    | Success of UserClaims
    | Failed of string
    | UnknownError of exn

type AuthApi =
    abstract member GetUserInfo: string -> Task<AuthResult>