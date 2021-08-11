## Akka messages in F#

I am unable to use discriminated unions as messages to akka actors.  If anyone can point me at an example that does this, it would be much appreciated.

My own attempt at this is at . (snippets below).  It is a cutdown version of a sample found at https://github.com/rikace/AkkaActorModel.git (Chat project)


### Problem
The DU message never finds its target on the server actor, but is sent to the deadletter box.  If I send Objects, instead, they do arrive.

If I send a DU, but set my server actor to listen for generic Objects, the message does arrive, but its type is
```
    seq [seq [seq []]
```
and I can't get at underlying DU.

### The DU I am trying to send as message
``` 
    type PrinterJob =
    | PrintThis of string
    | Teardown 
```

 ### The client code
 ```
     let system = System.create "MyClient" config

    let chatClientActor =
        spawn system "ChatClient" <| fun mailbox ->
            let server = mailbox.Context.ActorSelection("akka.tcp://MyServer@localhost:8081/user/ChatServer")

            let rec loop nick = actor {
                let! (msg:PrinterJob) = mailbox.Receive()
                server.Tell(msg)
                return! loop nick
                }
            loop ""

    while true do
        let input = Console.ReadLine()        
        chatClientActor.Tell(PrintThis(input))
```
Messages are forwarded to the client from console input
```
    while true do
        let input = Console.ReadLine()     
        chatClientActor.Tell(PrintThis(input))
```

### The server code
```
    let system = System.create "MyServer" config
    let chatServerActor =
        spawn system "ChatServer" <| fun (mailbox:Actor<_>) ->
            let rec loop (clients:Akka.Actor.IActorRef list) = actor {
                let! (msg:PrinterJob) = mailbox.Receive()

                printfn "Received %A" msg   //Received seq [seq [seq []]; seq [seq [seq []]]]  ???

                match msg with                
                    | PrintThis str -> 
                        Console.WriteLine("Printing: {0} Do we get this?", str)
                        return! loop clients
                    | Teardown -> 
                        Console.WriteLine("Tearing down now")                        
                        return! loop clients                
            }
    loop []
```

## Dependencies 
(I am not using paket here) - PM commands below: 

Install-Package Akka -Version 1.4.23
Install-Package Akka.Remote -Version 1.4.23
Install-Package Akka.FSharp -Version 1.4.23

I am hosting the application in net5.0 


### Constructor argument names - oddity?
When passing in class instances as objects, akka seems to be sensitive to the name of constructor parameters.  The message gets handled, but the data is not copied across from client to server.  If you have a property called Username, the constructor parameter cannot be, for example, uName, otherwise its value is null when it reaches the server.  Code for this is in branch params.  

```
type DoesWork(montelimar: string) =
    member x.Montelimar = montelimar
    
type DoesNotWork(montelimaro: string) =
    member x.Montelimar = montelimaro
```
