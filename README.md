# FsRelativityOne

*This is a proof of concept and incomplete.*

DSL for Relativity.

The idea is to create something that is more Ops friendly:

```fsharp
export_settings {

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
```