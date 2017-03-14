open System.Net
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

        match msg with
        | Hello name -> printf "Hello, %s!\n" name
        | Goodbye name -> printf "Goodbye, %s!\n" name

        return! loop()
    }
    loop()

let server = spawn system "server" <| fun (mailbox: Actor<obj>) ->
    let rec loop() = actor {
        let! msg = mailbox.Receive()
        
        match msg with
        | :? Tcp.Bound as bound ->
            printf "Listening on %O\n" bound.LocalAddress
        | _ -> mailbox.Unhandled()

        return! loop()
    }

    mailbox.Context.System.Tcp() <! Tcp.Bind(mailbox.Self, IPEndPoint(IPAddress.Any, 9090))
    loop()    

System.Console.ReadLine() |> ignore