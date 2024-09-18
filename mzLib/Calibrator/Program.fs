open System
open Readers

[<EntryPoint>]
let main argv =
    let path = 
        @"D:\MannPeptideResults\A549_AllPeptides.psmtsv"
    let file = 
        Readers.PsmFromTsvFile path
    file.LoadResults()

    for psm in file.Results do
        printfn "%s" psm.BaseSequence
    0
