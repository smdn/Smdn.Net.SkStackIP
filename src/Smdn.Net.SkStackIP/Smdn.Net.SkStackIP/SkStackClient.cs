// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.IO.Ports;

using Microsoft.Extensions.Logging;

using Smdn.Buffers;
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

  /// <summary>
  /// The default baud rate for the <see cref="SerialPort"/>.
  /// </summary>
  public const int DefaultBaudRate = 115200;

  private static readonly TimeSpan ContinuousReadingInterval = TimeSpan.FromMilliseconds(10); // TODO: make configurable

  /*
   * instance members
   */
  private Stream stream;

  private PipeWriter streamWriter;
  private readonly IBufferWriter<byte> writer;
  private PipeReader streamReader;

  private readonly ILogger? logger;

  private readonly ArrayBufferWriter<byte>? logWriter;

  /// <summary>
  /// Initializes a new instance of the <see cref="SkStackClient"/> class with specifying the serial port name.
  /// </summary>
  /// <param name="serialPortName">
  /// A <see cref="string"/> that holds the serial port name for communicating with the device that implements the SKSTACK-IP protocol.
  /// </param>
  /// <param name="baudRate">
  /// A <see cref="int"/> value that represents the baud rate of the serial port for communicating with the device.
  /// </param>
  /// <param name="logger">The <see cref="ILogger"/> to report the situation.</param>
  public SkStackClient(
    string serialPortName,
    int baudRate = DefaultBaudRate,
    ILogger? logger = null
  )
    : this(
      stream: OpenSerialPortStream(serialPortName, baudRate),
      logger: logger
    )
  {
  }

  private static Stream OpenSerialPortStream(string serialPortName, int baudRate)
  {
    if (serialPortName is null)
      throw new ArgumentNullException(nameof(serialPortName));
    if (serialPortName.Length == 0)
      throw new ArgumentException("must be non-empty string", nameof(serialPortName));

#pragma warning disable CA2000
    var port = new SerialPort(
      portName: serialPortName,
      baudRate: baudRate,
      parity: Parity.None,
      dataBits: 8
      // stopBits: StopBits.None
    ) {
      Handshake = Handshake.None, // TODO: RequestToSend
      DtrEnable = false,
      RtsEnable = false,
      NewLine = SkStack.DefaultEncoding.GetString(SkStack.CRLFSpan),
    };
#pragma warning restore CA2000

    port.Open();

    return port.BaseStream;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="SkStackClient"/> class with specifying the serial port name.
  /// </summary>
  /// <param name="stream">
  /// The data stream for transmitting SKSTACK-IP protocol.
  /// </param>
  /// <param name="logger">The <see cref="ILogger"/> to report the situation.</param>
  public SkStackClient(
    Stream stream,
    ILogger? logger = null
  )
  {
    if (stream is null)
      throw new ArgumentNullException(nameof(stream));
    if (!stream.CanRead)
      throw new ArgumentException($"{nameof(stream)} must be readable stream", nameof(stream));
    if (!stream.CanWrite)
      throw new ArgumentException($"{nameof(stream)} must be writable stream", nameof(stream));

    this.stream = stream;

    streamWriter = PipeWriter.Create(
      stream,
      new(leaveOpen: true, minimumBufferSize: 64)
    );
    streamReader = PipeReader.Create(
      stream,
      new(leaveOpen: true, bufferSize: 1024, minimumReadSize: 256)
    );

    this.logger = logger;

    if (logger is not null && logger.IsCommandLoggingEnabled()) {
      logWriter = new ArrayBufferWriter<byte>(initialCapacity: 64);
      writer = DuplicateBufferWriter.Create(streamWriter, logWriter);
    }
    else {
      writer = streamWriter;
    }

    parseSequenceContext = new ParseSequenceContext();
    streamReaderSemaphore = new(initialCount: 1, maxCount: 1);

    StartCapturingUdpReceiveEvents(SkStackKnownPortNumbers.EchonetLite);
  }

  private void ThrowIfDisposed()
  {
    if (stream is null)
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

      stream?.Close();
      stream = null!;

      streamReaderSemaphore?.Dispose();
      streamReaderSemaphore = null!;
    }
  }
}
