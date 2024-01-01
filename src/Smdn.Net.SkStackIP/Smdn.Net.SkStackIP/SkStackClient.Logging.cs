// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Logging;

namespace Smdn.Net.SkStackIP;

#pragma warning disable IDE0040
partial class SkStackClient {
#pragma warning restore IDE0040
  internal static readonly EventId EventIdReceivingStatus = new(1, "receiving status");
  internal static readonly EventId EventIdCommandSequence = new(2, "sent command sequence");
  internal static readonly EventId EventIdResponseSequence = new(3, "received response sequence");

  internal static readonly EventId EventIdIPEventReceived = new(6, "IP event received");
  internal static readonly EventId EventIdPanaEventReceived = new(7, "PANA event received");
  internal static readonly EventId EventIdAribStdT108EventReceived = new(8, "ARIB STD-T108 event received");
}
