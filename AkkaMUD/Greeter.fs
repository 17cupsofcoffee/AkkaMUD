module AkkaMUD.Greeter

open Akka.FSharp

type GreeterMsg =
    | Hello of string
    | Goodbye of string

let greeter (mailbox: Actor<GreeterMsg>) =
    let rec loop() = actor {
        let! msg = mailbox.Receive()
        let sender = mailbox.Sender()

        match msg with
        | Hello name -> sender <! sprintf "Hello, %s!\n" name
        | Goodbye name -> sender <! sprintf "Goodbye, %s!\n" name

        return! loop()
    }
    loop()