namespace SpiralOSS.FsRelativity.Export

open SpiralOSS.FsRelativity
open System
open Relativity.Export.V1.Model
open System.Collections.Generic

module ExportRelativityServices =

    module internal Builders =

        //////////////////////////////////////////////////////////////////////////////
        // EXPORT ARTIFACT SETTINGS
        let buildExportArtifactSettings (state:ExportSettings.BuilderState) =
            let bld = Relativity.Export.V1.Builders.ExportSettings.ExportArtifactSettingsBuilder.Create()

            // FILE NAME PATTERN
            let bld =
                let rec applyTokens (pbs: Relativity.Export.V1.Builders.ExportSettings.IPatternBuilderStep) (tokens:CustomFilenameToken list) =
                    match tokens with
                    | [] -> pbs
                    | token :: tokens ->
                        let pbs =
                            match token with
                            | CustomFilenameToken.Identifier -> pbs.AppendIdentifier()
                            | CustomFilenameToken.ProductionBeginBates -> pbs.AppendProductionBeginBates()
                            | CustomFilenameToken.OriginalFilename -> pbs.AppendOriginalFileName()
                            | CustomFilenameToken.Text text -> pbs.AppendCustomText(text)
                        applyTokens pbs tokens

                match state.FileNamingFormat with
                | Default -> bld.WithDefaultFileNamePattern()
                | Identifier -> bld.WithNamingAfterIdentifierPattern()
                | CustomFilename pattern -> bld.WithCustomFileNamePattern(pattern)
                | CustomPattern customPattern ->
                    (applyTokens (bld.WithCustomPatternBuilder(customPattern.Separator)) customPattern.Tokens).BuildPattern()

            // IMAGES - Naming
            let bld =
                match state.ImageSettings |> Option.map (_.ApplyFilenamePattern) with
                | Some true -> bld.ApplyFileNamePatternToImages()
                | _ -> bld.WithoutApplyingFileNamePatternToImages()

            // IMAGES - Output
            let bld =
                match state.ImageSettings with
                | None -> bld.WithoutExportingImages()
                | Some imageSettings -> bld.ExportImages(fun settings ->
                    settings
                        .WithImagePrecedenceArtifactIDs(System.Collections.Generic.List<int> imageSettings.Precedence)
                        .WithTypeOfImage(
                            match imageSettings.OutputFormat with
                            | Pdf -> Relativity.Export.V1.Model.ExportJobSettings.ImageType.Pdf
                            | SinglePage -> Relativity.Export.V1.Model.ExportJobSettings.ImageType.SinglePage
                            | MultiPage -> Relativity.Export.V1.Model.ExportJobSettings.ImageType.MultiPageTiff
                            )
                        |> ignore
                    )

            // TEXT
            let bld =
                match state.TextSettings with
                | None -> bld.WithoutExportingFullText()
                | Some textSettings ->
                    bld.ExportFullText(fun settings ->
                        settings
                            .ExportFullTextAsFile()
                            .WithTextFileEncoding(textSettings.Encoding)
                            .WithPrecedenceFieldsArtifactIDs(System.Collections.Generic.List<int> textSettings.Precedence)
                        |> ignore
                        )

            // NATIVES
            let bld =
                match state.NativeSettings with
                | None -> bld.WithoutExportingNative()
                | Some nativeSettings ->
                    bld.ExportNative(fun settings ->
                        settings
                            .WithNativePrecedenceArtifactIDs(System.Collections.Generic.List<int> nativeSettings.Precedence)
                        |> ignore
                        )

            // PDFS
            let bld =
                match state.PdfSettings with
                | true -> bld.ExportPdf()
                | false -> bld.WithoutExportingPdf()

            // FIELDS - Artifact Ids
            let bld =
                match state.FieldSettings with
                | None -> bld.WithFieldArtifactIDs(System.Collections.Generic.List<int> [])
                | Some fieldSettings -> bld.WithFieldArtifactIDs(System.Collections.Generic.List<int> fieldSettings.FieldArtifactIds)

            // FIELDS - Nested Multi Choice
            let bld =
                match state.FieldSettings |> Option.map (_.NestMultiChoiceFields) with
                | Some true -> bld.ExportMultiChoicesAsNested()
                | _ -> bld.WithoutExportingMultiChoicesAsNested()

            bld.Build()

        //////////////////////////////////////////////////////////////////////////////
        // LOAD FILE SETTINGS
        let buildLoadFileSettings (state:LoadFileSettings.BuilderState) =
            let bld = Relativity.Export.V1.Builders.ExportSettings.LoadFileSettingsBuilder.Create()

            let bld = bld.WithoutExportingMsAccess()

            let bld = bld.WithCustomCultureInfo(state.Culture)

            let bld =
                match state.DateFormat with
                | None -> bld.WithDefaultDateTimeFormat()
                | Some dateFormat -> bld.WithCustomDateTimeFormat(dateFormat)

            let bld =
                match state.RecordFormat, state.RecordColumnFormat with
                | a,b when a=CsvFormat && b=DefaultColumnFormat -> bld.WithLoadFileFormat(Relativity.Export.V1.Model.ExportJobSettings.LoadFileFormat.CSV)
                | a,b when a=ConcordanceFormat && b=DefaultColumnFormat -> bld.WithLoadFileFormat(Relativity.Export.V1.Model.ExportJobSettings.LoadFileFormat.DAT)
                | _ -> bld.WithLoadFileFormat(Relativity.Export.V1.Model.ExportJobSettings.LoadFileFormat.TXT)

            let bld = bld.WithEncoding(state.Encoding)

            let bld = bld.WithImageLoadFileFormat(state.ImageLoadfileFormat.ToRelativityImageLoadFileFormat)

            let bld = bld.WithPdfFileFormat(state.PdfLoadfileFormat.ToRelativityPdfLoadFileFormat)

            let bld = bld.WithDelimiterSettings(fun delimSettings ->
                delimSettings
                    .WithCustomRecordDelimiters(state.RecordFormat.Column)
                    .WithQuoteDelimiter(state.RecordFormat.Quote)
                    .WithNewLineDelimiter(state.RecordColumnFormat.Newline)
                    .WithNestedValueDelimiter(state.RecordColumnFormat.NestedValue)
                    .WithMultiValueDelimiter(state.RecordColumnFormat.MultiValue)
                |> ignore
                )

            bld.Build()

        //////////////////////////////////////////////////////////////////////////////
        // VOLUME SETTINGS
        let buildVolumeSettings (state:FolderingSettings.BuilderState) =
            Relativity.Export.V1.Builders.ExportSettings.VolumeSettingsBuilder.Create()
                .WithVolumePrefix(state.VolumePrefix)
                .WithVolumeStartNumber(state.VolumeStartNumber)
                .WithVolumeMaxSizeInMegabytes(state.VolumeSizeInMB)
                .WithVolumeDigitPadding(state.VolumePadding)
                .Build()

        //////////////////////////////////////////////////////////////////////////////
        // SUB DIRECTORY SETTINGS
        let buildSubdirectorySettings (state:FolderingSettings.BuilderState) =
            Relativity.Export.V1.Builders.ExportSettings.SubdirectorySettingsBuilder.Create()
                .WithSubdirectoryStartNumber(state.StartNumber)
                .WithMaxNumberOfFilesInDirectory(state.MaxFilesInDirectory)
                .WithImageSubdirectoryPrefix(state.ImageFolderPrefix)
                .WithNativeSubdirectoryPrefix(state.NativeFolderPrefix)
                .WithFullTextSubdirectoryPrefix(state.TextFolderPrefix)
                .WithPdfSubdirectoryPrefix(state.PdfFolderPrefix)
                .WithSubdirectoryDigitPadding(state.SubFolderPadding)
                .Build()

        //////////////////////////////////////////////////////////////////////////////
        // EXPORT SOURCE SETTINGS
        let buildExportSourceSettings (state:SourceSettings.BuilderState) =
            let bld = Relativity.Export.V1.Builders.ExportSettings.ExportSourceSettingsBuilder.Create()

            let bld =
                match state.Source with
                | Some (FolderId (artifactId, viewId, includeSubfolders)) ->
                    match includeSubfolders with
                    | true -> bld.FromFolder(artifactId, viewId).WithSubfolders()
                    | false -> bld.FromFolder(artifactId, viewId)
                | Some (ObjectsId (artifactId, viewId)) -> bld.FromObjects(artifactId, viewId)
                | Some (ProductionId artifactId) -> bld.FromProduction(artifactId)
                | Some (SavedSearchId artifactId) -> bld.FromSavedSearch(artifactId)
                | _ -> failwith "No 'source' provided"

            let bld = bld.WithCustomStartAtDocumentNumber(state.DocumentStartNumber)

            bld.Build()

    module internal Utility =

        type AuthorizationProvider (relativityInstance:RelOneInstance.BuilderState) =
            member _.Instance = relativityInstance
            member _.Uri = System.Uri relativityInstance.Uri
            interface Relativity.Transfer.SDK.Interfaces.Authentication.IRelativityAuthenticationProvider with
                member self.BaseAddress = self.Uri
                member self.GetCredentialsAsync cancellationToken =
                    let token =
                        match self.Instance.Credentials with
                        | Some creds -> creds.ToAccessToken
                        | _ -> failwith "No credentials provided"
                    let relCreds = Relativity.Transfer.SDK.Interfaces.Authentication.RelativityCredentials (token, self.Uri)
                    System.Threading.Tasks.Task.FromResult (relCreds)

        let captureWithRetry (job:unit -> Async<'T>) = async {
            let mutable result = Error (exn "")
            let mutable retryJob = 0

            while result |> Result.isError && retryJob < 3 do
                try
                    let! response = job ()
                    if response = null then
                        failwith "Invalid response"
                    result <- Ok response
                with
                | ex ->
                    do! Async.Sleep 1000
                    retryJob <- retryJob + 1
                    result <- Error ex

            return result
            }

    type ExportJobState =
    | WaitingToBeCreated
    | ValidationErrors of Dictionary<string, string>
    | FailedCreation of errorCode: string * errorMessage: string
    | ServerCommunicationError of errorMessage: string
    | New
    | Scheduled
    | Running
    | Completed
    | CompletedWithErrors of errorCode: string * errorMessage: string
    | FailedExport of errorCode: string * errorMessage: string
    | Cancelled
    | Transferring
        with
        member self.IsInFinalState =
            match self with
            | WaitingToBeCreated | ValidationErrors _ | New | Scheduled | Running | Transferring | ServerCommunicationError _ -> false
            | Completed | CompletedWithErrors _ | FailedCreation _ | FailedExport _ | Cancelled -> true

    type ExportJobContext = private {
        Instance: RelOneInstance.BuilderState
        WorkspaceId: int
        JobId: System.Guid
        SourceSettings: SourceSettings.BuilderState
        ExportSettings: ExportSettings.BuilderState
        OutputPath: string
        JobState: ExportJobState
        JobName: string
        JobManager: Relativity.Export.V1.IExportJobManager option
        StagingPath: string option
        }
        with
        member self.ExportOutputSettings =
             Relativity.Export.V1.Builders.ExportSettings.ExportOutputSettingsBuilder.Create()
                .WithoutArchiveCreation()
                .WithDefaultFolderStructure()
                .WithoutTransferJobID()
                .WithDefaultDestinationPath()
                .WithSubdirectorySettings(Builders.buildSubdirectorySettings self.ExportSettings.FolderSettings)
                .WithVolumeSettings(Builders.buildVolumeSettings self.ExportSettings.FolderSettings)
                .WithLoadFileSettings(Builders.buildLoadFileSettings self.ExportSettings.LoadfileSettings)
                .Build()

        member self.ExportJobSettings =
            Relativity.Export.V1.Builders.ExportSettings.ExportJobSettingsBuilder.Create()
                .WithExportSourceSettings(Builders.buildExportSourceSettings self.SourceSettings)
                .WithExportArtifactSettings(Builders.buildExportArtifactSettings self.ExportSettings)
                .WithExportOutputSettings(self.ExportOutputSettings)
                .Build()

    module Result =
        let collapse (self:Result<'T,'T>) : 'T =
            match self with
            | Ok value -> value
            | Error value -> value

    module ExportJobContext =

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        let createContext
            (relativityInstance:RelOneInstance.BuilderState)
            (workspaceId:int)
            (sourceSettings:SourceSettings.BuilderState)
            (exportSettings:ExportSettings.BuilderState)
            (jobName:string)
            (outputPath:string) =
                {
                    Instance = relativityInstance
                    WorkspaceId = workspaceId
                    JobId = System.Guid.NewGuid ()
                    SourceSettings = sourceSettings
                    ExportSettings = exportSettings
                    OutputPath = outputPath
                    JobState = WaitingToBeCreated
                    JobName = jobName
                    JobManager = None
                    StagingPath = None
                }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        let createJobManager (instance:RelOneInstance.BuilderState) =
            try
                let relativityRestUri = System.Uri $"{instance.Uri}/relativity.rest/api"
                let credentials =
                    match instance.Credentials with
                    | Some (UsernamePassword (username, password)) -> Relativity.Services.ServiceProxy.UsernamePasswordCredentials (username, password)
                    | _ -> raise (System.NotImplementedException ("Authorization type not implemented"))

                let serviceFactorySettings = Relativity.Services.ServiceProxy.ServiceFactorySettings (relativityRestUri, credentials)
                let serviceFactory = Relativity.Services.ServiceProxy.ServiceFactory (serviceFactorySettings)
                Ok (serviceFactory.CreateProxy<Relativity.Export.V1.IExportJobManager>())
            with
            | ex -> Error ex.Message

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        let private addJobManager (exportJobContext:ExportJobContext) =
            match exportJobContext.JobManager with
            | Some _ -> Ok exportJobContext
            | _ ->
                match createJobManager exportJobContext.Instance with
                | Ok jobManager -> Ok { exportJobContext with JobManager = Some jobManager }
                | Error errorMsg -> Error (FailedCreation ("-1", errorMsg))

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        let createJob (exportJobContext:ExportJobContext) : Async<ExportJobContext> =
            exportJobContext |> Ok
            |> Result.bind (fun exportJobContext ->
                match exportJobContext.JobState with
                | WaitingToBeCreated -> Ok exportJobContext
                | _ -> failwith "Trying to Create a job not in a WaitingToBeCreated state"
                )
            |> Result.bind addJobManager
            |> Result.bind (fun exportJobContext ->
                match exportJobContext.JobManager with
                | Some jobManager ->
                    Ok <| async {
                        let! responseResult = Utility.captureWithRetry (fun () -> jobManager.CreateAsync (exportJobContext.WorkspaceId, exportJobContext.JobId, exportJobContext.ExportJobSettings, "FsRelativity", exportJobContext.JobName) |> Async.AwaitTask)
                        match responseResult with
                        | Error ex -> return { exportJobContext with JobState = ServerCommunicationError ex.Message }
                        | Ok job ->
                            if job.IsSuccess then
                                return { exportJobContext with JobState = New }
                            elif (job.Value.ValidationErrors.Count > 0) then
                                return { exportJobContext with JobState = ValidationErrors (job.Value.ValidationErrors) }
                            else
                                return { exportJobContext with JobState = FailedCreation (job.ErrorCode, job.ErrorMessage) }
                        }
                | _ -> Error (FailedCreation ("-1", "No Job Manager"))
                )
            |> Result.mapError (fun errorState -> async { return { exportJobContext with JobState = errorState } })  // convert error into Async<ExportJobContext>
            |> Result.collapse

        let validString = System.String.IsNullOrEmpty >> not

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        let startJob (exportJobContext:ExportJobContext) : Async<ExportJobContext> =
            exportJobContext |> Ok
            |> Result.bind (fun exportJobContext ->
                match exportJobContext.JobState with
                | New -> Ok exportJobContext
                | _ -> failwith "Trying to Start a job not in a New state"
                )
            |> Result.bind (fun exportJobContext ->
                match exportJobContext.JobManager with
                | Some jobManager ->
                    Ok <| async {
                        let! responseResult = Utility.captureWithRetry (fun () -> jobManager.StartAsync (exportJobContext.WorkspaceId, exportJobContext.JobId) |> Async.AwaitTask)
                        match responseResult with
                        | Error ex -> return { exportJobContext with JobState = ServerCommunicationError ex.Message }
                        | Ok startResponse ->
                            if (validString startResponse.ErrorMessage) then
                                return { exportJobContext with JobState = FailedExport (startResponse.ErrorCode,startResponse.ErrorMessage) }
                            else
                                return { exportJobContext with JobState = Scheduled }
                            }
                | _ -> Error (FailedExport ("-1", "No Job Manager"))
                )
            |> Result.mapError (fun errorState -> async { return { exportJobContext with JobState = errorState } })  // convert error into Async<ExportJobContext>
            |> Result.collapse

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        let exportJobToExportJobStatus (exportJob:Relativity.Export.V1.Model.ExportJob) =
            match exportJob.JobStatus with
            | ExportStatus.Cancelled -> ExportJobState.Cancelled
            | ExportStatus.Completed -> ExportJobState.Completed
            | ExportStatus.CompletedWithErrors -> ExportJobState.CompletedWithErrors (exportJob.ErrorCode, exportJob.ErrorMessage)
            | ExportStatus.Failed -> ExportJobState.FailedExport (exportJob.ErrorCode, exportJob.ErrorMessage)
            | ExportStatus.New -> ExportJobState.New
            | ExportStatus.Running -> ExportJobState.Running
            | ExportStatus.Scheduled -> ExportJobState.Scheduled
            | ExportStatus.Transferring -> ExportJobState.Transferring
            | _ -> failwith "Unkown Relativity Job Status"

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        let isInFinalState (exportJobContext:ExportJobContext) : bool = exportJobContext.JobState.IsInFinalState

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        let updateJobState (exportJobContext:ExportJobContext) : Async<ExportJobContext> =
            exportJobContext |> Ok
            |> Result.bind (fun exportJobContext ->
                if exportJobContext |> isInFinalState |> not
                then Ok exportJobContext
                else Error exportJobContext.JobState  // the job is complete; we don't need to ask the server
                )
            |> Result.bind (fun exportJobContext ->
                match exportJobContext.JobManager with
                | Some jobManager ->
                    Ok <| async {
                        let! responseResult = Utility.captureWithRetry (fun () -> jobManager.GetAsync(exportJobContext.WorkspaceId, exportJobContext.JobId) |> Async.AwaitTask)
                        match responseResult with
                        | Error ex -> return { exportJobContext with JobState = (ServerCommunicationError ex.Message) }
                        | Ok response ->
                            return
                                { exportJobContext with
                                    JobState = exportJobToExportJobStatus response.Value
                                    StagingPath = Some response.Value.OutputUrl }
                        }
                | _ -> Error (FailedCreation ("-1", "No Job Manager"))
                )
            |> Result.mapError (fun errorState -> async { return { exportJobContext with JobState = errorState } })  // convert error into Async<ExportJobContext>
            |> Result.collapse

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        let close (exportJobContext:ExportJobContext) : Async<ExportJobContext> =
            match exportJobContext.JobManager with
            | Some jobManager ->
                jobManager.Dispose ()
                async { return { exportJobContext with JobManager = None } }
            | _ -> async { return exportJobContext }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        let awaitJobCompletion (exportJobContext:ExportJobContext) : Async<ExportJobContext> = async {
            let mutable exportJobContext' = exportJobContext
            while exportJobContext' |> isInFinalState |> not do
                let! response = updateJobState exportJobContext'
                exportJobContext' <- response
                if exportJobContext' |> isInFinalState |> not then
                    do! Async.Sleep 2000

            let! exportJobContext' = close exportJobContext'
            return exportJobContext'
            }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        let transfer (exportJobContext:ExportJobContext) : Async<ExportJobContext> = async {
            let createLocalPath () =
                let datetime = System.DateTime.Now.ToShortTimeString().Replace(':', '_')
                let create = System.IO.Directory.CreateDirectory($@"C:\temp\{datetime}")
                create.FullName

            let stagingPath =
                match exportJobContext.StagingPath with
                | Some stagingPath -> stagingPath
                | _ -> failwith "No staging path"

            let sourceDirectory = Relativity.Transfer.SDK.Interfaces.Paths.DirectoryPath (stagingPath)
            let destinationDirectory = Relativity.Transfer.SDK.Interfaces.Paths.DirectoryPath (createLocalPath ())

            let transferClient =
                Relativity.Transfer.SDK.TransferClientBuilder.FullPathWorkflow
                    .WithAuthentication(Utility.AuthorizationProvider (exportJobContext.Instance))
                    .WithClientName("FsRelativity")
                    .Build()

            use cancellationTokenSource = (new System.Threading.CancellationTokenSource())
            let! result = transferClient.DownloadDirectoryAsync(Guid.NewGuid(), sourceDirectory, destinationDirectory, cancellationTokenSource.Token) |> Async.AwaitTask

            return exportJobContext
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        let bind (func:ExportJobContext -> Async<ExportJobContext>) (exportJobContext:Async<ExportJobContext>) : Async<ExportJobContext> =
            let exportJobContext' = exportJobContext |> Async.RunSynchronously
            func exportJobContext'

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    let executeJob
        (relativityInstance:RelOneInstance.BuilderState)
        (workspaceId:int)
        (sourceSettings:SourceSettings.BuilderState)
        (exportSettings:ExportSettings.BuilderState)
        (jobName:string)
        (outputPath:string) =
            async { return ExportJobContext.createContext relativityInstance workspaceId sourceSettings exportSettings jobName outputPath }
            |> ExportJobContext.bind ExportJobContext.createJob
            |> ExportJobContext.bind ExportJobContext.startJob
            |> ExportJobContext.bind ExportJobContext.awaitJobCompletion
            |> ExportJobContext.bind ExportJobContext.transfer

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    let listJobs
        (relativityInstance:RelOneInstance.BuilderState)
        (workspaceId:int) =
        let jobManager = ExportJobContext.createJobManager relativityInstance
        let response =
            match jobManager with
            | Ok jm -> jm.ListAsync(workspaceId, 0, 100) |> Async.AwaitTask |> Async.RunSynchronously
            | Error _ -> failwith "nope"
        response


