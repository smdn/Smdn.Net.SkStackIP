// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.NetworkInformation;

namespace Smdn.Net.SkStackIP;

/// <summary>
/// A class representing information about an established PANA session.
/// </summary>
public sealed class SkStackPanaSessionInfo {
  /// <summary>
  /// Gets the <see cref="IPAddress"/> representing the local IP address of the PANA session.
  /// </summary>
  public IPAddress LocalAddress { get; }

  /// <summary>
  /// Gets the <see cref="PhysicalAddress"/> representing the local MAC address of the PANA session.
  /// </summary>
  public PhysicalAddress LocalMacAddress { get; }

  /// <summary>
  /// Gets the <see cref="IPAddress"/> representing the peer IP address of the PANA session.
  /// </summary>
  /// <seealso cref="SkStackClient.PanaSessionPeerAddress"/>
  public IPAddress PeerAddress { get; }

  /// <summary>
  /// Gets the <see cref="PhysicalAddress"/> representing the peer MAC address of the PANA session.
  /// </summary>
  public PhysicalAddress PeerMacAddress { get; }

  /// <summary>
  /// Gets the <see cref="SkStackChannel"/> representing the logical channel number used in the PANA session.
  /// </summary>
  public SkStackChannel Channel { get; }

  /// <summary>
  /// Gets the value representing the ID for the Personal Area Network (PAN) used in the PANA session.
  /// </summary>
  public int PanId { get; }

  internal SkStackPanaSessionInfo(
    IPAddress localAddress,
    PhysicalAddress localMacAddress,
    IPAddress peerAddress,
    PhysicalAddress peerMacAddress,
    SkStackChannel channel,
    int panId
  )
  {
    LocalAddress = localAddress;
    LocalMacAddress = localMacAddress;
    PeerAddress = peerAddress;
    PeerMacAddress = peerMacAddress;
    Channel = channel;
    PanId = panId;
  }
}
