// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

namespace Smdn.Net.SkStackIP.Protocol;

/// <remarks>
///   <para>See 'BP35A1コマンドリファレンス 4. イベント' for detailed specifications.</para>
/// </remarks>
internal enum SkStackEventCode {
  Undefined = 0,

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 4.1. ERXUDP' for detailed specifications.</para>
  /// </remarks>
  ERXUDP = 0x41,

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 4.2. EPONG' for detailed specifications.</para>
  /// </remarks>
  EPONG = 0x42,

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 4.3. EADDR' for detailed specifications.</para>
  /// </remarks>
  EADDR = 0x43,

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 4.4. ENEIGHBOR' for detailed specifications.</para>
  /// </remarks>
  ENEIGHBOR = 0x44,

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 4.5. EPANDESC' for detailed specifications.</para>
  /// </remarks>
  EPANDESC = 0x45,

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 4.6. EEDSCAN' for detailed specifications.</para>
  /// </remarks>
  EEDSCAN = 0x46,

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 4.7. EPORT' for detailed specifications.</para>
  /// </remarks>
  EPORT = 0x47,

  /// <remarks>
  ///   <para>See 'BP35A1コマンドリファレンス 4.8. EVENT' for detailed specifications.</para>
  /// </remarks>
  EVENT = 0x48,
}
