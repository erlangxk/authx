namespace authx

open System.Text
open System.Security.Cryptography
open System

type AuthRequest =
    { ClientId: string
      Operator: string
      Token: string
      Sign: string }

module AuthRequest =
    let checkSum clientId operator token (secret: string) =
        $"{clientId}{secret}{operator}{token}"
        |> Encoding.UTF8.GetBytes
        |> SHA512.HashData
        |> Convert.ToBase64String

    let checkSign (this: AuthRequest) (secret: string) =
        this.Sign = checkSum this.ClientId this.Operator this.Token secret
