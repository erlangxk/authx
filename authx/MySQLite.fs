namespace authx

open System.Data
open Microsoft.Data.Sqlite
open Donald

module Domain =
    type Client =
        { Id: string
          Secret: string
          Enabled: bool }

    type Operator =
        { Name: string
          AuthUrl: string
          Enabled: bool }

    type OperatorPrincipal =
        { Name: string
          ClientId: string
          ClientSecret: string }

module MySQLite =
    open Domain

    let readClient (rd: IDataReader) : Client =
        { Id = rd.ReadString "client_id"
          Secret = rd.ReadString "client_secret"
          Enabled = rd.ReadBoolean "enabled" }

    let readOperator (rd: IDataReader) : Operator =
        { Name = rd.ReadString "name"
          AuthUrl = rd.ReadString "auth_url"
          Enabled = rd.ReadBoolean "enabled" }

    let readOperatorPrincipal (rd: IDataReader) : Operator =
        { Name = rd.ReadString "name"
          AuthUrl = rd.ReadString "client_id"
          Enabled = rd.ReadBoolean "client_secret" }

    let getClientById (clientId: string) (conn: SqliteConnection) =
        let sql = "SELECT * FROM clients where client_id = @client_id"

        conn
        |> Db.newCommand sql
        |> Db.setParams [ "client_id", SqlType.String clientId ]
        |> Db.Async.querySingle readClient

    let getOperatorByName (name: string) (conn: SqliteConnection) =
        let sql = "select * from operators where name = @name"

        conn
        |> Db.newCommand sql
        |> Db.setParams [ "name", SqlType.String name ]
        |> Db.Async.querySingle readOperator

    let getOperatorPrincipalByName (name: string) (conn: SqliteConnection) =
        let sql = "select * from operator_principals where name = @name"

        conn
        |> Db.newCommand sql
        |> Db.setParams [ "name", SqlType.String name ]
        |> Db.Async.querySingle readOperator
