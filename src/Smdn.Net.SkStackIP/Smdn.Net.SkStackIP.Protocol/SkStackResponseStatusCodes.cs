// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

namespace Smdn.Net.SkStackIP.Protocol;

internal static class SkStackResponseStatusCodes {
  public static ReadOnlySpan<byte> OK => "OK"u8;

  public static ReadOnlySpan<byte> FAIL => "FAIL"u8;
}
