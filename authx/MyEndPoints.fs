namespace authx

open Autofac
open Falco
open Falco.Routing
open Microsoft.AspNetCore.Http
open System.Threading.Tasks
open AuthRequest
open Microsoft.Extensions.Options
open Microsoft.FSharp.Control
open authx.AuthToken
open authx.MyOperator

module MyEndPoints =
    //TODO configure issuer and ttl for jwt token
    let processAuth (auth: AuthToken.AuthToken) (authReq: AuthRequest) (ctx: HttpContext) : Task =
        task {
            match! auth.GetUserInfo(authReq) with
            | AuthTokenResult.Success(token) -> return! ctx |> Response.ofPlainText token
            | AuthTokenResult.Failed(ex) -> return! ctx |> SharedHandlers.badRequest ex.Message
        }

    let authHandler: HttpHandler =
        Services.inject<AuthToken.AuthToken> (fun auth ctx -> ctx |> Request.mapJson (processAuth auth))

 
    let lists: list<HttpEndpoint> =
        [ get "/" (Response.ofPlainText "Hello World")
          post "/auth" authHandler]
