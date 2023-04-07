namespace authx

open System
open System.IO
open System.Xml
open MyJwtToken
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Options

open W88Operator
open System.Net.Http

module AuthXml =

    let readStr (xml: Stream) =
        use reader = XmlReader.Create(xml)
        reader.MoveToContent() |> ignore

        seq {
            while reader.Read() do
                if (reader.NodeType = XmlNodeType.Element) then
                    let name = reader.Name
                    reader.Read() |> ignore
                    yield (name, reader.Value)
        }
        |> Map.ofSeq


    type Op1AuthResult =
        | Success of UserClaims
        | Failed of string
        | UnknownError of exn

    let parseDict (dict: Map<string, string>) : Op1AuthResult =
        try
            let statusCode = dict.["statusCode"]

            if statusCode = "00" then
                let currency = dict.["currency"]
                let memberId = dict.["memberId"]
                let memberCode = dict.["memberCode"]
                let testAccountStr = dict.["testAccount"]
                let test = testAccountStr = "1"

                let userClaims: UserClaims =
                    [ MyJwtClaims.test, test
                      MyJwtClaims.currency, currency
                      MyJwtClaims.subject, memberId
                      MyJwtClaims.name, memberCode ]

                Success userClaims
            else
                Failed statusCode
        with ex ->
            UnknownError ex

    let parseOp1 xml = xml |> readStr |> parseDict


    let authOp12 (uri: Uri) (httpClientFactory: IHttpClientFactory) =
        task {
            try
                use httpClient = httpClientFactory.CreateClient()
                httpClient.Timeout <- TimeSpan(0, 0, 30)
                let! res = httpClient.PostAsync(uri, null)
                let success = res.EnsureSuccessStatusCode()
                let! stream = success.Content.ReadAsStreamAsync()
                return parseOp1 stream
            with ex ->
                return UnknownError ex
        }

    type OperatorW88Service(option: IOptions<W88Operator>, httpClientFactory: IHttpClientFactory) =
        let w88 = option.Value

        member this.getUserInfo(token: string) =
            let url = MyOperator.buildUri w88 token
            authOp12 url httpClientFactory
