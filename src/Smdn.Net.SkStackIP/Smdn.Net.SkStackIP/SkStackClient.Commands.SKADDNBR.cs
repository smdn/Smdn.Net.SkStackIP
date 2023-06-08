// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
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
  /// <remarks>reference: BP35A1コマンドリファレンス 3.18. SKADDNBR</remarks>
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

    return SKADDNBR();

    async ValueTask<SkStackResponse> SKADDNBR()
    {
      byte[] IPADDR = null;
      byte[] MACADDR = null;

      try {
        IPADDR = ArrayPool<byte>.Shared.Rent(SkStackCommandArgs.LengthOfIPADDR);
        MACADDR = ArrayPool<byte>.Shared.Rent(SkStackCommandArgs.LengthOfADDR64);

        SkStackCommandArgs.TryConvertToIPADDR(IPADDR, ipv6Address, out var lengthOfIPADDR);
        SkStackCommandArgs.TryConvertToADDR64(MACADDR, macAddress, out var lengthOfMACADDR);

        return await SendCommandAsync(
          command: SkStackCommandNames.SKADDNBR,
          arguments: SkStackCommandArgs.CreateEnumerable(
            IPADDR.AsMemory(0, lengthOfIPADDR),
            MACADDR.AsMemory(0, lengthOfMACADDR)
          ),
          throwIfErrorStatus: true,
          cancellationToken: cancellationToken
        ).ConfigureAwait(false);
      }
      finally {
        if (IPADDR is not null)
          ArrayPool<byte>.Shared.Return(IPADDR);
        if (MACADDR is not null)
          ArrayPool<byte>.Shared.Return(MACADDR);
      }
    }
  }
}
