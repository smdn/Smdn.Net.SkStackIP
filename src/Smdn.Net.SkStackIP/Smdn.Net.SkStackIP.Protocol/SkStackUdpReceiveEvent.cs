// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Net;
using System.Net.NetworkInformation;

namespace Smdn.Net.SkStackIP {
  /// <remarks>reference: BP35A1コマンドリファレンス 4.1. ERXUDP</remarks>
  internal readonly struct SkStackUdpReceiveEvent {
    public IPEndPoint RemoteEndPoint { get; }
    public IPEndPoint LocalEndPoint { get; }
    public PhysicalAddress RemoteLinkLocalAddress { get; }
    public bool IsSecured { get; }

    internal SkStackUdpReceiveEvent(
      IPAddress sender,
      IPAddress dest,
      uint rport,
      uint lport,
      PhysicalAddress senderlla,
      bool secured
    )
    {
      this.RemoteEndPoint = new IPEndPoint(sender, (int)rport);
      this.LocalEndPoint = new IPEndPoint(dest, (int)lport);
      this.RemoteLinkLocalAddress = senderlla;
      this.IsSecured = secured;
    }
  }
}