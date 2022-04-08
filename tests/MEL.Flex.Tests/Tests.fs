namespace MEL.Flex.Tests

open System
open Expecto
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Logging.Abstractions
open MEL.Flex
open System.Collections.Generic
module Helpers =
    let kvp k v = KeyValuePair<string, obj>(k, box v)
type InMemoryLogger() =
    let beginScopeCalls = Stack<obj>()
    let logCalls = ResizeArray<_>()
    let getStateValues state =
        match box state with
        | :? IReadOnlyCollection<KeyValuePair<string,obj>> as s -> s :> seq<_> |> Seq.toArray
        | _ -> Array.empty

    member _.LogCalls = logCalls
    interface ILogger with
        member this.Log<'TState>(logLevel: LogLevel, eventId: EventId, state: 'TState, ex : exn, formatter: Func<'TState,exn,string>) : unit =
            let stateValues =
                getStateValues state
            let message = formatter.Invoke(state, ex)
            let scopes = beginScopeCalls |> Seq.toArray |> Array.collect getStateValues
            logCalls.Add(logLevel, eventId, message, stateValues, ex, scopes)
            ()
        member this.IsEnabled(logLevel: LogLevel) : bool = true
        member this.BeginScope<'TState>(state: 'TState) : IDisposable =
            beginScopeCalls.Push state
            { new IDisposable with
                member _.Dispose() = beginScopeCalls.Pop() |> ignore }

module TupleTests =
    open Helpers
    open MEL.Flex.Tuple
    [<Tests>]
    let tests =
        testList "Tuples" [
            testCase "No items to interpolate" <| fun _ ->
                let logger = InMemoryLogger()

                let expectedState = [|
                    kvp "{OriginalFormat}" "LOL"
                |]

                logger.LogICritical $"""LOL"""
                let (level, eventId, message, state, ex, scopes) = logger.LogCalls |> Seq.head
                Expect.equal level LogLevel.Critical ""
                Expect.equal eventId (EventId.op_Implicit 0) ""
                Expect.equal message "LOL" ""
                Expect.sequenceEqual state expectedState ""
                Expect.equal ex null ""
                Expect.equal scopes Array.empty ""

            testCase "One tuple to interpolate" <| fun _ ->
                let logger = InMemoryLogger()

                let theConst = "UserName"
                let theUser = "KirkJ1701"
                let expectedState = [|
                    kvp $"@{theConst}" theUser
                    kvp "{OriginalFormat}" $"Some user {{@{theConst}}} logged into starship"
                |]

                logger.LogIError $"""Some user {(theConst, theUser)} logged into starship"""

                let (level, eventId, message, state, ex, scopes) = logger.LogCalls |> Seq.head
                Expect.equal level LogLevel.Error ""
                Expect.equal eventId (EventId.op_Implicit 0) ""
                Expect.equal message $"Some user {theUser} logged into starship" ""
                Expect.sequenceEqual state expectedState ""
                Expect.equal ex null ""
                Expect.equal scopes Array.empty ""

            testCase "Non tuple to interpolate" <| fun _ ->
                let logger = InMemoryLogger()

                let theConst = "item0"
                let theUser = "SpockV"
                let expectedState = [|
                    kvp $"@{theConst}" theUser
                    kvp "{OriginalFormat}" $"Some user {{@{theConst}}} logged into starship"
                |]

                logger.LogIWarning $"""Some user {theUser} logged into starship"""

                let (level, eventId, message, state, ex, scopes) = logger.LogCalls |> Seq.head
                Expect.equal level LogLevel.Warning ""
                Expect.equal eventId (EventId.op_Implicit 0) ""
                Expect.equal message $"Some user {theUser} logged into starship" ""
                Expect.sequenceEqual state expectedState ""
                Expect.equal ex null ""
                Expect.equal scopes Array.empty ""
        ]
