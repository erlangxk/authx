namespace authx

open Falco
open Falco.HostBuilder
open Microsoft.AspNetCore.Builder

open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Configuration

module Program =

    let addConfigFiles (context: HostBuilderContext) (configBuilder: IConfigurationBuilder) =

        let file =
            if context.HostingEnvironment.IsDevelopment() then
                "w88UAT.json"
            else
                "w88.json"

        configBuilder.AddJsonFile(file) |> ignore


    let configureHost (host: IHostBuilder) =
        host.ConfigureAppConfiguration addConfigFiles




    [<EntryPoint>]
    let main args =
        webHost args {
            host configureHost

            use_if FalcoExtensions.IsDevelopment DeveloperExceptionPageExtensions.UseDeveloperExceptionPage

            use_ifnot
                FalcoExtensions.IsDevelopment
                (FalcoExtensions.UseFalcoExceptionHandler SharedHandlers.serverError)

            add_http_client
            add_service MyServices.addDatasource
            add_service MyServices.addCachedStorage
            add_service MyServices.addCache
            add_service AuthXml.addW88AuthConfig

            endpoints MyEndPoints.lists

        }



        0
