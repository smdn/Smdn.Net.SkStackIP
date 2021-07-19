// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Smdn.Buffers;
using Smdn.Net.SkStackIP.Protocol;
#if DEBUG
using Smdn.Text.Unicode.ControlPictures;
#endif

namespace Smdn.Net.SkStackIP {
  partial class SkStackClient {
    private enum ParseSequenceStatus {
      Initial,
      Undetermined,   // the parsing finished without declaring its state (invalid state)
      Ignored,        // the content of current buffer is a sequence which is not a target for this parser (ex. echoback line)
      Incomplete,     // the content of current buffer is a incomplete sequence to complete parsing
      Continueing,    // the content of current buffer is a complete sequence but needs more data sequence to return final result
      Completed,      // the parsing completed to return final result
    }

    private class ParseSequenceContext : ISkStackSequenceParserContext {
      public ReadOnlySequence<byte> UnparsedSequence { get; internal set; }
      public ParseSequenceStatus Status { get; private set; } = ParseSequenceStatus.Initial;
      public ILogger Logger { get; }

      public ParseSequenceContext(ILogger logger)
      {
        this.Logger = logger;
      }

      public void Initialize()
      {
        (this as ISkStackSequenceParserContext).State = null;
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
      object ISkStackSequenceParserContext.State { get; set; }

      ISkStackSequenceParserContext ISkStackSequenceParserContext.CreateCopy()
        => (ISkStackSequenceParserContext)MemberwiseClone();

      void ISkStackSequenceParserContext.Continue()
        => Status = ParseSequenceStatus.Continueing;

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

    private readonly ParseSequenceContext parseSequenceContext;

    private async ValueTask<TResult> ReadAsync<TArg, TResult>(
      Func<ISkStackSequenceParserContext, TArg, TResult> parseSequence,
      TArg arg,
      CancellationToken cancellationToken = default
    )
    {
      const int continuousReadingIntervalMilliseconds = 10; // TODO: make configurable

      //parseSequenceContext.Lock(); // TODO: lock or mutex
      parseSequenceContext.Initialize();

      logger?.LogReceivingStatus($"  begin read sequence");

      for (; ; ) {
        var reparse = parseSequenceContext.Status switch {
          ParseSequenceStatus.Ignored or ParseSequenceStatus.Continueing => !parseSequenceContext.UnparsedSequence.IsEmpty,
          _ => false
        };

        ReadOnlySequence<byte> buffer;

        if (reparse) {
          logger?.LogReceivingStatus($"    reparse buffered sequence");

          // reparse previous data sequence
          buffer = parseSequenceContext.UnparsedSequence;
        }
        else {
          logger?.LogReceivingStatus($"    read sequence from stream");
          logger?.LogReceivingStatus("      buffered: ", parseSequenceContext.UnparsedSequence);

          // receive data sequence and parse it
          var readResult = await streamReader.ReadAsync(cancellationToken).ConfigureAwait(false);

          if (readResult.IsCanceled)
            throw new OperationCanceledException("canceled");

          buffer = readResult.Buffer;
        }

        logger?.LogReceivingStatus("      sequence: ", buffer);

        parseSequenceContext.Update(buffer);

        TResult result = default;

        // process events which is received until this point
        if (!ProcessNotificationalEvents(parseSequenceContext)) {
          // if buffered data sequence does not contain any events, parse it with the specified parser
          logger?.LogReceivingStatus($"      parser: {parseSequence.Method}");

          try {
            result = parseSequence(parseSequenceContext, arg);
          }
          catch (SkStackUnexpectedResponseException ex) {
            logger?.LogReceivingStatus("      unexpected response: ", buffer, ex);
          }
        }

        logger?.LogReceivingStatus($"      status: {parseSequenceContext.Status}");

        var postAction = parseSequenceContext.Status switch {
          ParseSequenceStatus.Completed   => (markAsExamined: true,  advanceIfConsumed: true,  returnResult: true,  delay: default),
          ParseSequenceStatus.Ignored     => (markAsExamined: false, advanceIfConsumed: false, returnResult: true,  delay: default),
          ParseSequenceStatus.Incomplete  => (markAsExamined: true,  advanceIfConsumed: false, returnResult: false, delay: true),
          ParseSequenceStatus.Continueing => (markAsExamined: true,  advanceIfConsumed: true,  returnResult: false, delay: false),
          ParseSequenceStatus.Undetermined or _ => throw new InvalidOperationException("final status is invalid or remains undetermined"),
        };

        if (postAction.markAsExamined)
          // mark entire buffer as examined to receive the subsequent data
          streamReader.AdvanceTo(consumed: buffer.Start, examined: buffer.End);

        if (postAction.advanceIfConsumed && parseSequenceContext.IsConsumed(buffer)) {
          // advance the buffer to the position where parsing finished
          logger?.LogTraceResponse(buffer.Slice(0, parseSequenceContext.UnparsedSequence.Start), result);
          streamReader.AdvanceTo(consumed: parseSequenceContext.UnparsedSequence.Start);
        }

        if (postAction.returnResult)
          return result;

        if (postAction.delay)
          await Task.Delay(continuousReadingIntervalMilliseconds).ConfigureAwait(false);
      }
    }

    private async ValueTask<SkStackResponse<TPayload>> ReceiveResponseAsync<TPayload>(
      ReadOnlyMemory<byte> command,
      bool expectsStatusResponse,
      SkStackSequenceParser<TPayload> parseResponsePayload,
      CancellationToken cancellationToken
    )
    {
      logger?.LogReceivingStatus("ReceiveResponseAsync ", command);

      // try read and parse echoback
      await ReadAsync(
        parseSequence: ParseEchobackLine,
        arg: command,
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);

      var response = new SkStackResponse<TPayload>();

      // read and parse response payload
      if (parseResponsePayload is not null) {
        response.Payload = await ReadAsync(
          parseSequence: static (context, parser) => parser(context),
          arg: parseResponsePayload,
          cancellationToken: cancellationToken
        ).ConfigureAwait(false);
      }

      // read and parse response status line
      if (expectsStatusResponse) {
        (response.Status, response.StatusText) = await ReadAsync(
          parseSequence: ParseStatusLine,
          arg: command,
          cancellationToken: cancellationToken
        ).ConfigureAwait(false);
      }
      else {
        // (SKLL64) if the command which is not defined its status, always treat it as success
        response.Status = SkStackResponseStatus.Ok;
      }

      return response;
    }

    internal ValueTask<TResult> ReceiveEventAsync<TResult>(
      SkStackSequenceParser<TResult> parseEvent,
      CancellationToken cancellationToken
    )
      => ReadAsync(
        parseSequence: static (context, parser) => parser(context),
        arg: parseEvent,
        cancellationToken
      );

    private static object ParseEchobackLine(
      ISkStackSequenceParserContext context,
      ReadOnlyMemory<byte> command
    )
    {
      var comm = command.Span;
      var reader = context.CreateReader();

      if (comm.Length <= reader.Length && !reader.IsNext(comm, advancePast: false)) {
        context.Ignore();
        return null;
      }

      var echobackLineReader = reader;

      if (!reader.TryReadTo(out ReadOnlySequence<byte> echobackLine, delimiter: SkStack.CRLFSpan)) {
        context.SetAsIncomplete();
        return default;
      }

      if (echobackLine.Length < comm.Length) {
        context.Ignore();
        return null;
      }

      echobackLineReader.Advance(comm.Length); // advance to position right after the command

      if (echobackLineReader.IsNext(SkStack.SP) || echobackLineReader.IsNext(SkStack.CRLFSpan)) {
        context.Complete(reader);
        return SkStackClientLoggerExtensions.EchobackLineMarker;
      }
      else {
        context.Ignore();
        return null;
      }
    }

    private static (
      SkStackResponseStatus status,
      ReadOnlyMemory<byte> statusText
    )
    ParseStatusLine(
      ISkStackSequenceParserContext context,
      ReadOnlyMemory<byte> command // not used
    )
    {
      SkStackResponseStatus status = default;
      ReadOnlyMemory<byte> statusText = default;

      var reader = context.CreateReader();

      // TODO: ROHM product setting commands respond the status line terminated with CR, instead of CRLF
      if (!reader.TryReadTo(out ReadOnlySequence<byte> statusLine, delimiter: SkStack.CRLFSpan, advancePastDelimiter: true)) {
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
        //context.Ignore();
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
}