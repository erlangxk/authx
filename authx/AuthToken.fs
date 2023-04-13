module authx.AuthToken

open authx.MyClients
open authx.MyOperator
open Autofac
open AuthRequest
open MyJwtToken
open System.Threading.Tasks

exception ClientNotFound of string
exception OperatorNotFound of string
exception InvalidAuthRequestSign of string
exception UnSuccessfulResponse of string

type AuthTokenResult =
    | Success of string
    | Failed of exn

type AuthToken(clients: MyClients, myJwtToken: MyJwtToken, container: IComponentContext) =

    member this.GetUserInfo(authReq: AuthRequest) : Task<AuthTokenResult> =
        task {
            match clients.FindClientSecret authReq.ClientId with
            | Some(secret) ->
                if authReq.CheckSign(secret) then
                    match container.TryResolveNamed<AuthApi>(authReq.Operator) with
                    | true, service ->
                        let! result = service.GetUserInfo(authReq.Token)

                        match result with
                        | AuthResult.Success claims ->
                            let token = createToken claims myJwtToken.Config secret authReq
                            return AuthTokenResult.Success(token)
                        | AuthResult.Failed(s) -> return AuthTokenResult.Failed(UnSuccessfulResponse(s))
                        | AuthResult.UnknownError(ex) -> return AuthTokenResult.Failed(ex)
                    | _ -> return AuthTokenResult.Failed(OperatorNotFound(authReq.Operator))
                else
                    return Failed(InvalidAuthRequestSign(authReq.Sign))
            | None -> return Failed(ClientNotFound(authReq.ClientId))
        }
