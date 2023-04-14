namespace authx

open Autofac.Extensions.DependencyInjection
open Falco
open Falco.HostBuilder
open Microsoft.AspNetCore.Builder

open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Configuration
open Autofac

module Program =

    let configAllOperators (context: HostBuilderContext, svc: IServiceCollection) =
        W88Operator.configW88Operator (context.Configuration, svc)

    let addConfig (args: string[]) (configBuilder: IConfigurationBuilder) =
        configBuilder
            .AddJsonFile("clients.json", true)
            .AddJsonFile("w88.json", true)
            .AddEnvironmentVariables("AUTHX_")
            .AddCommandLine(args)
        |> ignore

    let addServices (context: HostBuilderContext) (svc: IServiceCollection) =
        MyClients.configClients (context.Configuration, svc)
        MyJwtToken.configJwtToken (context.Configuration, svc)
        configAllOperators (context, svc)
        svc.AddSingleton<AuthToken.AuthToken>() |> ignore

    let addAutofacConfig (builder: ContainerBuilder) = W88Operator.registerW88Auth (builder)

    let configureHost (args: string[]) (host: IHostBuilder) =
        host.UseServiceProviderFactory(AutofacServiceProviderFactory()) |> ignore
        host.ConfigureAppConfiguration(addConfig args) |> ignore
        host.ConfigureServices addServices |> ignore
        host.ConfigureContainer<ContainerBuilder> addAutofacConfig |> ignore
        host


    [<EntryPoint>]
    let main args =
        webHost args {

            host (configureHost args)

            use_if FalcoExtensions.IsDevelopment DeveloperExceptionPageExtensions.UseDeveloperExceptionPage

            use_ifnot
                FalcoExtensions.IsDevelopment
                (FalcoExtensions.UseFalcoExceptionHandler SharedHandlers.serverError)

            add_http_client
            endpoints MyEndPoints.lists

        }

        0
