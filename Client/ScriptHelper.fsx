
let start (filePath:string) =
    System.Diagnostics.Process.Start(filePath) |> ignore

let chatServer = __SOURCE_DIRECTORY__ + @"\..\Server\bin\Debug\net48\Server.exe"
let chatClient = __SOURCE_DIRECTORY__ + @"\..\Client\bin\Debug\net5.0\Client.exe"

chatServer |> start
chatClient |> start
//[0..2] |> Seq.iter (fun _ -> chatClient |> start)



