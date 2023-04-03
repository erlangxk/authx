module authx.AuthRequest

open System.Text
open System.Security.Cryptography
open System

type AuthRequest =
    { ClientId: string
      Operator: string
      Token: string }

let inline sha256Hash (bytes: byte[]) =
    use sha256 = SHA256.Create()
    sha256.ComputeHash bytes

let checkSum (req: AuthRequest) secret =
    $"{req.ClientId}{secret}{req.Operator}{req.Token}"
    |> Encoding.UTF8.GetBytes
    |> sha256Hash
    |> Convert.ToBase64String

let checkSign (req: AuthRequest) (secret: string) (sign: string) : bool =
    sign = checkSum req secret
