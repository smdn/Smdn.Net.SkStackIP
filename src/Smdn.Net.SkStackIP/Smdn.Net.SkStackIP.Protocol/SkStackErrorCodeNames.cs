// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

namespace Smdn.Net.SkStackIP.Protocol;

/// <remarks>
///   <para>See 'BP35A1コマンドリファレンス 7. エラーコード' for detailed specifications.</para>
/// </remarks>
internal static class SkStackErrorCodeNames {
  public static SkStackErrorCode ParseErrorCode(ReadOnlySpan<byte> errorCodeName)
  {
    if (errorCodeName.Length != 4)
      return SkStackErrorCode.Undefined;

    if (errorCodeName[0] != (byte)'E')
      return SkStackErrorCode.Undefined;

    if (errorCodeName[1] != (byte)'R')
      return SkStackErrorCode.Undefined;

    if (errorCodeName[2] == (byte)'0') {
      // ER01-ER09
      return errorCodeName[3] switch {
        (byte)'1' => SkStackErrorCode.ER01,
        (byte)'2' => SkStackErrorCode.ER02,
        (byte)'3' => SkStackErrorCode.ER03,
        (byte)'4' => SkStackErrorCode.ER04,
        (byte)'5' => SkStackErrorCode.ER05,
        (byte)'6' => SkStackErrorCode.ER06,
        (byte)'7' => SkStackErrorCode.ER07,
        (byte)'8' => SkStackErrorCode.ER08,
        (byte)'9' => SkStackErrorCode.ER09,
        _ => SkStackErrorCode.Undefined,
      };
    }
    else if (errorCodeName[2] == (byte)'1' && errorCodeName[3] == (byte)'0') {
      // ER10
      return SkStackErrorCode.ER10;
    }

    return SkStackErrorCode.Undefined;
  }
}
