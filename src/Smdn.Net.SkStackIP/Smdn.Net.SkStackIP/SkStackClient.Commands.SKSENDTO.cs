// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP;

#pragma warning disable IDE0040
partial class SkStackClient {
#pragma warning restore IDE0040
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
      destinationPort: destination.Port,
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
      destinationPort: destination.Port,
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
          throwIfErrorStatus: true,
          cancellationToken: cancellationToken
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
}
