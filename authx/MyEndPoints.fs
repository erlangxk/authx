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
open authx.MyClients
open authx.MyOperator

module MyEndPoints =
    //TODO configure issuer and ttl for jwt token
    let processAuth (auth: AuthToken.AuthToken)(authReq: AuthRequest) (ctx: HttpContext) : Task =
        task {
            match! auth.GetUserInfo(authReq) with
            | AuthTokenResult.Success(token) -> return! ctx |> Response.ofPlainText token
            | AuthTokenResult.Failed(ex) -> return! ctx |> SharedHandlers.badRequest (ex.Message)
        }

    let handleAuth (ctx: HttpContext) =
        let q = Request.getQuery ctx

        let handler =
           
                Services.inject<AuthToken.AuthToken> (fun auth ctx -> ctx |> Request.mapJson (processAuth auth))

        handler ctx

    let authHandler: HttpHandler = handleAuth

    let configHandler: HttpHandler =
        Services.inject<IOptions<W88Operator.W88Operator>> (fun op ->
            fun ctx -> ctx |> Response.ofPlainText (op.Value.ToString()))

    let clientsHandler: HttpHandler =
        Services.inject<IOptions<MyClients.Clients>> (fun op ->
            fun ctx -> ctx |> Response.ofPlainText (op.Value.ToString()))

    let userInfoHandler: HttpHandler =
        Services.inject<IComponentContext> (fun container ->
            fun ctx ->
                task {
                    match container.TryResolveNamed<AuthApi>(W88Operator.W88Operator.Name) with
                    | true, service ->
                        let! result = service.GetUserInfo "token"
                        return! ctx |> Response.ofPlainText (result.ToString())
                    | (false, _) ->
                        return! ctx |> SharedHandlers.badRequest $"{W88Operator.W88Operator.Name} not found"

                })

    let lists: list<HttpEndpoint> =
        [ get "/" (Response.ofPlainText "Hello World")
          post "/auth" authHandler
          get "/config" configHandler
          get "/clients" clientsHandler
          get "/user" userInfoHandler ]
