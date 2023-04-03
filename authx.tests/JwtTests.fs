module JwtTests

open Xunit

open authx
open authx.AuthRequest

open MyJwtToken

type User(name: string, test: bool, currency: string) =

    member this.Claims: MyJwtToken.UserClaims =
        [ MyJwtClaims.test, test
          MyJwtClaims.currency, currency
          MyJwtClaims.name, name ]

[<Fact>]

let testCreateToken () =

    let user = User("simon", true, "RMB")
    let issuer = "issuer"
    let expireTime: int64 = 3
    let secret = "secret"

    let authReq =
        { AuthRequest.ClientId = "clientId"
          Operator = "operator"
          Token = "token" }

    let result =
        createTokenInternal "jwtId" user.Claims issuer expireTime secret authReq

    let exp =
        "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzUxMiJ9.eyJleHAiOjMsImlzcyI6Imlzc3VlciIsInRlc3QiOnRydWUsImN1cnJlbmN5IjoiUk1CIiwibmFtZSI6InNpbW9uIiwidG9rZW4iOiJ0b2tlbiIsIm9wZXJhdG9yIjoib3BlcmF0b3IiLCJhdWQiOiJjbGllbnRJZCIsImp0aSI6Imp3dElkIn0.OGyx4vBvwJqCVnDklG_GavObX2g0lwIHhyYnPrLu3pTlRfF4JA11Q9uxiimp1qZwueRjLwkbIs7uLou0R31Zrg"

    Assert.Equal(exp, result)
