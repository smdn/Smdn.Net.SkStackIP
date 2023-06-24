// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net;

namespace Smdn.Net.SkStackIP;

#pragma warning disable IDE0040
partial class SkStackClient {
#pragma warning restore IDE0040
  /// <summary>Gets the <see cref="IPAddress"/> of current PANA session peer.</summary>
  /// <value><see langword="null"/> if PANA session has been terminated, expired, or not been established.</value>
  public IPAddress? PanaSessionPeerAddress { get; private set; }

  /// <summary>Gets a value indicating whether or not the PANA session is alive.</summary>
  /// <value><see langword="true"/> if PANA session is established and alive, <see langword="false"/> if PANA session has been terminated, expired, or not been established.</value>
  public bool IsPanaSessionAlive => PanaSessionPeerAddress is not null;

  protected internal void ThrowIfPanaSessionAlreadyEstablished()
  {
    if (PanaSessionPeerAddress is not null)
      throw new InvalidOperationException("PANA session has been already established or not been expired.");
  }
}
