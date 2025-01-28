namespace SpiralOSS.FsRelativity.Export

[<AutoOpen>]
module ExportSettings =

    type CustomFilenameToken =
    | Identifier
    | ProductionBeginBates
    | OriginalFilename
    | Text of string

    //////////////////////////////////////////////////////////////////////////////////
    // CUSTOM FILE NAMING
    module CustomFilenamePattern =

        type BuilderState = {
            Separator: char
            Tokens: CustomFilenameToken list
            }

        module BuilderState =
            let defaults = { Separator = '_'; Tokens = [] }

        type Builder internal () =
            member __.Zero (_) =
                BuilderState.defaults

            member __.Yield (_) =
                BuilderState.defaults

            [<CustomOperation("separator")>]
            member __.Seperator (state, input) =
                { state with Separator = input }

            [<CustomOperation("add_identifier")>]
            member __.Identifier (state) =
                { state with Tokens = state.Tokens @ [ Identifier ] }

            [<CustomOperation("add_production_bates")>]
            member __.ProductionBates (state) =
                { state with Tokens = state.Tokens @ [ ProductionBeginBates ] }

            [<CustomOperation("add_original_filename")>]
            member __.OriginalFilename (state) =
                { state with Tokens = state.Tokens @ [ OriginalFilename ] }

            [<CustomOperation("add_text")>]
            member __.Text (state, input) =
                { state with Tokens = state.Tokens @ [ Text input ] }

    type FileNamingFormat =
    | Default
    | Identifier
    | CustomFilename of pattern:string
    | CustomPattern of pattern:CustomFilenamePattern.BuilderState

    type ImageOutputFormat =
    | Pdf
    | SinglePage
    | MultiPage

    //////////////////////////////////////////////////////////////////////////////////
    // IMAGES
    module ImageSettings =

        type BuilderState = internal {
            Precedence: int list
            OutputFormat: ImageOutputFormat
            ApplyFilenamePattern: bool
            }

        module BuilderState =
            let defaults = {
                Precedence = [ ]
                OutputFormat = SinglePage
                ApplyFilenamePattern = true
                }

        type Builder internal () =
            member __.Yield (_) =
                BuilderState.defaults

            [<CustomOperation("add_production_artifact_id")>]
            member __.AddProductionArtifactId (state, input) =
                { state with Precedence = state.Precedence @ [ input ] }

            [<CustomOperation("add_default_images")>]
            member __.DefaultImages (state) =
                { state with Precedence = state.Precedence @ [ -1 ] }

            /// Pdf, SinglePage, or MultiPage
            [<CustomOperation("output_format")>]
            member __.OutputFormat (state, input) =
                { state with OutputFormat = input }

            [<CustomOperation("apply_filename_pattern")>]
            member __.ApplyFilenamePattern (state, input) =
                { state with ApplyFilenamePattern = input }

    //////////////////////////////////////////////////////////////////////////////////
    // TEXT
    module TextSettings =
        type BuilderState = internal {
            Precedence: int list
            Encoding: string
            }

        module BuilderState =
            let defaults = {
                Precedence = [ ]
                Encoding = "UTF-8"
                }

        type Builder internal () =
            member __.Yield (_) =
                BuilderState.defaults

            [<CustomOperation("add_field_artifact_id")>]
            member __.AddProductionArtifactId (state, input) =
                { state with Precedence = state.Precedence @ [ input ] }

            [<CustomOperation("encoding")>]
            member __.Encoding (state, input) =
                { state with Encoding = input }


    //////////////////////////////////////////////////////////////////////////////////
    // NATIVES
    module NativeSettings =
        type BuilderState = internal {
            Precedence: int list
            }

        module BuilderState =
            let defaults = {
                Precedence = [ ]
                }

        type Builder internal () =
            member __.Yield (_) =
                BuilderState.defaults

            [<CustomOperation("add_production_artifact_id")>]
            member __.AddProductionArtifactId (state, input) =
                { state with Precedence = state.Precedence @ [ input ] }

            [<CustomOperation("add_default_natives")>]
            member __.DefaultNatives (state) =
                { state with Precedence = state.Precedence @ [ -1 ] }

            [<CustomOperation("production_artifact_ids")>]
            member __.ProductionArtifactIds (state, input) =
                { state with Precedence = input }


    //////////////////////////////////////////////////////////////////////////////////
    // FIELDS
    module FieldSettings =
        type BuilderState = internal {
            FieldArtifactIds: int list
            NestMultiChoiceFields: bool
            }

        module BuilderState =
            let defaults = {
                FieldArtifactIds = []
                NestMultiChoiceFields = false
                }

        type Builder internal () =
            member __.Yield (_) =
                BuilderState.defaults

            [<CustomOperation("add_text_field_artifact_id")>]
            member __.FieldArtifactId (state, input) =
                { state with FieldArtifactIds = state.FieldArtifactIds @ [ input ] }

            [<CustomOperation("nest_multi_choice_fields")>]
            member __.NestMultiChoiceFields (state) =
                { state with NestMultiChoiceFields = true }

            [<CustomOperation("do_not_nest_multi_choice_fields")>]
            member __.DoNotNestMultiChoiceFields (state) =
                { state with NestMultiChoiceFields = false }


    type BuilderState = {
        FileNamingFormat: FileNamingFormat
        ImageSettings: ImageSettings.BuilderState option
        TextSettings: TextSettings.BuilderState option
        NativeSettings: NativeSettings.BuilderState option
        PdfSettings: bool
        FieldSettings: FieldSettings.BuilderState option
        FolderSettings: FolderingSettings.BuilderState
        LoadfileSettings: LoadFileSettings.BuilderState
        }

    module BuilderState =
        let defaults = {
            FileNamingFormat = Default
            ImageSettings = None
            TextSettings = None
            NativeSettings = None
            PdfSettings = false
            FieldSettings = None
            FolderSettings = FolderingSettings.BuilderState.defaults
            LoadfileSettings = LoadFileSettings.BuilderState.defaults
            }

    type Builder internal () =

        member __.Yield (_) =
            BuilderState.defaults

        /// <summary>
        /// The filename formatting:
        /// Default - ???
        /// Identifier - the Identifier field
        /// CustomFilename "pattern" - pattern specified by a string
        /// CustomPattern custom_filename_pattern { ... } - a builder to create a custom filename
        /// </summary>
        [<CustomOperation("filename_format")>]
        member __.FileNaming (state, input) =
            { state with FileNamingFormat = input }

        [<CustomOperation("no_images")>]
        member __.NoImages (state) =
            { state with ImageSettings = None }

        [<CustomOperation("export_images")>]
        member __.ExportImages (state, input) =
            { state with ImageSettings = Some input }

        [<CustomOperation("no_text")>]
        member __.NoText (state) =
            { state with TextSettings = None }

        [<CustomOperation("export_text")>]
        member __.ExportText (state, input) =
            { state with TextSettings = Some input }

        [<CustomOperation("no_natives")>]
        member __.NoNatives (state) =
            { state with NativeSettings = None }

        [<CustomOperation("export_natives")>]
        member __.ExportNatives (state, input) =
            { state with NativeSettings = Some input }

        [<CustomOperation("no_pdfs")>]
        member __.NoPdfs (state) =
            { state with PdfSettings = false }

        [<CustomOperation("export_pdfs")>]
        member __.ExportPdfs (state) =
            { state with PdfSettings = true }

        [<CustomOperation("no_fields")>]
        member __.Fields (state) =
            { state with FieldSettings = None }

        [<CustomOperation("export_fields")>]
        member __.Fields (state, input) =
            { state with FieldSettings = Some input }

        [<CustomOperation("foldering")>]
        member __.FolderingSettings (state, input) =
            { state with FolderSettings = input }

        [<CustomOperation("loadfiles")>]
        member __.LoadfileSettings (state, input) =
            { state with LoadfileSettings = input }
