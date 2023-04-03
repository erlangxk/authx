namespace authx

open Falco
open Falco.Routing
open Domain
open Microsoft.AspNetCore.Http
open System.Threading.Tasks
open AuthRequest
open authx.Domain
open authx.MyJwtToken

module MyEndPoints =

    let processAuth (storage: IStorage) (sign: string) (authReq: AuthRequest) (ctx: HttpContext) =
        task {
            let! client = storage.GetClientById(authReq.ClientId)

            match client with
            | Some (c) when checkSign (authReq) (c.Secret) (sign) ->
                let! operator = storage.GetOperatorByName(authReq.Operator)

                match operator with
                | None -> return! ctx |> SharedHandlers.badRequest
                | Some (op) ->
                    //let userClaims = getUserClaims(operator, authReq.Token)
                    return! ctx |> Response.ofPlainText $"{c.Secret}{authReq.ClientId}#{sign}"
            | _ -> return! ctx |> SharedHandlers.badRequest
        }
        :> Task

    let handleAuth (storage: IStorage) (ctx: HttpContext) =
        let q = Request.getQuery ctx

        match q.TryGet("sign") with
        | None -> ctx |> SharedHandlers.badRequest
        | Some (sign) -> ctx |> Request.mapJson (processAuth storage sign)

    let authHandler: HttpHandler = Services.inject<IStorage> handleAuth

    let configHandler: HttpHandler =
        Services.inject<AuthXml.W88AuthConfig> (fun cfg ->
            fun ctx ->
                let config = cfg.AppConfig
                ctx |> Response.ofPlainText (config.ToString())

        )

    let userInfoHandler: HttpHandler =
        Services.inject<AuthXml.W88AuthService> (fun service ->
            fun ctx ->
                task {
                    let! result = service.getUserInfo "token"
                    return! ctx |> Response.ofPlainText (result.ToString())
                })

    let lists: list<HttpEndpoint> =
        [ get "/" (Response.ofPlainText "Hello World")
          post "/auth" authHandler
          get "/config" configHandler
          get "/user" userInfoHandler]
