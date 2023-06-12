// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

namespace Smdn.Net.SkStackIP.Protocol;

/// <remarks>reference: BP35A1コマンドリファレンス 4. イベント</remarks>
internal enum SkStackEventCode {
  Undefined = 0,

  /// <remarks>reference: BP35A1コマンドリファレンス 4.1. ERXUDP</remarks>
  ERXUDP = 0x41,

  /// <remarks>reference: BP35A1コマンドリファレンス 4.2. EPONG</remarks>
  EPONG = 0x42,

  /// <remarks>reference: BP35A1コマンドリファレンス 4.3. EADDR</remarks>
  EADDR = 0x43,

  /// <remarks>reference: BP35A1コマンドリファレンス 4.4. ENEIGHBOR</remarks>
  ENEIGHBOR = 0x44,

  /// <remarks>reference: BP35A1コマンドリファレンス 4.5. EPANDESC</remarks>
  EPANDESC = 0x45,

  /// <remarks>reference: BP35A1コマンドリファレンス 4.6. EEDSCAN</remarks>
  EEDSCAN = 0x46,

  /// <remarks>reference: BP35A1コマンドリファレンス 4.7. EPORT</remarks>
  EPORT = 0x47,

  /// <remarks>reference: BP35A1コマンドリファレンス 4.8. EVENT</remarks>
  EVENT = 0x48,
}
