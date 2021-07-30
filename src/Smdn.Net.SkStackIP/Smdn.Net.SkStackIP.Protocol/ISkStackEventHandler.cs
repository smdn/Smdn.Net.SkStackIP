// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Net;

namespace Smdn.Net.SkStackIP.Protocol {
  public interface ISkStackEventHandler {
    bool TryProcessEvent(SkStackEventNumber eventNumber, IPAddress senderAddress);
    void ProcessSubsequentEvent(ISkStackSequenceParserContext context);
  }
}