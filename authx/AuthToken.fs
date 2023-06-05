module authx.AuthToken

open authx.MyClients
open authx.MyJwtToken
open authx.MyOperator
open Autofac
open AuthRequest
open System.Threading.Tasks
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Options
open Thoth.Json.Net

let findAuthApi (container: IComponentContext) (operator: string) =
#if FUNPLAY
    Some(FunPlay.authApi)
#else
    match container.TryResolveNamed<AuthApi>(operator) with
    | true, service -> Some(service)
    | _ -> None
#endif

let ERR_FAILED_AUTH = 1000
let ERR_CLIENT_NOT_FOUND = 1001
let ERR_INVALID_SIGN = 1002
let ERR_OPERATOR_NOT_FOUND = 1003
let ERR_UNKNOWN_EXN = 2000


type AuthTokenResult =
    | Success of string
    | Failure of code: int * reason: string
    | Error of exn

let errorJson (code: int, reason: string) =
    Encode.object [ "code", Encode.int code; "reason", Encode.string reason ]
    |> Encode.toString 0

let toJson (result: AuthTokenResult) =
    match result with
    | Success(token) -> Encode.object [ "token", Encode.string token ] |> Encode.toString 0
    | Failure(code, reason) -> errorJson (code, reason)
    | Error(ex) -> errorJson (ERR_UNKNOWN_EXN, ex.ToString())


type AuthToken(clients: MyClients, jwt: IOptions<JwtConfig>, container: IComponentContext) =
    let jwtCfg = jwt.Value

    member this.GetUserInfo(authReq: AuthRequest) : Task<AuthTokenResult> =
        task {
            match clients.FindClientSecret authReq.ClientId with
            | Some(secret) ->
                if authReq.CheckSign(secret) then
                    match findAuthApi container authReq.Operator with
                    | Some(service) ->
                        match! service.GetUserInfo(authReq.Token) with
                        | AuthResult.Success claims ->
                            let token = createToken claims (jwtCfg.Issuer, jwtCfg.Ttl) secret authReq
                            return Success(token)
                        | AuthResult.Failed(s) -> return Failure(code = ERR_FAILED_AUTH, reason = s)
                        | AuthResult.UnknownError(ex) -> return Error(ex)
                    | None -> return Failure(ERR_OPERATOR_NOT_FOUND, $"Operator:{authReq.Operator} not found")
                else
                    return Failure(ERR_INVALID_SIGN, $"Invalid sign:{authReq.Sign}")
            | None -> return Failure(ERR_CLIENT_NOT_FOUND, $"Client:{authReq.ClientId} not found")
        }


let configAuthToken (cfg: IConfiguration, svc: IServiceCollection) =
    svc
        .Configure<MyJwtToken.JwtConfig>(cfg.GetSection("JWT"))
        .AddSingleton<AuthToken>()
    |> ignore
