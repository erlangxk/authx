namespace authx

open System.Data
open Donald
open Domain

module MyDatabase =

    let readClient (rd: IDataReader) : Client =
        { Id = rd.ReadString "client_id"
          Secret = rd.ReadString "client_secret"
          Enabled = rd.ReadInt32 "enabled" }

    let getClientById (clientId: string) =
        Db.newCommand "SELECT * FROM clients where client_id = @client_id"
        >> Db.setParams [ "client_id", SqlType.String clientId ]
        >> Db.Async.querySingle readClient

    let loadAllClients conn =
        conn |> Db.newCommand "SELECT * FROM clients" |> Db.Async.query readClient

    let insertClient (client: Client) =
        Db.newCommand
            "INSERT INTO clients (client_id, client_secret, enabled) VALUES (@client_id, @client_secret,@enabled)"
        >> Db.setParams
            [ "client_id", SqlType.String client.Id
              "client_secret", SqlType.String client.Secret
              "enabled", SqlType.Int client.Enabled ]
        >> Db.Async.exec

    let readOperator (rd: IDataReader) : Operator =
        { Name = rd.ReadString "name"
          AuthUrl = rd.ReadString "auth_url"
          Enabled = rd.ReadInt32 "enabled"
          Params = rd.ReadStringOption "params"
          Pipeline = rd.ReadInt32 "pipeline" }

    let getOperatorByName (name: string) =
        Db.newCommand "select * from operators where name = @name"
        >> Db.setParams [ "name", SqlType.String name ]
        >> Db.Async.querySingle readOperator

    let loadAllOperators conn =
        conn |> Db.newCommand "select * from operators" |> Db.Async.query readOperator

    let insertOperator (operator: Operator) =
        Db.newCommand
            "insert into operators (name, auth_url, pipeline, params) values (@name, @auth_url, @pipeline, @params)"
        >> Db.setParams
            [ "name", SqlType.String operator.Name
              "auth_url", SqlType.String operator.AuthUrl
              "pipeline", SqlType.Int32 operator.Pipeline
              "params",
              match operator.Params with
              | None -> SqlType.Null
              | Some p -> SqlType.String p ]
        >> Db.exec
