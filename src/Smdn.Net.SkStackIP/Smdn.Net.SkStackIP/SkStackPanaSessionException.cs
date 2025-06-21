// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1032

using System;
using System.Net;

namespace Smdn.Net.SkStackIP;

/// <summary>
/// The exception that represents an error during the establishment or
/// termination of a PANA session.
/// </summary>
public abstract class SkStackPanaSessionException : InvalidOperationException {
  public IPAddress Address { get; }
  public SkStackEventNumber EventNumber { get; }

  private protected SkStackPanaSessionException(
    string message,
    IPAddress address,
    SkStackEventNumber eventNumber,
    Exception? innerException = null
  )
    : base(
      message: message,
      innerException: innerException
    )
  {
    Address = address;
    EventNumber = eventNumber;
  }
}
