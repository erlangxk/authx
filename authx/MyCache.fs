namespace authx


open Microsoft.Extensions.Logging
open LazyCache
open System.Data.Common
open authx.Domain

module MyCache =

    type CachedStorage(cache: IAppCache, ds: DbDataSource, logger: ILogger<CachedStorage>) =

        let lookUpFromCache (cacheKey: string) lookupDb =

            let create () =
                task {
                    let! conn = ds.OpenConnectionAsync()

                    match! (lookupDb conn) with
                    | Error err ->
                        logger.LogError("failed when lookupDb {cacheKey}, error is {message}", cacheKey, err.ToString())
                        return raise (Donald.DbFailureException(err))
                    | Ok c -> return c
                }

            cache.GetOrAddAsync(cacheKey, create)


        interface IStorage with
            member this.GetClientById(clientId: string) =
                MyDatabase.getClientById clientId |> lookUpFromCache $"GetClientById#{clientId}"

            member this.GetOperatorByName(name: string) =
                MyDatabase.getOperatorByName name |> lookUpFromCache $"GetOperatorByName#{name}"

