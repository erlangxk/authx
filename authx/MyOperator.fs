module authx.MyOperator

open System
open System.Web

type Operator =
    abstract member Name: string
    abstract member AuthUrl: string
    abstract member Params: string -> list<string * string>

let buildUri (operator: Operator) (token: string) =
    let uriBuilder = UriBuilder(operator.AuthUrl)
    let query = HttpUtility.ParseQueryString(String.Empty)

    for k, v in operator.Params(token) do
        query.Add(k, v)

    uriBuilder.Query <- query.ToString()
    uriBuilder.Uri
