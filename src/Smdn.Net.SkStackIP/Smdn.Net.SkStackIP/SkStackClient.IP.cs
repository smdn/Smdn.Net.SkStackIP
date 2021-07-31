// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Smdn.Buffers;
using Smdn.Net.SkStackIP.Protocol;
#if DEBUG
using Smdn.Text.Unicode.ControlPictures;
#endif

namespace Smdn.Net.SkStackIP {
  partial class SkStackClient {
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

    private const int udpReceiveEventLengthOfRemoteAddress = 16;
    private const int udpReceiveEventLengthOfDataLength = sizeof(ushort);

    private ValueTask OnERXUDPAsync(
      int localPort,
      IPAddress remoteAddress,
      ReadOnlySequence<byte> data,
      int dataLength,
      SkStackERXUDPDataFormat dataFormat
    )
    {
      if (!udpReceiveEventPipes.TryGetValue(localPort, out var pipe))
        // not capturing
#if NET5_0_OR_GREATER
        return ValueTask.CompletedTask;
#else
        return default(ValueTask);
#endif

      return OnERXUDPAsyncCore(pipe.Writer, remoteAddress, data, dataLength, dataFormat);

      static async ValueTask OnERXUDPAsyncCore(
        PipeWriter writer,
        IPAddress remoteAddress,
        ReadOnlySequence<byte> data,
        int dataLength,
        SkStackERXUDPDataFormat dataFormat
      )
      {
        var packetLength = udpReceiveEventLengthOfRemoteAddress + udpReceiveEventLengthOfDataLength + (int)dataLength;
        var memory = writer.GetMemory(dataLength);

        // BYTE[16]: remote address
        if (!remoteAddress.TryWriteBytes(memory.Span, out var bytesWritten) && bytesWritten != udpReceiveEventLengthOfRemoteAddress)
          throw new InvalidOperationException("unexpected format of remote address");

        // UINT16: length of data
        BinaryPrimitives.WriteUInt16LittleEndian(memory.Span.Slice(udpReceiveEventLengthOfRemoteAddress), (ushort)dataLength);

        // BYTE[n]: data
        if (dataFormat == SkStackERXUDPDataFormat.Raw) {
          data.CopyTo(memory.Span.Slice(udpReceiveEventLengthOfRemoteAddress + udpReceiveEventLengthOfDataLength));
        }
        else {
          SkStackTokenParser.ToByteSequence(
            data,
            dataLength,
            memory.Span.Slice(udpReceiveEventLengthOfRemoteAddress + udpReceiveEventLengthOfDataLength)
          );
        }

        writer.Advance(packetLength);

        var result = await writer.FlushAsync(/*cancellationToken*/).ConfigureAwait(false);

        if (result.IsCompleted)
          return;
          //throw new InvalidOperationException("writer is completed");
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
        SkStackClient _this,
        PipeReader pipeReader,
        CancellationToken cancellationToken
      )
      {
        for (;;) {
          if (!pipeReader.TryRead(out var readResult)) {
            var receiveNotificationalEventResult = await _this.ReceiveNotificationalEventAsync(cancellationToken).ConfigureAwait(false);

            if (!receiveNotificationalEventResult.Received)
              await Task.Delay(SkStackClient.continuousReadingInterval).ConfigureAwait(false);

            continue;
          }

          if (readResult.IsCanceled)
            return default;

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
        out SkStackUdpReceiveResult result
      )
      {
        result = default;

        var reader = new SequenceReader<byte>(unreadSequence);

        if (reader.Remaining < udpReceiveEventLengthOfRemoteAddress + udpReceiveEventLengthOfDataLength)
          return false; // need more

        // BYTE[16]: remote address
        Span<byte> remoteAddressBytes = stackalloc byte[udpReceiveEventLengthOfRemoteAddress];

        reader.TryCopyTo(remoteAddressBytes);
        reader.Advance(udpReceiveEventLengthOfRemoteAddress);

        var remoteAddress = new IPAddress(remoteAddressBytes);

        // UINT16: length of data
        reader.TryReadLittleEndian(out short signedLengthOfData);

        var dataLength = unchecked((ushort)signedLengthOfData);

        // BYTE[n]: data
        if (reader.Remaining < dataLength)
          return false; // need more

        var data = MemoryPool<byte>.Shared.Rent(dataLength);

        reader.GetUnreadSequence().Slice(0, dataLength).CopyTo(data.Memory.Span);
        reader.Advance(dataLength);

        unreadSequence = reader.GetUnreadSequence();

        result = new SkStackUdpReceiveResult(
          remoteAddress: remoteAddress,
          length: dataLength,
          data: data
        );

        return true;
      }
    }
  }
}