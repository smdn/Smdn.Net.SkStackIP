// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;

using Microsoft.Extensions.Logging;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP;

/// <summary>
/// Provides a client implementation that sends SKSTACK-IP commands and receives responses and handles events.
/// </summary>
public partial class SkStackClient :
  IDisposable
{
  /*
   * static members
   */
  private static readonly TimeSpan ContinuousReadingInterval = TimeSpan.FromMilliseconds(10); // TODO: make configurable

  /*
   * instance members
   */
  private PipeWriter streamWriter;
  private readonly SkStackCommandLineWriter commandLineWriter;
  private PipeReader streamReader;

  protected ILogger? Logger { get; }

  private readonly ArrayBufferWriter<byte>? logWriter;

  /// <summary>
  /// Initializes a new instance of the <see cref="SkStackClient"/> class with specifying the <see cref="Stream"/> for transmitting SKSTACK-IP protocol.
  /// </summary>
  /// <param name="stream">
  /// The data stream for transmitting SKSTACK-IP protocol.
  /// </param>
  /// <param name="leaveStreamOpen">
  /// A <see langworkd="bool"/> value specifying whether the <paramref name="stream"/> should be left open or not when disposing instance.
  /// </param>
  /// <param name="erxudpDataFormat">
  /// A value that specifies the format of the data part received in the event <c>ERXUDP</c>. See <see cref="SkStackERXUDPDataFormat"/>.
  /// </param>
  /// <param name="logger">The <see cref="ILogger"/> to report the situation.</param>
  public SkStackClient(
    Stream stream,
    bool leaveStreamOpen = true,
    SkStackERXUDPDataFormat erxudpDataFormat = default,
    ILogger? logger = null
  )
    : this(
      sender: PipeWriter.Create(
        ValidateStream(stream, nameof(stream)),
        new(leaveOpen: leaveStreamOpen, minimumBufferSize: 64)
      ),
      receiver: PipeReader.Create(
        stream,
        new(leaveOpen: leaveStreamOpen, bufferSize: 1024, minimumReadSize: 256)
      ),
      erxudpDataFormat: erxudpDataFormat,
      logger: logger
    )
  {
  }

  private static Stream ValidateStream(Stream stream, string paramNameOfStream)
  {
    if (stream is null)
      throw new ArgumentNullException(paramName: paramNameOfStream);
    if (!stream.CanRead)
      throw new ArgumentException(message: $"{nameof(stream)} must be readable stream", paramName: paramNameOfStream);
    if (!stream.CanWrite)
      throw new ArgumentException(message: $"{nameof(stream)} must be writable stream", paramName: paramNameOfStream);

    return stream;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="SkStackClient"/> class with specifying the <see cref="PipeWriter"/> and <see cref="PipeReader"/>.
  /// </summary>
  /// <param name="sender">
  /// A <see cref="PipeWriter"/> for sending SKSTACK-IP protocol commands.
  /// </param>
  /// <param name="receiver">
  /// A <see cref="PipeReader"/> for receiving SKSTACK-IP protocol responses.
  /// </param>
  /// <param name="erxudpDataFormat">
  /// A value that specifies the format of the data part received in the event <c>ERXUDP</c>. See <see cref="SkStackERXUDPDataFormat"/>.
  /// </param>
  /// <param name="logger">The <see cref="ILogger"/> to report the situation.</param>
  public SkStackClient(
    PipeWriter sender,
    PipeReader receiver,
    SkStackERXUDPDataFormat erxudpDataFormat = default,
    ILogger? logger = null
  )
  {
    streamReader = receiver ?? throw new ArgumentNullException(nameof(receiver));
    streamWriter = sender ?? throw new ArgumentNullException(nameof(sender));
    this.erxudpDataFormat = ValidateERXUDPDataFormat(erxudpDataFormat, nameof(erxudpDataFormat));
    Logger = logger;

    if (Logger is not null && Logger.IsCommandLoggingEnabled()) {
      logWriter = new ArrayBufferWriter<byte>(initialCapacity: 64);

      commandLineWriter = new(streamWriter, logWriter);
    }
    else {
      commandLineWriter = new(streamWriter, null);
    }

    parseSequenceContext = new ParseSequenceContext();
    streamReaderSemaphore = new(initialCount: 1, maxCount: 1);

    StartCapturingUdpReceiveEvents(SkStackKnownPortNumbers.EchonetLite);
  }

  protected void ThrowIfDisposed()
  {
    if (streamWriter is null)
      throw new ObjectDisposedException(GetType().FullName);
  }

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing)
  {
    if (disposing) {
      streamWriter?.Complete();
      streamWriter = null!;

      streamReader?.Complete();
      streamReader = null!;

      streamReaderSemaphore?.Dispose();
      streamReaderSemaphore = null!;
    }
  }
}
