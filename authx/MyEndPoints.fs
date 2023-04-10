namespace authx

open Autofac
open Falco
open Falco.Routing
open Microsoft.AspNetCore.Http
open System.Threading.Tasks
open AuthRequest
open Microsoft.Extensions.Options
open authx.MyJwtToken
open authx.MyOperator
open authx.W88Operator

module MyEndPoints =

    let processAuth (sign: string) (authReq: AuthRequest) (ctx: HttpContext) =
        task { return! ctx |> SharedHandlers.badRequest } :> Task

    let handleAuth (ctx: HttpContext) =
        let q = Request.getQuery ctx

        match q.TryGet("sign") with
        | None -> ctx |> SharedHandlers.badRequest
        | Some (sign) -> ctx |> Request.mapJson (processAuth sign)

    let authHandler: HttpHandler = handleAuth

    let configHandler: HttpHandler =
        Services.inject<IOptions<W88Operator.W88Operator>> (fun op ->
            fun ctx -> ctx |> Response.ofPlainText (op.Value.ToString())

        )

    let clientsHandler: HttpHandler =
        Services.inject<IOptions<MyClients.Clients>> (fun op ->
            fun ctx -> ctx |> Response.ofPlainText (op.Value.ToString())

        )

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
