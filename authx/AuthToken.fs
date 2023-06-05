namespace authx

open Autofac
open System.Threading.Tasks
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Options
open Thoth.Json.Net

module AuthToken =
    let findAuthApi (container: IComponentContext) (operator: string) =
#if FUNPLAY
        Some(FunPlay.authApi)
#else
        match container.TryResolveNamed<AuthApi>(operator) with
        | true, service -> Some(service)
        | _ -> None
#endif

    let private ERR_FAILED_AUTH = 1000
    let private ERR_CLIENT_NOT_FOUND = 1001
    let private ERR_INVALID_SIGN = 1002
    let private ERR_OPERATOR_NOT_FOUND = 1003
    let private ERR_UNKNOWN_EXN = 2000

    type AuthTokenResult =
        | Success of string
        | Failure of string
        | InvalidSign of string
        | OperatorNotFound of string
        | ClientNotFound of string
        | UnknownError of exn

    let inline private errorJson (code: int, reason: string) =
        Encode.object [ "code", Encode.int code; "reason", Encode.string reason ]
        |> Encode.toString 0

    let toJson (result: AuthTokenResult) =
        match result with
        | Success(token) -> Encode.object [ "token", Encode.string token ] |> Encode.toString 0
        | Failure(reason) -> errorJson (ERR_FAILED_AUTH, $"Failed auth:{reason}")
        | InvalidSign(sign) -> errorJson (ERR_INVALID_SIGN, $"Invalid sign:{sign}")
        | OperatorNotFound(operator) -> errorJson (ERR_OPERATOR_NOT_FOUND, $"Operator:{operator} not found")
        | ClientNotFound(clientId) -> errorJson (ERR_CLIENT_NOT_FOUND, $"ClientId:{clientId} not found")
        | UnknownError(ex) -> errorJson (ERR_UNKNOWN_EXN, ex.Message)

    type AuthToken(clients: MyClients, jwt: IOptions<JwtConfig>, container: IComponentContext) =
        let jwtCfg = jwt.Value

        let createToken = JwtToken.createToken jwtCfg.Ttl jwtCfg.Issuer

        member this.GetUserInfo(authReq: AuthRequest) : Task<AuthTokenResult> =
            task {
                match clients.FindClientSecret authReq.ClientId with
                | Some(secret) ->
                    if AuthRequest.checkSign authReq secret then
                        match findAuthApi container authReq.Operator with
                        | Some(service) ->
                            match! service.GetUserInfo(authReq.Token) with
                            | AuthResult.Success claims -> return Success(createToken claims secret authReq)
                            | AuthResult.Failed(s) -> return Failure(s)
                            | AuthResult.UnknownError(ex) -> return UnknownError(ex)
                        | None -> return OperatorNotFound(authReq.Operator)
                    else
                        return InvalidSign(authReq.Sign)
                | None -> return ClientNotFound(authReq.ClientId)
            }

    let configAuthToken (cfg: IConfiguration, svc: IServiceCollection) =
        svc.Configure<JwtConfig>(cfg.GetSection("JWT")).AddSingleton<AuthToken>()
        |> ignore
