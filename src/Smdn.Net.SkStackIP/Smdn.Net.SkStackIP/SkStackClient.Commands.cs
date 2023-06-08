// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;
using System.Collections.Generic;
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
  /// <summary>sets the value of register <paramref name="register"/> to <paramref name="value"/></value>
  /// <remarks>reference: BP35A1コマンドリファレンス 3.1. SKSREG</remarks>
  public ValueTask<SkStackResponse> SendSKSREGAsync<TValue>(
    SkStackRegister.RegisterEntry<TValue> register,
    TValue value,
    CancellationToken cancellationToken = default
  )
  {
    if (register is null)
      throw new ArgumentNullException(nameof(register));
    if (!register.IsWritable)
      throw new InvalidOperationException($"register {register.Name} is not writable");

    register.ThrowIfValueIsNotInRange(value, nameof(value));

    return SendCommandAsync(
      command: SkStackCommandNames.SKSREG,
      arguments: SkStackCommandArgs.CreateEnumerable(register.SREG, register.CreateSKSREGArgument(value)),
      cancellationToken: cancellationToken,
      throwIfErrorStatus: true
    );
  }

  /// <summary>gets the value of register <paramref name="register"/></value>
  /// <remarks>reference: BP35A1コマンドリファレンス 3.1. SKSREG</remarks>
  public ValueTask<SkStackResponse<TValue>> SendSKSREGAsync<TValue>(
    SkStackRegister.RegisterEntry<TValue> register,
    CancellationToken cancellationToken = default
  )
  {
    if (register is null)
      throw new ArgumentNullException(nameof(register));
    if (!register.IsReadable)
      throw new InvalidOperationException($"register {register.Name} is not readable");

    return SendCommandAsync(
      command: SkStackCommandNames.SKSREG,
      arguments: SkStackCommandArgs.CreateEnumerable(register.SREG),
      parseResponsePayload: register.ParseESREG,
      cancellationToken: cancellationToken,
      throwIfErrorStatus: true
    );
  }

  /// <remarks>reference: BP35A1コマンドリファレンス 3.2. SKINFO</remarks>
  public ValueTask<SkStackResponse<(
    IPAddress LinkLocalAddress,
    PhysicalAddress MacAddress,
    SkStackChannel Channel,
    int PanId,
    int Addr16
  )>>
  SendSKINFOAsync(
    CancellationToken cancellationToken = default
  )
    => SendCommandAsync(
      command: SkStackCommandNames.SKINFO,
      arguments: null,
      parseResponsePayload: static context => {
        var reader = context.CreateReader();

        if (
          SkStackTokenParser.ExpectToken(ref reader, EINFO) &&
          SkStackTokenParser.ExpectIPADDR(ref reader, out var linkLocalAddress) &&
          SkStackTokenParser.ExpectADDR64(ref reader, out var macAddress) &&
          SkStackTokenParser.ExpectCHANNEL(ref reader, out var channel) &&
          SkStackTokenParser.ExpectUINT16(ref reader, out var panID) &&
          SkStackTokenParser.ExpectADDR16(ref reader, out var addr16) &&
          SkStackTokenParser.ExpectEndOfLine(ref reader)
        ) {
          context.Complete(reader);
          return (
            linkLocalAddress,
            macAddress,
            channel,
            (int)panID,
            (int)addr16
          );
        }

        context.SetAsIncomplete();
        return default;
      },
      cancellationToken: cancellationToken,
      throwIfErrorStatus: true
    );

  private static readonly ReadOnlyMemory<byte> EINFO = SkStack.ToByteSequence(nameof(EINFO));

  /// <remarks>reference: BP35A1コマンドリファレンス 3.7. SKSENDTO</remarks>
  public ValueTask<SkStackResponse> SendSKSENDTOAsync(
    SkStackUdpPort port,
    IPEndPoint destination,
    ReadOnlyMemory<byte> data,
    SkStackUdpEncryption encryption = SkStackUdpEncryption.EncryptIfAble,
    CancellationToken cancellationToken = default
  )
    => SendSKSENDTOAsync(
      handle: port.Handle,
      destinationAddress: (destination ?? throw new ArgumentNullException(nameof(destination))).Address,
      destinationPort: (destination ?? throw new ArgumentNullException(nameof(destination))).Port,
      data: data,
      encryption: encryption,
      cancellationToken: cancellationToken
    );

  /// <remarks>reference: BP35A1コマンドリファレンス 3.7. SKSENDTO</remarks>
  public ValueTask<SkStackResponse> SendSKSENDTOAsync(
    SkStackUdpPort port,
    IPAddress destinationAddress,
    int destinationPort,
    ReadOnlyMemory<byte> data,
    SkStackUdpEncryption encryption = SkStackUdpEncryption.EncryptIfAble,
    CancellationToken cancellationToken = default
  )
    => SendSKSENDTOAsync(
      handle: port.Handle,
      destinationAddress: destinationAddress,
      destinationPort: destinationPort,
      data: data,
      encryption: encryption,
      cancellationToken: cancellationToken
    );

  /// <remarks>reference: BP35A1コマンドリファレンス 3.7. SKSENDTO</remarks>
  public ValueTask<SkStackResponse> SendSKSENDTOAsync(
    SkStackUdpPortHandle handle,
    IPEndPoint destination,
    ReadOnlyMemory<byte> data,
    SkStackUdpEncryption encryption = SkStackUdpEncryption.EncryptIfAble,
    CancellationToken cancellationToken = default
  )
    => SendSKSENDTOAsync(
      handle: handle,
      destinationAddress: (destination ?? throw new ArgumentNullException(nameof(destination))).Address,
      destinationPort: (destination ?? throw new ArgumentNullException(nameof(destination))).Port,
      data: data,
      encryption: encryption,
      cancellationToken: cancellationToken
    );

  /// <remarks>reference: BP35A1コマンドリファレンス 3.7. SKSENDTO</remarks>
  public ValueTask<SkStackResponse> SendSKSENDTOAsync(
    SkStackUdpPortHandle handle,
    IPAddress destinationAddress,
    int destinationPort,
    ReadOnlyMemory<byte> data,
    SkStackUdpEncryption encryption = SkStackUdpEncryption.EncryptIfAble,
    CancellationToken cancellationToken = default
  )
  {
    const int minDataLength = 0x0001;
    const int maxDataLength = 0x04D0;

    SkStackUdpPort.ThrowIfPortHandleIsNotDefined(handle, nameof(handle));
#if SYSTEM_ENUM_ISDEFINED_OF_TENUM
    if (!Enum.IsDefined(encryption))
#else
    if (!Enum.IsDefined(typeof(SkStackUdpEncryption), encryption))
#endif
      throw new ArgumentException($"undefined value of {nameof(SkStackUdpEncryption)}", nameof(encryption));
    if (destinationAddress is null)
      throw new ArgumentNullException(nameof(destinationAddress));
    SkStackUdpPort.ThrowIfPortNumberIsOutOfRange(destinationPort, nameof(destinationPort));
    if (data.IsEmpty)
      throw new ArgumentException("must be non-empty sequence", nameof(data));
    if (data.Length is not (>= minDataLength and <= maxDataLength))
      throw new ArgumentException($"length of {nameof(data)} must be in range of {minDataLength}~{maxDataLength}", nameof(data));

    return SKSENDTO();

    async ValueTask<SkStackResponse> SKSENDTO()
    {
      byte[] IPADDR = default;
      byte[] PORT = default;
      byte[] DATALEN = default;

      try {
        IPADDR = ArrayPool<byte>.Shared.Rent(SkStackCommandArgs.LengthOfIPADDR);
        PORT = ArrayPool<byte>.Shared.Rent(4);
        DATALEN = ArrayPool<byte>.Shared.Rent(4);

        SkStackCommandArgs.TryConvertToIPADDR(IPADDR, destinationAddress, out var lengthOfIPADDR);
        SkStackCommandArgs.TryConvertToUINT16(PORT, (ushort)destinationPort, out var lengthOfPORT, zeroPadding: true);
        SkStackCommandArgs.TryConvertToUINT16(DATALEN, (ushort)data.Length, out var lengthOfDATALEN, zeroPadding: true);

        return await SendCommandAsync(
          command: SkStackCommandNames.SKSENDTO,
          arguments: SkStackCommandArgs.CreateEnumerable(
            SkStackCommandArgs.GetHex((byte)handle),
            IPADDR.AsMemory(0, lengthOfIPADDR),
            PORT.AsMemory(0, lengthOfPORT),
            SkStackCommandArgs.GetHex((byte)encryption),
            DATALEN.AsMemory(0, lengthOfDATALEN),
            data
          ),
          syntax: SkStackProtocolSyntax.SKSENDTO, // SKSENDTO must terminate the command line without CRLF
          cancellationToken: cancellationToken,
          throwIfErrorStatus: true
        ).ConfigureAwait(false);
      }
      finally {
        if (IPADDR is not null)
          ArrayPool<byte>.Shared.Return(IPADDR);
        if (PORT is not null)
          ArrayPool<byte>.Shared.Return(PORT);
        if (DATALEN is not null)
          ArrayPool<byte>.Shared.Return(DATALEN);
      }
    }
  }

  /// <remarks>reference: BP35A1コマンドリファレンス 3.8. SKPING</remarks>

  /// <remarks>reference: BP35A1コマンドリファレンス 3.16. SKSETPWD</remarks>
  public ValueTask<SkStackResponse> SendSKSETPWDAsync(
    string password,
    CancellationToken cancellationToken = default
  )
  {
    if (password is null)
      throw new ArgumentNullException(nameof(password));

    var length = SkStack.DefaultEncoding.GetByteCount(password);

    if (length is not (>= SKSETPWDMinLength and <= SKSETPWDMaxLength))
      throw new ArgumentException($"length of `{nameof(password)}` must be in range of {SKSETPWDMinLength}~{SKSETPWDMaxLength}", nameof(password));

    return Core();

    async ValueTask<SkStackResponse> Core()
    {
      byte[] PWD = null;

      try {
        PWD = ArrayPool<byte>.Shared.Rent(length);

        var lengthOfPWD = SkStack.DefaultEncoding.GetBytes(password.AsSpan(), PWD.AsSpan());

        return await SendSKSETPWDAsync(
          password: PWD.AsMemory(0, lengthOfPWD),
          cancellationToken: cancellationToken
        ).ConfigureAwait(false);
      }
      finally {
        if (PWD is not null)
          ArrayPool<byte>.Shared.Return(PWD, clearArray: true);
      }
    }
  }

  private const int SKSETPWDMinLength = 1;
  private const int SKSETPWDMaxLength = 32;


  /// <remarks>reference: BP35A1コマンドリファレンス 3.16. SKSETPWD</remarks>
  public ValueTask<SkStackResponse> SendSKSETPWDAsync(
    ReadOnlyMemory<byte> password,
    CancellationToken cancellationToken = default
  )
  {
    if (password.Length is not (>= SKSETPWDMinLength and <= SKSETPWDMaxLength))
      throw new ArgumentException($"length of `{nameof(password)}` must be in range of {SKSETPWDMinLength}~{SKSETPWDMaxLength}", nameof(password));

    return SKSETPWD();

    async ValueTask<SkStackResponse> SKSETPWD()
    {
      byte[] LEN = default;

      try {
        LEN = ArrayPool<byte>.Shared.Rent(2);

        SkStackCommandArgs.TryConvertToUINT8(LEN, (byte)password.Length, out var lengthOfLEN, zeroPadding: false);

        return await SendCommandAsync(
          command: SkStackCommandNames.SKSETPWD,
          arguments: SkStackCommandArgs.CreateEnumerable(LEN.AsMemory(0, lengthOfLEN), password),
          cancellationToken: cancellationToken,
          throwIfErrorStatus: true
        ).ConfigureAwait(false);
      }
      finally {
        if (LEN is not null)
          ArrayPool<byte>.Shared.Return(LEN);
      }
    }
  }

  /// <remarks>reference: BP35A1コマンドリファレンス 3.17. SKSETRBID</remarks>
  public ValueTask<SkStackResponse> SendSKSETRBIDAsync(
    string routeBID,
    CancellationToken cancellationToken = default
  )
  {
    if (routeBID is null)
      throw new ArgumentNullException(nameof(routeBID));

    var length = SkStack.DefaultEncoding.GetByteCount(routeBID);

    if (length != SKSETRBIDLengthOfID)
      throw new ArgumentException($"length of `{nameof(routeBID)}` must be exact {SKSETRBIDLengthOfID}", nameof(routeBID));

    return Core();

    async ValueTask<SkStackResponse> Core()
    {
      byte[] ID = null;

      try {
        ID = ArrayPool<byte>.Shared.Rent(length);

        var lengthOfID = SkStack.DefaultEncoding.GetBytes(routeBID.AsSpan(), ID.AsSpan());

        return await SendSKSETRBIDAsync(
          routeBID: ID.AsMemory(0, lengthOfID),
          cancellationToken: cancellationToken
        ).ConfigureAwait(false);
      }
      finally {
        if (ID is not null)
          ArrayPool<byte>.Shared.Return(ID, clearArray: true);
      }
    }
  }

  private const int SKSETRBIDLengthOfID = 32;

  /// <remarks>reference: BP35A1コマンドリファレンス 3.17. SKSETRBID</remarks>
  public ValueTask<SkStackResponse> SendSKSETRBIDAsync(
    ReadOnlyMemory<byte> routeBID,
    CancellationToken cancellationToken = default
  )
  {
    if (routeBID.Length != SKSETRBIDLengthOfID)
      throw new ArgumentException($"length of `{nameof(routeBID)}` must be exact {SKSETRBIDLengthOfID}", nameof(routeBID));

    return SendCommandAsync(
      command: SkStackCommandNames.SKSETRBID,
      arguments: SkStackCommandArgs.CreateEnumerable(routeBID),
      cancellationToken: cancellationToken,
      throwIfErrorStatus: true
    );
  }

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
          cancellationToken: cancellationToken,
          throwIfErrorStatus: true
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

  /// <remarks>reference: BP35A1コマンドリファレンス 3.19. SKUDPPORT</remarks>
  public ValueTask<(SkStackResponse, SkStackUdpPort)> SendSKUDPPORTAsync(
    SkStackUdpPortHandle handle,
    int port,
    CancellationToken cancellationToken = default
  )
  {
    SkStackUdpPort.ThrowIfPortHandleIsNotDefined(handle, nameof(handle));
    SkStackUdpPort.ThrowIfPortNumberIsOutOfRangeOrUnused(port, nameof(port));

    return SendSKUDPPORTAsyncCore(
      handle: handle,
      port: port,
      cancellationToken: cancellationToken
    );
  }

  /// <remarks>reference: BP35A1コマンドリファレンス 3.19. SKUDPPORT</remarks>
  public ValueTask<SkStackResponse> SendSKUDPPORTUnsetAsync(
    SkStackUdpPortHandle handle,
    CancellationToken cancellationToken = default
  )
  {
    SkStackUdpPort.ThrowIfPortHandleIsNotDefined(handle, nameof(handle));

    return Core();

    async ValueTask<SkStackResponse> Core()
    {
      var (resp, _) = await SendSKUDPPORTAsyncCore(
        handle: handle,
        port: SkStackKnownPortNumbers.SetUnused,
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);

      return resp;
    }
  }

  private async ValueTask<(SkStackResponse, SkStackUdpPort)> SendSKUDPPORTAsyncCore(
    SkStackUdpPortHandle handle,
    int port,
    CancellationToken cancellationToken = default
  )
  {
    byte[] PORT = null;

    try {
      PORT = ArrayPool<byte>.Shared.Rent(4);

      SkStackCommandArgs.TryConvertToUINT16(PORT, (ushort)port, out var lengthOfPORT, zeroPadding: true);

      var resp = await SendCommandAsync(
        command: SkStackCommandNames.SKUDPPORT,
        arguments: SkStackCommandArgs.CreateEnumerable(
          SkStackCommandArgs.GetHex((int)handle),
          PORT.AsMemory(0, lengthOfPORT)
        ),
        cancellationToken: cancellationToken,
        throwIfErrorStatus: true
      ).ConfigureAwait(false);

      return (resp, new SkStackUdpPort(handle, port));
    }
    finally {
      if (PORT is not null)
        ArrayPool<byte>.Shared.Return(PORT);
    }
  }

  /// <remarks>reference: BP35A1コマンドリファレンス 3.20. SKSAVE</remarks>
  public ValueTask<SkStackResponse> SendSKSAVEAsync(
    CancellationToken cancellationToken = default
  )
    => SendFlashMemoryCommand(
      command: SkStackCommandNames.SKSAVE,
      ioErrorMessage: "Failed to save the register values to the flash memory.",
      cancellationToken: cancellationToken
    );

  /// <remarks>reference: BP35A1コマンドリファレンス 3.21. SKLOAD</remarks>
  public ValueTask<SkStackResponse> SendSKLOADAsync(
    CancellationToken cancellationToken = default
  )
    => SendFlashMemoryCommand(
      command: SkStackCommandNames.SKLOAD,
      ioErrorMessage: "Failed to load the register values from the flash memory or the register values have not been saved in the flash memory.",
      cancellationToken: cancellationToken
    );

  private async ValueTask<SkStackResponse> SendFlashMemoryCommand(
    ReadOnlyMemory<byte> command,
    string ioErrorMessage,
    CancellationToken cancellationToken = default
  )
  {
    var resp = await SendCommandAsync(
      command: command,
      arguments: Array.Empty<ReadOnlyMemory<byte>>(),
      cancellationToken: cancellationToken,
      throwIfErrorStatus: false
    ).ConfigureAwait(false);

    resp.ThrowIfErrorStatus(
      (r, code, text) => code == SkStackErrorCode.ER10
        ? new SkStackFlashMemoryIOException(r, code, text.Span, ioErrorMessage)
        : null
    );

    return resp;
  }

  /// <remarks>reference: BP35A1コマンドリファレンス 3.22. SKERASE</remarks>
  public ValueTask<SkStackResponse> SendSKERASEAsync(
    CancellationToken cancellationToken = default
  )
    => SendCommandAsync(
      command: SkStackCommandNames.SKERASE,
      arguments: Array.Empty<ReadOnlyMemory<byte>>(),
      cancellationToken: cancellationToken,
      throwIfErrorStatus: false
    );

  /// <remarks>reference: BP35A1コマンドリファレンス 3.23. SKVER</remarks>
  public ValueTask<SkStackResponse<Version>> SendSKVERAsync(
    CancellationToken cancellationToken = default
  )
    => SendCommandAsync(
      command: SkStackCommandNames.SKVER,
      arguments: null,
      parseResponsePayload: static context => {
        var reader = context.CreateReader();

        if (
          SkStackTokenParser.ExpectToken(ref reader, EVER) &&
          SkStackTokenParser.ExpectCharArray(ref reader, out string version) &&
          SkStackTokenParser.ExpectEndOfLine(ref reader)
        ) {
          context.Complete(reader);
          return Version.Parse(version);
        }

        context.SetAsIncomplete();
        return default;
      },
      cancellationToken: cancellationToken,
      throwIfErrorStatus: true
    );

  private static readonly ReadOnlyMemory<byte> EVER = SkStack.ToByteSequence(nameof(EVER));

  /// <remarks>reference: BP35A1コマンドリファレンス 3.24. SKAPPVER</remarks>
  public ValueTask<SkStackResponse<string>> SendSKAPPVERAsync(
    CancellationToken cancellationToken = default
  )
    => SendCommandAsync(
      command: SkStackCommandNames.SKAPPVER,
      arguments: null,
      parseResponsePayload: static context => {
        var reader = context.CreateReader();

        if (
          SkStackTokenParser.ExpectToken(ref reader, EAPPVER) &&
          SkStackTokenParser.ExpectCharArray(ref reader, out string appver) &&
          SkStackTokenParser.ExpectEndOfLine(ref reader)
        ) {
          context.Complete(reader);
          return appver;
        }

        context.SetAsIncomplete();
        return default;
      },
      cancellationToken: cancellationToken,
      throwIfErrorStatus: true
    );

  private static readonly ReadOnlyMemory<byte> EAPPVER = SkStack.ToByteSequence(nameof(EAPPVER));

  /// <remarks>reference: BP35A1コマンドリファレンス 3.25. SKRESET</remarks>
  public ValueTask<SkStackResponse> SendSKRESETAsync(
    CancellationToken cancellationToken = default
  )
    => SendCommandAsync(
      command: SkStackCommandNames.SKRESET,
      arguments: Array.Empty<ReadOnlyMemory<byte>>(),
      cancellationToken: cancellationToken,
      throwIfErrorStatus: true
    );

  /// <summary>`SKTABLE 1`</summary>
  /// <remarks>reference: BP35A1コマンドリファレンス 3.26. SKTABLE</remarks>
  public ValueTask<SkStackResponse<IReadOnlyList<IPAddress>>> SendSKTABLEAvailableAddressListAsync(
    CancellationToken cancellationToken = default
  )
    => SendCommandAsync(
      command: SkStackCommandNames.SKTABLE,
      arguments: SkStackCommandArgs.CreateEnumerable(SkStackCommandArgs.GetHex(0x1)),
      parseResponsePayload: SkStackEventParser.ExpectEADDR,
      cancellationToken: cancellationToken,
      throwIfErrorStatus: true
    );

  /// <summary>`SKTABLE 2`</summary>
  /// <remarks>reference: BP35A1コマンドリファレンス 3.26. SKTABLE</remarks>
  public ValueTask<SkStackResponse<IReadOnlyDictionary<IPAddress, PhysicalAddress>>> SendSKTABLENeighborCacheListAsync(
    CancellationToken cancellationToken = default
  )
    => SendCommandAsync(
      command: SkStackCommandNames.SKTABLE,
      arguments: SkStackCommandArgs.CreateEnumerable(SkStackCommandArgs.GetHex(0x2)),
      parseResponsePayload: SkStackEventParser.ExpectENEIGHBOR,
      cancellationToken: cancellationToken,
      throwIfErrorStatus: true
    );

  /// <summary>`SKTABLE E`</summary>
  /// <remarks>reference: BP35A1コマンドリファレンス 3.26. SKTABLE</remarks>
  public ValueTask<SkStackResponse<IReadOnlyList<SkStackUdpPort>>> SendSKTABLEListeningPortListAsync(
    CancellationToken cancellationToken = default
  )
    => SendCommandAsync(
      command: SkStackCommandNames.SKTABLE,
      arguments: SkStackCommandArgs.CreateEnumerable(SkStackCommandArgs.GetHex(0xE)),
      parseResponsePayload: SkStackEventParser.ExpectEPORT,
      cancellationToken: cancellationToken,
      throwIfErrorStatus: true
    );

  /// <remarks>reference: BP35A1コマンドリファレンス 3.29. SKLL64</remarks>
  public ValueTask<SkStackResponse<IPAddress>> SendSKLL64Async(
    PhysicalAddress macAddress,
    CancellationToken cancellationToken = default
  )
  {
    if (macAddress is null)
      throw new ArgumentNullException(nameof(macAddress));

    return SKLL64();

    async ValueTask<SkStackResponse<IPAddress>> SKLL64()
    {
      byte[] ADDR64 = null;

      try {
        ADDR64 = ArrayPool<byte>.Shared.Rent(SkStackCommandArgs.LengthOfADDR64);

        SkStackCommandArgs.TryConvertToADDR64(ADDR64, macAddress, out var lengthOfADDR64);

        return await SendCommandAsync(
          command: SkStackCommandNames.SKLL64,
          arguments: SkStackCommandArgs.CreateEnumerable(ADDR64.AsMemory(0, lengthOfADDR64)),
          parseResponsePayload: static context => {
            var reader = context.CreateReader();

            if (
              SkStackTokenParser.ExpectIPADDR(ref reader, out var linkLocalAddress) &&
              SkStackTokenParser.ExpectEndOfLine(ref reader)
            ) {
              context.Complete(reader);
              return linkLocalAddress;
            }

            context.SetAsIncomplete();
            return default;
          },
          syntax: SkStackProtocolSyntax.SKLL64, // SKLL64 does not define its status
          cancellationToken: cancellationToken,
          throwIfErrorStatus: true
        ).ConfigureAwait(false);
      }
      finally {
        if (ADDR64 is not null)
          ArrayPool<byte>.Shared.Return(ADDR64);
      }
    }
  }
}
