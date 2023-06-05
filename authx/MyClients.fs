namespace authx

open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Options

[<CLIMutable>]
type Client = { Id: string; Secret: string }

[<CLIMutable>]
type Clients = { All: seq<Client> }

type MyClients(clients: IOptions<Clients>) =
    let all = Map [ for c in clients.Value.All -> c.Id, c.Secret ]
    member _.FindClientSecret(clientId: string) : option<string> = all |> Map.tryFind clientId

module MyClients =
    let configClients (config: IConfiguration, svc: IServiceCollection) =
        let clients = config.GetSection(nameof Clients)
        svc.Configure<Clients>(clients).AddSingleton<MyClients>() |> ignore
