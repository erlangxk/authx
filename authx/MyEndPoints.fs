namespace authx

open Falco
open Falco.Routing
open Microsoft.AspNetCore.Http
open System.Threading.Tasks
open AuthRequest
open Microsoft.Extensions.Options
open authx.AuthXml
open authx.MyJwtToken

module MyEndPoints =

    let processAuth (sign: string) (authReq: AuthRequest) (ctx: HttpContext) =
        task {
            return! ctx |> SharedHandlers.badRequest
        }
        :> Task

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

        Services.inject<OperatorW88Service> (fun service ->
            fun ctx ->
                task {
                    let! result = service.getUserInfo "token"
                    return! ctx |> Response.ofPlainText (result.ToString())
                })

    let lists: list<HttpEndpoint> =
        [ get "/" (Response.ofPlainText "Hello World")
          post "/auth" authHandler
          get "/config" configHandler
          get "/clients" clientsHandler
          get "/user" userInfoHandler ]
