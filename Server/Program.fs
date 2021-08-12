﻿open System
open System.Linq
open Akka.FSharp
open Akka.Remote
open Akka.Routing
open ChatMessages
open System
open OSIsoft.AF
open OSIsoft.AF.Asset
open OSIsoft.AF.Data
open OSIsoft.AF.PI
open PiWriteTypes

[<EntryPoint>]
let main argv =

    Console.Title <- (sprintf "Chat Server : %d" (System.Diagnostics.Process.GetCurrentProcess().Id))

    let config =
            Configuration.parse """
                akka {
                    log-config-on-start = on
                    stdout-loglevel = DEBUG
                    loglevel = DEBUG
                    actor {
                        provider = "Akka.Remote.RemoteActorRefProvider, Akka.Remote"
                    }
                    remote {
                        helios.tcp {
                            transport-class = "Akka.Remote.Transport.Helios.HeliosTcpTransport, Akka.Remote"
                            applied-adapters = []
                            transport-protocol = tcp
                            port = 8081
                            hostname = localhost
                        }
                    }
                }
                """

    let system = System.create "MyServer" config

    let chatServerActor =
        spawn system "ChatServer" <| fun (mailbox) ->
            let rec loop (piWriteState: PiWriteTypes.PiWriteState) = actor {

                let! (msg:obj) = mailbox.Receive()

                printfn "Received %A" msg   //Received seq [seq [seq []]; seq [seq [seq []]]]  ???

                match msg with
                
                    | :? PrintJob as pj -> 
                        let maybeServer = Option.ofObj(piWriteState.PIServer) 
                        match maybeServer with 
                        | Some piServer ->
                            let writeResponse, newTagMap = PIAPI.piWrite(piWriteState, pj.TTSVs)
                            let newState = {piWriteState with TagMap = newTagMap}
                            mailbox.Sender().Tell(writeResponse, mailbox.Self)
                            return! loop newState
                        | None ->
                            let writeRes = WriteResponse(NoServer)
                            return! loop piWriteState
                    | _ ->                   
                        printfn "Some other message\n" 
                        return! loop piWriteState
                
            }
            // Create Dynamic Attribute to a PIPoint to Update
            let piServer: PIServer  = PIServers.GetPIServers().DefaultPIServer
            let piWriteState = {PIServer = piServer; TagMap = Map.empty}
            loop piWriteState

    Console.ReadLine() |> ignore

    system.Dispose()



    0 // return an integer exit code


