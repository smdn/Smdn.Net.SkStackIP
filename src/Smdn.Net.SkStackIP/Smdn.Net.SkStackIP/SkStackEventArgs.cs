// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Net;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP {
  public class SkStackEventArgs : EventArgs {
    private protected IPAddress SenderAddress { get; }
    public SkStackEventNumber EventNumber { get; }

    internal SkStackEventArgs(SkStackEvent baseEvent)
    {
      this.SenderAddress = baseEvent.SenderAddress;
      this.EventNumber = baseEvent.Number;
    }
  }
}