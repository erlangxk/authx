namespace authx

open Autofac
open Autofac.Extensions.DependencyInjection
open Falco
open Falco.HostBuilder
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Configuration

module Program =

    let addConfig (args: string[]) (configBuilder: IConfigurationBuilder) =
        configBuilder          
            .AddJsonFile("clients.json", true)
            .AddJsonFile("w88.json", true)
            .AddEnvironmentVariables("AUTHX_")
            .AddCommandLine(args)
        |> ignore

    let addService (context: HostBuilderContext) (svc: IServiceCollection) =
        let cfg = context.Configuration
        MyClients.configClients (cfg, svc)
        W88Operator.configW88Operator (cfg, svc)
        AuthToken.configAuthToken (cfg, svc)

    let addAutofacConfig (builder: ContainerBuilder) = W88Operator.registerW88Auth builder

    let configHost (args: string[]) (host: IHostBuilder) =
        host
            .UseServiceProviderFactory(AutofacServiceProviderFactory())
            .ConfigureAppConfiguration(addConfig args)
            .ConfigureServices(addService)
            .ConfigureContainer<ContainerBuilder>(addAutofacConfig)

    [<EntryPoint>]
    let main args =
        webHost args {

            host (configHost args)

            use_if FalcoExtensions.IsDevelopment DeveloperExceptionPageExtensions.UseDeveloperExceptionPage

            use_ifnot
                FalcoExtensions.IsDevelopment
                (FalcoExtensions.UseFalcoExceptionHandler (SharedHandlers.serverError "Server Error"))

            add_http_client
            endpoints MyEndPoints.lists

        }

        0
