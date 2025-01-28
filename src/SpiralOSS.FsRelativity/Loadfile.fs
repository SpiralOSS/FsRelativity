namespace SpiralOSS.FsRelativity

open SpiralOSS.FsRelativity.Constants

[<AutoOpen>]
module Loadfile =

    type LoadfileFormat = {
        Column: char
        Quote: char
        }

    let ConcordanceFormat = { Column = ConcordanceColumnDelimiter; Quote = ConcordanceColumnQuantifier }
    let PcaretFormat = { Column = '|'; Quote = '^' }
    let CsvFormat = { Column = ','; Quote = '"' }

    type LoadfileColumnFormat = {
        Newline: char
        NestedValue: char
        MultiValue: char
        }

    let DefaultColumnFormat = { Newline = RegisterTrademark; NestedValue = '/'; MultiValue = ';' }

    type ImageLoadfileFormat =
    | Opticon
    | Ipro
    | IproWithFullText
        member internal self.ToRelativityImageLoadFileFormat =
            match self with
            | Opticon -> Relativity.Export.V1.Model.ExportJobSettings.ImageLoadFileFormat.Opticon
            | Ipro -> Relativity.Export.V1.Model.ExportJobSettings.ImageLoadFileFormat.IPRO
            | IproWithFullText -> Relativity.Export.V1.Model.ExportJobSettings.ImageLoadFileFormat.IPRO_FullText
        member internal self.ToRelativityPdfLoadFileFormat =
            match self with
            | Opticon -> Relativity.Export.V1.Model.ExportJobSettings.PdfLoadFileFormat.Opticon
            | Ipro -> Relativity.Export.V1.Model.ExportJobSettings.PdfLoadFileFormat.IPRO
            | IproWithFullText -> Relativity.Export.V1.Model.ExportJobSettings.PdfLoadFileFormat.IPRO_FullText