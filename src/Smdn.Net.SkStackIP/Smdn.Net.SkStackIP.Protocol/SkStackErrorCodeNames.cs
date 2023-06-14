// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;

namespace Smdn.Net.SkStackIP.Protocol;

/// <remarks>
///   <para>See 'BP35A1コマンドリファレンス 7. エラーコード' for detailed specifications.</para>
/// </remarks>
internal static class SkStackErrorCodeNames {
  private static readonly IReadOnlyDictionary<SkStackErrorCode, ReadOnlyMemory<byte>> errorCodeAndNames =
    new Dictionary<SkStackErrorCode, ReadOnlyMemory<byte>>() {
      { SkStackErrorCode.ER01, SkStack.ToByteSequence(nameof(SkStackErrorCode.ER01)) },
      { SkStackErrorCode.ER02, SkStack.ToByteSequence(nameof(SkStackErrorCode.ER02)) },
      { SkStackErrorCode.ER03, SkStack.ToByteSequence(nameof(SkStackErrorCode.ER03)) },
      { SkStackErrorCode.ER04, SkStack.ToByteSequence(nameof(SkStackErrorCode.ER04)) },
      { SkStackErrorCode.ER05, SkStack.ToByteSequence(nameof(SkStackErrorCode.ER05)) },
      { SkStackErrorCode.ER06, SkStack.ToByteSequence(nameof(SkStackErrorCode.ER06)) },
      { SkStackErrorCode.ER07, SkStack.ToByteSequence(nameof(SkStackErrorCode.ER07)) },
      { SkStackErrorCode.ER08, SkStack.ToByteSequence(nameof(SkStackErrorCode.ER08)) },
      { SkStackErrorCode.ER09, SkStack.ToByteSequence(nameof(SkStackErrorCode.ER09)) },
      { SkStackErrorCode.ER10, SkStack.ToByteSequence(nameof(SkStackErrorCode.ER10)) },
    };

  public static SkStackErrorCode ParseErrorCode(ReadOnlySpan<byte> errorCodeName)
  {
    foreach (var (code, name) in errorCodeAndNames) {
      if (name.Span.SequenceEqual(errorCodeName))
        return code;
    }

    return SkStackErrorCode.Undefined;
  }
}
