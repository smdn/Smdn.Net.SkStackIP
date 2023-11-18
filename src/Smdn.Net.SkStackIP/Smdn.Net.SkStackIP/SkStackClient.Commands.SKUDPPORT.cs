// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Threading;
using System.Threading.Tasks;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP;

#pragma warning disable IDE0040
partial class SkStackClient {
#pragma warning restore IDE0040
  /// <summary>
  ///   <para>Sends a command <c>SKUDPPORT</c>.</para>
  /// </summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.19. SKUDPPORT' for detailed specifications.</para>
  /// </remarks>
  public ValueTask<(SkStackResponse Response, SkStackUdpPort UdpPort)> SendSKUDPPORTAsync(
    SkStackUdpPortHandle handle,
    int port,
    CancellationToken cancellationToken = default
  )
  {
    SkStackUdpPort.ThrowIfPortHandleIsOutOfRange(handle, nameof(handle));
    SkStackUdpPort.ThrowIfPortNumberIsOutOfRangeOrUnused(port, nameof(port));

    return SKUDPPORT();

    async ValueTask<(SkStackResponse Response, SkStackUdpPort UdpPort)> SKUDPPORT()
    {
      var resp = await SendCommandAsync(
        command: SkStackCommandNames.SKUDPPORT,
        writeArguments: writer => {
          writer.WriteTokenHex((byte)handle);
          writer.WriteTokenUINT16((ushort)port, zeroPadding: true);
        },
        throwIfErrorStatus: true,
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);

      return (resp, new SkStackUdpPort(handle, port));
    }
  }

  /// <summary>
  ///   <para>Sends a command <c>SKUDPPORT</c>.</para>
  /// </summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.19. SKUDPPORT' for detailed specifications.</para>
  /// </remarks>
  public ValueTask<SkStackResponse> SendSKUDPPORTUnsetAsync(
    SkStackUdpPortHandle handle,
    CancellationToken cancellationToken = default
  )
  {
    SkStackUdpPort.ThrowIfPortHandleIsOutOfRange(handle, nameof(handle));

    return SendCommandAsync(
      command: SkStackCommandNames.SKUDPPORT,
      writeArguments: writer => {
        writer.WriteTokenHex((byte)handle);
        writer.WriteTokenUINT16(SkStackKnownPortNumbers.SetUnused, zeroPadding: true);
      },
      throwIfErrorStatus: true,
      cancellationToken: cancellationToken
    );
  }
}
