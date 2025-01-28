namespace SpiralOSS.FsRelativity.Export

module FolderingSettings =

    type BuilderState = {

        // VOLUME
        VolumePrefix: string
        VolumeStartNumber: int
        VolumeSizeInMB: int
        VolumePadding: int

        // SUB DIRECTORIES
        StartNumber: int
        MaxFilesInDirectory: int
        ImageFolderPrefix: string
        NativeFolderPrefix: string
        TextFolderPrefix: string
        PdfFolderPrefix: string
        SubFolderPadding: int
        }

    module BuilderState =
        let defaults = {
            VolumePrefix = "VOL"
            VolumeStartNumber = 1
            VolumeSizeInMB = 650
            VolumePadding = 5

            StartNumber = 1
            MaxFilesInDirectory = 500
            ImageFolderPrefix = "IMG"
            NativeFolderPrefix = "NATIVE"
            TextFolderPrefix = "TEXT"
            PdfFolderPrefix = "PDF"
            SubFolderPadding = 3
            }

    type Builder internal () =

        [<CustomOperation("volume_prefix")>]
        member __.VolumePrefix (state, input) =
            { state with VolumePrefix = input }

        [<CustomOperation("volume_start_number")>]
        member __.VolumeStartNumber (state, input) =
            { state with VolumeStartNumber = input }

        [<CustomOperation("volume_size_in_megabytes")>]
        member __.VolumeSizeInMB (state, input) =
            { state with VolumeSizeInMB = input }

        [<CustomOperation("volume_folder_padding")>]
        member __.VolumePadding (state, input) =
            { state with VolumePadding = input }

        [<CustomOperation("subfolder_start_number")>]
        member __.StartNumber (state, input) =
            { state with StartNumber = input }

        [<CustomOperation("subfolder_max_files")>]
        member __.MaxFilesInDirectory (state, input) =
            { state with MaxFilesInDirectory = input }

        [<CustomOperation("subfolder_image_prefix")>]
        member __.ImageFolderPrefix (state, input) =
            { state with ImageFolderPrefix = input }

        [<CustomOperation("subfolder_native_prefix")>]
        member __.NativeFolderPrefix (state, input) =
            { state with NativeFolderPrefix = input }

        [<CustomOperation("subfolder_text_prefix")>]
        member __.TextFolderPrefix (state, input) =
            { state with TextFolderPrefix = input }

        [<CustomOperation("subfolder_pdf_prefix")>]
        member __.PdfFolderPrefix (state, input) =
            { state with PdfFolderPrefix = input }

        [<CustomOperation("subfolder_padding")>]
        member __.SubFolderPadding (state, input) =
            { state with SubFolderPadding = input }
