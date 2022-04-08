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
        member this.LogFTrace(eventId: EventId, ex: Exception, message: FormattableString) =
            this.LogF(LogLevel.Trace, eventId, ex, message)

        member this.LogFTrace(eventId: EventId, message: FormattableString) =
            this.LogF(LogLevel.Trace, eventId, message)

        member this.LogFTrace(ex: Exception, message: FormattableString) = this.LogF(LogLevel.Trace, ex, message)
        member this.LogFTrace(message: FormattableString) = this.LogF(LogLevel.Trace, message)

        member this.LogFDebug(eventId: EventId, ex: Exception, message: FormattableString) =
            this.LogF(LogLevel.Debug, eventId, ex, message)

        member this.LogFDebug(eventId: EventId, message: FormattableString) =
            this.LogF(LogLevel.Debug, eventId, message)

        member this.LogFDebug(ex: Exception, message: FormattableString) = this.LogF(LogLevel.Debug, ex, message)
        member this.LogFDebug(message: FormattableString) = this.LogF(LogLevel.Debug, message)

        member this.LogFInformation(eventId: EventId, ex: Exception, message: FormattableString) =
            this.LogF(LogLevel.Information, eventId, ex, message)

        member this.LogFInformation(eventId: EventId, message: FormattableString) =
            this.LogF(LogLevel.Information, eventId, message)

        member this.LogFInformation(ex: Exception, message: FormattableString) =
            this.LogF(LogLevel.Information, ex, message)

        member this.LogFInformation(message: FormattableString) =
            this.LogF(LogLevel.Information, message)

        member this.LogFWarning(eventId: EventId, ex: Exception, message: FormattableString) =
            this.LogF(LogLevel.Warning, eventId, ex, message)

        member this.LogFWarning(eventId: EventId, message: FormattableString) =
            this.LogF(LogLevel.Warning, eventId, message)

        member this.LogFWarning(ex: Exception, message: FormattableString) =
            this.LogF(LogLevel.Warning, ex, message)

        member this.LogFWarning(message: FormattableString) = this.LogF(LogLevel.Warning, message)

        member this.LogFError(eventId: EventId, ex: Exception, message: FormattableString) =
            this.LogF(LogLevel.Error, eventId, ex, message)

        member this.LogFError(eventId: EventId, message: FormattableString) =
            this.LogF(LogLevel.Error, eventId, message)

        member this.LogFError(ex: Exception, message: FormattableString) = this.LogF(LogLevel.Error, ex, message)
        member this.LogFError(message: FormattableString) = this.LogF(LogLevel.Error, message)

        member this.LogFCritical(eventId: EventId, ex: Exception, message: FormattableString) =
            this.LogF(LogLevel.Critical, eventId, ex, message)

        member this.LogFCritical(eventId: EventId, message: FormattableString) =
            this.LogF(LogLevel.Critical, eventId, message)

        member this.LogFCritical(ex: Exception, message: FormattableString) =
            this.LogF(LogLevel.Critical, ex, message)

        member this.LogFCritical(message: FormattableString) = this.LogF(LogLevel.Critical, message)

        member this.LogF(logLevel: LogLevel, message: FormattableString) = this.LogF(logLevel, 0, null, message)

        member this.LogF(logLevel: LogLevel, eventId: EventId, message: FormattableString) =
            this.LogF(logLevel, eventId, null, message)

        member this.LogF(logLevel: LogLevel, ex: Exception, message: FormattableString) =
            this.LogF(logLevel, 0, ex, message)

        member this.LogF(logLevel: LogLevel, eventId: EventId, ex: Exception, message: FormattableString) =
            this.Log(
                logLevel,
                eventId,
                (TupleLogValuesFormatter message),
                ex,
                (fun formatter _ -> formatter.ToString())
            )

        member this.BeginScopeI(message: FormattableString) =
            this.BeginScope(TupleLogValuesFormatter message)
