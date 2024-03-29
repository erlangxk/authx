module JwtTests

open Xunit

open authx
open JwtToken

type User(name: string, test: bool, currency: string) =

    member this.Claims: UserClaims =
        [ JwtClaims.test, test
          JwtClaims.currency, currency
          JwtClaims.name, name ]
[<Fact>]

let testCreateToken () =

    let user = User("simon", true, "RMB")
    let issuer = "issuer"
    let expireTime: int64 = 3
    let secret = "secret"

    let authReq =
        { AuthRequest.ClientId = "clientId"
          Operator = "operator"
          Token = "token"
          Sign = "xxx" }

    let result =
        createTokenInternal expireTime issuer user.Claims secret authReq
    let exp ="eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJleHAiOjMsImlzcyI6Imlzc3VlciIsInRlc3QiOnRydWUsImN1cnJlbmN5IjoiUk1CIiwibmFtZSI6InNpbW9uIiwidG9rZW4iOiJ0b2tlbiIsIm9wZXJhdG9yIjoib3BlcmF0b3IiLCJhdWQiOiJjbGllbnRJZCJ9.kLeiQ1spE2a3AErLVKO6JETHruzMZ_vXlCUOS0dW1It7xRQpJ8Po3TydHrlP4cgwIaktLVY7kuS1BHaLvi4YEw"
    Assert.Equal(exp, result)
