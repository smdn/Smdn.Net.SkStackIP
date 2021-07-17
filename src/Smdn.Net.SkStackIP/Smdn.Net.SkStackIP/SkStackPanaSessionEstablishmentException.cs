// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP {
  public class SkStackPanaSessionEstablishmentException : SkStackPanaSessionException {
    internal SkStackPanaSessionEstablishmentException(
      string message,
      SkStackEvent causedEvent,
      Exception innerException = null
    )
      : base(
        message: message,
        address: causedEvent.SenderAddress,
        eventNumber: causedEvent.Number,
        innerException: innerException
      )
    {
    }
  }
}