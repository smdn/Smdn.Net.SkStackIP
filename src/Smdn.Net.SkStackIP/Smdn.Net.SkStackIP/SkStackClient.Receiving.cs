// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP;

#pragma warning disable IDE0040
partial class SkStackClient {
#pragma warning restore IDE0040
  private enum ParseSequenceStatus {
    Initial,
    Undetermined,   // the parsing finished without declaring its state (invalid state)
    Ignored,        // the content of current buffer is a sequence which is not a target for this parser (ex. echoback line)
    Incomplete,     // the content of current buffer is a incomplete sequence to complete parsing
    Continuing,     // the content of current buffer is a complete sequence but needs more data sequence to return final result
    Completed,      // the parsing completed to return final result
  }

  private class ParseSequenceContext : ISkStackSequenceParserContext {
    public ReadOnlySequence<byte> UnparsedSequence { get; internal set; }
    public ParseSequenceStatus Status { get; private set; } = ParseSequenceStatus.Initial;

    public ParseSequenceContext()
    {
    }

    public void Update(ReadOnlySequence<byte> unparsedSequence)
    {
      UnparsedSequence = unparsedSequence;
      Status = ParseSequenceStatus.Undetermined;
    }

    public bool IsConsumed(ReadOnlySequence<byte> sequence)
      => !sequence.Start.Equals(UnparsedSequence.Start);

    /*
     * ISkStackSequenceParserContext
     */
    ISkStackSequenceParserContext ISkStackSequenceParserContext.CreateCopy()
      => (ISkStackSequenceParserContext)MemberwiseClone();

    void ISkStackSequenceParserContext.Continue()
      => Status = ParseSequenceStatus.Continuing;

    void ISkStackSequenceParserContext.Complete()
      => Status = ParseSequenceStatus.Completed;

    void ISkStackSequenceParserContext.Complete(SequenceReader<byte> consumedReader)
    {
      Status = ParseSequenceStatus.Completed;
      UnparsedSequence = consumedReader.GetUnreadSequence();
    }

    void ISkStackSequenceParserContext.Ignore()
      => Status = ParseSequenceStatus.Ignored;

    void ISkStackSequenceParserContext.SetAsIncomplete()
      => Status = ParseSequenceStatus.Incomplete;

    void ISkStackSequenceParserContext.SetAsIncomplete(SequenceReader<byte> incompleteReader)
    {
      Status = ParseSequenceStatus.Incomplete;
      UnparsedSequence = incompleteReader.GetUnreadSequence();
    }
  }

  private static readonly object EchobackLineMarker = new();

  private static readonly TimeSpan ReceiveResponseDelayDefault = TimeSpan.FromMilliseconds(10);

  private TimeSpan receiveResponseDelay = ReceiveResponseDelayDefault;

  /// <summary>
  /// Gets or sets the interval to delay before attempting to receive a subsequent sequence
  /// if the response sequence currently received is incomplete.
  /// </summary>
  public TimeSpan ReceiveResponseDelay {
    get => receiveResponseDelay;
    set {
      if (value <= TimeSpan.Zero)
        throw new ArgumentOutOfRangeException(message: "must be non-zero positive value", actualValue: value, paramName: nameof(ReceiveResponseDelay));

      receiveResponseDelay = value;
    }
  }

  private readonly ParseSequenceContext parseSequenceContext;
  private SemaphoreSlim streamReaderSemaphore;

#pragma warning disable CA1502 // TODO: refactor
#pragma warning disable CA1873
  private async ValueTask<TResult?> ReadAsync<TArg, TResult>(
    Func<ISkStackSequenceParserContext, TArg, TResult> parseSequence,
    TArg arg,
    SkStackEventHandlerBase? eventHandler,
    bool processOnlyERXUDP = false,
    CancellationToken cancellationToken = default,
    [CallerMemberName] string? callerMemberName = default
  )
  {
#if DEBUG
    if (parseSequence is null)
      throw new ArgumentNullException(nameof(parseSequence));
#endif

    if (loggerReceivingStatus is not null)
      LogReceivingStatus(loggerReceivingStatus, $"{callerMemberName} waiting");

    await streamReaderSemaphore.WaitAsync().ConfigureAwait(false);

    if (loggerReceivingStatus is not null)
      LogReceivingStatus(loggerReceivingStatus, $"{callerMemberName} entered");

    try {
      if (loggerReceivingStatus is not null)
        LogReceivingStatus(loggerReceivingStatus, $"{callerMemberName} reading");

      for (; ; ) {
        var reparse = parseSequenceContext.Status switch {
          ParseSequenceStatus.Ignored or ParseSequenceStatus.Continuing => !parseSequenceContext.UnparsedSequence.IsEmpty,
          _ => false,
        };

        ReadOnlySequence<byte> buffer;
        TResult? result = default;
        IDisposable? scopeReadAndParse = null;

        try {
          if (reparse) {
            scopeReadAndParse = Logger?.BeginScope($"{callerMemberName} reparse buffered sequence");

            // reparse previous data sequence
            buffer = parseSequenceContext.UnparsedSequence;
          }
          else {
            scopeReadAndParse = Logger?.BeginScope($"{callerMemberName} read sequence from stream");

            if (loggerReceivingStatus is not null)
              LogReceivingStatus(loggerReceivingStatus, "buffered: ", parseSequenceContext.UnparsedSequence);

            // receive data sequence and parse it
            var readResult = await streamReader.ReadAsync(cancellationToken).ConfigureAwait(false);

            if (readResult.IsCanceled)
              throw new OperationCanceledException("canceled");

            buffer = readResult.Buffer;
          }

          if (loggerReceivingStatus is not null)
            LogReceivingStatus(loggerReceivingStatus, "sequence: ", buffer);

          parseSequenceContext.Update(buffer);

          try {
            // process events which is received until this point
            var eventProcessed = await ProcessEventsAsync(
              parseSequenceContext,
              eventHandler,
              cancellationToken
            ).ConfigureAwait(false);

            if (loggerReceivingStatus is not null)
              LogReceivingStatus(loggerReceivingStatus, $"status: {parseSequenceContext.Status}");

            if (eventProcessed) {
              if (processOnlyERXUDP && parseSequenceContext.Status == ParseSequenceStatus.Continuing)
                (parseSequenceContext as ISkStackSequenceParserContext).Complete(); // reset status as Completed to stop reading
            }
            else if (parseSequenceContext.Status != ParseSequenceStatus.Incomplete) {
              // if buffered data sequence does not contain any events, parse it with the specified parser
              if (loggerReceivingStatus is not null)
                LogReceivingStatus(loggerReceivingStatus, $"parser: {parseSequence.Method.Name} -- {parseSequence.Method}");

              result = parseSequence(parseSequenceContext, arg);

              if (loggerReceivingStatus is not null)
                LogReceivingStatus(loggerReceivingStatus, $"parse status: {parseSequenceContext.Status}");
            }
          }
          catch (SkStackUnexpectedResponseException ex) {
            // The `loggerReceivingStatus` is non-null only when IsEnabled is true,
            // whereas the `Logger` simply holds the reference of given ILogger instance.
            // Therefore, it is necessary to check `IsEnabled` for `Logger` here.
            if (Logger is { } l && l.IsEnabled(LogLevelReceivingStatusUnexpectedResponseException))
              LogReceivingStatusUnexpectedResponseException(l, buffer, ex);

            throw;
          }
        }
        finally {
          scopeReadAndParse?.Dispose();
        }

        var (markAsExamined, advanceIfConsumed, returnResult, delay) = parseSequenceContext.Status switch {
          ParseSequenceStatus.Completed => (markAsExamined: true, advanceIfConsumed: true, returnResult: true, delay: default),
          ParseSequenceStatus.Ignored => (markAsExamined: false, advanceIfConsumed: false, returnResult: true, delay: default),
          ParseSequenceStatus.Incomplete => (markAsExamined: true, advanceIfConsumed: false, returnResult: false, delay: true),
          ParseSequenceStatus.Continuing => (markAsExamined: true, advanceIfConsumed: true, returnResult: false, delay: false),
          ParseSequenceStatus.Undetermined or _ => throw new InvalidOperationException("final status is invalid or remains undetermined"),
        };

        if (advanceIfConsumed && parseSequenceContext.IsConsumed(buffer)) {
          if (Logger is { } l && l.IsEnabled(LogLevelResponse))
            LogDebugResponse(l, buffer.Slice(0, parseSequenceContext.UnparsedSequence.Start), result);

          // advance the buffer to the position where parsing finished
          streamReader.AdvanceTo(consumed: parseSequenceContext.UnparsedSequence.Start);
        }
        else if (markAsExamined) {
          // mark entire buffer as examined to receive the subsequent data
          streamReader.AdvanceTo(consumed: buffer.Start, examined: buffer.End);
        }

        if (returnResult)
          return result;

        if (delay)
          await Task.Delay(receiveResponseDelay).ConfigureAwait(false);

        if (loggerReceivingStatus is not null)
          LogReceivingStatus(loggerReceivingStatus, $"{callerMemberName} continue reading");
      } // for infinite
    }
    finally {
      if (loggerReceivingStatus is not null)
        LogReceivingStatus(loggerReceivingStatus, $"{callerMemberName} exited");

      streamReaderSemaphore.Release();
    }
  }
#pragma warning restore CA1873
#pragma warning restore CA1502

  private async ValueTask<SkStackResponse<TPayload>> ReceiveResponseAsync<TPayload>(
    ReadOnlyMemory<byte> command,
    SkStackSequenceParser<TPayload?>? parseResponsePayload,
    SkStackEventHandlerBase? commandEventHandler,
    SkStackProtocolSyntax syntax,
    CancellationToken cancellationToken
  )
  {
    if (loggerReceivingStatus is not null)
      LogReceivingStatus(loggerReceivingStatus, $"{nameof(ReceiveResponseAsync)} ", command);

    // try read and parse echoback
    await ReadAsync(
      parseSequence: ParseEchobackLine,
      arg: (command, syntax),
      eventHandler: null,
      cancellationToken: cancellationToken
    ).ConfigureAwait(false);

    var response = new SkStackResponse<TPayload>();

    // read and parse response payload
    if (parseResponsePayload is not null) {
      response.Payload = await ReadAsync(
        parseSequence: static (context, parser) => parser(context),
        arg: parseResponsePayload, // TODO: syntax
        eventHandler: null,
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);
    }

    // read and parse response status line
    if (syntax.ExpectStatusLine) {
      (response.Status, response.StatusText) = await ReadAsync(
        parseSequence: ParseStatusLine,
        arg: (command, syntax),
        eventHandler: null,
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);
    }
    else {
      // (SKLL64) if the command which is not defined its status, always treat it as success
      response.Status = SkStackResponseStatus.Ok;
    }

    if (commandEventHandler is not null && commandEventHandler.DoContinueHandlingEvents(response.Status)) {
      const int ParseSequenceEmptyResult = default;

#pragma warning disable CA1873
      if (loggerReceivingStatus is not null)
        LogReceivingStatus(loggerReceivingStatus, $"{nameof(ReceiveResponseAsync)} {commandEventHandler.GetType().Name}");
#pragma warning restore CA1873

      await ReadAsync(
        parseSequence: static (context, handler) => { handler.ProcessSubsequentEvent(context); return ParseSequenceEmptyResult; },
        arg: commandEventHandler,
        eventHandler: commandEventHandler,
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);
    }

    return response;
  }

  internal readonly struct ReceiveNotifyingEventResult {
    public static readonly ReceiveNotifyingEventResult NotReceivedResult = new(~default(int));
    public static readonly ReceiveNotifyingEventResult ReceivedResult = default;

    public bool Received => value == ReceivedResult.value;

    private readonly int value;

    private ReceiveNotifyingEventResult(int value)
    {
      this.value = value;
    }
  }

  internal ValueTask<ReceiveNotifyingEventResult> ReceiveNotifyingEventAsync(
    CancellationToken cancellationToken
  )
    => ReadAsync(
      parseSequence: static (context, _) => ReceiveNotifyingEventResult.NotReceivedResult,
      arg: default(int),
      eventHandler: null,
      processOnlyERXUDP: true,
      cancellationToken: cancellationToken
    );

  private static object? ParseEchobackLine(
    ISkStackSequenceParserContext context,
    (ReadOnlyMemory<byte> Command, SkStackProtocolSyntax Syntax) args
  )
  {
    var (command, syntax) = args;

    // SKSENDTO occasionally echoes back the line with only CRLF even if the register SFE is set to 0 (???)
    if (syntax == SkStackProtocolSyntax.SKSENDTO) {
      var sksendtoEchobackLineReader = context.CreateReader();

      if (sksendtoEchobackLineReader.IsNext(syntax.EndOfEchobackLine, advancePast: true)) {
        context.Complete(sksendtoEchobackLineReader);
        return EchobackLineMarker;
      }
    }

    var comm = command.Span;
    var reader = context.CreateReader();

    if (comm.Length <= reader.Length && !reader.IsNext(comm, advancePast: false)) {
      context.Ignore(); // echoback line does not start with the command
      return null;
    }

    var echobackLineReader = reader;

    if (!reader.TryReadTo(out ReadOnlySequence<byte> echobackLine, delimiter: syntax.EndOfEchobackLine)) {
      context.SetAsIncomplete(); // end of echoback line is not found
      return default;
    }

    if (echobackLine.Length < comm.Length) {
      context.Ignore();
      return null;
    }

    echobackLineReader.Advance(comm.Length); // advance to position right after the command

    if (echobackLineReader.IsNext(SkStack.SP) || echobackLineReader.IsNext(syntax.EndOfEchobackLine)) {
      context.Complete(reader);
      return EchobackLineMarker;
    }
    else {
      context.Ignore();
      return null;
    }
  }

  private static (
    SkStackResponseStatus Status,
    ReadOnlyMemory<byte> StatusText
  )
  ParseStatusLine(
    ISkStackSequenceParserContext context,
    (ReadOnlyMemory<byte> Command, SkStackProtocolSyntax Syntax) args
  )
  {
    SkStackResponseStatus status = default;
    ReadOnlyMemory<byte> statusText = default;

    var (_, syntax) = args;

    var reader = context.CreateReader();

    if (!reader.TryReadTo(out ReadOnlySequence<byte> statusLine, delimiter: syntax.EndOfStatusLine, advancePastDelimiter: true)) {
      context.SetAsIncomplete();
      return default;
    }

    var statusLineReader = new SequenceReader<byte>(statusLine);

    if (statusLineReader.IsNext(SkStackResponseStatusCodes.OK, advancePast: true))
      status = SkStackResponseStatus.Ok;
    else if (statusLineReader.IsNext(SkStackResponseStatusCodes.FAIL, advancePast: true))
      status = SkStackResponseStatus.Fail;

    if (status == default) {
      // if the line starts with unknown status code, mark entire buffer as consumed to discard buffer
      // context.Ignore();
      context.Complete(reader);
      return default;
    }
    else {
      if (statusLineReader.IsNext(SkStack.SP, advancePast: true))
        statusText = statusLineReader.GetUnreadSequence().ToArray();

      context.Complete(reader);

      return (status, statusText);
    }
  }
}
