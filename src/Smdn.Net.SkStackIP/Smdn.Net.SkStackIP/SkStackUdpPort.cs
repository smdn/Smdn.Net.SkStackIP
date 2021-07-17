// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

namespace Smdn.Net.SkStackIP {
  /// <remarks>reference: BP35A1コマンドリファレンス 5. 待ち受けポート番号</remarks>
  public readonly struct SkStackUdpPort {
    internal const int NumberOfPorts = 6;
    internal static readonly SkStackUdpPortHandle HandleMin = SkStackUdpPortHandle.Handle1;
    internal static readonly SkStackUdpPortHandle HandleMax = SkStackUdpPortHandle.Handle6;

    /// <remarks>reference: BP35A1コマンドリファレンス 5.1. UDP ポート</remarks>
    public const int PortEchonetLite = 3610;
    /// <remarks>reference: BP35A1コマンドリファレンス 5.1. UDP ポート</remarks>
    public const int PortPana = 716;

    public SkStackUdpPortHandle Handle { get; }
    public int Port { get; }

    internal SkStackUdpPort(SkStackUdpPortHandle handle, int port)
    {
      this.Handle = handle;
      this.Port = port;
    }

    public override string ToString()
      => $"{Port} (#{(byte)Handle})";
  }
}