namespace authx

open Falco
open Falco.Routing

module MyEndPoints =

    let authHandler: HttpHandler =
        fun ctx ->
            let q = Request.getQuery ctx

            let processAuth (sign: string) (authReq: Core.AuthRequest) =
                //authReq.ClientId
                //send a jwt token back to
                Response.ofPlainText $"{authReq.ClientId}#{sign}"


            let handler =
                match q.TryGet("sign") with
                | None -> SharedHandlers.badRequest
                | Some(sign) -> Request.mapJson (processAuth sign)

            handler ctx

    let lists: list<HttpEndpoint> =
        [ get "/" (Response.ofPlainText "Hello World"); post "/auth" authHandler ]
