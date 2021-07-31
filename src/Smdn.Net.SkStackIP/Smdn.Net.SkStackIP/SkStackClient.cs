// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.IO.Ports;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Smdn.Buffers;
using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP {
  public partial class SkStackClient :
    IDisposable
  {
    /*
     * static members
     */
    public const int DefaultBaudRate = 115200;

    private static readonly TimeSpan continuousReadingInterval = TimeSpan.FromMilliseconds(10); // TODO: make configurable

    /*
     * instance members
     */
    private Stream stream;

    private PipeWriter streamWriter;
    private readonly IBufferWriter<byte> writer;
    private PipeReader streamReader;

    private readonly ILogger logger;

    private readonly ArrayBufferWriter<byte> logWriter;

    public SkStackClient(
      string serialPortName,
      int baudRate = DefaultBaudRate,
      IServiceProvider serviceProvider = null
    ) :
      this(
        stream: OpenSerialPortStream(serialPortName, baudRate),
        serviceProvider: serviceProvider
      )
    {
    }

    private static Stream OpenSerialPortStream(string serialPortName, int baudRate)
    {
      if (serialPortName is null)
        throw new ArgumentNullException(nameof(serialPortName));
      if (serialPortName.Length == 0)
        throw new ArgumentException("must be non-empty string", nameof(serialPortName));

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
      port.NewLine = SkStack.DefaultEncoding.GetString(SkStack.CRLFSpan);

      port.Open();

      return port.BaseStream;
    }

    public SkStackClient(
      Stream stream,
      IServiceProvider serviceProvider = null
    )
    {
      if (stream is null)
        throw new ArgumentNullException(nameof(stream));
      if (!stream.CanRead)
        throw new ArgumentException($"{nameof(stream)} must be readable stream", nameof(stream));
      if (!stream.CanWrite)
        throw new ArgumentException($"{nameof(stream)} must be writable stream", nameof(stream));

      this.stream = stream ?? throw new ArgumentNullException(nameof(stream));

      this.streamWriter = PipeWriter.Create(
        stream,
        new(leaveOpen: true, minimumBufferSize: 64)
      );
      this.streamReader = PipeReader.Create(
        stream,
        new(leaveOpen: true, bufferSize: 1024, minimumReadSize: 256)
      );

      this.logger = serviceProvider?.GetService<ILoggerFactory>()?.CreateLogger<SkStackClient>();

      if (this.logger is not null && this.logger.IsCommandLoggingEnabled()) {
        this.logWriter = new ArrayBufferWriter<byte>(initialCapacity: 64);
        this.writer = DuplicateBufferWriter.Create(this.streamWriter, this.logWriter);
      }
      else {
        this.writer = this.streamWriter;
      }

      this.parseSequenceContext = new ParseSequenceContext();
      this.streamReaderSemaphore = new(initialCount: 1, maxCount: 1);

      StartCapturingUdpReceiveEvents(SkStackKnownPortNumbers.EchonetLite);
    }

    public void Close()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    private void ThrowIfDisposed()
    {
      if (stream is null)
        throw new ObjectDisposedException(GetType().FullName);
    }

    void IDisposable.Dispose() => Close();

    protected virtual void Dispose(bool disposing)
    {
      if (disposing) {
        streamWriter?.Complete();
        streamWriter = null;

        streamReader?.Complete();
        streamReader = null;

        stream?.Close();
        stream = null;

        streamReaderSemaphore?.Dispose();
        streamReaderSemaphore = null;
      }
    }
  }
}
