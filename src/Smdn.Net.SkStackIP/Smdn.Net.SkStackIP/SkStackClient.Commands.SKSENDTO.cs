// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP;

#pragma warning disable IDE0040
partial class SkStackClient {
#pragma warning restore IDE0040
  /// <summary>
  ///   <para>Sends a command <c>SKSENDTO</c>.</para>
  /// </summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.7. SKSENDTO' for detailed specifications.</para>
  /// </remarks>
  public ValueTask<(
    SkStackResponse Response,
    bool IsCompletedSuccessfully
  )> SendSKSENDTOAsync(
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

  /// <summary>
  ///   <para>Sends a command <c>SKSENDTO</c>.</para>
  /// </summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.7. SKSENDTO' for detailed specifications.</para>
  /// </remarks>
  public ValueTask<(
    SkStackResponse Response,
    bool IsCompletedSuccessfully
  )> SendSKSENDTOAsync(
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

  /// <summary>
  ///   <para>Sends a command <c>SKSENDTO</c>.</para>
  /// </summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.7. SKSENDTO' for detailed specifications.</para>
  /// </remarks>
  public ValueTask<(
    SkStackResponse Response,
    bool IsCompletedSuccessfully
  )> SendSKSENDTOAsync(
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

  /// <summary>
  ///   <para>Sends a command <c>SKSENDTO</c>.</para>
  /// </summary>
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.7. SKSENDTO' for detailed specifications.</para>
  /// </remarks>
  public ValueTask<(
    SkStackResponse Response,
    bool IsCompletedSuccessfully
  )> SendSKSENDTOAsync(
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

    SkStackUdpPort.ThrowIfPortHandleIsOutOfRange(handle, nameof(handle));
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

    async ValueTask<(SkStackResponse, bool)> SKSENDTO()
    {
      SkStackResponse response;
      bool hasUdpSendResultStored;
      bool isCompletedSuccessfully;

      try {
        response = await SendCommandAsync(
          command: SkStackCommandNames.SKSENDTO,
          writeArguments: writer => {
            writer.WriteTokenHex((byte)handle);
            writer.WriteTokenIPADDR(destinationAddress);
            writer.WriteTokenUINT16((ushort)destinationPort, zeroPadding: true);
            writer.WriteTokenHex((byte)encryption);
            writer.WriteTokenUINT16((ushort)data.Length, zeroPadding: true);
            writer.WriteToken(data.Span);
          },
          syntax: SkStackProtocolSyntax.SKSENDTO, // SKSENDTO must terminate the command line without CRLF
          throwIfErrorStatus: true,
          cancellationToken: cancellationToken
        ).ConfigureAwait(false);
      }
      finally {
        hasUdpSendResultStored = lastUdpSendResult.Remove(
          destinationAddress,
          out isCompletedSuccessfully
        );
      }

      if (!hasUdpSendResultStored) // in case when the 'EVENT 21' was not raised after SKSENDTO
        throw new SkStackUdpSendResultIndeterminateException();

      return (response, isCompletedSuccessfully);
    }
  }
}
