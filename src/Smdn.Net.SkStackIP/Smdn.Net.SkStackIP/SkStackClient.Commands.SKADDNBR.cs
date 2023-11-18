// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP;

#pragma warning disable IDE0040
partial class SkStackClient {
#pragma warning restore IDE0040
  /// <summary>
  ///   <para>Sends a command <c>SKADDNBR</c>.</para>
  /// </summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.18. SKADDNBR' for detailed specifications.</para>
  /// </remarks>
  public ValueTask<SkStackResponse> SendSKADDNBRAsync(
    IPAddress ipv6Address,
    PhysicalAddress macAddress,
    CancellationToken cancellationToken = default
  )
  {
    if (ipv6Address is null)
      throw new ArgumentNullException(nameof(ipv6Address));
    if (ipv6Address.AddressFamily != AddressFamily.InterNetworkV6)
      throw new ArgumentException($"`{nameof(ipv6Address)}.{nameof(IPAddress.AddressFamily)}` must be {nameof(AddressFamily.InterNetworkV6)}");
    if (macAddress is null)
      throw new ArgumentNullException(nameof(macAddress));

    return SendCommandAsync(
      command: SkStackCommandNames.SKADDNBR,
      writeArguments: writer => {
        writer.WriteTokenIPADDR(ipv6Address);
        writer.WriteTokenADDR64(macAddress);
      },
      throwIfErrorStatus: true,
      cancellationToken: cancellationToken
    );
  }
}
