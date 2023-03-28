namespace authx

open Falco
open Falco.Routing
open Domain
open Microsoft.AspNetCore.Http
open System.Threading.Tasks

module MyEndPoints =

    let processAuth (storage: IStorage) (sign: string) (authReq: Core.AuthRequest) (ctx: HttpContext) =
        task {
            let! client = storage.GetClientById(authReq.ClientId)

            match client with
            | Some(c) when Core.checkSign (authReq) (c.Secret) (sign) ->
                return! ctx |> Response.ofPlainText $"{c.Secret}{authReq.ClientId}#{sign}"
            | _ -> return! ctx |> SharedHandlers.badRequest
        }
        :> Task

    let handleAuth (storage: IStorage) (ctx: HttpContext) =
        let q = Request.getQuery ctx

        match q.TryGet("sign") with
        | None -> ctx |> SharedHandlers.badRequest
        | Some(sign) -> ctx |> Request.mapJson (processAuth storage sign)

    let authHandler: HttpHandler = Services.inject<IStorage> handleAuth


    let lists: list<HttpEndpoint> =
        [ get "/" (Response.ofPlainText "Hello World"); post "/auth" authHandler ]
