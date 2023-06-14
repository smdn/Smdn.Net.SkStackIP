// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1032

using System;
using System.Net;

namespace Smdn.Net.SkStackIP;

/// <summary>
/// The exception that represents an error on the establishment of a PANA session.
/// </summary>
/// <seealso cref="SkStackClient.SendSKJOINAsync"/>
public class SkStackPanaSessionEstablishmentException : SkStackPanaSessionException {
  internal SkStackPanaSessionEstablishmentException(
    string message,
    IPAddress address,
    SkStackEventNumber eventNumber,
    Exception? innerException = null
  )
    : base(
      message: message,
      address: address,
      eventNumber: eventNumber,
      innerException: innerException
    )
  {
  }
}
