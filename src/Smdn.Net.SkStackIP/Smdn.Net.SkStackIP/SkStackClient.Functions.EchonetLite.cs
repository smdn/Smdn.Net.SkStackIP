// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Polly;

namespace Smdn.Net.SkStackIP;

#pragma warning disable IDE0040
partial class SkStackClient {
#pragma warning restore IDE0040
  /// <summary>
  /// An UDP port handle currently assigned to the port for ECHONET Lite.
  /// </summary>
  /// <remarks>
  /// This value will be updated each time an <c>EPORT</c> is received. See implementation of <see cref="SendSKTABLEListeningPortListAsync"/>.
  /// </remarks>
  /// <seealso cref="SkStackKnownPortNumbers.EchonetLite"/>
  /// <seealso cref="SendUdpEchonetLiteAsync"/>
  private SkStackUdpPortHandle udpPortHandleForEchonetLite;

  public ValueTask<IPAddress> ReceiveUdpEchonetLiteAsync(
    IBufferWriter<byte> buffer,
    CancellationToken cancellationToken = default
  )
  {
    if (buffer is null)
      throw new ArgumentNullException(nameof(buffer));

    ThrowIfDisposed();

    return ReceiveUdpAsync(
      port: SkStackKnownPortNumbers.EchonetLite,
      buffer: buffer,
      cancellationToken: cancellationToken
    );
  }

  [CLSCompliant(false)] // ResilienceStrategy is not CLS compliant
  public ValueTask SendUdpEchonetLiteAsync(
    ReadOnlyMemory<byte> buffer,
    ResilienceStrategy? resilienceStrategy = null,
    CancellationToken cancellationToken = default
  )
  {
    if (SkStackUdpPort.IsPortHandleIsOutOfRange(udpPortHandleForEchonetLite))
      throw new InvalidOperationException($"UDP port {SkStackKnownPortNumbers.EchonetLite} is not listening. Call {nameof(PrepareUdpPortAsync)} or {nameof(SendSKUDPPORTAsync)} in advance to listen the port.");

    ThrowIfDisposed();
    ThrowIfPanaSessionIsNotEstablished();

    resilienceStrategy ??= NullResilienceStrategy.Instance;

    return resilienceStrategy.ExecuteAsync(
      ct => SendUdpEchonetLiteAsyncCore(
        thisClient: this,
        udpPortHandleForEchonetLite: udpPortHandleForEchonetLite,
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLATTRIBUTE
        peerAddress: PanaSessionPeerAddress,
#else
        peerAddress: PanaSessionPeerAddress!,
#endif
        buffer: buffer,
        cancellationToken: ct
      ),
      cancellationToken: cancellationToken
    );

    static async ValueTask SendUdpEchonetLiteAsyncCore(
      SkStackClient thisClient,
      SkStackUdpPortHandle udpPortHandleForEchonetLite,
      IPAddress peerAddress,
      ReadOnlyMemory<byte> buffer,
      CancellationToken cancellationToken
    )
    {
      var (_, isCompletedSuccessfully) = await thisClient.SendSKSENDTOAsync(
        handle: udpPortHandleForEchonetLite,
        destinationAddress: peerAddress,
        destinationPort: SkStackKnownPortNumbers.EchonetLite,
        data: buffer,
        encryption: SkStackUdpEncryption.ForceEncrypt,
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);

      if (!isCompletedSuccessfully) {
        throw new SkStackUdpSendFailedException(
          message: $"Failed to send ECHONET Lite frame. (Handle: {udpPortHandleForEchonetLite}, Peer: {peerAddress})",
          portHandle: udpPortHandleForEchonetLite,
          peerAddress: peerAddress
        );
      }
    }
  }
}
