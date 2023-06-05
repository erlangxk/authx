namespace authx

open System
open System.Text
open Jose

[<CLIMutable>]
type JwtConfig = { Issuer: string; Ttl: int }

type Claim = string * obj
type UserClaims = seq<Claim>

module JwtClaims =
    let issuer = "iss"
    let subject = "sub"
    let audience = "aud"
    let expirationTime = "exp"
    let operator = "operator"
    let token = "token"
    let name = "name"
    let test = "test"
    let currency = "currency"

module JwtToken =

    let configClaims (issuer: string) (expireTime: int64) : UserClaims =
        [ JwtClaims.expirationTime, expireTime; JwtClaims.issuer, issuer ]

    let authReqClaims (authReq: AuthRequest) : UserClaims =
        [ JwtClaims.token, authReq.Token
          JwtClaims.operator, authReq.Operator
          JwtClaims.audience, authReq.ClientId ]

    let createTokenInternal
        (expireTime: int64)
        (issuer: string)
        (userClaims: UserClaims)
        (clientSecret: string)
        (authReq: AuthRequest)
        : string =
        let claims =
            seq {
                yield! configClaims issuer expireTime
                yield! userClaims
                yield! authReqClaims authReq
            }
            |> dict

        let secretKey = Encoding.UTF8.GetBytes(clientSecret)
        JWT.Encode(claims, secretKey, JwsAlgorithm.HS512)

    let createToken (ttl: int) =
        let expireTime = DateTimeOffset.UtcNow.AddMinutes(ttl).ToUnixTimeSeconds()
        createTokenInternal expireTime
