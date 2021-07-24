// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Smdn.Net.SkStackIP.Protocol;
#if DEBUG
using Smdn.Text.Unicode.ControlPictures;
#endif

namespace Smdn.Net.SkStackIP {
  partial class SkStackClient {
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
        var ret = await SKJOIN_SKREJOIN(SkStackCommandNames.SKJOIN, addr, ct).ConfigureAwait(false);

        return ret.Response;
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
          cancellationToken: cancellationToken,
          throwIfErrorStatus: true
        ).ConfigureAwait(false);
      }
      finally {
        if (IPADDR is not null)
          ArrayPool<byte>.Shared.Return(IPADDR);
      }

      var finalStatusEvent = await ReceiveEventAsync(
        parseEvent: ParseSKJOINEvent,
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);

      if (finalStatusEvent.Number == SkStackEventNumber.PanaSessionEstablishmentCompleted)
        RaiseEventPanaSessionEstablished(finalStatusEvent);
      if (finalStatusEvent.Number == SkStackEventNumber.PanaSessionEstablishmentError)
        throw new SkStackPanaSessionEstablishmentException($"PANA session establishment failed", finalStatusEvent);

      return (resp, finalStatusEvent.SenderAddress);
    }

    private static SkStackEvent ParseSKJOINEvent(
      ISkStackSequenceParserContext context
    )
    {
      static bool IsPanaSessionEstablishmentEvent(SkStackEventNumber eventNumber)
        => eventNumber switch {
          SkStackEventNumber.PanaSessionEstablishmentCompleted => true,
          SkStackEventNumber.PanaSessionEstablishmentError => true,
          _ => false,
        };

      if (SkStackEventParser.TryExpectEVENT(context, IsPanaSessionEstablishmentEvent, out var ev24or25)) {
        context.Logger?.LogInfoPanaEventReceived(ev24or25);
        context.Complete();
        return ev24or25;
      }

      context.SetAsIncomplete();
      return default;
    }
  }
}