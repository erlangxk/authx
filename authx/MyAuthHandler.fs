namespace authx

open Autofac
open System.Threading.Tasks
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Options

module MyAuthHandler =

    let findAuthApi (container: IComponentContext) (operator: string) =
#if FUNPLAY
        Some(FunPlay.authApi)
#else
        match container.TryResolveNamed<AuthApi>(operator) with
        | true, service -> Some(service)
        | _ -> None
#endif

    type MyAuthHandler(clients: MyClients, jwt: IOptions<JwtConfig>, container: IComponentContext) =
        let jwtCfg = jwt.Value
        let createToken = JwtToken.createToken jwtCfg.Ttl jwtCfg.Issuer

        interface AuthHandler with
            member this.GetUserInfo(authReq: AuthRequest) : Task<AuthResponse> =
                task {
                    match clients.FindClientSecret authReq.ClientId with
                    | Some(secret) ->
                        if AuthHandler.checkSign authReq secret then
                            match findAuthApi container authReq.Operator with
                            | Some(service) ->
                                match! service.GetUserInfo(authReq.Token) with
                                | AuthResult.Success claims ->
                                    return AuthResponse.Success(createToken claims secret authReq)
                                | AuthResult.Failed(s) -> return AuthResponse.Failure(s)
                                | AuthResult.UnknownError(ex) -> return AuthResponse.UnknownError(ex)
                            | None -> return AuthResponse.OperatorNotFound(authReq.Operator)
                        else
                            return AuthResponse.InvalidSign(authReq.Sign)
                    | None -> return AuthResponse.ClientNotFound(authReq.ClientId)
                }

    let configAuthHandler (cfg: IConfiguration, svc: IServiceCollection) =
        let bind jwt = cfg.GetSection("JWT").Bind(jwt)

        svc
            .ConfigureAndValidate<JwtConfig>(bind)
            .AddSingleton<AuthHandler, MyAuthHandler>()
        |> ignore
