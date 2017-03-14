open System.Net
open System.Text
open Akka
open Akka.FSharp
open Akka.Actor
open Akka.IO

let system = System.create "system" (Configuration.defaultConfig())

type GreeterMsg =
    | Hello of string
    | Goodbye of string

let greeter = spawn system "greeter" <| fun mailbox ->
    let rec loop() = actor {
        let! msg = mailbox.Receive()
        let sender = mailbox.Sender()

        match msg with
        | Hello name -> sender <! sprintf "Hello, %s!\n" name
        | Goodbye name -> sender <! sprintf "Goodbye, %s!\n" name

        return! loop()
    }
    loop()

let handler connection (mailbox: Actor<obj>) =  
    let rec loop connection = actor {
        let! msg = mailbox.Receive()

        match msg with
        | :? Tcp.Received as received ->
            let data = (Encoding.ASCII.GetString (received.Data.ToArray())).Trim().Split([|' '|], 2)

            match data with
            | [| "hello"; name |] -> greeter <! Hello (name.Trim())
            | [| "goodbye"; name |] -> greeter <! Goodbye (name.Trim())
            | _ -> connection <! Tcp.Write.Create (ByteString.FromString "Invalid request.\n")
        | :? string as response ->
            connection <! Tcp.Write.Create (ByteString.FromString response)
        | _ -> mailbox.Unhandled()

        return! loop connection
    }

    loop connection

let server = spawn system "server" <| fun (mailbox: Actor<obj>) ->
    let rec loop() = actor {
        let! msg = mailbox.Receive()
        let sender = mailbox.Sender()
        
        match msg with
        | :? Tcp.Bound as bound ->
            printf "Listening on %O\n" bound.LocalAddress
        | :? Tcp.Connected as connected -> 
            printf "%O connected to the server\n" connected.RemoteAddress
            let handlerName = "handler_" + connected.RemoteAddress.ToString().Replace("[", "").Replace("]", "")
            let handlerRef = spawn mailbox handlerName (handler sender)
            sender <! Tcp.Register handlerRef
        | _ -> mailbox.Unhandled()

        return! loop()
    }

    mailbox.Context.System.Tcp() <! Tcp.Bind(mailbox.Self, IPEndPoint(IPAddress.Any, 9090))
    loop()    

System.Console.ReadLine() |> ignore