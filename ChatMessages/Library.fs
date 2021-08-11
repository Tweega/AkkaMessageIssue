module ChatMessages

open System

type DoesWork(montelimar: string) =
    member x.Montelimar = montelimar
    
type WorksAlso(montelimar: string) =
    let _montelimar = montelimar
    member x.Montelimar with get() = _montelimar

type DoesNotWork(montelimaro: string) =
    member x.Montelimar = montelimaro

type PrintThis(somethingToPrint: string ) = 
    member x.SomethingToPrint = somethingToPrint
    
type ConnectRequest(username:string) =
    member x.UserName = username

type ConnectResponse(message:string) =
    member x.Message = message

type NickRequest(oldUsername:string, newUsername:string) =
    member x.OldUsername = oldUsername
    member x.NewUSername = newUsername

type NickResponse(oldUsername:string, newUsername:string) =
    member x.OldUsername = oldUsername
    member x.NewUSername = newUsername

type SayRequest(username:string, text:string) =
    member x.Username = username
    member x.Text = text

type SayResponse(username:string, text:string) =
    member x.Username = username
    member x.Text = text

    