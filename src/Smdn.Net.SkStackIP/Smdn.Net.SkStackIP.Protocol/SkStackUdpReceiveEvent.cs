// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.NetworkInformation;

namespace Smdn.Net.SkStackIP.Protocol;

/// <remarks>
///   <para>See 'BP35A1コマンドリファレンス 4.1. ERXUDP' for detailed specifications.</para>
/// </remarks>
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
    RemoteEndPoint = new(sender, (int)rport);
    LocalEndPoint = new(dest, (int)lport);
    RemoteLinkLocalAddress = senderlla;
    IsSecured = secured;
  }
}
