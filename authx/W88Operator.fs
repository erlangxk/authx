module authx.W88Operator


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
