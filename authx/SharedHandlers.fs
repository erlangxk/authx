namespace authx

open Falco

module SharedHandlers =

    let badRequest msg : HttpHandler =
        Response.withStatusCode 400 >> Response.ofPlainText $"{msg}"

    let serverError msg: HttpHandler =
        Response.withStatusCode 500 >> Response.ofPlainText $"{msg}"
