module AkkaMUD.Main

open Akka.FSharp
open Greeter
open Network

let system = System.create "system" (Configuration.defaultConfig())
let greeterRef = spawn system "greeter" greeter
let serverRef = spawn system "server" (server greeterRef)

System.Console.ReadLine() |> ignore