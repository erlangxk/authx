module WebApiTests

open System.Net
open FsHttp
open Xunit
open authx.AuthHandler

let baseUrl = "http://localhost:5124"
let authUrl = $"{baseUrl}/auth"

[<Fact>]
let testHelloWorld () =
    let res = http { GET baseUrl } |> Request.send |> Response.toText
    Assert.Equal("Hello World", res)


[<Fact>]
let testAuthClientNotFound () =
    let res =
        let clientId = "first_client"
        let operator = "operator"
        let token = "token"
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
        |> Response.assertHttpStatusCode HttpStatusCode.NotFound
        |> Response.toJson

    Assert.Equal(1001, res.GetProperty("code").GetInt32())


[<Fact>]
let testAuthInvalidSign () =
    let res =
        let clientId = "client1"
        let operator = "w88"
        let token = "token"
        http {
            POST authUrl
            body
            json
                $"""{{
                    "clientId":"{clientId}",
                    "operator":"{operator}",
                    "token":"{token}",
                    "sign": "xxx"
                       }}"""
        }
        |> Request.send
        |> Response.assertHttpStatusCode HttpStatusCode.BadRequest
        |> Response.toJson

    Assert.Equal(1002, res.GetProperty("code").GetInt32())

[<Fact>]
let testAuthOperatorNotFound () =
    let res =
       
        let clientId = "client1"
        let operator = "operator"
        let token = "token"
        let sign = checkSum clientId operator token "client1Secret"
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
        |> Response.assertHttpStatusCode HttpStatusCode.NotFound
        |> Response.toJson
    
    Assert.Equal(1003, res.GetProperty("code").GetInt32())


[<Fact>]
let testAuth () =
    let res =
        let clientId = "client1"
        let operator = "w88"
        let token = "token"
        let sign = checkSum clientId operator token "client1Secret"

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
        |> Response.assertHttpStatusCode HttpStatusCode.InternalServerError
        |> Response.toJson
    Assert.Equal(2000, res.GetProperty("code").GetInt32())
