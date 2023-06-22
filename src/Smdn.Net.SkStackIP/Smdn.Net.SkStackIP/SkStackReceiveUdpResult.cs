// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;
using System.Net;

namespace Smdn.Net.SkStackIP;

/// <summary>
/// The type that represents the data received by <c>ERXUDP</c> events.
/// This type is used for the return value as a result of the method <seealso cref="SkStackClient.ReceiveUdpAsync"/>.
/// </summary>
/// <remarks>
/// The returned <see cref="SkStackReceiveUdpResult"/> from the method <seealso cref="SkStackClient.ReceiveUdpAsync"/> should be disposed by the caller.
/// </remarks>
/// <seealso cref="SkStackClient.ReceiveUdpAsync"/>
public sealed class SkStackReceiveUdpResult : IDisposable {
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

  internal SkStackReceiveUdpResult(
    IPAddress remoteAddress,
    int length,
    IMemoryOwner<byte> data
  )
  {
    this.RemoteAddress = remoteAddress;
    this.length = length;
    this.data = data;
  }

  ~SkStackReceiveUdpResult()
    => Dispose();

  public void Dispose()
  {
    data?.Dispose();
    data = null!;

    GC.SuppressFinalize(this);
  }
}
