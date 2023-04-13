namespace authx

open Falco

module SharedHandlers =

    let badRequest msg : HttpHandler =
        Response.withStatusCode 400 >> Response.ofPlainText $"Bad request:{msg}"

    let serverError: HttpHandler =
        Response.withStatusCode 500 >> Response.ofPlainText "Server Error"
