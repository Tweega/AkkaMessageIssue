module PIAPI

open OSIsoft.AF
open OSIsoft.AF.Asset
open OSIsoft.AF.Data
open OSIsoft.AF.PI
open ChatMessages
open OSIsoft.AF.Search
open OSIsoft.AF.Time
open OSIsoft.AF.UnitsOfMeasure
open OSIsoft.AF.EventFrame
open PiWriteTypes

let tryParseFloat s = try Some (float s) with | _ -> None 
let tryParseDouble d = try Some (double d) with | _ -> None 


let logErrors(eDict: System.Collections.Generic.IDictionary<'AF, exn>) =
    eDict |> Option.ofObj |>
    Option.iter(fun errErrs ->
        errErrs |>
        Seq.iter(fun (kvp) ->
            let afValue = kvp.Key
            let exn = kvp.Value
            let msg = sprintf "AFValue \'%s\': %s" (afValue.ToString()) exn.Message   
            printfn "%s" msg
        )
    )

let piWrite(piWriteState: PiWriteState, ttsvs: list<TaggedTsvs>) =
   
    let (newTagMap: Map<string, AFAttribute>, newValues:  List<AFValue>) = 
        ttsvs |>
        List.fold(fun ((tagMapAcc: Map<string, AFAttribute>), (afValuesAcc:list<AFValue>)) ttsv ->
            let tagName = ttsv.Tag
            printfn "%s\n" ttsv.Tag

            let (success, afAttribute) = piWriteState.TagMap.TryGetValue(tagName)
            let (maybeAFAttribute, tMap) = 
                match success with 
                | true ->
                    (Some afAttribute, tagMapAcc)
                | false ->
                    // create an attribute for this tag
                    let (bSuccess, piPoint) = PIPoint.TryFindPIPoint(piWriteState.PIServer, tagName)
                    if (bSuccess) then
                        let mutable myAttr = AFAttribute(piPoint)
                        let configStr = myAttr.ConfigString + ";ReadOnly=False"
                        //myAttr.DefaultUOM <- uom
                        myAttr.ConfigString <- configStr
                        (Some myAttr, (tagMapAcc.Add(tagName, myAttr)))
                        
                    else
                        (None, tagMapAcc)
            match maybeAFAttribute with 
            | None -> tMap, afValuesAcc
            | Some attr -> 
                let newAFValues = 
                    ttsv.Values |>
                        List.fold(fun acc tagValue ->
                            let aft = AFTime(tagValue.Timestamp)
                            let newValue = AFValue(attr, tagValue.Value, aft)
                            newValue :: afValuesAcc
                    ) afValuesAcc
                tMap, newAFValues
        ) (piWriteState.TagMap, List.empty)


    let writeResponse: WriteResponse = 
        match (newValues.IsEmpty) with
        | true ->
            WriteResponse(WriteGood)    //Bad?
        | false ->

            // F# lists and ILists are different so a bit of jiggling needed
            let iNewValues = Array.ofList(newValues) :> System.Collections.Generic.IList<AFValue>

            // Update the Attribute Values and Display any Errors
            let errors: AFErrors<AFValue>  =
                AFListData.UpdateValues(iNewValues, AFUpdateOption.Replace)
            
            let maybeErrors = Option.ofObj(errors)
            match maybeErrors with 
            | Some errs ->
                if (errs.HasErrors) then
                    logErrors(errors.Errors)
                    logErrors(errors.PIServerErrors)
                    logErrors(errors.PISystemErrors)
                WriteResponse(WriteBad)

            | None -> 
                WriteResponse(WriteGood)

    (writeResponse, newTagMap)

