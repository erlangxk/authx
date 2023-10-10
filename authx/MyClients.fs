namespace authx

open System.ComponentModel.DataAnnotations
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Options

[<CLIMutable>]
type Client = {
    [<Required>]
    Id: string
    
    [<Required>]
    Secret: string
}

[<CLIMutable>]
type Clients = {
    [<Required>]
    All: seq<Client>
}

type MyClients(clients: IOptions<Clients>) =
    let all = Map [ for c in clients.Value.All -> c.Id, c.Secret ]
    member _.FindClientSecret(clientId: string) : option<string> = all |> Map.tryFind clientId

module MyClients =
    let configClients (config: IConfiguration, svc: IServiceCollection) =
        let bind clients = config.GetSection(nameof Clients).Bind(clients)
        svc.ConfigureAndValidate<Clients>(bind).AddSingleton<MyClients>() |> ignore
