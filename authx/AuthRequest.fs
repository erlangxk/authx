module authx.AuthRequest

open System.Text
open System.Security.Cryptography
open System

type AuthRequest =
    { ClientId: string
      Operator: string
      Token: string }

    member this.CheckSum secret =
        $"{this.ClientId}{secret}{this.Operator}{this.Token}"

let hash (req: AuthRequest) (secret: string) =
    req.CheckSum(secret)
    |> Encoding.UTF8.GetBytes
    |> SHA512.HashData
    |> Convert.ToBase64String

let checkSign (req: AuthRequest) (secret: string) (sign: string) : bool = hash req secret = sign
