// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Text;

namespace Smdn.Devices.BP35XX;

/// <remarks>
///   <para>See 'BP35A1コマンドリファレンス 3. コマンドリファレンス' for detailed specifications.</para>
/// </remarks>
internal class BP35CommandNames {
  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.30. WOPT (プロダクト設定コマンド)' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> WOPT { get; } = Encoding.ASCII.GetBytes(nameof(WOPT));

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.31. ROPT (プロダクト設定コマンド)' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> ROPT { get; } = Encoding.ASCII.GetBytes(nameof(ROPT));

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.32. WUART (プロダクト設定コマンド)' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> WUART { get; } = Encoding.ASCII.GetBytes(nameof(WUART));

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 3.33. RUART (プロダクト設定コマンド)' for detailed specifications.</para>
  /// </remarks>
  public static ReadOnlyMemory<byte> RUART { get; } = Encoding.ASCII.GetBytes(nameof(RUART));
}
