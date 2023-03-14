namespace authx

open Falco

module SharedHandlers =
    let unauthorized: HttpHandler =
        Response.withStatusCode 401 >> Response.ofPlainText "Unauthorized"

    let forbidden: HttpHandler =
        Response.withStatusCode 403 >> Response.ofPlainText "Forbidden"

    let badRequest: HttpHandler =
        Response.withStatusCode 400 >> Response.ofPlainText "Bad request"

    let serverError: HttpHandler =
        Response.withStatusCode 500 >> Response.ofPlainText "Server Error"
