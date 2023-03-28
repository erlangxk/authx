module Tests

open System
open Npgsql
open Xunit
open authx
open authx.Domain


let connectionString =
    "Host=localhost;Database=postgres;Username=postgres;Password=mysecretpassword"

type DatabaseTests() as self =

    let ds = NpgsqlDataSource.Create(connectionString)

    do
        use conn = ds.OpenConnection()

        conn
        |> MyDatabase.insertClient (
            { Client.Id = "first_client"
              Secret = "first_secret"
              Enabled = 0 }
        )
        |> ignore

        conn
        |> MyDatabase.insertClient (
            { Client.Id = "second_client"
              Secret = "second_secret"
              Enabled = 0 }
        )
        |> ignore

        conn
        |> MyDatabase.insertOperator (
            { Operator.Name = "operator_a"
              AuthUrl = "auth_url"
              Enabled = 1 }
        )
        |> ignore


    interface IDisposable with
        member this.Dispose() = ds.Dispose()

    [<Fact>]
    member self.``My test``() =
        let f = MyDatabase.getClientById "first_client"

        task {
            match! f (ds.CreateConnection()) with
            | Ok(None) -> Assert.True(false, "client not found")
            | Ok(Some(client)) -> Assert.Equal("first_secret", client.Secret)
            | Error(err) -> Assert.True(false, $"{err}")
        }
