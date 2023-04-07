namespace authx

open Falco
open Falco.HostBuilder
open Microsoft.AspNetCore.Builder

open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Configuration

module Program =

    let addConfigFiles (context: HostBuilderContext) (configBuilder: IConfigurationBuilder) =

        let file =
            if context.HostingEnvironment.IsDevelopment() then
                "w88UAT.json"
            else
                "w88.json"

        configBuilder.AddJsonFile("clients.json") |> ignore
        configBuilder.AddJsonFile(file) |> ignore


    let addServices (context: HostBuilderContext) (svc: IServiceCollection) =
        let w88 = context.Configuration.GetSection(W88Operator.W88Operator.Name)
        svc.Configure<W88Operator.W88Operator>(w88) |> ignore

        let clients = context.Configuration.GetSection(nameof MyClients.Clients)
        svc.Configure<MyClients.Clients>(clients) |> ignore

        svc.AddSingleton<AuthXml.OperatorW88Service, AuthXml.OperatorW88Service>()
        |> ignore



    let configureHost (host: IHostBuilder) =
        host.ConfigureAppConfiguration addConfigFiles |> ignore
        host.ConfigureServices addServices |> ignore
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
