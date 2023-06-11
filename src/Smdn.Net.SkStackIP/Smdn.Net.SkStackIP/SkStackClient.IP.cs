// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
#if NULL_STATE_STATIC_ANALYSIS_ATTRIBUTES
using System.Diagnostics.CodeAnalysis;
#endif
using System.IO.Pipelines;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Smdn.Buffers;
using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP;

#pragma warning disable IDE0040
partial class SkStackClient {
#pragma warning restore IDE0040
  private readonly Dictionary<int/*port*/, Pipe> udpReceiveEventPipes = new(
    capacity: SkStackUdpPort.NumberOfPorts
  );

  public void StartCapturingUdpReceiveEvents(int port)
  {
    ThrowIfDisposed();

    SkStackUdpPort.ThrowIfPortNumberIsOutOfRangeOrUnused(port, nameof(port));

    lock (udpReceiveEventPipes) {
      udpReceiveEventPipes[port] = new Pipe(new PipeOptions()); // TODO: options
    }
  }

  public void StopCapturingUdpReceiveEvents(int port)
  {
    ThrowIfDisposed();

    SkStackUdpPort.ThrowIfPortNumberIsOutOfRangeOrUnused(port, nameof(port));

    lock (udpReceiveEventPipes) {
      udpReceiveEventPipes.Remove(port);
    }
  }

  private const int UdpReceiveEventLengthOfRemoteAddress = 16;
  private const int UdpReceiveEventLengthOfDataLength = sizeof(ushort);

  private ValueTask OnERXUDPAsync(
    int localPort,
    IPAddress remoteAddress,
    ReadOnlySequence<byte> data,
    int dataLength,
    SkStackERXUDPDataFormat dataFormat,
    CancellationToken cancellationToken
  )
  {
    if (!udpReceiveEventPipes.TryGetValue(localPort, out var pipe))
      // not capturing
#if SYSTEM_THREADING_TASKS_VALUETASK_COMPLETEDTASK
      return ValueTask.CompletedTask;
#else
      return default;
#endif

    return OnERXUDPAsyncCore(pipe.Writer, remoteAddress, data, dataLength, dataFormat, cancellationToken);

    static async ValueTask OnERXUDPAsyncCore(
      PipeWriter writer,
      IPAddress remoteAddress,
      ReadOnlySequence<byte> data,
      int dataLength,
      SkStackERXUDPDataFormat dataFormat,
      CancellationToken cancellationToken
    )
    {
      var packetLength = UdpReceiveEventLengthOfRemoteAddress + UdpReceiveEventLengthOfDataLength + dataLength;
      var memory = writer.GetMemory(dataLength);

      // BYTE[16]: remote address
      if (!remoteAddress.TryWriteBytes(memory.Span, out var bytesWritten) && bytesWritten != UdpReceiveEventLengthOfRemoteAddress)
        throw new InvalidOperationException("unexpected format of remote address");

      // UINT16: length of data
      BinaryPrimitives.WriteUInt16LittleEndian(memory.Span.Slice(UdpReceiveEventLengthOfRemoteAddress), (ushort)dataLength);

      // BYTE[n]: data
      if (dataFormat == SkStackERXUDPDataFormat.Raw) {
        data.CopyTo(memory.Span.Slice(UdpReceiveEventLengthOfRemoteAddress + UdpReceiveEventLengthOfDataLength));
      }
      else {
        SkStackTokenParser.ToByteSequence(
          data,
          dataLength,
          memory.Span.Slice(UdpReceiveEventLengthOfRemoteAddress + UdpReceiveEventLengthOfDataLength)
        );
      }

      writer.Advance(packetLength);

      var result = await writer.FlushAsync(cancellationToken).ConfigureAwait(false);

      if (result.IsCompleted)
        return;
        // throw new InvalidOperationException("writer is completed");
    }
  }

  public ValueTask<SkStackUdpReceiveResult> UdpReceiveAsync(
    int port,
    CancellationToken cancellationToken = default
  )
  {
    ThrowIfDisposed();

    SkStackUdpPort.ThrowIfPortNumberIsOutOfRangeOrUnused(port, nameof(port));

    if (!udpReceiveEventPipes.TryGetValue(port, out var pipe))
      throw new InvalidOperationException($"The port number {port} is not configured to capture receiving events. Call the method `{nameof(StartCapturingUdpReceiveEvents)}` first.");

    return UdpReceiveAsyncCore(this, pipe.Reader, cancellationToken);

    static async ValueTask<SkStackUdpReceiveResult> UdpReceiveAsyncCore(
      SkStackClient thisClient,
      PipeReader pipeReader,
      CancellationToken cancellationToken
    )
    {
      for (; ;) {
        if (!pipeReader.TryRead(out var readResult)) {
          var receiveNotificationalEventResult = await thisClient.ReceiveNotificationalEventAsync(cancellationToken).ConfigureAwait(false);

          if (!receiveNotificationalEventResult.Received)
            await Task.Delay(ContinuousReadingInterval, cancellationToken).ConfigureAwait(false);

          continue;
        }

        if (readResult.IsCanceled)
          throw new InvalidOperationException("pending read was cancelled");

        var buffer = readResult.Buffer;

        if (TryReadReceiveResult(ref buffer, out var result)) {
          // advance the buffer to the position where the reading finished
          pipeReader.AdvanceTo(consumed: buffer.Start);

          return result;
        }
        else {
          // mark entire buffer as examined to receive the subsequent data
          pipeReader.AdvanceTo(consumed: readResult.Buffer.Start, examined: readResult.Buffer.End);

          // continue;
        }
      }
    }

    static bool TryReadReceiveResult(
      ref ReadOnlySequence<byte> unreadSequence,
#if NULL_STATE_STATIC_ANALYSIS_ATTRIBUTES
      [NotNullWhen(true)]
#endif
      out SkStackUdpReceiveResult? result
    )
    {
      result = default;

      var reader = new SequenceReader<byte>(unreadSequence);

      if (reader.Remaining < UdpReceiveEventLengthOfRemoteAddress + UdpReceiveEventLengthOfDataLength)
        return false; // need more

      // BYTE[16]: remote address
      Span<byte> remoteAddressBytes = stackalloc byte[UdpReceiveEventLengthOfRemoteAddress];

      reader.TryCopyTo(remoteAddressBytes);
      reader.Advance(UdpReceiveEventLengthOfRemoteAddress);

      var remoteAddress = new IPAddress(remoteAddressBytes);

      // UINT16: length of data
      reader.TryReadLittleEndian(out short signedLengthOfData);

      var dataLength = unchecked((ushort)signedLengthOfData);

      // BYTE[n]: data
      if (reader.Remaining < dataLength)
        return false; // need more

      var data = MemoryPool<byte>.Shared.Rent(dataLength);

      reader.TryCopyTo(data.Memory.Span.Slice(0, dataLength));
      reader.Advance(dataLength);

      unreadSequence = reader.GetUnreadSequence();

#pragma warning disable CA2000
      result = new SkStackUdpReceiveResult(
        remoteAddress: remoteAddress,
        length: dataLength,
        data: data
      );
#pragma warning restore CA2000

      return true;
    }
  }
}
