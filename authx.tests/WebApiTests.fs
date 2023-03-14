module WebApiTests

open FsHttp
open Xunit

[<Fact>]
let testHelloWorld () =
    let res = http { GET "http://localhost:5124" } |> Request.send |> Response.toText
    Assert.Equal("Hello World", res)


[<Fact>]
let testAuth () =
    let res =
        http {
            POST "http://localhost:5124/auth"
            query [ "sign", "xxx" ]
            body
            json
                """
                {
                "clientId":"client",
                "operator":"operator",
                "token":"token"
                   }
            """
        }
        |> Request.send
        |> Response.toText

    Assert.Equal("client#xxx", res)
