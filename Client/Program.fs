﻿open System
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

            let rec loop nick = actor {
                let! (msg:obj) = mailbox.Receive()
                match msg with 
                | :? WriteResponse as wr ->
                    match wr.WriteResult with 
                    | WriteGood -> printfn "Hooray"
                    | WriteBad -> printfn "Boo"
                    
                    return! loop nick
                | _ ->
                    server.Tell(msg)
                    return! loop nick
                }
            loop ""

    let userName = "Me"

    while true do
        let input = Console.ReadLine()
        //chatClientActor.Tell(SayRequest(username, input))
        let now = DateTime.Now
        let tsvs = [{Timestamp = now; Value = 11.11}]
        let ttsvs = [{Tag = "TagA"; Values = tsvs}]
        chatClientActor.Tell(PrintJob(ttsvs))
           

    0
