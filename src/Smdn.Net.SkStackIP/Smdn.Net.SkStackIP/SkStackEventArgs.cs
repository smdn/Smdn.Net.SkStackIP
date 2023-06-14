// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Net;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP;

/// <summary>
/// Provides data for the events <see cref="SkStackClient.WokeUp"/> and <see cref="SkStackClient.Slept"/>.
/// </summary>
public class SkStackEventArgs : EventArgs {
  private protected IPAddress? SenderAddress { get; }

  /// <summary>
  /// Gets the <see cref="SkStackEventNumber"/> that represents the event that occurred.
  /// </summary>
  public SkStackEventNumber EventNumber { get; }

  internal SkStackEventArgs(SkStackEvent baseEvent)
  {
    EventNumber = baseEvent.Number;
    SenderAddress = baseEvent.Number switch {
      SkStackEventNumber.Undefined => null,
      SkStackEventNumber.WakeupSignalReceived => null,
      _ => baseEvent.SenderAddress ?? throw new InvalidOperationException($"{nameof(baseEvent.SenderAddress)} must not be null"),
    };
  }
}
