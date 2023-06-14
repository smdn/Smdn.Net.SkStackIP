// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

namespace Smdn.Net.SkStackIP;

/// <remarks>
///   <para>See 'BP35A1コマンドリファレンス 3.30. WOPT (プロダクト設定コマンド)' for detailed specifications.</para>
/// </remarks>
public enum SkStackERXUDPDataFormat {
  /// <summary>Use raw binary format.</summary>
  Raw,

  /// <summary>Use hexadecimal text format.</summary>
  HexAsciiText,
}
