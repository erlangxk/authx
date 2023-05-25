module authx.FunPlay

open System
open MyOperator
open MyJwtToken
open System.Threading.Tasks

let funPlayClaims (token: string) : UserClaims =
    [ MyJwtClaims.test, true
      MyJwtClaims.currency, "USD"
      MyJwtClaims.subject, Guid.NewGuid().ToString()
      MyJwtClaims.name, token ]

let authApi =
    { new AuthApi with
        member this.GetUserInfo(token: string) =
            funPlayClaims token |> AuthResult.Success |> Task.FromResult }
