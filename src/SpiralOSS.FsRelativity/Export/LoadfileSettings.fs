namespace SpiralOSS.FsRelativity.Export

open SpiralOSS.FsRelativity.Loadfile

module LoadFileSettings =

    type BuilderState = internal {
        RecordFormat: LoadfileFormat
        RecordColumnFormat: LoadfileColumnFormat
        Culture: string
        DateFormat: string option
        Encoding: string
        ImageLoadfileFormat: ImageLoadfileFormat
        PdfLoadfileFormat: ImageLoadfileFormat
        }

    module BuilderState =
        let defaults = {
            RecordFormat = CsvFormat
            RecordColumnFormat = DefaultColumnFormat
            Culture = "en-US"
            DateFormat = None
            Encoding = "UTF-8"
            ImageLoadfileFormat = Opticon
            PdfLoadfileFormat = Opticon
            }

    type Builder internal () =
        member __.Yield (_) =
            BuilderState.defaults

        [<CustomOperation("format")>]
        member __.RecordFormat (state, input) =
            { state with RecordFormat = input }

        [<CustomOperation("column_format")>]
        member __.ColumnFormat (state, input) =
            { state with RecordColumnFormat = input }

        [<CustomOperation("default_culture")>]
        member __.Culture (state) =
            { state with Culture = BuilderState.defaults.Culture }

        [<CustomOperation("culture")>]
        member __.Culture (state, input) =
            { state with Culture = input }

        [<CustomOperation("default_date_format")>]
        member __.DateFormat (state) =
            { state with DateFormat = BuilderState.defaults.DateFormat }

        [<CustomOperation("custom_date_format")>]
        member __.DateFormat (state, input) =
            { state with DateFormat = Some input }

        [<CustomOperation("default_encoding")>]
        member __.Encoding (state) =
            { state with Encoding = BuilderState.defaults.Encoding }

        [<CustomOperation("encoding")>]
        member __.Encoding (state, input) =
            { state with Encoding = input }

        [<CustomOperation("image_loadfile_format")>]
        member __.ImageLoadfileFormat (state, input) =
            { state with ImageLoadfileFormat = input }

        [<CustomOperation("pdf_loadfile_format")>]
        member __.PdfLoadfileFormat (state, input) =
            { state with PdfLoadfileFormat = input }
