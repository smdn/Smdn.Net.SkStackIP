// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1032

using System;

namespace Smdn.Net.SkStackIP;

/// <summary>Describes the error code <c>ER10</c> of <c>SKSAVE</c> or <c>SKLOAD</c> response.</summary>
/// <remarks>
///   <para>See below for detailed specifications.</para>
///   <list type="bullet">
///     <item><description>'BP35A1コマンドリファレンス 3.20. SKSAVE'</description></item>
///     <item><description>'BP35A1コマンドリファレンス 3.21. SKLOAD'</description></item>
///     <item><description>'BP35A1コマンドリファレンス 7. エラーコード'</description></item>
///   </list>
/// </remarks>
/// <seealso cref="SkStackClient.SendSKSAVEAsync"/>
/// <seealso cref="SkStackClient.SendSKLOADAsync"/>
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
