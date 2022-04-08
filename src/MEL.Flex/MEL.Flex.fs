namespace MEL.Flex

open System



module Tuple =
    open System
    open System.Runtime.CompilerServices
    open System.Collections
    open System.Collections.Generic
    open Microsoft.Extensions.Logging

    [<Literal>]
    let OriginalFormat = "{OriginalFormat}"

    let internal setMessageInterpolated (message: FormattableString) : obj array * string array * obj array =
        let msgArgs = message.GetArguments()
        let names = Array.zeroCreate msgArgs.Length
        let namesTemplate = Array.zeroCreate msgArgs.Length
        let args = Array.zeroCreate msgArgs.Length

        for i = 0 to msgArgs.Length - 1 do
            match msgArgs.[i] with
            | :? ITuple as t when t.Length = 2 ->
                names.[i] <- $"@{t.Item(0)}"
                namesTemplate.[i] <- box $"{{{names.[i]}}}"
                args.[i] <- t.Item(1)
            | other ->
                names.[i] <- $"@item{i}"
                namesTemplate.[i] <- box $"{{{names.[i]}}}"
                args.[i] <- other

        namesTemplate, names, args

    // TODO: Convert to struct
    // [<Struct>]
    type internal TupleLogValuesFormatter(formattable: FormattableString) =

        let _count = formattable.ArgumentCount + 1

        let results =
            lazy
                (
                // printfn "TupleLogValuesFormatter.results"
                setMessageInterpolated formattable)

        let namesTemplate =
            lazy
                (let (namesTemplate, _, _) = results.Value
                 namesTemplate)

        let names =
            lazy
                (let (_, names, _) = results.Value
                 names)

        let args =
            lazy
                (let (_, _, args) = results.Value
                 args)

        let messageFormat = lazy (String.Format(formattable.Format, namesTemplate.Value))
        let formattedMessage = lazy (String.Format(formattable.Format, args.Value))
        member this.Names = names.Value
        member this.Args = args.Value
        member this.MessageTemplate = messageFormat.Value

        override this.ToString() =

            // printfn "TupleLogValuesFormatter.ToString"
            formattedMessage.Value

        interface IReadOnlyList<KeyValuePair<string, obj>> with
            member this.Item
                with get (index: int): KeyValuePair<string, obj> =
                    // printfn "TupleLogValuesFormatter.get"
                    if index < 0 || index >= _count then
                        raise (IndexOutOfRangeException(nameof (index)))
                    elif (index = (_count - 1)) then
                        KeyValuePair<string, obj>(OriginalFormat, this.MessageTemplate)
                    else
                        KeyValuePair<string, obj>(this.Names.[index], this.Args.[index])

            member this.Count: int = _count

            member this.GetEnumerator() : IEnumerator<KeyValuePair<string, obj>> =

                // printfn "TupleLogValuesFormatter.GetEnumerator"
                (seq {
                    for i = 0 to _count - 1 do
                        (this :> IReadOnlyList<_>)[i]
                })
                    .GetEnumerator()

            member this.GetEnumerator() : IEnumerator =
                (this :> IEnumerable<_>).GetEnumerator() :> IEnumerator

    type ILogger with
        member this.LogITrace(eventId: EventId, ex: Exception, message: FormattableString) =
            this.LogI(LogLevel.Trace, eventId, ex, message)

        member this.LogITrace(eventId: EventId, message: FormattableString) =
            this.LogI(LogLevel.Trace, eventId, message)

        member this.LogITrace(ex: Exception, message: FormattableString) = this.LogI(LogLevel.Trace, ex, message)
        member this.LogITrace(message: FormattableString) = this.LogI(LogLevel.Trace, message)

        member this.LogIDebug(eventId: EventId, ex: Exception, message: FormattableString) =
            this.LogI(LogLevel.Debug, eventId, ex, message)

        member this.LogIDebug(eventId: EventId, message: FormattableString) =
            this.LogI(LogLevel.Debug, eventId, message)

        member this.LogIDebug(ex: Exception, message: FormattableString) = this.LogI(LogLevel.Debug, ex, message)
        member this.LogIDebug(message: FormattableString) = this.LogI(LogLevel.Debug, message)

        member this.LogIInformation(eventId: EventId, ex: Exception, message: FormattableString) =
            this.LogI(LogLevel.Information, eventId, ex, message)

        member this.LogIInformation(eventId: EventId, message: FormattableString) =
            this.LogI(LogLevel.Information, eventId, message)

        member this.LogIInformation(ex: Exception, message: FormattableString) =
            this.LogI(LogLevel.Information, ex, message)

        member this.LogIInformation(message: FormattableString) =
            this.LogI(LogLevel.Information, message)

        member this.LogIWarning(eventId: EventId, ex: Exception, message: FormattableString) =
            this.LogI(LogLevel.Warning, eventId, ex, message)

        member this.LogIWarning(eventId: EventId, message: FormattableString) =
            this.LogI(LogLevel.Warning, eventId, message)

        member this.LogIWarning(ex: Exception, message: FormattableString) =
            this.LogI(LogLevel.Warning, ex, message)

        member this.LogIWarning(message: FormattableString) = this.LogI(LogLevel.Warning, message)

        member this.LogIError(eventId: EventId, ex: Exception, message: FormattableString) =
            this.LogI(LogLevel.Error, eventId, ex, message)

        member this.LogIError(eventId: EventId, message: FormattableString) =
            this.LogI(LogLevel.Error, eventId, message)

        member this.LogIError(ex: Exception, message: FormattableString) = this.LogI(LogLevel.Error, ex, message)
        member this.LogIError(message: FormattableString) = this.LogI(LogLevel.Error, message)

        member this.LogICritical(eventId: EventId, ex: Exception, message: FormattableString) =
            this.LogI(LogLevel.Critical, eventId, ex, message)

        member this.LogICritical(eventId: EventId, message: FormattableString) =
            this.LogI(LogLevel.Critical, eventId, message)

        member this.LogICritical(ex: Exception, message: FormattableString) =
            this.LogI(LogLevel.Critical, ex, message)

        member this.LogICritical(message: FormattableString) = this.LogI(LogLevel.Critical, message)

        member this.LogI(logLevel: LogLevel, message: FormattableString) = this.LogI(logLevel, 0, null, message)

        member this.LogI(logLevel: LogLevel, eventId: EventId, message: FormattableString) =
            this.LogI(logLevel, eventId, null, message)

        member this.LogI(logLevel: LogLevel, ex: Exception, message: FormattableString) =
            this.LogI(logLevel, 0, ex, message)

        member this.LogI(logLevel: LogLevel, eventId: EventId, ex: Exception, message: FormattableString) =
            this.Log(
                logLevel,
                eventId,
                (TupleLogValuesFormatter message),
                ex,
                (fun formatter _ -> formatter.ToString())
            )

        member this.BeginScopeI(message: FormattableString) =
            this.BeginScope(TupleLogValuesFormatter message)
