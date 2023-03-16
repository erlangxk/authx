module JwtTests

open Xunit

open authx
open authx.Core
open authx.Domain

[<Fact>]

let testCreateToken () =

    let user = MyJwtToken.User("simon", true, "RMB")
    let subject ="subject"
    let issuer ="issuer"
    let expireTime:int64 = 3
    let secret = "secret"

    let authReq =
        { AuthRequest.ClientId = "clientId"
          Operator = "operator"
          Token = "token" }
        
        
    let r = MyJwtToken.createToken user (subject,issuer,30) secret authReq
    printfn $"{r}"

    let result = MyJwtToken.createTokenInternal "jwtId" user subject issuer expireTime secret authReq
    let expected = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzUxMiJ9.eyJleHAiOjMsInN1YiI6InN1YmplY3QiLCJpc3MiOiJpc3N1ZXIiLCJ0ZXN0Ijp0cnVlLCJjdXJyZW5jeSI6IlJNQiIsIm5hbWUiOiJzaW1vbiIsInRva2VuIjoidG9rZW4iLCJvcGVyYXRvciI6Im9wZXJhdG9yIiwiYXVkIjoiY2xpZW50SWQiLCJqdGkiOiJqd3RJZCJ9.zuDdG3oKGieUHCy7uYvaixut2IMfWMLu8J6nfn6lienFNvxdLbv_E76q78_QFssFS64KPqRl4UfoUhX1g_VQWQ"
    Assert.Equal(expected, result)
