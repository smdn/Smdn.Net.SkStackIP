// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP;

#pragma warning disable IDE0040
partial class SkStackClient {
#pragma warning restore IDE0040
  /// <remarks>reference: BP35A1コマンドリファレンス 3.19. SKUDPPORT</remarks>
  public ValueTask<(SkStackResponse, SkStackUdpPort)> SendSKUDPPORTAsync(
    SkStackUdpPortHandle handle,
    int port,
    CancellationToken cancellationToken = default
  )
  {
    SkStackUdpPort.ThrowIfPortHandleIsNotDefined(handle, nameof(handle));
    SkStackUdpPort.ThrowIfPortNumberIsOutOfRangeOrUnused(port, nameof(port));

    return SendSKUDPPORTAsyncCore(
      handle: handle,
      port: port,
      cancellationToken: cancellationToken
    );
  }

  /// <remarks>reference: BP35A1コマンドリファレンス 3.19. SKUDPPORT</remarks>
  public ValueTask<SkStackResponse> SendSKUDPPORTUnsetAsync(
    SkStackUdpPortHandle handle,
    CancellationToken cancellationToken = default
  )
  {
    SkStackUdpPort.ThrowIfPortHandleIsNotDefined(handle, nameof(handle));

    return Core();

    async ValueTask<SkStackResponse> Core()
    {
      var (resp, _) = await SendSKUDPPORTAsyncCore(
        handle: handle,
        port: SkStackKnownPortNumbers.SetUnused,
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);

      return resp;
    }
  }

  private async ValueTask<(SkStackResponse, SkStackUdpPort)> SendSKUDPPORTAsyncCore(
    SkStackUdpPortHandle handle,
    int port,
    CancellationToken cancellationToken = default
  )
  {
    byte[] PORT = null;

    try {
      PORT = ArrayPool<byte>.Shared.Rent(4);

      SkStackCommandArgs.TryConvertToUINT16(PORT, (ushort)port, out var lengthOfPORT, zeroPadding: true);

      var resp = await SendCommandAsync(
        command: SkStackCommandNames.SKUDPPORT,
        arguments: SkStackCommandArgs.CreateEnumerable(
          SkStackCommandArgs.GetHex((int)handle),
          PORT.AsMemory(0, lengthOfPORT)
        ),
        throwIfErrorStatus: true,
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);

      return (resp, new SkStackUdpPort(handle, port));
    }
    finally {
      if (PORT is not null)
        ArrayPool<byte>.Shared.Return(PORT);
    }
  }
}
