namespace authx

open System
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open System.Data.Common
open authx.Domain
open Npgsql

module MyServices =

    let addCache (svc: IServiceCollection) = svc.AddLazyCache()

    let createDataSource (provider: IServiceProvider) : DbDataSource =
        let config = provider.GetRequiredService<IConfiguration>()
        let connStr = config["DB:ConnStr"]
        NpgsqlDataSource.Create(connStr)

    let addDatasource (svc: IServiceCollection) =
        svc.AddSingleton<DbDataSource>(createDataSource)

    let addMemoryStorage (svc: IServiceCollection) =
        svc.AddSingleton<IStorage, MyStorage.MemoryStorage>()

    let addCachedStorage (svc: IServiceCollection) =
        svc.AddSingleton<IStorage, MyCache.CachedStorage>()
