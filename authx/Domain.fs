namespace authx

open System.Threading.Tasks

module Domain =

    type Client =
        { Id: string
          Secret: string
          Enabled: int }

    type Operator =
        { Name: string
          AuthUrl: string
          Enabled: int }

    type OperatorPrincipal =
        { OperatorName: string
          ClientId: string
          ClientSecret: string }

    type IStorage =
        abstract member GetClientById: string -> Task<option<Client>>
        abstract member GetOperatorByName: string -> Task<option<Operator>>
        abstract member GetPrincipalOfOperator: string -> Task<option<OperatorPrincipal>>
