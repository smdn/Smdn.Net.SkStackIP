// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Net.SkStackIP;

#pragma warning disable IDE0040
partial class SkStackClient {
#pragma warning restore IDE0040
  /// <inheritdoc cref="AuthenticateAsPanaClientAsyncCore"/>
  public ValueTask<SkStackPanaSessionInfo> AuthenticateAsPanaClientAsync(
    ReadOnlyMemory<char> rbid,
    ReadOnlyMemory<char> password,
    SkStackActiveScanOptions? scanOptions = null,
    CancellationToken cancellationToken = default
  )
  {
    ThrowIfPanaSessionAlreadyEstablished();

    return AuthenticateAsPanaClientAsyncCore(
      rbid: rbid,
      password: password,
      getPaaAddressTask: default,
      channel: null,
      panId: null,
      scanOptions: scanOptions ?? SkStackActiveScanOptions.Default,
      cancellationToken: cancellationToken
    );
  }

  /// <inheritdoc cref="AuthenticateAsPanaClientAsyncCore"/>
  public ValueTask<SkStackPanaSessionInfo> AuthenticateAsPanaClientAsync(
    ReadOnlyMemory<char> rbid,
    ReadOnlyMemory<char> password,
    IPAddress paaAddress,
    int channelNumber,
    int panId,
    CancellationToken cancellationToken = default
  )
  {
    ThrowIfPanaSessionAlreadyEstablished();

    return AuthenticateAsPanaClientAsyncCore(
      rbid: rbid,
      password: password,
      getPaaAddressTask: new(paaAddress ?? throw new ArgumentNullException(nameof(paaAddress))),
      channel: SkStackChannel.FindByChannelNumber(channelNumber, nameof(channelNumber)),
      panId: ValidatePanIdAndThrowIfInvalid(panId, nameof(panId)),
      scanOptions: SkStackActiveScanOptions.Null, // scanning will not be performed and therefore this will not be referenced
      cancellationToken: cancellationToken
    );
  }

  /// <inheritdoc cref="AuthenticateAsPanaClientAsyncCore"/>
  public ValueTask<SkStackPanaSessionInfo> AuthenticateAsPanaClientAsync(
    ReadOnlyMemory<char> rbid,
    ReadOnlyMemory<char> password,
    SkStackPanDescription pan,
    CancellationToken cancellationToken = default
  )
    => AuthenticateAsPanaClientAsync(
      rbid: rbid,
      password: password,
      paaMacAddress: pan.MacAddress,
      channel: pan.Channel,
      panId: pan.Id,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc cref="AuthenticateAsPanaClientAsyncCore"/>
  public ValueTask<SkStackPanaSessionInfo> AuthenticateAsPanaClientAsync(
    ReadOnlyMemory<char> rbid,
    ReadOnlyMemory<char> password,
    PhysicalAddress paaMacAddress,
    SkStackChannel channel,
    int panId,
    CancellationToken cancellationToken = default
  )
  {
    ThrowIfPanaSessionAlreadyEstablished();

    return AuthenticateAsPanaClientAsyncCore(
      rbid: rbid,
      password: password,
#pragma warning disable CA2012, CS8620
      getPaaAddressTask: ConvertToIPv6LinkLocalAddressAsync(
        paaMacAddress ?? throw new ArgumentNullException(nameof(paaMacAddress)),
        cancellationToken
      ),
#pragma warning restore CA2012, CS8620
      channel: channel,
      panId: ValidatePanIdAndThrowIfInvalid(panId, nameof(panId)),
      scanOptions: SkStackActiveScanOptions.Null, // scanning will not be performed and therefore this will not be referenced
      cancellationToken: cancellationToken
    );
  }

  /// <inheritdoc cref="AuthenticateAsPanaClientAsyncCore"/>
  public ValueTask<SkStackPanaSessionInfo> AuthenticateAsPanaClientAsync(
    ReadOnlyMemory<char> rbid,
    ReadOnlyMemory<char> password,
    PhysicalAddress paaMacAddress,
    int channelNumber,
    int panId,
    CancellationToken cancellationToken = default
  )
  {
    ThrowIfPanaSessionAlreadyEstablished();

    return AuthenticateAsPanaClientAsyncCore(
      rbid: rbid,
      password: password,
#pragma warning disable CA2012, CS8620
      getPaaAddressTask: ConvertToIPv6LinkLocalAddressAsync(
        paaMacAddress ?? throw new ArgumentNullException(nameof(paaMacAddress)),
        cancellationToken
      ),
#pragma warning restore CA2012, CS8620
      channel: SkStackChannel.FindByChannelNumber(channelNumber, nameof(channelNumber)),
      panId: ValidatePanIdAndThrowIfInvalid(panId, nameof(panId)),
      scanOptions: SkStackActiveScanOptions.Null, // scanning will not be performed and therefore this will not be referenced
      cancellationToken: cancellationToken
    );
  }

  /// <inheritdoc cref="AuthenticateAsPanaClientAsyncCore"/>
  public ValueTask<SkStackPanaSessionInfo> AuthenticateAsPanaClientAsync(
    ReadOnlyMemory<char> rbid,
    ReadOnlyMemory<char> password,
    IPAddress paaAddress,
    SkStackChannel channel,
    int panId,
    CancellationToken cancellationToken = default
  )
  {
    ThrowIfPanaSessionAlreadyEstablished();

    if (channel.IsEmpty)
      throw new ArgumentException(message: "invalid channel (empty channel)", paramName: nameof(channel));

    return AuthenticateAsPanaClientAsyncCore(
      rbid: rbid,
      password: password,
      getPaaAddressTask: new(paaAddress ?? throw new ArgumentNullException(nameof(paaAddress))),
      channel: channel,
      panId: ValidatePanIdAndThrowIfInvalid(panId, nameof(panId)),
      scanOptions: SkStackActiveScanOptions.Null, // scanning will not be performed and therefore this will not be referenced
      cancellationToken: cancellationToken
    );
  }

  private static int ValidatePanIdAndThrowIfInvalid(int panId, string paramName)
  {
    if (SkStackRegister.PanId.MinValue <= panId && panId <= SkStackRegister.PanId.MaxValue)
      return panId;

    throw new ArgumentOutOfRangeException(paramName, panId, $"must be in range of {SkStackRegister.PanId.MinValue}~{SkStackRegister.PanId.MaxValue}");
  }

  /// <summary>
  /// Starts the PANA authentication sequence with the current instance as the PaC.
  /// </summary>
  /// <param name="rbid">A Route-B ID used for PANA authentication.</param>
  /// <param name="password">A password ID used for PANA authentication.</param>
  /// <param name="getPaaAddressTask">
  /// An <see cref="ValueTask{IPAddress}"/> that returns IP address of the PANA Authentication Agent (PAA).
  /// If returns <see langword="null"/>, an active scan will be performed to discover the PAAs.
  /// </param>
  /// <param name="channel">A channel number to be used.</param>
  /// <param name="panId">A PAN ID.</param>
  /// <param name="scanOptions">Options such as scanning behavior when performing active scanning.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken" /> to monitor for cancellation requests.</param>
  /// <returns>
  /// A <see cref="ValueTask{SkStackPanaSessionInfo}"/> representing the established PANA session information.
  /// </returns>
  /// <seealso cref="SkStackPanaSessionInfo"/>
  /// <seealso cref="PanaSessionPeerAddress"/>
  /// <seealso cref="IsPanaSessionAlive"/>
  private async ValueTask<SkStackPanaSessionInfo> AuthenticateAsPanaClientAsyncCore(
    ReadOnlyMemory<char> rbid,
    ReadOnlyMemory<char> password,
    ValueTask<IPAddress?> getPaaAddressTask,
    SkStackChannel? channel,
    int? panId,
    SkStackActiveScanOptions scanOptions,
    CancellationToken cancellationToken = default
  )
  {
    var paaAddress = await getPaaAddressTask.ConfigureAwait(false);

    if (paaAddress is not null && !paaAddress.IsIPv6LinkLocal)
      throw new NotSupportedException($"Supplied IP address is not an IPv6 link local address. PAA Address: {paaAddress}");

    await SetRouteBCredentialAsync(
      rbid: rbid,
      rbidParamName: nameof(rbid),
      password: password,
      passwordParamName: nameof(password),
      cancellationToken: cancellationToken
    ).ConfigureAwait(false);

    PhysicalAddress? paaMacAddress = null;
    var needToFindPanaAuthenticationAgent = true;

    // If PAA address is IPv6 link local, construct MAC address from it and add to the neighbor address table.
    if (paaAddress is not null) {
      byte[]? linkLocalAddressBytes = null;

      try {
        linkLocalAddressBytes = ArrayPool<byte>.Shared.Rent(16);

        if (paaAddress.TryWriteBytes(linkLocalAddressBytes, out var bytesWritten) && 8 <= bytesWritten) {
          var linkLocalAddressMemory = linkLocalAddressBytes.AsMemory(0, bytesWritten);
          var macAddressMemory = linkLocalAddressMemory.Slice(linkLocalAddressMemory.Length - 8, 8);

          macAddressMemory.Span[0] &= 0b_1111_1101;

          paaMacAddress = new PhysicalAddress(macAddressMemory.ToArray());

          await SendSKADDNBRAsync(
            ipv6Address: paaAddress,
            macAddress: paaMacAddress,
            cancellationToken: cancellationToken
          ).ConfigureAwait(false);

          needToFindPanaAuthenticationAgent = false;
        }
      }
      finally {
        if (linkLocalAddressBytes is not null)
          ArrayPool<byte>.Shared.Return(linkLocalAddressBytes);
      }
    }

    // If channel or PAN ID is not supplied, have to scan PAN and retrieve them.
    needToFindPanaAuthenticationAgent |= !channel.HasValue;
    needToFindPanaAuthenticationAgent |= !panId.HasValue;

    IPAddress paaAddressNotNull;
    PhysicalAddress paaMacAddressNotNull;
    SkStackChannel channelNotNull;
    int panIdNotNull;

    if (needToFindPanaAuthenticationAgent) {
      var peer = await FindPanaAuthenticationAgentAsync(
        paaAddress: paaAddress,
        scanOptions: scanOptions,
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);

      channelNotNull = peer.Channel;
      panIdNotNull = peer.Id;
      paaMacAddressNotNull = peer.MacAddress;

      if (paaAddress is null) {
        var respSKLL64 = await SendSKLL64Async(
          macAddress: paaMacAddressNotNull,
          cancellationToken: cancellationToken
        ).ConfigureAwait(false);

        paaAddressNotNull = respSKLL64.Payload!;

        await SendSKADDNBRAsync(
          ipv6Address: paaAddressNotNull,
          macAddress: paaMacAddressNotNull,
          cancellationToken: cancellationToken
        ).ConfigureAwait(false);
      }
      else {
        paaAddressNotNull = paaAddress;
      }
    }
    else {
#if DEBUG
      if (paaAddress is null)
        throw new InvalidOperationException($"{nameof(paaAddress)} is null");
      if (paaMacAddress is null)
        throw new InvalidOperationException($"{nameof(paaMacAddress)} is null");
      if (!channel.HasValue)
        throw new InvalidOperationException($"{nameof(channel)} is null");
      if (!panId.HasValue)
        throw new InvalidOperationException($"{nameof(panId)} is null");
#endif

      paaAddressNotNull = paaAddress!;
      paaMacAddressNotNull = paaMacAddress!;
      channelNotNull = channel.Value!;
      panIdNotNull = panId.Value!;
    }

    // Set channel and PAN ID if needed.
    var resp = await SendSKINFOAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    var (localAddress, localMacAddress, currentChannel, currentPanId, _) = resp.Payload;

    if (!currentChannel.Equals(channelNotNull)) {
      await SendSKSREGAsync(
        SkStackRegister.Channel,
        channelNotNull,
        cancellationToken
      ).ConfigureAwait(false);
    }

    if (currentPanId != panIdNotNull) {
      await SendSKSREGAsync(
        SkStackRegister.PanId,
        (ushort)panIdNotNull,
        cancellationToken
      ).ConfigureAwait(false);
    }

    // Then attempt to establish the PANA session.
    await SendSKJOINAsync(
      ipv6address: paaAddressNotNull,
      cancellationToken: cancellationToken
    ).ConfigureAwait(false);

    return new(
      localAddress: localAddress,
      localMacAddress: localMacAddress,
      peerAddress: paaAddressNotNull,
      peerMacAddress: paaMacAddressNotNull,
      channel: channelNotNull,
      panId: panIdNotNull
    );
  }

  private ValueTask<SkStackPanDescription> FindPanaAuthenticationAgentAsync(
    IPAddress? paaAddress,
    SkStackActiveScanOptions scanOptions,
    CancellationToken cancellationToken
  )
  {
    if (paaAddress is null) {
#if DEBUG
      if (scanOptions is null)
        throw new ArgumentNullException(nameof(scanOptions));
#endif

      // If PAA address is not supplied, scan and select PAN with the specified selector.
      return ActiveScanPanaAuthenticationAgentAsync(
        baseScanOptions: scanOptions,
        selectPanaAuthenticationAgentAsync: static (options, desc, _) => new(options.SelectPanaAuthenticationAgent(desc)),
        arg: scanOptions,
        cancellationToken: cancellationToken
      );
    }
    else {
      // If PAA address is supplied, scan and resolve MAC address to collate with the supplied one.
      return ActiveScanPanaAuthenticationAgentAsync(
        baseScanOptions: scanOptions,
        selectPanaAuthenticationAgentAsync:
          async (address, desc, token) => address.Equals(
            await ConvertToIPv6LinkLocalAddressAsync(
              desc.MacAddress,
              token
            ).ConfigureAwait(false)
          ),
        arg: paaAddress,
        cancellationToken: cancellationToken
      );
    }
  }
}
