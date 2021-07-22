// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;
using System.Net;

namespace Smdn.Net.SkStackIP {
  public sealed class SkStackUdpReceiveResult : IDisposable {
    public IPAddress RemoteAddress { get; }

    private readonly int length;
    private IMemoryOwner<byte> data;

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
      data = null;

      GC.SuppressFinalize(this);
    }
  }
}