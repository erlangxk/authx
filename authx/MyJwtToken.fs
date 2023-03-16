namespace authx

open System
open System.Collections.Generic
open System.Runtime.InteropServices.JavaScript
open System.Security.Claims
open System.Text
open JWT.Builder
open JWT.Algorithms
open JWT.Serializers
open JWT
open authx.Core

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

    type UserClaims =
        abstract Claims: list<Claim>

    type User(name: string, test: bool, currency: string) =
        interface UserClaims with
            member this.Claims =
                [ MyJwtClaims.test, test
                  MyJwtClaims.currency, currency
                  MyJwtClaims.name, name ]

    let configClaims (subject: string) (issuer: string) (expireTime: int64) : list<Claim> =
        [ MyJwtClaims.expirationTime, expireTime
          MyJwtClaims.subject, subject
          MyJwtClaims.issuer, issuer ]

    let authReqClaims (authReq: AuthRequest) : list<Claim> =
        [ MyJwtClaims.token, authReq.Token
          MyJwtClaims.operator, authReq.Operator
          MyJwtClaims.audience, authReq.ClientId ]

    let createTokenInternal
        (jwtId: string)
        (user: UserClaims)
        (subject: string)
        (issuer: string)
        (expireTime: int64)
        (clientSecret: string)
        (authReq: AuthRequest)
        : string =
        let serializer = SystemTextSerializer()
        let urlEncoder = JwtBase64UrlEncoder()
        let algorithm = HMACSHA512Algorithm()
        let encoder = JwtEncoder(algorithm, serializer, urlEncoder)

        let claims =
            seq {
                yield! configClaims subject issuer expireTime
                yield! user.Claims
                yield! authReqClaims authReq
                yield MyJwtClaims.jwtId, jwtId
            }
            |> dict

        let secretKey = Encoding.UTF8.GetBytes(clientSecret)
        encoder.Encode(claims, secretKey)

    let createToken
        (user: UserClaims)
        (subject: string, issuer: string, ttl: int)
        (clientSecret: string)
        (authReq: AuthRequest)
        : string =
        let jwtId = Guid.NewGuid().ToString()
        let expireTime = DateTimeOffset.UtcNow.AddMinutes(ttl).ToUnixTimeSeconds()
        createTokenInternal jwtId user subject issuer expireTime clientSecret authReq
