module PiWriteTypes
open OSIsoft.AF
open OSIsoft.AF.Asset
open OSIsoft.AF.Data
open OSIsoft.AF.PI

type PiWriteState = {
    PIServer : PIServer
    TagMap: Map<string, AFAttribute>
}