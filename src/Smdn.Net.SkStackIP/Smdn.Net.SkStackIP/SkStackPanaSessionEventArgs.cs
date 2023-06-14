// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System.Net;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP;

/// <summary>
///   <para>Provides data for the following events.</para>
///   <list type="bullet">
///     <item><description><see cref="SkStackClient.PanaSessionEstablished"/></description></item>
///     <item><description><see cref="SkStackClient.PanaSessionTerminated"/></description></item>
///     <item><description><see cref="SkStackClient.PanaSessionExpired"/></description></item>
///   </list>
/// </summary>
public sealed class SkStackPanaSessionEventArgs : SkStackEventArgs {
  /// <summary>
  /// Gets the peer address of the PANA session to which the event occurred.
  /// </summary>
  public IPAddress PanaSessionPeerAddress => SenderAddress!;

  internal SkStackPanaSessionEventArgs(SkStackEvent baseEvent)
    : base(baseEvent)
  {
  }
}
