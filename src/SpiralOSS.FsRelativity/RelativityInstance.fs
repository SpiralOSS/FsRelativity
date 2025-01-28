namespace SpiralOSS.FsRelativity

type RelativityCredentials =
| BearerToken of string
| IntegratedAuth
| Token of string
| SecretKey of string
| UsernamePassword of string * string
    with
    member self.AsAccessToken =
        match self with
        | BearerToken token -> token
        | UsernamePassword (user, pass) ->
            System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes ($@"{user}:{pass}"))
        | Token token -> token
        | SecretKey secretKey -> failwith "Unsupported"
        | IntegratedAuth -> failwith "Unsupported"

[<AutoOpen>]
module RelOneInstance =

    type BuilderState = internal {
        Uri: string
        Credentials: RelativityCredentials option
        }

    module BuilderState =
        let defaults =
            { Uri = ""
              Credentials = None }

    type Builder internal () =
        member __.Yield (_) =
            BuilderState.defaults

        [<CustomOperation("uri")>]
        member __.Uri (state, input) : BuilderState  =
            { state with Uri = input }

        [<CustomOperation("auth_user_pass")>]
        member __.AuthUserPass (state, username, password) : BuilderState =
            { state with Credentials = Some (UsernamePassword (username, password)) }
