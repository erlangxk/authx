namespace authx

open System.Data
open System.Data.Common
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging
open Npgsql
open MyDatabase
open authx.Domain

module MyStorage =


    let loadAllData (logger: ILogger) (ds: DbDataSource) =
        let run dbOp =
            task {
                use! conn = ds.OpenConnectionAsync()

                match! (dbOp conn) with
                | Ok(rs) -> return rs |> List.map (fun r -> getKey r, r) |> Map.ofList
                | Error(err) ->
                    logger.LogError("{dbOp} failed, error is {err}", nameof dbOp, err)
                    return Map.empty
            }

        let clients = run loadAllClients
        let operators = run loadAllOperators
        let operatorPrincipals = run loadAllOperatorPrincipals

        (clients.Result, operators.Result, operatorPrincipals.Result)


    type MemoryStorage(config: IConfiguration, logger: ILogger<MemoryStorage>) =
        let store =
            let connStr = config["DB:ConnStr"]
            logger.LogInformation("database connection is {connStr}", connStr)
            use ds = NpgsqlDataSource.Create(connStr)
            loadAllData logger ds |||> createStorage

        member this.get() = store

    let addStorage (svc: IServiceCollection) =
        svc.AddSingleton<MemoryStorage, MemoryStorage>()
