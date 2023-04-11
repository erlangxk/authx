namespace authx

open System
open System.Text
open AuthRequest
open Jose

module MyJwtClaims =
    let jwtId = "jti"
    let issuer = "iss"
    let subject = "sub"
    let audience = "aud"
    let expirationTime = "exp"
    let operator = "operator"
    let token = "token"
    let name = "name"
    let test = "test"
    let currency = "currency"


module MyJwtToken =

    type Claim = string * obj
    type UserClaims = seq<Claim>

    let configClaims (issuer: string) (expireTime: int64) : UserClaims =
        [ MyJwtClaims.expirationTime, expireTime; MyJwtClaims.issuer, issuer ]

    let authReqClaims (authReq: AuthRequest) : UserClaims =
        [ MyJwtClaims.token, authReq.Token
          MyJwtClaims.operator, authReq.Operator
          MyJwtClaims.audience, authReq.ClientId ]

    let createTokenInternal
        (jwtId: string)
        (userClaims: UserClaims)
        (issuer: string)
        (expireTime: int64)
        (clientSecret: string)
        (authReq: AuthRequest)
        : string =
       
        let claims =
            seq {
                yield! configClaims issuer expireTime
                yield! userClaims
                yield! authReqClaims authReq
                yield MyJwtClaims.jwtId, jwtId
            }
            |> dict

        let secretKey = Encoding.UTF8.GetBytes(clientSecret)
        JWT.Encode(claims, secretKey, JwsAlgorithm.HS512)

       

    let createToken
        (user: UserClaims)
        (issuer: string, ttl: int)
        (clientSecret: string)
        (authReq: AuthRequest)
        : string =
        let jwtId = Guid.NewGuid().ToString()
        let expireTime = DateTimeOffset.UtcNow.AddMinutes(ttl).ToUnixTimeSeconds()
        createTokenInternal jwtId user issuer expireTime clientSecret authReq
