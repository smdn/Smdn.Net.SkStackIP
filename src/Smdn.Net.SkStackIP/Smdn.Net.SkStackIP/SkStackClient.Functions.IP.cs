// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Net.SkStackIP;

partial class SkStackClient {
  public async ValueTask<IReadOnlyList<IPAddress>> GetAvailableAddressListAsync(
    CancellationToken cancellationToken = default
  )
  {
    var resp = await SendSKTABLEAvailableAddressListAsync(
      cancellationToken: cancellationToken
    ).ConfigureAwait(false);

    return resp.Payload!;
  }

  public async ValueTask<IPAddress> ConvertToIPv6LinkLocalAddressAsync(
    PhysicalAddress macAddress,
    CancellationToken cancellationToken = default
  )
  {
    var resp = await SendSKLL64Async(
      macAddress: macAddress,
      cancellationToken: cancellationToken
    ).ConfigureAwait(false);

    return resp.Payload!;
  }

  public async ValueTask<IReadOnlyDictionary<IPAddress, PhysicalAddress>> GetNeighborCacheListAsync(
    CancellationToken cancellationToken = default
  )
  {
    var resp = await SendSKTABLENeighborCacheListAsync(
      cancellationToken: cancellationToken
    ).ConfigureAwait(false);

    return resp.Payload!;
  }
}
