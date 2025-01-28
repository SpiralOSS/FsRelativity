open SpiralOSS.FsRelativity
open SpiralOSS.FsRelativity.Export

module Entry =

    let relInstance = relativity_instance {
        uri @"uri"
        auth_user_pass "username" "pass"
        }

    let workspaceId = 12345678

    let exportSource = export_source {
        source (SavedSearchId 1234567)
        }

    let exportSettings = export_settings {

        filename_format Default

        export_images (image_settings {
            add_default_images
            output_format Pdf
            })

        export_natives (native_settings {
            add_default_natives
            })

        no_text

        loadfiles (loadfile_settings {
            format ConcordanceFormat
            })

        foldering folder_defaults
        }

    let outputPath = @"c:\temp\output2"

    let executeJob () =
        ExportRelativityServices.executeJob relInstance workspaceId exportSource exportSettings $"Sample-Job-{System.Guid.NewGuid().ToString().Substring(0, 5)}" outputPath
        |> Async.RunSynchronously

    let listJobs () =
        ExportRelativityServices.listJobs relInstance workspaceId
