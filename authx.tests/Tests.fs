module Tests

open System
open Microsoft.Data.Sqlite
open Xunit
open authx.MySQLite

let connStr = "Data Source=/Users/simonking/Microsoft/authx/authx.sqlite;"

type MySQLiteTests() as self =
    let conn = new SqliteConnection(connStr)
        
    interface IDisposable with
        member this.Dispose() = conn.Dispose()

    [<Fact>]
    member self.``My test``() =
        let f = getClientById ("first_client")

        task {
            match! f conn with
            | Ok(client) -> Assert.Equal("first_secret", client.Value.Secret)
            | Error(err) -> Assert.True(false)
        }
