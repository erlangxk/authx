module authx.FunPlay

open System
open System.Threading.Tasks

let funPlayClaims (token: string) : UserClaims =
    [ JwtClaims.test, true
      JwtClaims.currency, "USD"
      JwtClaims.subject, Guid.NewGuid().ToString()
      JwtClaims.name, token ]

let authApi =
    { new AuthApi with
        member this.GetUserInfo(token: string) =
            funPlayClaims token |> AuthResult.Success |> Task.FromResult }
