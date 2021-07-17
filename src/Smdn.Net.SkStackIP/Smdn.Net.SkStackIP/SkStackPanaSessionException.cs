// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Net;

namespace Smdn.Net.SkStackIP {
  public abstract class SkStackPanaSessionException : InvalidOperationException {
    public IPAddress Address { get; }
    public SkStackEventNumber EventNumber { get; }

    private protected SkStackPanaSessionException(
      string message,
      IPAddress address,
      SkStackEventNumber eventNumber,
      Exception innerException = null
    )
      : base(
        message: message,
        innerException: innerException
      )
    {
      this.Address = address;
      this.EventNumber = eventNumber;
    }
  }
}