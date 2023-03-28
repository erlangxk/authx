namespace authx

open System
open System.Security.Cryptography
open System.Text
open Oryx
module Core =

    type AuthRequest =
        { ClientId: string
          Operator: string
          Token: string }

        member this.signBytesWithSecret(secret: string) =
            $"{this.ClientId}{secret}{this.Operator}{this.Token}" |> Encoding.UTF8.GetBytes

    let checkSign (req: AuthRequest) (secret: string) (sign: string) : bool =
        use sha256 = SHA256.Create()

        req.signBytesWithSecret (secret)
        |> sha256.ComputeHash
        |> Convert.ToBase64String
        |> (=) sign
    
    
    //send http request to operator's auth url to get the info of the user
    //use asp.net core registered http client and oryx to build and cache the pipeline for each operator
    //might add polly for resilience
    let getUserInfo(token:string)(url:string) = 0
       
