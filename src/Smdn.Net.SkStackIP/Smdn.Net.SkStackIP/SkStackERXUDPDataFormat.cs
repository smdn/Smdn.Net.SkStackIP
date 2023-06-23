// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

namespace Smdn.Net.SkStackIP;

/// <remarks>
///   <para>See 'BP35A1コマンドリファレンス 3.30. WOPT (プロダクト設定コマンド)' for detailed specifications.</para>
/// </remarks>
public enum SkStackERXUDPDataFormat {
  /// <summary>The data part of <c>ERXUDP</c> is displayed in binary format.</summary>
  Binary = 0,

  /// <summary>The data part of <c>ERXUDP</c> is displayed in hex ASCII format.</summary>
  HexAsciiText = 1,
}
