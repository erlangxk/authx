module WebApiTests

open System.Net
open FsHttp
open Xunit
open authx.AuthRequest
open authx.MyOperator

[<Fact>]
let testHelloWorld () =
    let res = http { GET "http://localhost:5124" } |> Request.send |> Response.toText
    Assert.Equal("Hello World", res)


[<Fact>]
let testAuthClientNotFound () =
    let res =
        let clientId = "first_client"
        let operator = "operator"
        let token = "token"
        http {
            POST "http://localhost:5124/auth"
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
        |> Response.assertHttpStatusCode HttpStatusCode.BadRequest
        |> Response.toText

    Assert.Equal(@"Bad request:ClientNotFound ""first_client""", res)


[<Fact>]
let testAuthInvalidSign () =
    let res =
        let clientId = "client1"
        let operator = "w88"
        let token = "token"
        http {
            POST "http://localhost:5124/auth"
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
        |> Response.toText

    Assert.Equal(@"Bad request:InvalidAuthRequestSign ""xxx""", res)

[<Fact>]
let testAuthOperatorNotFound () =
    let res =
       
        let clientId = "client1"
        let operator = "operator"
        let token = "token"
        let sign = checkSum clientId operator token "client1Secret"
        http {
            POST "http://localhost:5124/auth"
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
        |> Response.assertHttpStatusCode HttpStatusCode.BadRequest
        |> Response.toText

    Assert.Equal(@"Bad request:OperatorNotFound ""operator""", res)


[<Fact>]
let testAuth () =
    let res =
        let clientId = "client1"
        let operator = "w88"
        let token = "token"
        let sign = checkSum clientId operator token "client1Secret"

        http {
            POST "http://localhost:5124/auth"
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
        |> Response.toText
    printfn $"{res}"
    Assert.Equal(@"Bad request:OperatorNotFound ""operator""", res)
