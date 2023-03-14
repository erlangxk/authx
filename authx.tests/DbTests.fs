module Tests

open System
open Microsoft.Data.Sqlite
open Xunit
open authx
open authx.Domain

let connStr = "Data Source=/Users/simonking/Microsoft/authx/authx.sqlite;"

let connectionString = "Data Source=MyReadonlyMemoryDb;Mode=Memory;Cache=Shared";

type MySQLiteTests() as self =
    let conn =
        use fileDb = new SqliteConnection(connStr)
        fileDb.Open()
        fileDb
        |> MySQLite.insertClient({Client.Id="first_client"; Secret="first_secret"; Enabled = 0})
        |> ignore
        
        fileDb
        |> MySQLite.insertClient({Client.Id="second_client"; Secret="second_secret"; Enabled = 0})
        |> ignore
        
        fileDb
        |> MySQLite.insertOperator({Operator.Name = "operator_a"; AuthUrl = "auth_url"; Enabled =1})
        |> ignore
        
        fileDb
        |> MySQLite.insertOperatorPrincipal({OperatorPrincipal.OperatorName = "operator_a"; ClientId = "client_id"; ClientSecret ="client_secret"})
        |> ignore
        
        let memoryDb = new SqliteConnection(connectionString)
        memoryDb.Open()
        fileDb.BackupDatabase(memoryDb)
        memoryDb
    
    
    interface IDisposable with
        member this.Dispose() = conn.Dispose()

    [<Fact>]
    member self.``My test``() =
        
        let f = MySQLite.getClientById ("third_client")

        task {
            match! f conn with
            | Ok(None) ->
                 Assert.True(false, "client not found")
            |Ok(Some(client))->
                Assert.Equal("third_secret", client.Secret)
            | Error(err) ->
                Assert.True(false, $"{err}")
        }
