namespace authx

open Falco
open Falco.Routing
open Microsoft.AspNetCore.Http
open System.Threading.Tasks
open Microsoft.Extensions.Logging
open Microsoft.FSharp.Control
open System.Text
open Microsoft.Extensions.Options
open Autofac

module MyEndPoints =
    let ofJson (str: string) : HttpHandler =
        Response.withContentType "application/json; charset=utf-8"
        >> Response.ofString Encoding.UTF8 str

    let processAuth (auth: AuthHandler) (authReq: AuthRequest) (ctx: HttpContext) : Task =
        task {
            let! result = auth.GetUserInfo(authReq)
            let json = result |> AuthHandler.toJson
            match result with
            | AuthResponse.Success _ | AuthResponse.Failure _ ->
                return! ctx |> ofJson json
            |AuthResponse.OperatorNotFound _ ->
                return! ctx |> ofJson json
            | AuthResponse.InvalidSign _ ->
                return! ctx |> SharedHandlers.badRequest json
            | AuthResponse.ClientNotFound _ ->
                return! ctx |>  SharedHandlers.notFound json
            | AuthResponse.UnknownError _ ->
                return! ctx |> SharedHandlers.serverError json
        }

    let authHandler: HttpHandler =
        Services.inject<AuthHandler> (fun auth ctx -> ctx |> Request.mapJson (processAuth auth))

    let configHandler: HttpHandler =
        Services.inject<IOptions<W88Operator>> (fun op ->
            fun ctx -> ctx |> Response.ofPlainText (op.Value.ToString()))

    let clientsHandler: HttpHandler =
        Services.inject<IOptions<Clients>> (fun op ->
            fun ctx -> ctx |> Response.ofPlainText (op.Value.ToString()))

    let userInfoHandler: HttpHandler =
        Services.inject<IComponentContext, ILogger<AuthApi>> (fun container logger ->
            fun ctx ->
                task {
                    try
                        match container.TryResolveNamed<AuthApi>(W88Operator.Name) with
                        | true, service ->
                            let! result = service.GetUserInfo "token"
                            return! ctx |> Response.ofPlainText (result.ToString())
                        | false, _ ->
                            return! ctx |> SharedHandlers.badRequest $"{W88Operator.Name} not found"
                    with ex ->
                        logger.LogError(ex, "Error resolving service")
                        return! ctx |> SharedHandlers.serverError ex.Message
                })

    let lists: list<HttpEndpoint> =
        [ get "/" (Response.ofPlainText "Hello World")
          post "/auth" authHandler
          get "/config" configHandler
          get "/clients" clientsHandler
          get "/user" userInfoHandler ]
