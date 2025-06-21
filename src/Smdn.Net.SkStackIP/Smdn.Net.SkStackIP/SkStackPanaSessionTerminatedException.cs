// SPDX-FileCopyrightText: 2025 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1032

using System;

namespace Smdn.Net.SkStackIP;

/// <summary>
/// The exception that represents an invalid operation that is not allowed in
/// a terminated PANA session.
/// </summary>
/// <remarks>
/// This exception is thrown when the <see cref="SkStackClient.PanaSessionState"/> is
/// <see cref="SkStackEventNumber.PanaSessionExpired"/>.
/// </remarks>
/// <seealso cref="SkStackClient.PanaSessionState"/>
/// <seealso cref="SkStackClient.IsPanaSessionAlive"/>
public class SkStackPanaSessionTerminatedException : SkStackPanaSessionStateException {
  public SkStackPanaSessionTerminatedException()
    : base(
      message: "An invalid operation was attempted that is not allowed in a terminated PANA session."
    )
  {
  }

  public SkStackPanaSessionTerminatedException(string message)
    : base(message: message)
  {
  }

  public SkStackPanaSessionTerminatedException(string message, Exception? innerException = null)
    : base(message: message, innerException: innerException)
  {
  }
}
