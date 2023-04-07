module authx.MyClients


[<CLIMutable>]
type Client = { Id: string; Secret: string }


[<CLIMutable>]
type Clients = { All: seq<Client> }



type MyClients() =

    member this.AllClients: Map<string, Client> = Map.empty
