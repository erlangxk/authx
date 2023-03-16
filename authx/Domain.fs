namespace authx

module Domain =
    type PrimaryKey =
        abstract Key: string

    type Client =
        { Id: string
          Secret: string
          Enabled: int }

        interface PrimaryKey with
            member this.Key = this.Id

    type Operator =
        { Name: string
          AuthUrl: string
          Enabled: int }

        interface PrimaryKey with
            member this.Key = this.Name

    type OperatorPrincipal =
        { OperatorName: string
          ClientId: string
          ClientSecret: string }

        interface PrimaryKey with
            member this.Key = this.OperatorName

    let getKey<'T when 'T :> PrimaryKey> (t: 'T) = t.Key

    type IStorage =
        abstract member GetClientById: string -> option<Client>
        abstract member GetOperatorByName: string -> option<Operator>
        abstract member GetPrincipalOfOperator: string -> option<OperatorPrincipal>

    let createStorage
        (allClients: Map<string, Client>)
        (allOperators: Map<string, Operator>)
        (allOperatorPrincipals: Map<string, OperatorPrincipal>)
        : IStorage =

        { new IStorage with
            member this.GetClientById(clientId: string) = allClients |> Map.tryFind clientId
            member this.GetOperatorByName(name: string) = allOperators |> Map.tryFind name

            member this.GetPrincipalOfOperator(operator: string) =
                allOperatorPrincipals |> Map.tryFind operator }
