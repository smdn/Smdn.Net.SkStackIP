// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

namespace Smdn.Net.SkStackIP.Protocol;

internal static class SkStackEventCodeNames {
  public static ReadOnlySpan<byte> ERXUDP => "ERXUDP"u8;
  public static ReadOnlySpan<byte> EPONG => "EPONG"u8;
  public static ReadOnlySpan<byte> EADDR => "EADDR"u8;
  public static ReadOnlySpan<byte> ENEIGHBOR => "ENEIGHBOR"u8;
  public static ReadOnlySpan<byte> EPANDESC => "EPANDESC"u8;
  public static ReadOnlySpan<byte> EEDSCAN => "EEDSCAN"u8;
  public static ReadOnlySpan<byte> EPORT => "EPORT"u8;
  public static ReadOnlySpan<byte> EVENT => "EVENT"u8;
}
