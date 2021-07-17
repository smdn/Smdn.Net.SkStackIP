// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Ports;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Smdn.Net.SkStackIP {
  public partial class SkStackClient :
    IDisposable
  {
    /*
     * static members
     */
    public const int DefaultBaudRate = 115200;

    public static SkStackClient Create(
      string serialPortName,
      int baudRate = DefaultBaudRate,
      IServiceProvider serviceProvider = null
    )
    {
      if (string.IsNullOrEmpty(serialPortName))
        throw new ArgumentException("must be non-null non-empty string", nameof(serialPortName));

      var port = new SerialPort(
        portName: serialPortName,
        baudRate: baudRate,
        parity: Parity.None,
        dataBits: 8
        //stopBits: StopBits.None
      );

      port.Handshake = Handshake.None; // TODO: RequestToSend
      port.DtrEnable = false;
      port.RtsEnable = false;
      port.NewLine = Encoding.ASCII.GetString(crlfSequence.Span);

      port.Open();

      return Create(port.BaseStream, serviceProvider);
    }

    public static SkStackClient Create(
      Stream stream,
      IServiceProvider serviceProvider = null
    )
    {
      if (stream is null)
        throw new ArgumentNullException(nameof(stream));
      if (!stream.CanRead)
        throw new ArgumentException($"{nameof(stream)} must be readable stream", nameof(stream));
      if (!stream.CanWrite)
        throw new ArgumentException($"{nameof(stream)} must be readable stream", nameof(stream));

      return new SkStackClient(
        stream,
        serviceProvider
      );
    }

    /*
     * instance members
     */
    private static readonly Memory<byte> crlfSequence = new[] {(byte)'\r', (byte)'\n'};

    private Stream stream;
    public Stream BaseStream => ThrowIfDisposed();
    private Stream ThrowIfDisposed() => stream ?? throw new ObjectDisposedException(GetType().FullName);

    private PipeWriter writer;
    private PipeReader reader;

    private readonly ILogger logger;

    private SkStackClient(Stream stream, IServiceProvider serviceProvider = null)
    {
      this.stream = stream;

      this.writer = PipeWriter.Create(
        stream,
        new(leaveOpen: true, minimumBufferSize: 64)
      );
      this.reader = PipeReader.Create(
        stream,
        new(leaveOpen: true, bufferSize: 1024, minimumReadSize: 1)
      );

      this.logger = serviceProvider?.GetService<ILoggerFactory>()?.CreateLogger<SkStackClient>();
    }

    public void Close()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    void IDisposable.Dispose() => Close();

    protected virtual void Dispose(bool disposing)
    {
      if (disposing) {
        writer.Complete();
        writer = null;

        reader.Complete();
        reader = null;

        stream?.Close();
        stream = null;
      }
    }

    public Task<SkStackResponse> SendCommandAsync(
      ReadOnlyMemory<byte> command,
      IEnumerable<string> arguments = null,
      CancellationToken cancellationToken = default,
      bool throwIfErrorStatus = true
    )
    {
      ThrowIfDisposed();

      if (command.IsEmpty)
        throw new ArgumentException("must be non-empty byte sequence", nameof(command));

      IBufferWriter<byte> writer = (logger?.IsCommandEnabled() ?? false)
        ? new DuplicateBufferWriter<byte>(this.writer, defaultSegmentSize: 2)
        : this.writer;

      // write command
      command.CopyTo(writer.GetMemory(command.Length));
      writer.Advance(command.Length);

      // write arguments
      foreach (var argument in arguments ?? Enumerable.Empty<string>()) {
        const byte sp = 0x20;

        writer.GetSpan(1)[0] = sp;
        writer.Advance(1);

        var span = writer.GetSpan(Encoding.ASCII.GetByteCount(argument));

        writer.Advance(Encoding.ASCII.GetBytes(argument, span));
      }

      // write CRLF
      crlfSequence.CopyTo(writer.GetMemory(crlfSequence.Length));
      writer.Advance(crlfSequence.Length);

      if (writer is DuplicateBufferWriter<byte> duplicateWriter)
        logger.LogTraceCommand(duplicateWriter.Sequence);

      // flush and send
      if (throwIfErrorStatus)
        return SendAndThrowIfErrorStatusAsync(command, cancellationToken);
      else
        return SendAsync(command, cancellationToken);

      async Task<SkStackResponse> SendAndThrowIfErrorStatusAsync(
        ReadOnlyMemory<byte> comm,
        CancellationToken token
      )
      {
        var response = await SendAsync(comm, token).ConfigureAwait(false);

        response.ThrowIfErrorStatus();

        return response;
      }
    }

    private async Task<SkStackResponse> SendAsync(
      ReadOnlyMemory<byte> command,
      CancellationToken cancellationToken = default
    )
    {
      var writerResult = await writer.FlushAsync(cancellationToken).ConfigureAwait(false);

      if (writerResult.IsCompleted)
        throw new InvalidOperationException("completed");

      return (
        await TryReceiveAsyncCore(
          command,
          cancellationToken
        ).ConfigureAwait(false)
      ).receivedResponse;
    }

    public Task<bool> TryReceiveAsync(
      CancellationToken cancellationToken = default
    )
    {
      ThrowIfDisposed();

      return Core();

      async Task<bool> Core()
        => (await TryReceiveAsyncCore(default, cancellationToken).ConfigureAwait(false)).hasEventsReceived;
    }

    private async Task<(SkStackResponse receivedResponse, bool hasEventsReceived)> TryReceiveAsyncCore(
      ReadOnlyMemory<byte> command,
      CancellationToken cancellationToken
    )
    {
      SkStackResponse response = null;

      for (;;) {
        var readerResult = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
        var readBufferSequence = readerResult.Buffer;

        if (readerResult.IsCanceled)
          return (null, false);

        try {
          var advance = false;

          for (;;) {
            if (TryParseResponseLine(command, ref response, ref readBufferSequence))
              advance = true;
            else
              break;
          }

          if (advance)
            logger?.LogDebugResponse(readerResult.Buffer.Slice(0, readBufferSequence.Start));

          reader.AdvanceTo(readBufferSequence.Start, readBufferSequence.End);
        }
        catch (SkStackResponseException ex) {
          logger?.LogDebugResponse(readerResult.Buffer.Slice(0, readBufferSequence.Start), exception: ex);
          throw;
        }

        if (response is not null && response.Status != SkStackResponseStatus.Undetermined)
          break;

        await Task.Delay(5).ConfigureAwait(false);
      }

      return (response, false);
    }

    private bool TryParseResponseLine(
      ReadOnlyMemory<byte> command,
      ref SkStackResponse response,
      ref ReadOnlySequence<byte> responseSequence
    )
    {
      var reader = new SequenceReader<byte>(responseSequence);

      // try read first line
      if (!reader.TryReadTo(out ReadOnlySequence<byte> line, delimiter: crlfSequence.Span, advancePastDelimiter: true))
        return false; // incomplete line

      responseSequence = reader.UnreadSequence;

      // test echoback line
      if (!command.IsEmpty) {
        var lineReader = new SequenceReader<byte>(line);

        if (lineReader.IsNext(command.Span, advancePast: false)) {
          // skip echoback line
          logger?.LogTraceResponse(line, isEchoBack: true);
          return true;
        }
      }

      logger?.LogTraceResponse(line, isEchoBack: false);

      response ??= new SkStackResponse();

      if (!TryParseStatusLine(response, line))
        response.ResponseLines.Add(line.ToArray());

      return true;
    }

    private static readonly ReadOnlyMemory<byte> statusOk = SkStack.ToByteSequence("OK");
    private static readonly ReadOnlyMemory<byte> statusFail = SkStack.ToByteSequence("FAIL");

    private static bool TryParseStatusLine(SkStackResponse response, ReadOnlySequence<byte> line)
    {
      var reader = new SequenceReader<byte>(line);
      SkStackResponseStatus? status = null;

      if (reader.IsNext(statusOk.Span, advancePast: true))
        status = SkStackResponseStatus.Ok;
      else if (reader.IsNext(statusFail.Span, advancePast: true))
        status = SkStackResponseStatus.Fail;

      if (!status.HasValue)
        return false;

      response.Status = status.Value;

      if (reader.TryPeek(out var next) && next == 0x20/*SP*/) {
        reader.Advance(1L);
        response.StatusText = reader.UnreadSequence.ToArray();
      }

      return true;
    }
  }
}
