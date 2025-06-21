// SPDX-FileCopyrightText: 2025 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1032

using System;

namespace Smdn.Net.SkStackIP;

/// <summary>
/// The exception that represents an invalid operation that is not allowed in
/// an unestablished PANA session.
/// </summary>
/// <remarks>
/// This exception is thrown when the <see cref="SkStackClient.PanaSessionState"/> is
/// <see cref="SkStackEventNumber.PanaSessionExpired"/>.
/// </remarks>
/// <seealso cref="SkStackClient.PanaSessionState"/>
/// <seealso cref="SkStackClient.IsPanaSessionAlive"/>
public class SkStackPanaSessionNotEstablishedException : SkStackPanaSessionStateException {
  public SkStackPanaSessionNotEstablishedException()
    : base(
      message: "An invalid operation was attempted that is not allowed in an unestablished PANA session."
    )
  {
  }

  public SkStackPanaSessionNotEstablishedException(string message)
    : base(message: message)
  {
  }

  public SkStackPanaSessionNotEstablishedException(string message, Exception? innerException = null)
    : base(message: message, innerException: innerException)
  {
  }
}
