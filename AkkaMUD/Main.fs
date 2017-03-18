module AkkaMUD.Main

open Akka.FSharp
open World
open Network

let system = System.create "system" (Configuration.defaultConfig())
let roomRef = spawn system "room" room
let serverRef = spawn system "server" (server roomRef)

System.Console.ReadLine() |> ignore