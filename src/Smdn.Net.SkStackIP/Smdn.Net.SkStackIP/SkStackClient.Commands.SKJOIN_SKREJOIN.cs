// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP;

#pragma warning disable IDE0040
partial class SkStackClient {
#pragma warning restore IDE0040
  /// <remarks>reference: BP35A1コマンドリファレンス 3.4. SKJOIN</remarks>
  public ValueTask<SkStackResponse> SendSKJOINAsync(
    IPAddress ipv6address,
    CancellationToken cancellationToken = default
  )
  {
    if (ipv6address is null)
      throw new ArgumentNullException(nameof(ipv6address));
    if (ipv6address.AddressFamily != AddressFamily.InterNetworkV6)
      throw new ArgumentException($"`{nameof(ipv6address)}.{nameof(IPAddress.AddressFamily)}` must be {nameof(AddressFamily.InterNetworkV6)}");

    return SKJOIN(ipv6address, cancellationToken);

    async ValueTask<SkStackResponse> SKJOIN(IPAddress addr, CancellationToken ct)
    {
      var (response, _) = await SKJOIN_SKREJOIN(SkStackCommandNames.SKJOIN, addr, ct).ConfigureAwait(false);

      return response;
    }
  }

  /// <remarks>reference: BP35A1コマンドリファレンス 3.5. SKREJOIN</remarks>
  public ValueTask<(
    SkStackResponse Response,
    IPAddress Address
  )> SendSKREJOINAsync(
    CancellationToken cancellationToken = default
  )
    => SKJOIN_SKREJOIN(SkStackCommandNames.SKREJOIN, ipv6address: null, cancellationToken);

  private async ValueTask<(
    SkStackResponse Response,
    IPAddress Address
  )>
  SKJOIN_SKREJOIN(
    ReadOnlyMemory<byte> command,
    IPAddress ipv6address,
    CancellationToken cancellationToken
  )
  {
    SkStackResponse resp = default;
    byte[] IPADDR = default;
    var eventHandler = new SKJOINEventHandler();

    try {
      int lengthOfIPADDR = default;

      if (ipv6address is not null) {
        IPADDR = ArrayPool<byte>.Shared.Rent(SkStackCommandArgs.LengthOfIPADDR);

        if (!SkStackCommandArgs.TryConvertToIPADDR(IPADDR, ipv6address, out lengthOfIPADDR))
          throw new InvalidOperationException("unexpected error in conversion");
      }

      resp = await SendCommandAsync(
        command: command,
        arguments: IPADDR is null ? null : SkStackCommandArgs.CreateEnumerable(IPADDR.AsMemory(0, lengthOfIPADDR)),
        commandEventHandler: eventHandler,
        throwIfErrorStatus: true,
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);
    }
    finally {
      if (IPADDR is not null)
        ArrayPool<byte>.Shared.Return(IPADDR);
    }

    eventHandler.ThrowIfEstablishmentError();

    return (resp, eventHandler.Address);
  }

  private class SKJOINEventHandler : SkStackEventHandlerBase {
    public bool IsSessionEstablishedSuccessfully { get; private set; } = false;
    public IPAddress Address { get; private set; }
    private SkStackEventNumber eventNumber;

    public void ThrowIfEstablishmentError()
    {
      if (eventNumber != SkStackEventNumber.PanaSessionEstablishmentCompleted)
        throw new SkStackPanaSessionEstablishmentException($"PANA session establishment failed", Address, eventNumber);
    }

    public override bool TryProcessEvent(SkStackEvent ev)
    {
      switch (ev.Number) {
        case SkStackEventNumber.PanaSessionEstablishmentCompleted:
        case SkStackEventNumber.PanaSessionEstablishmentError:
          eventNumber = ev.Number;
          Address = ev.SenderAddress;
          return true;

        default:
          return false;
      }
    }
  }
}
