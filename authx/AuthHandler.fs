namespace authx

open System.Text
open System.Security.Cryptography
open System.Threading.Tasks
open System
open Thoth.Json.Net

type AuthRequest =
    { ClientId: string
      Operator: string
      Token: string
      Sign: string }
    
[<RequireQualifiedAccess>]
type AuthResponse =
    | Success of token:string
    | Failure of reason:string
    | InvalidSign of sign:string
    | OperatorNotFound of operator:string
    | ClientNotFound of client:string
    | UnknownError of exn
    
type AuthHandler =
    abstract member GetUserInfo : AuthRequest -> Task<AuthResponse>

module AuthHandler =
    let checkSum clientId operator token (secret: string) =
        $"{clientId}{secret}{operator}{token}"
        |> Encoding.UTF8.GetBytes
        |> SHA512.HashData
        |> Convert.ToBase64String

    let checkSign (this: AuthRequest) (secret: string) =
        this.Sign = checkSum this.ClientId this.Operator this.Token secret

    let private ERR_FAILED_AUTH = 1000
    let private ERR_CLIENT_NOT_FOUND = 1001
    let private ERR_INVALID_SIGN = 1002
    let private ERR_OPERATOR_NOT_FOUND = 1003
    let private ERR_UNKNOWN_EXN = 2000

    let inline private errorJson (code: int, reason: string) =
        Encode.object [ "code", Encode.int code; "reason", Encode.string reason ]
        |> Encode.toString 0

    let toJson (res: AuthResponse) =
        match res with
        | AuthResponse.Success(token) -> Encode.object [ "token", Encode.string token ] |> Encode.toString 0
        | AuthResponse.Failure(reason) -> errorJson (ERR_FAILED_AUTH, $"Failed auth:{reason}")
        | AuthResponse.InvalidSign(sign) -> errorJson (ERR_INVALID_SIGN, $"Invalid sign:{sign}")
        | AuthResponse.OperatorNotFound(operator) ->
            errorJson (ERR_OPERATOR_NOT_FOUND, $"Operator:{operator} not found")
        | AuthResponse.ClientNotFound(clientId) -> errorJson (ERR_CLIENT_NOT_FOUND, $"Client:{clientId} not found")
        | AuthResponse.UnknownError(ex) -> errorJson (ERR_UNKNOWN_EXN, ex.Message)