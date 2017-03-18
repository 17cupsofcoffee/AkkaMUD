module AkkaMUD.World

open Akka.FSharp
open Akka.Actor

type RoomState = {
    actors: Set<IActorRef>
}

type RoomMsg =
    | Join of IActorRef
    | Leave of IActorRef
    | Send of string

let room (mailbox: Actor<RoomMsg>) =
    let rec loop state = actor {
        let! msg = mailbox.Receive()
        let sender = mailbox.Sender()

        match msg with
        | Join ref ->
            return! loop { state with actors = Set.add ref state.actors }
        
        | Leave ref ->
            return! loop { state with actors = Set.remove ref state.actors }

        | Send text -> 
            state.actors
            |> Set.remove sender
            |> Set.iter (fun a -> a <! text + "\n")
            
            return! loop state
    }
    loop { actors = Set.empty }