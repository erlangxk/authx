namespace authx

open System.Data.Common
open Microsoft.Extensions.Logging
open MyDatabase
open authx.Domain

module MyStorage =


    let loadAllData (logger: ILogger) (ds: DbDataSource) =
        let run dbOp getKey =
            task {
                use! conn = ds.OpenConnectionAsync()

                match! (dbOp conn) with
                | Ok(rs) -> return rs |> List.map (fun r -> getKey r, r) |> Map.ofList
                | Error(err) ->
                    logger.LogError("{dbOp} failed, error is {err}", nameof dbOp, err)
                    return Map.empty
            }

        let clients = run loadAllClients (fun r -> r.Id)
        let operators = run loadAllOperators (fun r -> r.Name)
        let operatorPrincipals = run loadAllOperatorPrincipals (fun r -> r.OperatorName)
        (clients.Result, operators.Result, operatorPrincipals.Result)


    type MemoryStorage(dataSource: DbDataSource, logger: ILogger<MemoryStorage>) =
        let (clients, operators, operatorPrincipals) = loadAllData logger dataSource


        interface IStorage with
            member this.GetClientById(clientId: string) =
                task { return clients |> Map.tryFind clientId }

            member this.GetOperatorByName(name: string) =
                task { return operators |> Map.tryFind name }

            member this.GetPrincipalOfOperator(operator: string) =
                task { return operatorPrincipals |> Map.tryFind operator }
