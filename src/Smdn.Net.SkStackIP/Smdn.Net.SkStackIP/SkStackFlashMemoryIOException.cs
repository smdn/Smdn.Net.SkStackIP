// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

namespace Smdn.Net.SkStackIP;

/// <summary>Represents error code ER10 of SKSAVE or SKLOAD response.</summary>
/// <remarks>
/// reference: BP35A1コマンドリファレンス 3.20. SKSAVE
/// reference: BP35A1コマンドリファレンス 3.21. SKLOAD
/// reference: BP35A1コマンドリファレンス 7. エラーコード
/// </remarks>
public class SkStackFlashMemoryIOException : SkStackErrorResponseException {
  internal SkStackFlashMemoryIOException(
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
