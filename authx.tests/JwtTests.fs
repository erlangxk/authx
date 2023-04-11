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
        
    let exp ="eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJleHAiOjMsImlzcyI6Imlzc3VlciIsInRlc3QiOnRydWUsImN1cnJlbmN5IjoiUk1CIiwibmFtZSI6InNpbW9uIiwidG9rZW4iOiJ0b2tlbiIsIm9wZXJhdG9yIjoib3BlcmF0b3IiLCJhdWQiOiJjbGllbnRJZCIsImp0aSI6Imp3dElkIn0.1o_ZOJmAF20rpHtVqm1z0REyr8SYuSMLgspAk6TgyYslvygzdNdm0y7ZGasg_GcihIpRaIm0O0zV_rw2K-J_8g"
    Assert.Equal(exp, result)
