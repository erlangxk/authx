namespace authx

open System
open System.Security.Cryptography
open System.Text

module Core =

    type AuthRequest =
        { ClientId: string
          Operator: string
          Token: string }

    let checkSign (req: AuthRequest) (secret: string) (sign: string) : bool =
        use sha256 = SHA256.Create()

        let expected =
            $"{req.ClientId}{secret}{req.Operator}{req.Token}"
            |> Encoding.UTF8.GetBytes
            |> sha256.ComputeHash
            |> Convert.ToBase64String

        expected = sign
