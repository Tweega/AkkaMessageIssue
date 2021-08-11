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
        spawn system "ChatServer" <| fun mailbox ->
            let rec loop state =
                actor {
                    let! (message:obj) = mailbox.Receive()
                    printfn "Received %A" message
                    match message with
                    | :? PrintThis as sr ->
                        printfn "Printing: %s" sr.WhatToPrint
                        let pr = new PrintResult(Good)
                        mailbox.Sender() <! pr
                        
                    | _ -> 
                        let pr = new PrintResult(Bad "Unrecognised message in ChatServer actor")
                        mailbox.Sender() <! pr
                        
                    
                }
               
            loop("some state")

    Console.ReadLine() |> ignore

    system.Dispose()



    0 // return an integer exit code


