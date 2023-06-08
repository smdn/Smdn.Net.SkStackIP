// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Net;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP;

public class SkStackPanaSessionEstablishmentException : SkStackPanaSessionException {
  internal SkStackPanaSessionEstablishmentException(
    string message,
    IPAddress address,
    SkStackEventNumber eventNumber,
    Exception innerException = null
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
