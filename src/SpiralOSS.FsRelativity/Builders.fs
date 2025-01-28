namespace SpiralOSS.FsRelativity

open SpiralOSS.FsRelativity.Export

[<AutoOpen>]
module Builders =

    let custom_filename_pattern = ExportSettings.CustomFilenamePattern.Builder()

    let image_defaults = ExportSettings.ImageSettings.BuilderState.defaults
    let image_settings = ExportSettings.ImageSettings.Builder()
    let text_defaults = ExportSettings.TextSettings.BuilderState.defaults
    let text_settings = ExportSettings.TextSettings.Builder()
    let native_defaults = ExportSettings.NativeSettings.BuilderState.defaults
    let native_settings = ExportSettings.NativeSettings.Builder()

    let folder_settings = FolderingSettings.Builder()
    let folder_defaults = FolderingSettings.BuilderState.defaults
    let loadfile_settings = LoadFileSettings.Builder()
    let loadfile_defaults = LoadFileSettings.BuilderState.defaults

    let export_settings = ExportSettings.Builder()

    let export_source = SourceSettings.Builder()

    let relativity_instance = RelOneInstance.Builder()