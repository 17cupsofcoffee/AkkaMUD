open Akka.FSharp
open Akka.Actor

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

greeter <! Hello "Joe"
greeter <! Goodbye "Joe"

System.Console.ReadLine() |> ignore