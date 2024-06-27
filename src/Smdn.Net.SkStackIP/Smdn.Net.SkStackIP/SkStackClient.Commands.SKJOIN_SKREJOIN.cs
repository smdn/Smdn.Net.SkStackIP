// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP;

#pragma warning disable IDE0040
partial class SkStackClient {
#pragma warning restore IDE0040
  private static SkStackPanaSessionEstablishmentException CreatePanaSessionEstablishmentException(
    IPAddress paaAddress,
    SkStackEventNumber eventNumber,
    IPAddress address
  )
    => new(
      message: null,
      paaAddress: paaAddress,
      address: address,
      eventNumber: eventNumber
    );

  /// <summary>
  ///   <para>Sends a command <c>SKJOIN</c>.</para>
  /// </summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.4. SKJOIN' for detailed specifications.</para>
  /// </remarks>
  public ValueTask<SkStackResponse> SendSKJOINAsync(
    IPAddress ipv6address,
    CancellationToken cancellationToken = default
  )
    => SendSKJOINAsync(
      ipv6address: ipv6address,
      createPanaSessionEstablishmentException: (eventNumber, address) => CreatePanaSessionEstablishmentException(ipv6address, eventNumber, address),
      cancellationToken: cancellationToken
    );

  internal ValueTask<SkStackResponse> SendSKJOINAsync(
    IPAddress ipv6address,
    Func<SkStackEventNumber, IPAddress, Exception>? createPanaSessionEstablishmentException,
    CancellationToken cancellationToken
  )
  {
    if (ipv6address is null)
      throw new ArgumentNullException(nameof(ipv6address));
    if (ipv6address.AddressFamily != AddressFamily.InterNetworkV6)
      throw new ArgumentException($"`{nameof(ipv6address)}.{nameof(IPAddress.AddressFamily)}` must be {nameof(AddressFamily.InterNetworkV6)}");

    return SKJOIN(ipv6address, createPanaSessionEstablishmentException, cancellationToken);

    async ValueTask<SkStackResponse> SKJOIN(
      IPAddress addr,
      Func<SkStackEventNumber, IPAddress, Exception>? createException,
      CancellationToken ct
    )
    {
      var (response, _) = await SKJOIN_SKREJOIN(SkStackCommandNames.SKJOIN, addr, createException, ct).ConfigureAwait(false);

      return response;
    }
  }

  /// <summary>
  ///   <para>Sends a command <c>SKREJOIN</c>.</para>
  /// </summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.5. SKREJOIN' for detailed specifications.</para>
  /// </remarks>
  public ValueTask<(
    SkStackResponse Response,
    IPAddress Address
  )> SendSKREJOINAsync(
    CancellationToken cancellationToken = default
  )
    => SKJOIN_SKREJOIN(
      SkStackCommandNames.SKREJOIN,
      ipv6address: null,
      createPanaSessionEstablishmentException: null,
      cancellationToken
    );

  private async ValueTask<(
    SkStackResponse Response,
    IPAddress Address
  )>
  SKJOIN_SKREJOIN(
    ReadOnlyMemory<byte> command,
    IPAddress? ipv6address,
    Func<SkStackEventNumber, IPAddress, Exception>? createPanaSessionEstablishmentException,
    CancellationToken cancellationToken
  )
  {
    var eventHandler = new SKJOINEventHandler(createPanaSessionEstablishmentException);
    var resp = await SendCommandAsync(
      command: command,
      writeArguments: writer => {
        if (ipv6address is not null)
          writer.WriteTokenIPADDR(ipv6address);
      },
      commandEventHandler: eventHandler,
      throwIfErrorStatus: true,
      cancellationToken: cancellationToken
    ).ConfigureAwait(false);

    eventHandler.ThrowIfEstablishmentError();

#if DEBUG
    if (eventHandler.Address is null)
      throw new InvalidOperationException($"{eventHandler.Address} has not been set");
#endif

    return (resp, eventHandler.Address!);
  }

  private sealed class SKJOINEventHandler(Func<SkStackEventNumber, IPAddress, Exception>? createPanaSessionEstablishmentException) : SkStackEventHandlerBase {
    public bool HasAddressSet { get; private set; }
    public IPAddress? Address { get; private set; }

    private readonly Func<SkStackEventNumber, IPAddress, Exception>? createPanaSessionEstablishmentException = createPanaSessionEstablishmentException;
    private SkStackEventNumber eventNumber;

    private static SkStackPanaSessionEstablishmentException CreatePanaSessionEstablishmentException(SkStackEventNumber eventNumber, IPAddress address)
      => new(null, address, eventNumber);

    public void ThrowIfEstablishmentError()
    {
      if (eventNumber != SkStackEventNumber.PanaSessionEstablishmentCompleted)
        throw createPanaSessionEstablishmentException?.Invoke(eventNumber, Address!) ?? CreatePanaSessionEstablishmentException(eventNumber, Address!);
    }

    public override bool TryProcessEvent(SkStackEvent ev)
    {
      switch (ev.Number) {
        case SkStackEventNumber.PanaSessionEstablishmentCompleted:
        case SkStackEventNumber.PanaSessionEstablishmentError:
          eventNumber = ev.Number;
#if DEBUG
          if (!ev.HasSenderAddress)
            throw new InvalidOperationException($"{nameof(ev.SenderAddress)} must not be null");
#endif
          Address = ev.SenderAddress!;
          return true;

        default:
          return false;
      }
    }
  }
}
