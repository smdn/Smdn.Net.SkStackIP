// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System.Net;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Net.SkStackIP;

public sealed class SkStackPanaSessionEventArgs : SkStackEventArgs {
  public IPAddress PanaSessionPeerAddress => SenderAddress!;

  internal SkStackPanaSessionEventArgs(SkStackEvent baseEvent)
    : base(baseEvent)
  {
  }
}
