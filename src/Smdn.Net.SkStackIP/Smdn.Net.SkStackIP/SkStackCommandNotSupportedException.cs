// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1032

using System;

namespace Smdn.Net.SkStackIP;

/// <summary>Represents error code ER04.</summary>
/// <remarks>reference: BP35A1コマンドリファレンス 7. エラーコード</remarks>
public class SkStackCommandNotSupportedException : SkStackErrorResponseException {
  internal SkStackCommandNotSupportedException(
    SkStackResponse response,
    SkStackErrorCode errorCode,
    ReadOnlySpan<byte> errorText,
    string message
  )
    : base(
      response: response,
      errorCode: errorCode,
      errorText: errorText,
      message: message
    )
  {
  }
}
