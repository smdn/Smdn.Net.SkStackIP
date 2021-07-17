// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Net;

namespace Smdn.Net.SkStackIP.Protocol {
  /// <remarks>reference: BP35A1コマンドリファレンス 4.8. EVENT</remarks>
  internal readonly struct SkStackEvent {
    public SkStackEventNumber Number { get; }
    public IPAddress SenderAddress { get; }
    public int Parameter { get; }
    public SkStackEventCode ExpectedSubsequentEventCode { get; }

    internal SkStackEvent(
      SkStackEventNumber number,
      IPAddress senderAddress,
      int parameter,
      SkStackEventCode expectedSubsequentEventCode
    )
    {
      this.Number = number;
      this.SenderAddress = senderAddress;
      this.Parameter = parameter;
      this.ExpectedSubsequentEventCode = expectedSubsequentEventCode;
    }
  }
}