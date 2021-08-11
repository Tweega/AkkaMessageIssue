open System
open System.Linq
open Akka.FSharp
open Akka.Actor
open Akka.Remote
open Akka.Routing
open ChatMessages

[<EntryPoint>]
let main argv =

    Console.Title <- (sprintf "Chat Client : %d" (System.Diagnostics.Process.GetCurrentProcess().Id))

    let config = Configuration.parse """
        akka {
            actor {
                provider = "Akka.Remote.RemoteActorRefProvider, Akka.Remote"
            }
            remote {
                helios.tcp {
                    transport-class = "Akka.Remote.Transport.Helios.HeliosTcpTransport, Akka.Remote"
		            applied-adapters = []
		            transport-protocol = tcp
		            port = 0
		            hostname = localhost
                }
            }
    }"""


    let system = System.create "MyClient" config

    let chatClientActor =
        spawn system "ChatClient" <| fun mailbox ->

            let server = mailbox.Context.ActorSelection("akka.tcp://MyServer@localhost:8081/user/ChatServer")

            let rec loop state =
                actor {
                    let! (message:ChatMessages.WriteResult) = mailbox.Receive()
                    printfn "Received %A" message
                    match message with
                    | Good ->
                        printfn "Write successful"
                        return! loop state
                   
                    | Bad err ->
                        printfn "Bad! %s" err
                        return! loop state
                    | Write whatToWrite ->
                        printfn "Request to write this data: %s" whatToWrite
                        //let wm: WriterMsg = WriteThis whatToWrite
                        let printable = new ChatMessages.PrintThis("jelly baby")
                        printfn "About to send %s" printable.WhatToPrint
                        server.Tell (printable, mailbox.Self)
                    
                }
            loop "Some state"
    
    let mutable carryOn = true
    while carryOn do
        let input = Console.ReadLine()
        if input.StartsWith("/") then
            carryOn <- false
        else
            chatClientActor <! Write "Hello"


    0
