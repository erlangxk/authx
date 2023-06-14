namespace authx

open Falco
open Falco.Routing
open Microsoft.AspNetCore.Http
open System.Threading.Tasks
open Microsoft.FSharp.Control

module MyEndPoints =
    let inline myStatusCode (code: int) (msg: string) : HttpHandler =
        Response.withStatusCode code >> Response.ofPlainText msg

    let serverError msg =
        myStatusCode StatusCodes.Status500InternalServerError msg

    let private ERR_FAILED_AUTH = 460
    let private ERR_CLIENT_NOT_FOUND = 461
    let private ERR_OPERATOR_NOT_FOUND = 462
    let private ERR_INVALID_SIGN = 463

    let processAuth (auth: AuthHandler) (authReq: AuthRequest) (ctx: HttpContext) : Task =
        task {
            let! result = auth.GetUserInfo(authReq)

            match result with
            | AuthResponse.Success token -> return! ctx |> Response.ofPlainText token
            | AuthResponse.Failure reason -> return! ctx |> myStatusCode ERR_FAILED_AUTH $"Failed auth:{reason}"
            | AuthResponse.OperatorNotFound operator ->
                return! ctx |> myStatusCode ERR_OPERATOR_NOT_FOUND $"Operator {operator} not found"
            | AuthResponse.InvalidSign sign -> return! ctx |> myStatusCode ERR_INVALID_SIGN $"Invalid sign:{sign}"
            | AuthResponse.ClientNotFound clientId ->
                return! ctx |> myStatusCode ERR_CLIENT_NOT_FOUND $"Client:{clientId} not found"
            | AuthResponse.UnknownError ex -> return! ctx |> serverError $"Unknown error:{ex.Message}"
        }

    let authHandler: HttpHandler =
        Services.inject<AuthHandler> (fun auth ctx -> ctx |> Request.mapJson (processAuth auth))

    let lists: list<HttpEndpoint> =
        [ get "/hc" (Response.ofPlainText "Hello"); post "/auth" authHandler ]
