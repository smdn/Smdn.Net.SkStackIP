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
  /// <param name="rbid">A Route-B ID used for PANA authentication.</param>
  /// <param name="password">A Route-B password used for PANA authentication.</param>
  /// <param name="scanOptions">Options such as scanning behavior when performing active scanning.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken" /> to monitor for cancellation requests.</param>
  /// <exception cref="SkStackPanaSessionStateException">PANA session has already been established.</exception>
  public ValueTask<SkStackPanaSessionInfo> AuthenticateAsPanaClientAsync(
    ReadOnlyMemory<byte> rbid,
    ReadOnlyMemory<byte> password,
    SkStackActiveScanOptions? scanOptions = null,
    CancellationToken cancellationToken = default
  )
    => AuthenticateAsPanaClientAsync(
      writeRBID: CreateActionForWritingRBID(rbid, nameof(rbid)),
      writePassword: CreateActionForWritingPassword(password, nameof(password)),
      scanOptions: scanOptions,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc cref="AuthenticateAsPanaClientAsyncCore"/>
  /// <param name="writeRBID">A delegate to write Route-B ID used for PANA authentication to the <see cref="IBufferWriter{Byte}"/>.</param>
  /// <param name="writePassword">A delegate to write Route-B password used for PANA authentication to the <see cref="IBufferWriter{Byte}"/>.</param>
  /// <param name="scanOptions">Options such as scanning behavior when performing active scanning.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken" /> to monitor for cancellation requests.</param>
  /// <exception cref="SkStackPanaSessionStateException">PANA session has already been established.</exception>
  public ValueTask<SkStackPanaSessionInfo> AuthenticateAsPanaClientAsync(
    Action<IBufferWriter<byte>> writeRBID,
    Action<IBufferWriter<byte>> writePassword,
    SkStackActiveScanOptions? scanOptions = null,
    CancellationToken cancellationToken = default
  )
  {
    ThrowIfPanaSessionAlreadyEstablished();

    return AuthenticateAsPanaClientAsyncCore(
      writeRBID: writeRBID ?? throw new ArgumentNullException(nameof(writeRBID)),
      writePassword: writePassword ?? throw new ArgumentNullException(nameof(writePassword)),
      getPaaAddressTask: default,
      channel: null,
      panId: null,
      scanOptions: scanOptions ?? SkStackActiveScanOptions.Default,
      cancellationToken: cancellationToken
    );
  }

  /// <inheritdoc cref="AuthenticateAsPanaClientAsyncCore"/>
  /// <param name="rbid">A Route-B ID used for PANA authentication.</param>
  /// <param name="password">A Route-B password used for PANA authentication.</param>
  /// <param name="paaAddress">An <see cref="IPAddress"/> representing the IP address of the PANA Authentication Agent (PAA).</param>
  /// <param name="channelNumber">A channel number to be used for PANA session.</param>
  /// <param name="panId">A Personal Area Network (PAN) ID to be used for PANA session.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken" /> to monitor for cancellation requests.</param>
  /// <exception cref="SkStackPanaSessionStateException">PANA session has already been established.</exception>
  public ValueTask<SkStackPanaSessionInfo> AuthenticateAsPanaClientAsync(
    ReadOnlyMemory<byte> rbid,
    ReadOnlyMemory<byte> password,
    IPAddress paaAddress,
    int channelNumber,
    int panId,
    CancellationToken cancellationToken = default
  )
    => AuthenticateAsPanaClientAsync(
      writeRBID: CreateActionForWritingRBID(rbid, nameof(rbid)),
      writePassword: CreateActionForWritingPassword(password, nameof(password)),
      paaAddress: paaAddress,
      channel: SkStackChannel.FindByChannelNumber(channelNumber, nameof(channelNumber)),
      panId: panId,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc cref="AuthenticateAsPanaClientAsyncCore"/>
  /// <param name="rbid">A Route-B ID used for PANA authentication.</param>
  /// <param name="password">A Route-B password used for PANA authentication.</param>
  /// <param name="paaAddress">An <see cref="IPAddress"/> representing the IP address of the PANA Authentication Agent (PAA).</param>
  /// <param name="channel">A <see cref="SkStackChannel"/> representing the channel to be used for PANA session.</param>
  /// <param name="panId">A Personal Area Network (PAN) ID to be used for PANA session.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken" /> to monitor for cancellation requests.</param>
  /// <exception cref="SkStackPanaSessionStateException">PANA session has already been established.</exception>
  public ValueTask<SkStackPanaSessionInfo> AuthenticateAsPanaClientAsync(
    ReadOnlyMemory<byte> rbid,
    ReadOnlyMemory<byte> password,
    IPAddress paaAddress,
    SkStackChannel channel,
    int panId,
    CancellationToken cancellationToken = default
  )
    => AuthenticateAsPanaClientAsync(
      writeRBID: CreateActionForWritingRBID(rbid, nameof(rbid)),
      writePassword: CreateActionForWritingPassword(password, nameof(password)),
      paaAddress: paaAddress,
      channel: channel,
      panId: panId,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc cref="AuthenticateAsPanaClientAsyncCore"/>
  /// <param name="writeRBID">A delegate to write Route-B ID used for PANA authentication to the <see cref="IBufferWriter{Byte}"/>.</param>
  /// <param name="writePassword">A delegate to write Route-B password used for PANA authentication to the <see cref="IBufferWriter{Byte}"/>.</param>
  /// <param name="paaAddress">An <see cref="IPAddress"/> representing the IP address of the PANA Authentication Agent (PAA).</param>
  /// <param name="channel">A <see cref="SkStackChannel"/> representing the channel to be used for PANA session.</param>
  /// <param name="panId">A Personal Area Network (PAN) ID to be used for PANA session.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken" /> to monitor for cancellation requests.</param>
  /// <exception cref="SkStackPanaSessionStateException">PANA session has already been established.</exception>
  public ValueTask<SkStackPanaSessionInfo> AuthenticateAsPanaClientAsync(
    Action<IBufferWriter<byte>> writeRBID,
    Action<IBufferWriter<byte>> writePassword,
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
      writeRBID: writeRBID ?? throw new ArgumentNullException(nameof(writeRBID)),
      writePassword: writePassword ?? throw new ArgumentNullException(nameof(writePassword)),
      getPaaAddressTask: new(paaAddress ?? throw new ArgumentNullException(nameof(paaAddress))),
      channel: channel,
      panId: ValidatePanIdAndThrowIfInvalid(panId, nameof(panId)),
      scanOptions: SkStackActiveScanOptions.Null, // scanning will not be performed and therefore this will not be referenced
      cancellationToken: cancellationToken
    );
  }

  /// <inheritdoc cref="AuthenticateAsPanaClientAsyncCore"/>
  /// <param name="rbid">A Route-B ID used for PANA authentication.</param>
  /// <param name="password">A Route-B password used for PANA authentication.</param>
  /// <param name="pan">A <see cref="SkStackPanDescription"/> representing the address of the PANA Authentication Agent (PAA), PAN ID, and channel used for PANA session.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken" /> to monitor for cancellation requests.</param>
  /// <exception cref="SkStackPanaSessionStateException">PANA session has already been established.</exception>
  public ValueTask<SkStackPanaSessionInfo> AuthenticateAsPanaClientAsync(
    ReadOnlyMemory<byte> rbid,
    ReadOnlyMemory<byte> password,
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
  /// <param name="writeRBID">A delegate to write Route-B ID used for PANA authentication to the <see cref="IBufferWriter{Byte}"/>.</param>
  /// <param name="writePassword">A delegate to write Route-B password used for PANA authentication to the <see cref="IBufferWriter{Byte}"/>.</param>
  /// <param name="pan">A <see cref="SkStackPanDescription"/> representing the address of the PANA Authentication Agent (PAA), PAN ID, and channel used for PANA session.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken" /> to monitor for cancellation requests.</param>
  /// <exception cref="SkStackPanaSessionStateException">PANA session has already been established.</exception>
  public ValueTask<SkStackPanaSessionInfo> AuthenticateAsPanaClientAsync(
    Action<IBufferWriter<byte>> writeRBID,
    Action<IBufferWriter<byte>> writePassword,
    SkStackPanDescription pan,
    CancellationToken cancellationToken = default
  )
    => AuthenticateAsPanaClientAsync(
      writeRBID: writeRBID,
      writePassword: writePassword,
      paaMacAddress: pan.MacAddress,
      channel: pan.Channel,
      panId: pan.Id,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc cref="AuthenticateAsPanaClientAsyncCore"/>
  /// <param name="rbid">A Route-B ID used for PANA authentication.</param>
  /// <param name="password">A Route-B password used for PANA authentication.</param>
  /// <param name="paaMacAddress">A <see cref="PhysicalAddress"/> representing the MAC address of the PANA Authentication Agent (PAA).</param>
  /// <param name="channelNumber">A channel number to be used for PANA session.</param>
  /// <param name="panId">A Personal Area Network (PAN) ID to be used for PANA session.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken" /> to monitor for cancellation requests.</param>
  /// <exception cref="SkStackPanaSessionStateException">PANA session has already been established.</exception>
  public ValueTask<SkStackPanaSessionInfo> AuthenticateAsPanaClientAsync(
    ReadOnlyMemory<byte> rbid,
    ReadOnlyMemory<byte> password,
    PhysicalAddress paaMacAddress,
    int channelNumber,
    int panId,
    CancellationToken cancellationToken = default
  )
    => AuthenticateAsPanaClientAsync(
      writeRBID: CreateActionForWritingRBID(rbid, nameof(rbid)),
      writePassword: CreateActionForWritingPassword(password, nameof(password)),
      paaMacAddress: paaMacAddress,
      channel: SkStackChannel.FindByChannelNumber(channelNumber, nameof(channelNumber)),
      panId: panId,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc cref="AuthenticateAsPanaClientAsyncCore"/>
  /// <param name="rbid">A Route-B ID used for PANA authentication.</param>
  /// <param name="password">A Route-B password used for PANA authentication.</param>
  /// <param name="paaMacAddress">A <see cref="PhysicalAddress"/> representing the MAC address of the PANA Authentication Agent (PAA).</param>
  /// <param name="channel">A <see cref="SkStackChannel"/> representing the channel to be used for PANA session.</param>
  /// <param name="panId">A Personal Area Network (PAN) ID to be used for PANA session.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken" /> to monitor for cancellation requests.</param>
  /// <exception cref="SkStackPanaSessionStateException">PANA session has already been established.</exception>
  public ValueTask<SkStackPanaSessionInfo> AuthenticateAsPanaClientAsync(
    ReadOnlyMemory<byte> rbid,
    ReadOnlyMemory<byte> password,
    PhysicalAddress paaMacAddress,
    SkStackChannel channel,
    int panId,
    CancellationToken cancellationToken = default
  )
    => AuthenticateAsPanaClientAsync(
      writeRBID: CreateActionForWritingRBID(rbid, nameof(rbid)),
      writePassword: CreateActionForWritingPassword(password, nameof(password)),
      paaMacAddress: paaMacAddress,
      channel: channel,
      panId: panId,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc cref="AuthenticateAsPanaClientAsyncCore"/>
  /// <param name="writeRBID">A delegate to write Route-B ID used for PANA authentication to the <see cref="IBufferWriter{Byte}"/>.</param>
  /// <param name="writePassword">A delegate to write Route-B password used for PANA authentication to the <see cref="IBufferWriter{Byte}"/>.</param>
  /// <param name="paaMacAddress">A <see cref="PhysicalAddress"/> representing the MAC address of the PANA Authentication Agent (PAA).</param>
  /// <param name="channel">A <see cref="SkStackChannel"/> representing the channel to be used for PANA session.</param>
  /// <param name="panId">A Personal Area Network (PAN) ID to be used for PANA session.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken" /> to monitor for cancellation requests.</param>
  /// <exception cref="SkStackPanaSessionStateException">PANA session has already been established.</exception>
  public ValueTask<SkStackPanaSessionInfo> AuthenticateAsPanaClientAsync(
    Action<IBufferWriter<byte>> writeRBID,
    Action<IBufferWriter<byte>> writePassword,
    PhysicalAddress paaMacAddress,
    SkStackChannel channel,
    int panId,
    CancellationToken cancellationToken = default
  )
  {
    ThrowIfPanaSessionAlreadyEstablished();

    return AuthenticateAsPanaClientAsyncCore(
      writeRBID: writeRBID ?? throw new ArgumentNullException(nameof(writeRBID)),
      writePassword: writePassword ?? throw new ArgumentNullException(nameof(writePassword)),
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

  private static int ValidatePanIdAndThrowIfInvalid(int panId, string paramName)
  {
    if (SkStackRegister.PanId.MinValue <= panId && panId <= SkStackRegister.PanId.MaxValue)
      return panId;

    throw new ArgumentOutOfRangeException(paramName, panId, $"must be in range of {SkStackRegister.PanId.MinValue}~{SkStackRegister.PanId.MaxValue}");
  }

  /// <summary>
  /// Starts the PANA authentication sequence with the current instance as the PaC.
  /// </summary>
  /// <param name="writeRBID">A delegate to write Route-B ID used for PANA authentication to the <see cref="IBufferWriter{Byte}"/>.</param>
  /// <param name="writePassword">A delegate to write Route-B password used for PANA authentication to the <see cref="IBufferWriter{Byte}"/>.</param>
  /// <param name="getPaaAddressTask">
  /// An <see cref="ValueTask{IPAddress}"/> that returns IP address of the PANA Authentication Agent (PAA).
  /// If returns <see langword="null"/>, an active scan will be performed to discover the PAAs.
  /// </param>
  /// <param name="channel">A <see cref="SkStackChannel"/> representing the channel to be used for PANA session.</param>
  /// <param name="panId">A Personal Area Network (PAN) ID to be used for PANA session.</param>
  /// <param name="scanOptions">Options such as scanning behavior when performing active scanning.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken" /> to monitor for cancellation requests.</param>
  /// <returns>
  /// A <see cref="ValueTask{SkStackPanaSessionInfo}"/> representing the established PANA session information.
  /// </returns>
  /// <seealso cref="SkStackPanaSessionInfo"/>
  /// <seealso cref="SkStackRegister.Channel"/>
  /// <seealso cref="SkStackRegister.PanId"/>
  /// <seealso cref="PanaSessionPeerAddress"/>
  /// <seealso cref="IsPanaSessionAlive"/>
  /// <seealso cref="SendSKJOINAsync(IPAddress, Func{SkStackEventNumber, IPAddress, Exception}?, CancellationToken)"/>
  /// <seealso cref="SendSKSETRBIDAsync(ReadOnlyMemory{byte}, CancellationToken)"/>
  /// <seealso cref="SendSKSETPWDAsync(ReadOnlyMemory{byte}, CancellationToken)"/>
  private async ValueTask<SkStackPanaSessionInfo> AuthenticateAsPanaClientAsyncCore(
    Action<IBufferWriter<byte>> writeRBID,
    Action<IBufferWriter<byte>> writePassword,
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
      writeRBID: writeRBID,
      writePassword: writePassword,
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

#if !DEBUG
#pragma warning disable CS8629
#endif
      channelNotNull = channel.Value!;
      panIdNotNull = panId.Value!;
#if !DEBUG
#pragma warning restore CS8629
#endif
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
      createPanaSessionEstablishmentException: (eventNumber, address) =>
        new SkStackPanaSessionEstablishmentException(
          message: null,
          paaAddress: paaAddressNotNull,
          channel: channelNotNull,
          panId: panIdNotNull,
          address: address,
          eventNumber: eventNumber
        ),
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
