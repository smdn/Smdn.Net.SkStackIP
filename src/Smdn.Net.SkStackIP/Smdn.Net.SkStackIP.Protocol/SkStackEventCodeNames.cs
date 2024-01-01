// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;

namespace Smdn.Net.SkStackIP.Protocol;

internal static class SkStackEventCodeNames {
  private static readonly IReadOnlyDictionary<SkStackEventCode, ReadOnlyMemory<byte>> EventCodeAndNames =
    new Dictionary<SkStackEventCode, ReadOnlyMemory<byte>>() {
      { SkStackEventCode.ERXUDP,      SkStack.ToByteSequence(nameof(SkStackEventCode.ERXUDP)) },
      { SkStackEventCode.EPONG,       SkStack.ToByteSequence(nameof(SkStackEventCode.EPONG)) },
      { SkStackEventCode.EADDR,       SkStack.ToByteSequence(nameof(SkStackEventCode.EADDR)) },
      { SkStackEventCode.ENEIGHBOR,   SkStack.ToByteSequence(nameof(SkStackEventCode.ENEIGHBOR)) },
      { SkStackEventCode.EPANDESC,    SkStack.ToByteSequence(nameof(SkStackEventCode.EPANDESC)) },
      { SkStackEventCode.EEDSCAN,     SkStack.ToByteSequence(nameof(SkStackEventCode.EEDSCAN)) },
      { SkStackEventCode.EPORT,       SkStack.ToByteSequence(nameof(SkStackEventCode.EPORT)) },
      { SkStackEventCode.EVENT,       SkStack.ToByteSequence(nameof(SkStackEventCode.EVENT)) },
    };

  public static ReadOnlyMemory<byte> ERXUDP => EventCodeAndNames[SkStackEventCode.ERXUDP];
  public static ReadOnlyMemory<byte> EPONG => EventCodeAndNames[SkStackEventCode.EPONG];
  public static ReadOnlyMemory<byte> EADDR => EventCodeAndNames[SkStackEventCode.EADDR];
  public static ReadOnlyMemory<byte> ENEIGHBOR => EventCodeAndNames[SkStackEventCode.ENEIGHBOR];
  public static ReadOnlyMemory<byte> EPANDESC => EventCodeAndNames[SkStackEventCode.EPANDESC];
  public static ReadOnlyMemory<byte> EEDSCAN => EventCodeAndNames[SkStackEventCode.EEDSCAN];
  public static ReadOnlyMemory<byte> EPORT => EventCodeAndNames[SkStackEventCode.EPORT];
  public static ReadOnlyMemory<byte> EVENT => EventCodeAndNames[SkStackEventCode.EVENT];

  public static bool TryGetEventName(SkStackEventCode eventCode, out ReadOnlyMemory<byte> eventName)
    => EventCodeAndNames.TryGetValue(eventCode, out eventName);

  public static bool TryGetEventCode(ReadOnlySpan<byte> eventName, out SkStackEventCode eventCode)
  {
    eventCode = default;

    foreach (var (code, name) in EventCodeAndNames) {
      if (name.Span.SequenceEqual(eventName)) {
        eventCode = code;
        return true;
      }
    }

    return false;
  }
}
