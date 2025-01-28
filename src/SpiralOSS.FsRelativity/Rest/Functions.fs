module SpiralOSS.FsRelativity.Rest.Functions

open System
open System.Net
open System.Net.Http
open System.IO
open System.Net.Http.Headers
open System.Net.Security
open System.Text
open Microsoft.FSharp.Control
open SpiralOSS.FsRelativity

[<Measure>] type artifactId
[<Measure>] type artifactTypeId

type RelOneInstance =
    { Uri: Uri
      Version: Version
      HttpClient: HttpClient }
    with
    interface IDisposable with
        member this.Dispose() = this.HttpClient.Dispose()

type RelOneField =
    | FieldRef of int
    | Full of string

type RelOneWorkspace =
    { ArtifactId:int
      Name:string }

type RelOneQueryCondition =
    | Empty
    | Basic of string

type RelOneObjectType =
    | Documents
    | Other of int<artifactTypeId>

type RelOneQueryRequest =
    { ObjectType: RelOneObjectType
      Fields: RelOneField
      Condition: RelOneQueryCondition }

type GetRelOneInstance = Uri -> RelativityCredentials -> Async<RelOneInstance>
type GetWorkspaceWithArtifactId = RelOneInstance -> int<artifactId> -> RelOneWorkspace
type GetWorkspaceWithName =  RelOneInstance -> string -> RelOneWorkspace

type RelOneConnection =
    | UriAndCredentials of relOneUri: Uri * credentials: RelativityCredentials
    | HttpClient of HttpClient
    | RelOneInstance of RelOneInstance

let getRelOneHttpClient uri (credentials:RelativityCredentials) =
    let httpHandler = new SocketsHttpHandler()
    httpHandler.PooledConnectionLifetime <- TimeSpan.FromMinutes 15
    let sslClientAuthOptions = SslClientAuthenticationOptions()
    sslClientAuthOptions.RemoteCertificateValidationCallback <- (fun sender cert chain policyErrors -> true)
    httpHandler.SslOptions <- sslClientAuthOptions

    let httpClient = new HttpClient()
    httpClient.BaseAddress <- uri
    httpClient.Timeout <- TimeSpan.FromMinutes 10
    httpClient.DefaultRequestHeaders.Add("X-CRSF-Header", "-")
    httpClient.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue("application/json"))
    httpClient.DefaultRequestHeaders.Add("Authorization", credentials.AsAccessToken)
    httpClient


let rec getRelOneInstanceVersion connection : Async<Result<string, string>> =
    async {
        match connection with
        | UriAndCredentials (uri, relOneCredentials) ->
            let httpClient = getRelOneHttpClient uri relOneCredentials
            return! getRelOneInstanceVersion (RelOneConnection.HttpClient httpClient)
        | RelOneInstance relOneInstance ->
            let httpClient = relOneInstance.HttpClient
            return! getRelOneInstanceVersion (RelOneConnection.HttpClient httpClient)
        | HttpClient httpClient ->
            let path = @"relativity.rest/api/Relativity.Services.InstanceDetails.IInstanceDetailsModule/InstanceDetailsService/GetRelativityVersionAsync"
            use stringContent = new StringContent(String.Empty, Encoding.UTF8, "application/json")
            let! response = httpClient.PostAsync(path, stringContent) |> Async.AwaitTask
            let result =
                if response.StatusCode = HttpStatusCode.Unauthorized then
                    Error "Unauthorized"
                elif not response.IsSuccessStatusCode then
                    Error (response.StatusCode.ToString())
                else
                    let text = response.Content.ReadAsStringAsync() |> Async.AwaitTask |> Async.RunSynchronously
                    Ok text
            return result
    }

let getRelOneInstance: GetRelOneInstance = fun uri creds ->
    async {

        return { Uri = uri; Version = Version(); HttpClient = null }
    }

let getRelativityInstanceVersion (relativityUri:Uri) =
    task {
        use client = new HttpClient()

        let! response = client.GetStringAsync(relativityUri)
        do! File.WriteAllTextAsync("response", response)
    } |> Async.AwaitTask