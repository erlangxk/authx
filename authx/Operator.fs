namespace authx

open System
open System.Web

type Operator =
    abstract member Name: string
    abstract member AuthUrl: string
    abstract member Params: string -> list<string * string>

module Operator =
    let buildUri (op: Operator) (token: string) =
        let uriBuilder = UriBuilder(op.AuthUrl)
        let query = HttpUtility.ParseQueryString(String.Empty)

        for k, v in op.Params(token) do
            query.Add(k, v)

        uriBuilder.Query <- query.ToString()
        uriBuilder.Uri
