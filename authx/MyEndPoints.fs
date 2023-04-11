namespace authx

open Autofac
open Falco
open Falco.Routing
open Microsoft.AspNetCore.Http
open System.Threading.Tasks
open AuthRequest
open Microsoft.Extensions.Options
open Microsoft.FSharp.Control
open authx.MyClients
open authx.MyOperator

module MyEndPoints =

    let processAuth
        (myClients: MyClients)
        (container: IComponentContext)
        (sign: string)
        (authReq: AuthRequest)
        (ctx: HttpContext)
        : Task =
        match myClients.clientSecret authReq.ClientId with
        | Some(s) when (checkSign authReq s sign) ->
            match container.TryResolveNamed<AuthApi>(authReq.Operator) with
            | true, service ->
                task {
                    let! result = service.getUserInfo (authReq.Token)

                    match result with
                    | Success claims ->
                        let token = MyJwtToken.createToken claims ("issuer", 30) s authReq
                        return! ctx |> Response.ofPlainText token
                    | _ -> return! ctx |> SharedHandlers.badRequest
                }
            | _ -> ctx |> SharedHandlers.badRequest
        | _ -> ctx |> SharedHandlers.badRequest


    let handleAuth (ctx: HttpContext) =
        let q = Request.getQuery ctx

        let handler =
            match q.TryGet("sign") with
            | None -> SharedHandlers.badRequest
            | Some(sign) ->
                Services.inject<MyClients, IComponentContext> (fun myClients container ctx ->
                    ctx |> Request.mapJson (processAuth myClients container sign))

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
                        let! result = service.getUserInfo "token"
                        return! ctx |> Response.ofPlainText (result.ToString())
                    | _ -> return! ctx |> SharedHandlers.badRequest

                })

    let lists: list<HttpEndpoint> =
        [ get "/" (Response.ofPlainText "Hello World")
          post "/auth" authHandler
          get "/config" configHandler
          get "/clients" clientsHandler
          get "/user" userInfoHandler ]
