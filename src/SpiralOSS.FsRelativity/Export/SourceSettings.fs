namespace SpiralOSS.FsRelativity.Export

open SpiralOSS.FsRelativity

[<AutoOpen>]
module SourceSettings =

    type Source =
    | FolderId of artifactId:int * viewId:int * includeSubfolders:bool
    | ObjectsId of artifactId:int * viewId:int
    | ProductionId of artifactId:int
    | SavedSearchId of artifactId:int

    type BuilderState = {
        Source: Source option
        DocumentStartNumber: int
        }

    module BuilderState =
        let defaults = {
            Source = None
            DocumentStartNumber = 1
            }

    type Builder internal () =

        member __.Yield (_) =
            BuilderState.defaults

        [<CustomOperation("source")>]
        member __.Source (state, input) =
            { state with Source = Some input }

        [<CustomOperation("start_number")>]
        member __.DocumentStartNumber (state, input) =
            { state with DocumentStartNumber = input }

