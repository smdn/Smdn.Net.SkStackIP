// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

namespace Smdn.Net.SkStackIP;

/// <remarks>reference: BP35A1コマンドリファレンス 3.1. SKSREG</remarks>
public static partial class SkStackRegister {
  private static readonly (bool isReadable, bool isWritable) RW = (isReadable: true, isWritable: true);
  private static readonly (bool isReadable, bool isWritable) R = (isReadable: true, isWritable: false);

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
  /// <summary>Register number S02</summary>
  public static RegisterEntry<SkStackChannel> Channel => S02;

  /// <summary>Register number S03</summary>
  [CLSCompliant(false)] public static RegisterEntry<ushort> PanID => S03;

  /// <summary>Register number S07</summary>
  [CLSCompliant(false)] public static RegisterEntry<uint> FrameCounter => S07;

  /// <summary>Register number S0A</summary>
  public static RegisterEntry<ReadOnlyMemory<byte>> PairingID => S0A;

  /// <summary>Register number S15</summary>
  public static RegisterEntry<bool> RespondBeaconRequest => S15;

  /// <summary>Register number S16</summary>
  [CLSCompliant(false)] public static RegisterEntry<TimeSpan> PanaSessionLifetimeInSeconds => S16;

  /// <summary>Register number S17</summary>
  public static RegisterEntry<bool> EnableAutoReauthentication => S17;

  /// <summary>Register number SA0</summary>
  public static RegisterEntry<bool> EncryptIPMulticast => SA0;

  /// <summary>Register number SA1</summary>
  public static RegisterEntry<bool> AcceptIcmpEcho => SA1;

  /// <summary>Register number SFB</summary>
  public static RegisterEntry<bool> IsSendingRestricted => SFB;

  /// <summary>Register number SFD</summary>
  [CLSCompliant(false)] public static RegisterEntry<ulong> AccumulatedSendTimeInMilliseconds => SFD;

  /// <summary>Register number SFE</summary>
  public static RegisterEntry<bool> EnableEchoback => SFE;

  /// <summary>Register number SFF</summary>
  public static RegisterEntry<bool> EnableAutoLoad => SFF;
 }
