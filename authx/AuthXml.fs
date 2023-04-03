namespace authx

open System.IO
open System.Xml
open Oryx
open MyJwtToken
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open FsConfig
open System.Net.Http

module AuthXml =
    exception MyConfigError of string

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

    let parseOp1 xml = xml |> readStr |> parseDict



    type W88AuthTokenConfig =
        { Url: string
          OperatorId: string
          Wallet: string
          SecretKey: string }

        member this.QueryParams(token:string) =
            seq {
                struct ("OperatorId", this.OperatorId)
                struct ("Wallet", this.Wallet)
                struct ("secretkey", this.SecretKey)
                struct ("token", token)
            }

    let authOp1 url query client =
        let pipeline =
            httpRequest
            |> POST
            |> withHttpClient client
            |> withUrl url
            |> withQuery query
            |> fetch
            |> Oryx.HttpHandler.parse parseOp1

        task {
            match! runAsync pipeline with
            | Ok r -> return r
            | Error exn -> return UnknownError exn
        }



    type W88AuthConfig(env: IHostEnvironment, logger: ILogger<W88AuthConfig>) =
        let fileName = if env.IsDevelopment() then "w88UAT.json" else "w88.json"

        let cb =
            ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile(fileName)
                .Build()

        let appConfig =
            match AppConfig(cb).Get<W88AuthTokenConfig>() with
            | Ok config -> config
            | Error err ->
                let info = err.ToString()
                logger.LogError("failed loading {fileName}, error is {error}", fileName, info)
                raise (MyConfigError info)

        member this.AppConfig = appConfig


    //TODO make this service to accept the options to specify the file name
    // example: like JWT authentication


    type W88AuthService(config: W88AuthConfig, httpClient: HttpClient) =
        let cfg = config.AppConfig

        member this.getUserInfo(token:string) =
            authOp1 cfg.Url (cfg.QueryParams token) httpClient

    let addW88AuthConfig (svc: IServiceCollection) =
        
     
        svc
            .AddSingleton<W88AuthConfig, W88AuthConfig>()
            .AddSingleton<W88AuthService, W88AuthService>()
            
    
