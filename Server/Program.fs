open System
open System.Linq
open Akka.FSharp
open Akka.Remote
open Akka.Routing
open ChatMessages
open System

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
            let rec loop (clients:Akka.Actor.IActorRef list) = actor {

                let! (msg:obj) = mailbox.Receive()

                printfn "Received %A" msg   //Received seq [seq [seq []]; seq [seq [seq []]]]  ???

                match msg with
                
                    | :? PrintJob as pj -> 
                        pj.TTSVs |> 
                        List.iter(fun ttsv ->
                            printfn "%s\n" ttsv.Tag
                            ttsv.Values |>
                            List.iter(fun tsv ->
                                printfn "%f,  " tsv.Value
                            )
                            let hh = WriteResponse(WriteGood)
                            mailbox.Sender().Tell(hh, mailbox.Self)
                        )
                        
                        return! loop clients
                    | _ ->                   
                        printfn "Some other message\n" 
                        return! loop clients
                
            }

            loop []

    Console.ReadLine() |> ignore

    system.Dispose()



    0 // return an integer exit code


