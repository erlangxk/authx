namespace authx

open Autofac
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open System
open System.IO
open System.Xml
open Microsoft.Extensions.Logging
open System.Net.Http
open Microsoft.Extensions.Options

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

module W88Operator =
    
    let interestedKeys = set [
        "statusCode"
        "currency"
        "memberId"
        "memberCode"
        "testAccount"
    ]
    let readXml (xml: Stream) =
        use reader = XmlReader.Create(xml)
        reader.MoveToContent() |> ignore

        seq {
            while reader.Read() do
                if (reader.NodeType = XmlNodeType.Element) then
                    let name = reader.Name
                    if interestedKeys.Contains(name) then
                        let content = reader.ReadElementContentAsString()
                        yield (name, content)
        }
        |> Map.ofSeq

    let parseDict (dict: Map<string, string>) : AuthResult =
        try
            let statusCode = dict["statusCode"]

            if statusCode = "00" then
                let currency = dict["currency"]
                let memberId = dict["memberId"]
                let memberCode = dict["memberCode"]
                let testAccountStr = dict["testAccount"]
                let test = testAccountStr = "1"

                let userClaims: UserClaims =
                    [ JwtClaims.test, test
                      JwtClaims.currency, currency
                      JwtClaims.subject, memberId
                      JwtClaims.name, memberCode ]

                AuthResult.Success userClaims
            else
                AuthResult.Failed statusCode
        with ex ->
            AuthResult.UnknownError ex

    type W88AuthApi(option: IOptions<W88Operator>, httpClientFactory: IHttpClientFactory, log: ILogger<W88AuthApi>) =
        let w88Op = option.Value

        let doAuth (uri: Uri) =
            task {
                try
                    use httpClient = httpClientFactory.CreateClient()
                    httpClient.Timeout <- TimeSpan.FromSeconds(30.0)
                    use! res = httpClient.PostAsync(uri, null)
                    use! stream = res.EnsureSuccessStatusCode().Content.ReadAsStreamAsync()
                    return stream |> (readXml >> parseDict)
                with ex ->
                    log.LogError(ex, "auth error")
                    return AuthResult.UnknownError ex
            }

        interface AuthApi with
            member this.GetUserInfo(token: string) = Operator.buildUri w88Op token |> doAuth

    let configW88Operator (config: IConfiguration, svc: IServiceCollection) =
        let w88 = config.GetSection(W88Operator.Name)
        svc.Configure<W88Operator>(w88) |> ignore

    let registerW88Auth (builder: ContainerBuilder) =
        builder.RegisterType<W88AuthApi>().Named<AuthApi>(W88Operator.Name) |> ignore
