// SPDX-FileCopyrightText: 2025 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1032

using System;

namespace Smdn.Net.SkStackIP;

/// <summary>
/// The exception that represents an invalid operation that is not allowed
/// in the current PANA session state.
/// </summary>
/// <seealso cref="SkStackClient.PanaSessionState"/>
/// <seealso cref="SkStackClient.IsPanaSessionAlive"/>
public class SkStackPanaSessionStateException : InvalidOperationException {
  public SkStackPanaSessionStateException()
    : base(
      message: "An invalid operation was attempted that is not allowed in the current PANA session state."
    )
  {
  }

  public SkStackPanaSessionStateException(string message)
    : base(message: message)
  {
  }

  public SkStackPanaSessionStateException(string message, Exception? innerException = null)
    : base(message: message, innerException: innerException)
  {
  }
}
