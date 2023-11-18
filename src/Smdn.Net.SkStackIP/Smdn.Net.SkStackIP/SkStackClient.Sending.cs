// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Threading;
using System.Threading.Tasks;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP;

#pragma warning disable IDE0040
partial class SkStackClient {
#pragma warning restore IDE0040
  /// <summary>Sends a command.</summary>
  /// <param name="command">The command to be sent.</param>
  /// <param name="writeArguments">The <see cref="Action{ISkStackCommandLineWriter}"/> for write command arguments to the buffer. Can be <see langword="null"/> if command has no arguments.</param>
  /// <param name="syntax">The <see cref="SkStackProtocolSyntax"/> that describes the command syntax.</param>
  /// <param name="throwIfErrorStatus">The <see langword="bool"/> value that specifies whether throw exception if the response status is error.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken" /> to monitor for cancellation requests.</param>
  protected internal async ValueTask<SkStackResponse> SendCommandAsync(
    ReadOnlyMemory<byte> command,
    Action<ISkStackCommandLineWriter>? writeArguments = null,
    SkStackProtocolSyntax? syntax = null,
    bool throwIfErrorStatus = true,
    CancellationToken cancellationToken = default
  )
  {
    var resp = await SendCommandAsyncCore<SkStackResponse.NullPayload>(
      command: command,
      writeArguments: writeArguments,
      parseResponsePayload: null,
      commandEventHandler: null,
      syntax: syntax ?? SkStackProtocolSyntax.Default,
      throwIfErrorStatus: throwIfErrorStatus,
      cancellationToken: cancellationToken
    ).ConfigureAwait(false);

    return resp;
  }

  /// <summary>Sends a command.</summary>
  /// <typeparam name="TPayload">The type of response payload. See <paramref name="parseResponsePayload"/>.</typeparam>
  /// <param name="command">The command to be sent.</param>
  /// <param name="writeArguments">The <see cref="Action{ISkStackCommandLineWriter}"/> for write command arguments to the buffer. Can be <see langword="null"/> if command has no arguments.</param>
  /// <param name="parseResponsePayload">The delegate for parsing the response payload. If <see langword="null"/>, parsing response payload will not be attempted.</param>
  /// <param name="syntax">The <see cref="SkStackProtocolSyntax"/> that describes the command syntax.</param>
  /// <param name="throwIfErrorStatus">The <see langword="bool"/> value that specifies whether throw exception if the response status is error.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken" /> to monitor for cancellation requests.</param>
  protected internal ValueTask<SkStackResponse<TPayload>> SendCommandAsync<TPayload>(
    ReadOnlyMemory<byte> command,
    Action<ISkStackCommandLineWriter>? writeArguments,
    SkStackSequenceParser<TPayload?> parseResponsePayload,
    SkStackProtocolSyntax? syntax = null,
    bool throwIfErrorStatus = true,
    CancellationToken cancellationToken = default
  )
    => SendCommandAsyncCore(
      command: command,
      writeArguments: writeArguments,
      parseResponsePayload: parseResponsePayload ?? throw new ArgumentNullException(nameof(parseResponsePayload)),
      commandEventHandler: null,
      syntax: syntax ?? SkStackProtocolSyntax.Default,
      throwIfErrorStatus: throwIfErrorStatus,
      cancellationToken: cancellationToken
    );

  /// <summary>Sends a command.</summary>
  /// <param name="command">The command to be sent.</param>
  /// <param name="writeArguments">The <see cref="Action{ISkStackCommandLineWriter}"/> for write command arguments to the buffer. Can be <see langword="null"/> if command has no arguments.</param>
  /// <param name="commandEventHandler">The <see cref="SkStackEventHandlerBase" /> that handles the events that will occur until the response is received.</param>
  /// <param name="syntax">The <see cref="SkStackProtocolSyntax"/> that describes the command syntax.</param>
  /// <param name="throwIfErrorStatus">The <see langword="bool"/> value that specifies whether throw exception if the response status is error.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken" /> to monitor for cancellation requests.</param>
  internal async ValueTask<SkStackResponse> SendCommandAsync(
    ReadOnlyMemory<byte> command,
    Action<ISkStackCommandLineWriter>? writeArguments,
    SkStackEventHandlerBase? commandEventHandler,
    SkStackProtocolSyntax? syntax = null,
    bool throwIfErrorStatus = true,
    CancellationToken cancellationToken = default
  )
  {
    var resp = await SendCommandAsyncCore<SkStackResponse.NullPayload>(
      command: command,
      writeArguments: writeArguments,
      parseResponsePayload: null,
      commandEventHandler: commandEventHandler,
      syntax: syntax ?? SkStackProtocolSyntax.Default,
      throwIfErrorStatus: throwIfErrorStatus,
      cancellationToken: cancellationToken
    ).ConfigureAwait(false);

    return resp;
  }

  /// <summary>Sends a command.</summary>
  /// <typeparam name="TPayload">The type of response payload. See <paramref name="parseResponsePayload"/>.</typeparam>
  /// <param name="command">The command to be sent.</param>
  /// <param name="writeArguments">The <see cref="Action{ISkStackCommandLineWriter}"/> for write command arguments to the buffer. Can be <see langword="null"/> if command has no arguments.</param>
  /// <param name="parseResponsePayload">The delegate for parsing the response payload. If <see langword="null"/>, parsing response payload will not be attempted.</param>
  /// <param name="commandEventHandler">The <see cref="SkStackEventHandlerBase" /> that handles the events that will occur until the response is received.</param>
  /// <param name="syntax">The <see cref="SkStackProtocolSyntax"/> that describes the command syntax.</param>
  /// <param name="throwIfErrorStatus">The <see langword="bool"/> value that specifies whether throw exception if the response status is error.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken" /> to monitor for cancellation requests.</param>
  private ValueTask<SkStackResponse<TPayload>> SendCommandAsyncCore<TPayload>(
    ReadOnlyMemory<byte> command,
    Action<ISkStackCommandLineWriter>? writeArguments,
    SkStackSequenceParser<TPayload?>? parseResponsePayload,
    SkStackEventHandlerBase? commandEventHandler,
    SkStackProtocolSyntax syntax,
    bool throwIfErrorStatus,
    CancellationToken cancellationToken
  )
  {
    ThrowIfDisposed();

    syntax ??= SkStackProtocolSyntax.Default;

    // write command line
    WriteCommandLine(
      command,
      writeArguments,
      syntax
    );

    // flush command and receive response
    return FlushAndReceive(
      command,
      parseResponsePayload,
      commandEventHandler,
      syntax,
      throwIfErrorStatus,
      cancellationToken
    );
  }

  private void WriteCommandLine(
    ReadOnlyMemory<byte> command,
    Action<ISkStackCommandLineWriter>? writeArguments,
    SkStackProtocolSyntax syntax
  )
  {
    if (command.IsEmpty)
      throw new ArgumentException("must be non-empty byte sequence", nameof(command));

    // write command
    commandLineWriter.Write(command.Span);

    // write arguments
    if (writeArguments is not null)
      writeArguments(commandLineWriter);

    // write end of command line
    if (!syntax.EndOfCommandLine.IsEmpty)
      // must terminate the SKSENDTO command line without CRLF
      // ROHM product setting commands line must be terminated with CR instead of CRLF
      commandLineWriter.Write(syntax.EndOfCommandLine);

    // write command to logger
    if (Logger is not null && logWriter is not null) {
      Logger.LogDebugCommand(logWriter.WrittenMemory);
      logWriter.Clear();
    }
  }

  private async ValueTask<SkStackResponse<TPayload>> FlushAndReceive<TPayload>(
    ReadOnlyMemory<byte> command,
    SkStackSequenceParser<TPayload?>? parseResponsePayload,
    SkStackEventHandlerBase? commandEventHandler,
    SkStackProtocolSyntax syntax,
    bool throwIfErrorStatus,
    CancellationToken cancellationToken
  )
  {
    var writerResult = await streamWriter.FlushAsync(cancellationToken).ConfigureAwait(false);

    if (writerResult.IsCompleted)
      throw new InvalidOperationException("writer is completed");

    var response = await ReceiveResponseAsync(
      command,
      parseResponsePayload,
      commandEventHandler,
      syntax,
      cancellationToken
    ).ConfigureAwait(false);

    if (throwIfErrorStatus)
      response.ThrowIfErrorStatus(translateException: null);

    return response;
  }
}
