module WebApiTests

open System.Net
open FsHttp
open Xunit
open authx.AuthHandler

let baseUrl = "http://localhost:7071"
let authUrl = $"{baseUrl}/auth"

[<Fact>]
let testHelloWorld () =
    let res = http { GET $"{baseUrl}/hc" } |> Request.send |> Response.toText
    Assert.Equal("Hello", res)


[<Fact>]
let testAuthClientNotFound () =
    let clientId = "first_client"
    let operator = "operator"
    let token = "token"

    let res =
        http {
            POST authUrl
            body

            json
                $"""{{
                    "clientId":"{clientId}",
                    "operator":"{operator}",
                    "token":"{token}",
                    "sign": "xxxx"
                       }}"""
        }
        |> Request.send
        |> Response.assertStatusCode 461
        |> Response.toText

    Assert.Equal($"Client:{clientId} not found", res)


[<Fact>]
let testAuthInvalidSign () =
    let clientId = "client1"
    let operator = "w88"
    let token = "token"
    let sign = "xxx"

    let res =
        http {
            POST authUrl
            body

            json
                $"""{{
                    "clientId":"{clientId}",
                    "operator":"{operator}",
                    "token":"{token}",
                    "sign":"{sign}"
                       }}"""
        }
        |> Request.send
        |> Response.assertStatusCode 463
        |> Response.toText

    Assert.Equal($"Invalid sign:{sign}", res)

[<Fact>]
let testAuthOperatorNotFound () =
    let clientId = "client1"
    let operator = "operator"
    let token = "token"
    let sign = checkSum clientId operator token "client1Secret"

    let res =
        http {
            POST authUrl
            body

            json
                $"""{{
                    "clientId":"{clientId}",
                    "operator":"{operator}",
                    "token":"{token}",
                    "sign": "{sign}"
                       }}"""
        }
        |> Request.send
        |> Response.assertStatusCode 462
        |> Response.toText

    Assert.Equal($"Operator {operator} not found", res)


[<Fact>]
let testAuth () =
    let clientId = "client1"
    let operator = "w88"
    let token = "token"
    let sign = checkSum clientId operator token "client1Secret"

    let res =
        http {
            POST authUrl
            body

            json
                $"""{{
                    "clientId":"{clientId}",
                    "operator":"{operator}",
                    "token":"{token}",
                    "sign": "{sign}"
                       }}"""
        }
        |> Request.send
        |> Response.assertStatusCode 200
        |> Response.toText

    Assert.True(res.Length > 0)
