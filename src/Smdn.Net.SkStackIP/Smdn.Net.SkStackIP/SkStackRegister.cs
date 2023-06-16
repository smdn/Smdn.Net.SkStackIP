// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

namespace Smdn.Net.SkStackIP;

/// <remarks>
///   <para>See 'BP35A1コマンドリファレンス 3.1. SKSREG' for detailed specifications.</para>
/// </remarks>
public static partial class SkStackRegister {
  private static readonly (bool IsReadable, bool IsWritable) RW = (IsReadable: true, IsWritable: true);
  private static readonly (bool IsReadable, bool IsWritable) R = (IsReadable: true, IsWritable: false);

  public static RegisterEntry<SkStackChannel> S02 { get; } = new RegisterChannelEntry(name: nameof(S02), readWrite: RW, valueRange: (minValue: SkStackChannel.Channel33, maxValue: SkStackChannel.Channel60));
  [CLSCompliant(false)]
  public static RegisterEntry<ushort> S03 { get; } = new RegisterUINT16Entry(name: nameof(S03), readWrite: RW, valueRange: (minValue: 0x0000, maxValue: 0xFFFF));
  [CLSCompliant(false)]
  public static RegisterEntry<uint> S07 { get; } = new RegisterUINT32Entry(name: nameof(S07), readWrite: R, valueRange: default);
  public static RegisterEntry<ReadOnlyMemory<byte>> S0A { get; } = new RegisterCHARArrayEntry(name: nameof(S0A), readWrite: RW, minLength: 8, maxLength: 8);
  public static RegisterEntry<bool> S15 { get; } = new RegisterBinaryEntry(name: nameof(S15), readWrite: RW);
  [CLSCompliant(false)]
  public static RegisterEntry<TimeSpan> S16 { get; } = new RegisterUINT32SecondsTimeSpanEntry(name: nameof(S16), readWrite: RW, valueRange: (minValue: 0x_0000_003C, maxValue: 0x_FFFF_FFFF));
  public static RegisterEntry<bool> S17 { get; } = new RegisterBinaryEntry(name: nameof(S17), readWrite: RW);
  public static RegisterEntry<bool> SA0 { get; } = new RegisterBinaryEntry(name: nameof(SA0), readWrite: RW);
  public static RegisterEntry<bool> SA1 { get; } = new RegisterBinaryEntry(name: nameof(SA1), readWrite: RW);
  public static RegisterEntry<bool> SFB { get; } = new RegisterBinaryEntry(name: nameof(SFB), readWrite: R);
  [CLSCompliant(false)]
  public static RegisterEntry<ulong> SFD { get; } = new RegisterUINT64Entry(name: nameof(SFD), readWrite: R, valueRange: default);
  public static RegisterEntry<bool> SFE { get; } = new RegisterBinaryEntry(name: nameof(SFE), readWrite: RW);
  public static RegisterEntry<bool> SFF { get; } = new RegisterBinaryEntry(name: nameof(SFF), readWrite: RW);

  /*
   *  alias of SXX
   */

  /// <remarks>This property is an alias for the register number <see cref="S02"/>.</remarks>
  public static RegisterEntry<SkStackChannel> Channel => S02;

  /// <remarks>This property is an alias for the register number <see cref="S03"/>.</remarks>
  [CLSCompliant(false)] public static RegisterEntry<ushort> PanId => S03;

  /// <remarks>This property is an alias for the register number <see cref="S07"/>.</remarks>
  [CLSCompliant(false)] public static RegisterEntry<uint> FrameCounter => S07;

  /// <remarks>This property is an alias for the register number <see cref="S0A"/>.</remarks>
  public static RegisterEntry<ReadOnlyMemory<byte>> PairingId => S0A;

  /// <remarks>This property is an alias for the register number <see cref="S15"/>.</remarks>
  public static RegisterEntry<bool> RespondBeaconRequest => S15;

  /// <remarks>This property is an alias for the register number <see cref="S16"/>.</remarks>
  [CLSCompliant(false)] public static RegisterEntry<TimeSpan> PanaSessionLifetimeInSeconds => S16;

  /// <remarks>This property is an alias for the register number <see cref="S17"/>.</remarks>
  public static RegisterEntry<bool> EnableAutoReauthentication => S17;

  /// <remarks>This property is an alias for the register number <see cref="SA0"/>.</remarks>
  public static RegisterEntry<bool> EncryptIPMulticast => SA0;

  /// <remarks>This property is an alias for the register number <see cref="SA1"/>.</remarks>
  public static RegisterEntry<bool> AcceptIcmpEcho => SA1;

  /// <remarks>This property is an alias for the register number <see cref="SFB"/>.</remarks>
  public static RegisterEntry<bool> IsSendingRestricted => SFB;

  /// <remarks>This property is an alias for the register number <see cref="SFD"/>.</remarks>
  [CLSCompliant(false)] public static RegisterEntry<ulong> AccumulatedSendTimeInMilliseconds => SFD;

  /// <remarks>This property is an alias for the register number <see cref="SFE"/>.</remarks>
  public static RegisterEntry<bool> EnableEchoback => SFE;

  /// <remarks>This property is an alias for the register number <see cref="SFF"/>.</remarks>
  public static RegisterEntry<bool> EnableAutoLoad => SFF;
}
