// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;

namespace Smdn.Net.SkStackIP.Protocol;

internal static class SkStackEventCodeNames {
#pragma warning disable CA1859
  private static readonly IReadOnlyDictionary<SkStackEventCode, ReadOnlyMemory<byte>> EventCodeAndNames =
#pragma warning restore CA1859
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

  public static ReadOnlySpan<byte> ERXUDP => EventCodeAndNames[SkStackEventCode.ERXUDP].Span;
  public static ReadOnlySpan<byte> EPONG => EventCodeAndNames[SkStackEventCode.EPONG].Span;
  public static ReadOnlySpan<byte> EADDR => EventCodeAndNames[SkStackEventCode.EADDR].Span;
  public static ReadOnlySpan<byte> ENEIGHBOR => EventCodeAndNames[SkStackEventCode.ENEIGHBOR].Span;
  public static ReadOnlySpan<byte> EPANDESC => EventCodeAndNames[SkStackEventCode.EPANDESC].Span;
  public static ReadOnlySpan<byte> EEDSCAN => EventCodeAndNames[SkStackEventCode.EEDSCAN].Span;
  public static ReadOnlySpan<byte> EPORT => EventCodeAndNames[SkStackEventCode.EPORT].Span;
  public static ReadOnlySpan<byte> EVENT => EventCodeAndNames[SkStackEventCode.EVENT].Span;
}
