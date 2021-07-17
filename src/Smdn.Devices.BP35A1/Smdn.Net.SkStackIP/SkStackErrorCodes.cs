// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

namespace Smdn.Net.SkStackIP {
  /// <summary>BP35A1コマンドリファレンス 7. エラーコード</summary>
  public static class SkStackErrorCodes {
    public static ReadOnlyMemory<byte> ER01 { get; } = SkStack.ToByteSequence("ER01");
    public static ReadOnlyMemory<byte> ER02 { get; } = SkStack.ToByteSequence("ER02");
    public static ReadOnlyMemory<byte> ER03 { get; } = SkStack.ToByteSequence("ER03");
    public static ReadOnlyMemory<byte> ER04 { get; } = SkStack.ToByteSequence("ER04");
    public static ReadOnlyMemory<byte> ER05 { get; } = SkStack.ToByteSequence("ER05");
    public static ReadOnlyMemory<byte> ER06 { get; } = SkStack.ToByteSequence("ER06");
    public static ReadOnlyMemory<byte> ER07 { get; } = SkStack.ToByteSequence("ER07");
    public static ReadOnlyMemory<byte> ER08 { get; } = SkStack.ToByteSequence("ER08");
    public static ReadOnlyMemory<byte> ER09 { get; } = SkStack.ToByteSequence("ER09");
    public static ReadOnlyMemory<byte> ER10 { get; } = SkStack.ToByteSequence("ER10");
  }
}