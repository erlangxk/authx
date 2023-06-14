namespace authx

open System.Text
open System.Security.Cryptography
open System.Threading.Tasks
open System

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