namespace authx

open Falco
open Falco.Routing
open authx.MyStorage

module MyEndPoints =

    let authHandler: HttpHandler =
        Services.inject<MemoryStorage> (fun storage ->
            fun ctx ->
                let q = Request.getQuery ctx

                let processAuth (sign: string) (authReq: Core.AuthRequest) =
                    //authReq.ClientId
                    //send a jwt token back to
                    let client = storage.get().GetClientById(authReq.ClientId)

                    match client with
                    | Some(c) -> Response.ofPlainText $"{c.Secret}{authReq.ClientId}#{sign}"
                    | None -> SharedHandlers.badRequest


                let handler =
                    match q.TryGet("sign") with
                    | None -> SharedHandlers.badRequest
                    | Some(sign) -> Request.mapJson (processAuth sign)

                handler ctx)

    let lists: list<HttpEndpoint> =
        [ get "/" (Response.ofPlainText "Hello World"); post "/auth" authHandler ]
