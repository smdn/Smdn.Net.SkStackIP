// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

namespace Smdn.Net.SkStackIP {
  /// <summary>BP35A1コマンドリファレンス 4. イベント</summary>
  public static class SkStackEventCodes {
    /// <summary>BP35A1コマンドリファレンス 4.1. ERXUDP</summary>
    public static ReadOnlyMemory<byte> ERXUDP { get; } = SkStack.ToByteSequence("ERXUDP");

    /// <summary>BP35A1コマンドリファレンス 4.2. EPONG</summary>
    public static ReadOnlyMemory<byte> EPONG { get; } = SkStack.ToByteSequence("EPONG");

    /// <summary>BP35A1コマンドリファレンス 4.3. EADDR</summary>
    public static ReadOnlyMemory<byte> EADDR { get; } = SkStack.ToByteSequence("EADDR");

    /// <summary>BP35A1コマンドリファレンス 4.4. ENEIGHBOR</summary>
    public static ReadOnlyMemory<byte> ENEIGHBOR { get; } = SkStack.ToByteSequence("ENEIGHBOR");

    /// <summary>BP35A1コマンドリファレンス 4.5. EPANDESC</summary>
    public static ReadOnlyMemory<byte> EPANDESC { get; } = SkStack.ToByteSequence("EPANDESC");

    /// <summary>BP35A1コマンドリファレンス 4.6. EEDSCAN</summary>
    public static ReadOnlyMemory<byte> EEDSCAN { get; } = SkStack.ToByteSequence("EEDSCAN");

    /// <summary>BP35A1コマンドリファレンス 4.7. EPORT</summary>
    public static ReadOnlyMemory<byte> EPORT { get; } = SkStack.ToByteSequence("EPORT");

    /// <summary>BP35A1コマンドリファレンス 4.8. EVENT</summary>
    public static ReadOnlyMemory<byte> EVENT { get; } = SkStack.ToByteSequence("EVENT");
  }
}