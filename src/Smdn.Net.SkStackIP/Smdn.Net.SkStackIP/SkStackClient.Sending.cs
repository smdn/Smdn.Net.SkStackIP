// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP {
  partial class SkStackClient {
    public async ValueTask<SkStackResponse> SendCommandAsync(
      ReadOnlyMemory<byte> command,
      IEnumerable<ReadOnlyMemory<byte>> arguments = null,
      CancellationToken cancellationToken = default,
      bool throwIfErrorStatus = true
    )
    {
      var resp = await SendCommandAsyncCore<SkStackResponse.NullPayload>(
        command: command,
        arguments: arguments ?? Enumerable.Empty<ReadOnlyMemory<byte>>(),
        endOfCommandLine: SkStack.CRLFMemory,
        parseResponsePayload: null,
        expectsStatusResponse: true,
        cancellationToken: cancellationToken,
        throwIfErrorStatus: throwIfErrorStatus
      ).ConfigureAwait(false);

      return resp;
    }

    /// <param name="arguments">can be null</param>
    public async ValueTask<SkStackResponse> SendCommandAsync(
      ReadOnlyMemory<byte> command,
      IEnumerable<ReadOnlyMemory<byte>> arguments,
      ReadOnlyMemory<byte> endOfCommandLine,
      CancellationToken cancellationToken = default,
      bool throwIfErrorStatus = true
    )
    {
      var resp = await SendCommandAsyncCore<SkStackResponse.NullPayload>(
        command: command,
        arguments: arguments ?? Enumerable.Empty<ReadOnlyMemory<byte>>(),
        endOfCommandLine: endOfCommandLine,
        parseResponsePayload: null,
        expectsStatusResponse: true,
        cancellationToken: cancellationToken,
        throwIfErrorStatus: throwIfErrorStatus
      ).ConfigureAwait(false);

      return resp;
    }

    /// <param name="arguments">can be null</param>
    public ValueTask<SkStackResponse<TPayload>> SendCommandAsync<TPayload>(
      ReadOnlyMemory<byte> command,
      IEnumerable<ReadOnlyMemory<byte>> arguments,
      SkStackSequenceParser<TPayload> parseResponsePayload,
      CancellationToken cancellationToken = default,
      bool throwIfErrorStatus = true
    )
      => SendCommandAsyncCore(
        command: command,
        arguments: arguments ?? Enumerable.Empty<ReadOnlyMemory<byte>>(),
        endOfCommandLine: SkStack.CRLFMemory,
        parseResponsePayload: parseResponsePayload ?? throw new ArgumentNullException(nameof(parseResponsePayload)),
        expectsStatusResponse: true,
        cancellationToken: cancellationToken,
        throwIfErrorStatus: throwIfErrorStatus
      );

    /// <param name="arguments">can be null</param>
    public ValueTask<SkStackResponse<TPayload>> SendCommandStatusUndefinedAsync<TPayload>(
      ReadOnlyMemory<byte> command,
      IEnumerable<ReadOnlyMemory<byte>> arguments,
      SkStackSequenceParser<TPayload> parseResponsePayload,
      CancellationToken cancellationToken = default,
      bool throwIfErrorStatus = true
    )
      => SendCommandAsyncCore(
        command: command,
        arguments: arguments,
        endOfCommandLine: SkStack.CRLFMemory,
        parseResponsePayload: parseResponsePayload ?? throw new ArgumentNullException(nameof(parseResponsePayload)),
        expectsStatusResponse: false,
        cancellationToken: cancellationToken,
        throwIfErrorStatus: throwIfErrorStatus
      );

    /// <param name="parseResponsePayload">If null, parsing response payload will not be attempted.</param>
    /// <param name="expectsStatusResponse">If true, expects that response has its status line.</param>
    private ValueTask<SkStackResponse<TPayload>> SendCommandAsyncCore<TPayload>(
      ReadOnlyMemory<byte> command,
      IEnumerable<ReadOnlyMemory<byte>> arguments,
      ReadOnlyMemory<byte> endOfCommandLine,
      SkStackSequenceParser<TPayload> parseResponsePayload,
      bool expectsStatusResponse,
      CancellationToken cancellationToken,
      bool throwIfErrorStatus
    )
    {
      ThrowIfDisposed();

      // write command line
      WriteCommandLine(
        command,
        arguments,
        endOfCommandLine
      );

      // flush command and receive response
      return FlushAndReceive(
        command,
        parseResponsePayload,
        expectsStatusResponse,
        throwIfErrorStatus,
        cancellationToken
      );
    }

    private void WriteCommandLine(
      ReadOnlyMemory<byte> command,
      IEnumerable<ReadOnlyMemory<byte>> arguments,
      ReadOnlyMemory<byte> endOfCommandLine
    )
    {
      if (command.IsEmpty)
        throw new ArgumentException("must be non-empty byte sequence", nameof(command));
#if DEBUG
      if (arguments is null)
        throw new ArgumentNullException(nameof(arguments));
#endif

      foreach (var argument in arguments) {
        if (argument.IsEmpty)
          throw new ArgumentException("cannot send command with empty argument", nameof(arguments));
      }

      // write command
      command.CopyTo(writer.GetMemory(command.Length));
      writer.Advance(command.Length);

      // write arguments
      foreach (var argument in arguments) {
        writer.GetSpan(1)[0] = SkStack.SP;
        writer.Advance(1);

        argument.Span.CopyTo(writer.GetSpan(argument.Length));

        writer.Advance(argument.Length);
      }

      // write end of command line
      if (!endOfCommandLine.IsEmpty) {
        // must terminate the SKSENDTO command line without CRLF
        // ROHM product setting commands line must be terminated with CR instead of CRLF
        endOfCommandLine.Span.CopyTo(writer.GetMemory(endOfCommandLine.Length).Span);
        writer.Advance(endOfCommandLine.Length);
      }

      // write command to logger
      if (logWriter is not null) {
        logger.LogDebugCommand(logWriter.WrittenMemory);
        logWriter.Clear();
      }
    }

    private async ValueTask<SkStackResponse<TPayload>> FlushAndReceive<TPayload>(
      ReadOnlyMemory<byte> command,
      SkStackSequenceParser<TPayload> parseResponsePayload,
      bool expectsStatusResponse,
      bool throwIfErrorStatus,
      CancellationToken cancellationToken
    )
    {
      var writerResult = await this.streamWriter.FlushAsync(cancellationToken).ConfigureAwait(false);

      if (writerResult.IsCompleted)
        throw new InvalidOperationException("writer is completed");

      var response = await ReceiveResponseAsync(
        command,
        expectsStatusResponse,
        parseResponsePayload,
        cancellationToken
      ).ConfigureAwait(false);

      if (throwIfErrorStatus)
        response.ThrowIfErrorStatus();

      return response;
    }
  }
}