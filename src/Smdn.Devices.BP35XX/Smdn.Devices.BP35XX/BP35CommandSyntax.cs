// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

using Smdn.Net.SkStackIP.Protocol;

namespace Smdn.Devices.BP35XX;

/// <remarks>
///   <para>See below for detailed specifications.</para>
///   <list type="bullet">
///     <item><description>'BP35A1コマンドリファレンス 3.30. WOPT (プロダクト設定コマンド)'</description></item>
///     <item><description>'BP35A1コマンドリファレンス 3.31. ROPT (プロダクト設定コマンド)'</description></item>
///     <item><description>'BP35A1コマンドリファレンス 3.32. WUART (プロダクト設定コマンド)'</description></item>
///     <item><description>'BP35A1コマンドリファレンス 3.33. RUART (プロダクト設定コマンド)'</description></item>
///   </list>
/// </remarks>
/// <seealso cref="SkStackProtocolSyntax"/>
internal sealed class BP35CommandSyntax : SkStackProtocolSyntax {
  /// <summary>
  /// Gets the newline character used in the product configuration commands (プロダクト設定コマンド).
  /// Only <c>CR</c> is used as a newline character in the product configuration command and its response.
  /// </summary>
  public override ReadOnlySpan<byte> EndOfCommandLine => "\r"u8;
  public override bool ExpectStatusLine => true;
  public override ReadOnlySpan<byte> EndOfStatusLine => "\r"u8;
}
