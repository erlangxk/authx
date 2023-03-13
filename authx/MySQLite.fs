namespace authx

open System.Data
open Donald

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

module MySQLite =
    open Domain

    let readClient (rd: IDataReader) : Client =
        { Id = rd.ReadString "client_id"
          Secret = rd.ReadString "client_secret"
          Enabled = rd.ReadInt32 "enabled" }

    let getClientById (clientId: string) =
        Db.newCommand "SELECT * FROM clients where client_id = @client_id"
        >> Db.setParams [ "client_id", SqlType.String clientId ]
        >> Db.Async.querySingle readClient

    let insertClient (client: Client) =
        Db.newCommand "INSERT INTO clients (client_id, client_secret, enabled) VALUES (@client_id, @client_secret,@enabled)"
        >> Db.setParams
            [ "client_id", SqlType.String client.Id
              "client_secret", SqlType.String client.Secret
              "enabled", SqlType.Int client.Enabled]
        >> Db.Async.exec

    let readOperator (rd: IDataReader) : Operator =
        { Name = rd.ReadString "name"
          AuthUrl = rd.ReadString "auth_url"
          Enabled = rd.ReadInt32 "enabled" }

    let getOperatorByName (name: string) =
        Db.newCommand "select * from operators where name = @name"
        >> Db.setParams [ "name", SqlType.String name ]
        >> Db.Async.querySingle readOperator

    let insertOperator (operator: Operator) =
        Db.newCommand "insert into operators (name, auth_url) values (@name, @auth_url)"
        >> Db.setParams
            [ "name", SqlType.String operator.Name
              "auth_url", SqlType.String operator.AuthUrl ]
        >> Db.exec

    let readOperatorPrincipal (rd: IDataReader) : OperatorPrincipal =
        { OperatorName = rd.ReadString "operator_name"
          ClientId = rd.ReadString "client_id"
          ClientSecret = rd.ReadString "client_secret" }
    
    let getOperatorPrincipalByName (operatorName: string) =
        Db.newCommand "select * from operator_principals where operator_name = @operatorName"
        >> Db.setParams [ "operatorName", SqlType.String operatorName ]
        >> Db.Async.querySingle readOperator

    let insertOperatorPrincipal (operatorPrincipal: OperatorPrincipal) =
        Db.newCommand
            "insert into operator_principals (operator_name, client_id, client_secret) values (@operator_name, @client_id, @client_secret)"
        >> Db.setParams
            [ "operator_name", SqlType.String operatorPrincipal.OperatorName
              "client_id", SqlType.String operatorPrincipal.ClientId
              "client_secret", SqlType.String operatorPrincipal.ClientSecret ]
        >> Db.Async.exec
