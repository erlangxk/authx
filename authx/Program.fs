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

    let addConfigFiles (context: HostBuilderContext) (configBuilder: IConfigurationBuilder) =

        let file =
            if context.HostingEnvironment.IsDevelopment() then
                "w88UAT.json"
            else
                "w88.json"

        configBuilder.AddJsonFile("clients.json") |> ignore
        configBuilder.AddJsonFile(file) |> ignore


    let addServices (context: HostBuilderContext) (svc: IServiceCollection) =
        configAllOperators (context, svc)
        MyClients.configClients (context.Configuration, svc)

    let addAutofacConfig (context: HostBuilderContext) (builder: ContainerBuilder) =
        builder.RegisterType<W88Operator.W88AuthApi>()
            .Named<MyOperator.AuthApi>(W88Operator.W88Operator.Name)
        |> ignore

    let configureHost (host: IHostBuilder) =
        host.UseServiceProviderFactory(AutofacServiceProviderFactory()) |> ignore
        host.ConfigureAppConfiguration addConfigFiles |> ignore
        host.ConfigureServices addServices |> ignore
        host.ConfigureContainer<ContainerBuilder> addAutofacConfig |> ignore
        host


    [<EntryPoint>]
    let main args =
        webHost args {

            host configureHost
            
            

            use_if FalcoExtensions.IsDevelopment DeveloperExceptionPageExtensions.UseDeveloperExceptionPage

            use_ifnot
                FalcoExtensions.IsDevelopment
                (FalcoExtensions.UseFalcoExceptionHandler SharedHandlers.serverError)

            add_http_client
            endpoints MyEndPoints.lists

        }

        0
