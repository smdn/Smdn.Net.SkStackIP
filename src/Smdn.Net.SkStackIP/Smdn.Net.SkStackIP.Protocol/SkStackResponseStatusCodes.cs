// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

namespace Smdn.Net.SkStackIP.Protocol;

internal static class SkStackResponseStatusCodes {
  private static readonly ReadOnlyMemory<byte> statusOk = SkStack.ToByteSequence("OK");
  public static ReadOnlySpan<byte> OK => statusOk.Span;

  private static readonly ReadOnlyMemory<byte> statusFail = SkStack.ToByteSequence("FAIL");
  public static ReadOnlySpan<byte> FAIL => statusFail.Span;
}
