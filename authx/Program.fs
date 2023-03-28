namespace authx

open Falco
open Falco.HostBuilder
open Microsoft.AspNetCore.Builder


module Program =

    [<EntryPoint>]
    let main args =
        webHost args {
            use_if FalcoExtensions.IsDevelopment DeveloperExceptionPageExtensions.UseDeveloperExceptionPage

            use_ifnot
                FalcoExtensions.IsDevelopment
                (FalcoExtensions.UseFalcoExceptionHandler SharedHandlers.serverError)
            add_service MyServices.addDatasource
            add_service MyServices.addCachedStorage
            add_service MyServices.addCache
            endpoints MyEndPoints.lists
            
        }

        0
