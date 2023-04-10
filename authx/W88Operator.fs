module authx.W88Operator

open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open System
open System.IO
open System.Xml
open MyJwtToken
open System.Net.Http
open Microsoft.Extensions.Options
open MyOperator

[<CLIMutable>]
type W88Operator =
    { Url: string
      OperatorId: string
      Wallet: string
      SecretKey: string }

    static member Name = "w88"

    interface Operator with
        member this.AuthUrl = this.Url
        member this.Name = W88Operator.Name

        member this.Params(token: string) =
            [ "OperatorId", this.OperatorId
              "Wallet", this.Wallet
              "secretkey", this.SecretKey
              "token", token ]

let configW88Operator (config: IConfiguration, svc: IServiceCollection) =
    let w88 = config.GetSection(W88Operator.Name)
    svc.Configure<W88Operator>(w88) |> ignore


let readXml (xml: Stream) =
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

let parseDict (dict: Map<string, string>) : AuthResult =
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

let authImpl (uri: Uri) (httpClientFactory: IHttpClientFactory) =
    task {
        try
            use httpClient = httpClientFactory.CreateClient()
            httpClient.Timeout <- TimeSpan(0, 0, 30)
            let! res = httpClient.PostAsync(uri, null)
            let success = res.EnsureSuccessStatusCode()
            let! stream = success.Content.ReadAsStreamAsync()
            return stream |> (readXml >> parseDict)
        with ex ->
            return UnknownError ex
    }

type W88AuthApi(option: IOptions<W88Operator>, httpClientFactory: IHttpClientFactory) =
    let w88 = option.Value

    interface AuthApi with
        member this.getUserInfo(token: string) =
            let url = buildUri w88 token
            authImpl url httpClientFactory
