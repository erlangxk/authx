namespace authx

open Falco
open Falco.HostBuilder
open Microsoft.AspNetCore.Builder

module Program =



    [<EntryPoint>]
    let main args =
        webHost args {
            use_if FalcoExtensions.IsDevelopment DeveloperExceptionPageExtensions.UseDeveloperExceptionPage
            use_ifnot FalcoExtensions.IsDevelopment (FalcoExtensions.UseFalcoExceptionHandler MyEndPoints.serverError)
            endpoints MyEndPoints.lists
        }

        0
