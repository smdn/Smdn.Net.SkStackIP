// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1848

using System;
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLWHENATTRIBUTE
using System.Diagnostics.CodeAnalysis;
#endif
#if !SYSTEM_CONVERT_TOHEXSTRING
using System.Buffers; // ArrayPool
#endif
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Smdn.Net.SkStackIP;

namespace Smdn.Devices.BP35XX;

public abstract partial class BP35Base : SkStackClient {
  internal const bool DefaultValueForTryLoadFlashMemory = true;

  private protected static async ValueTask<TBP35XX> InitializeAsync<TBP35XX>(
    TBP35XX device,
    bool tryLoadFlashMemory = DefaultValueForTryLoadFlashMemory,
    IServiceProvider? serviceProvider = null,
    CancellationToken cancellationToken = default
  )
    where TBP35XX : BP35Base
  {
#pragma warning disable CA1510
    if (device is null)
      throw new ArgumentNullException(nameof(device));
#pragma warning disable CA1510

    try {
      await device.InitializeAsync(
        tryLoadFlashMemory,
        serviceProvider,
        cancellationToken
      ).ConfigureAwait(false);

      return device;
    }
    catch {
      device.Dispose();

      throw;
    }
  }

  private protected static InvalidOperationException CreateNotInitializedException()
    => new(message: "not initialized");

  /*
   * instance members
   */
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLWHENATTRIBUTE
  [MemberNotNullWhen(true, nameof(skstackVersion))]
  [MemberNotNullWhen(true, nameof(skstackAppVersion))]
  [MemberNotNullWhen(true, nameof(linkLocalAddress))]
  [MemberNotNullWhen(true, nameof(macAddress))]
  [MemberNotNullWhen(true, nameof(rohmUserId))]
  [MemberNotNullWhen(true, nameof(rohmPassword))]
#endif
  private protected bool IsInitialized { get; private set; }

#if !SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLWHENATTRIBUTE
#pragma warning disable CS8603
#endif
  private Version? skstackVersion;
  public Version SkStackVersion => IsInitialized ? skstackVersion : throw CreateNotInitializedException();

  private string? skstackAppVersion;
  public string SkStackAppVersion => IsInitialized ? skstackAppVersion : throw CreateNotInitializedException();

  private IPAddress? linkLocalAddress;
  public IPAddress LinkLocalAddress => IsInitialized ? linkLocalAddress : throw CreateNotInitializedException();

  private PhysicalAddress? macAddress;
  public PhysicalAddress MacAddress => IsInitialized ? macAddress : throw CreateNotInitializedException();

  private string? rohmUserId;
  public string RohmUserId => IsInitialized ? rohmUserId : throw CreateNotInitializedException();

  private string? rohmPassword;
  public string RohmPassword => IsInitialized ? rohmPassword : throw CreateNotInitializedException();
#pragma warning restore CS8603

  /// <summary>
  /// Initializes a new instance of the <see cref="BP35Base"/> class with specifying the serial port name.
  /// </summary>
  /// <param name="configurations">
  /// A <see cref="IBP35Configurations"/> that holds the configurations to the <see cref="BP35Base"/> instance.
  /// </param>
  /// <param name="serialPortStreamFactory">
  /// A <see cref="IBP35SerialPortStreamFactory"/> that provides the function to create the serial port stream according to the <paramref name="configurations"/>.
  /// </param>
  /// <param name="logger">The <see cref="ILogger"/> to report the situation.</param>
#pragma warning disable IDE0290
  private protected BP35Base(
    IBP35Configurations configurations,
    IBP35SerialPortStreamFactory? serialPortStreamFactory,
    ILogger? logger
  )
#pragma warning restore IDE0290
    : base(
      stream: (serialPortStreamFactory ?? DefaultSerialPortStreamFactory.Instance).CreateSerialPortStream(
        configurations ?? throw new ArgumentNullException(nameof(configurations))
      ),
      leaveStreamOpen: false, // should close the opened stream
      erxudpDataFormat: configurations.ERXUDPDataFormat,
      logger: logger
    )
  {
  }

  private async ValueTask InitializeAsync(
    bool tryLoadFlashMemory,
    IServiceProvider? serviceProvider,
    CancellationToken cancellationToken
  )
  {
    // reset first before configuring
    await SendSKRESETAsync(cancellationToken).ConfigureAwait(false);

    // retrieve firmware version
    skstackVersion = (await SendSKVERAsync(cancellationToken).ConfigureAwait(false)).Payload;

    Logger?.LogInformation("{Name}: {Value}", nameof(SkStackVersion), skstackVersion);

    skstackAppVersion = (await SendSKAPPVERAsync(cancellationToken).ConfigureAwait(false)).Payload;

    Logger?.LogInformation("{Name}: {Value}", nameof(SkStackAppVersion), skstackAppVersion);

    // retrieve EINFO
    var respInfo = await SendSKINFOAsync(cancellationToken).ConfigureAwait(false);
    var einfo = respInfo.Payload!;

    linkLocalAddress = einfo.LinkLocalAddress;
    macAddress = einfo.MacAddress;

    Logger?.LogInformation("{Name}: {Value}", nameof(LinkLocalAddress), linkLocalAddress);
    Logger?.LogInformation("{Name}: {Value}", nameof(MacAddress), macAddress);

    Logger?.LogInformation("{Name}: {Value}", nameof(einfo.Channel), einfo.Channel);
    Logger?.LogInformation("{Name}: {Value} (0x{ValueToBeDisplayedInHex:X4})", nameof(einfo.PanId), einfo.PanId, einfo.PanId);

    // parse ROHM user ID and password
    (rohmUserId, rohmPassword) = ParseRohmUserIdAndPassword(linkLocalAddress);

    // try load configuration from flash memory
    if (tryLoadFlashMemory) {
      try {
        await SendSKLOADAsync(cancellationToken).ConfigureAwait(false);
      }
      catch (SkStackFlashMemoryIOException) {
        Logger?.LogWarning("Could not load configuration from flash memory.");
      }
    }

    // disable echoback (override loaded configuration)
    await SendSKSREGAsync(
      register: SkStackRegister.EnableEchoback,
      value: false,
      cancellationToken: cancellationToken
    ).ConfigureAwait(false);

    // set ERXUDP data format
    var udpDataFormat = await GetUdpDataFormatAsync(cancellationToken).ConfigureAwait(false);

#pragma warning disable IDE0055, IDE0072
    ERXUDPDataFormat = udpDataFormat switch {
      BP35UdpReceiveDataFormat.HexAscii => SkStackERXUDPDataFormat.HexAsciiText,
      /*BP35UdpReceiveDataFormat.Binary,*/ _ => SkStackERXUDPDataFormat.Binary,
    };
#pragma warning restore IDE0055, IDE0072

    await InitializeAsyncCore(serviceProvider, cancellationToken).ConfigureAwait(false);

    IsInitialized = true;

    static (string, string) ParseRohmUserIdAndPassword(IPAddress linkLocalAddress)
    {
#if SYSTEM_CONVERT_TOHEXSTRING
      Span<byte> addressBytes = stackalloc byte[16];

      if (linkLocalAddress.TryWriteBytes(addressBytes, out var bytesWritten) && (8 + 2) <= bytesWritten) {
        return (
          Convert.ToHexString(addressBytes.Slice(0, 2)),
          Convert.ToHexString(addressBytes.Slice(8, 2))
        );
      }
#else
      byte[]? addressBytes = null;

      try {
        addressBytes = ArrayPool<byte>.Shared.Rent(16);

        if (linkLocalAddress.TryWriteBytes(addressBytes, out var bytesWritten) && (8 + 2) <= bytesWritten) {
          return (
            $"{addressBytes[0]:X2}{addressBytes[1]:X2}",
            $"{addressBytes[8]:X2}{addressBytes[9]:X2}"
          );
        }
      }
      finally {
        if (addressBytes is not null)
          ArrayPool<byte>.Shared.Return(addressBytes);
      }
#endif

      return default; // or throw exception?
    }
  }

  private protected virtual ValueTask InitializeAsyncCore(
    IServiceProvider? serviceProvider,
    CancellationToken cancellationToken
  )
  {
    // nothing to do in this class
    return default;
  }
}
