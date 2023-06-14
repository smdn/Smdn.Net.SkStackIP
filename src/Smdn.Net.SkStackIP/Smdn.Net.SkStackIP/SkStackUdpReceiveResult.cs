// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;
using System.Net;

namespace Smdn.Net.SkStackIP;

/// <summary>
/// The type that represents the data received by <c>ERXUDP</c> events.
/// This type is used for the return value as a result of the method <seealso cref="SkStackClient.UdpReceiveAsync"/>.
/// </summary>
/// <remarks>
/// The returned <see cref="SkStackUdpReceiveResult"/> from the method <seealso cref="SkStackClient.UdpReceiveAsync"/> should be disposed by the caller.
/// </remarks>
/// <seealso cref="SkStackClient.UdpReceiveAsync"/>
public sealed class SkStackUdpReceiveResult : IDisposable {
  /// <summary>
  /// Gets the remote address of the UDP packet.
  /// </summary>
  public IPAddress RemoteAddress { get; }

  private readonly int length;
  private IMemoryOwner<byte> data;

  /// <summary>
  /// Gets the buffer that holds the received data.
  /// </summary>
  public ReadOnlyMemory<byte> Buffer => (data ?? throw new ObjectDisposedException(GetType().FullName)).Memory.Slice(0, length);

  internal SkStackUdpReceiveResult(
    IPAddress remoteAddress,
    int length,
    IMemoryOwner<byte> data
  )
  {
    this.RemoteAddress = remoteAddress;
    this.length = length;
    this.data = data;
  }

  ~SkStackUdpReceiveResult()
    => Dispose();

  public void Dispose()
  {
    data?.Dispose();
    data = null!;

    GC.SuppressFinalize(this);
  }
}
