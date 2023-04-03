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
          Params: option<string>
          Enabled: int
          Pipeline: int }

    type IStorage =
        abstract member GetClientById: string -> Task<option<Client>>
        abstract member GetOperatorByName: string -> Task<option<Operator>>
