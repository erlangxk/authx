module authx.AuthRequest

open System.Text
open System.Security.Cryptography
open System

let checkSum (clientId) (operator) (token) (secret: string) =
    $"{clientId}{secret}{operator}{token}"
    |> Encoding.UTF8.GetBytes
    |> SHA512.HashData
    |> Convert.ToBase64String

type AuthRequest =
    { ClientId: string
      Operator: string
      Token: string
      Sign: string }

    member this.CheckSign(secret: string) =
        this.Sign = checkSum (this.ClientId) (this.Operator) (this.Token) secret
