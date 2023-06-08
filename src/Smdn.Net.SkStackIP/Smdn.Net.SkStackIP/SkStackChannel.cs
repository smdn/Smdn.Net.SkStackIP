// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Text;

namespace Smdn.Net.SkStackIP;

/// <remarks>reference: BP35A1コマンドリファレンス 6. 周波数とチャネル番号</remarks>
public readonly struct SkStackChannel : IEquatable<SkStackChannel>, IComparable<SkStackChannel> {
  public static readonly IReadOnlyDictionary<int, SkStackChannel> Channels = new Dictionary<int, SkStackChannel> {
    { 33, new(channelNumber: 33, frequencyMHz: 922.5m) },
    { 34, new(channelNumber: 34, frequencyMHz: 922.7m) },
    { 35, new(channelNumber: 35, frequencyMHz: 922.9m) },
    { 36, new(channelNumber: 36, frequencyMHz: 923.1m) },
    { 37, new(channelNumber: 37, frequencyMHz: 923.3m) },
    { 38, new(channelNumber: 38, frequencyMHz: 923.5m) },
    { 39, new(channelNumber: 39, frequencyMHz: 923.7m) },
    { 40, new(channelNumber: 40, frequencyMHz: 923.9m) },
    { 41, new(channelNumber: 41, frequencyMHz: 924.1m) },
    { 42, new(channelNumber: 42, frequencyMHz: 924.3m) },
    { 43, new(channelNumber: 43, frequencyMHz: 924.5m) },
    { 44, new(channelNumber: 44, frequencyMHz: 924.7m) },
    { 45, new(channelNumber: 45, frequencyMHz: 924.9m) },
    { 46, new(channelNumber: 46, frequencyMHz: 925.1m) },
    { 47, new(channelNumber: 47, frequencyMHz: 925.3m) },
    { 48, new(channelNumber: 48, frequencyMHz: 925.5m) },
    { 49, new(channelNumber: 49, frequencyMHz: 925.7m) },
    { 50, new(channelNumber: 50, frequencyMHz: 925.9m) },
    { 51, new(channelNumber: 51, frequencyMHz: 926.1m) },
    { 52, new(channelNumber: 52, frequencyMHz: 926.3m) },
    { 53, new(channelNumber: 53, frequencyMHz: 926.5m) },
    { 54, new(channelNumber: 54, frequencyMHz: 926.7m) },
    { 55, new(channelNumber: 55, frequencyMHz: 926.9m) },
    { 56, new(channelNumber: 56, frequencyMHz: 927.1m) },
    { 57, new(channelNumber: 57, frequencyMHz: 927.3m) },
    { 58, new(channelNumber: 58, frequencyMHz: 927.5m) },
    { 59, new(channelNumber: 59, frequencyMHz: 927.7m) },
    { 60, new(channelNumber: 60, frequencyMHz: 927.9m) },
  };

  public static SkStackChannel Channel33 => Channels[33];
  public static SkStackChannel Channel34 => Channels[34];
  public static SkStackChannel Channel35 => Channels[35];
  public static SkStackChannel Channel36 => Channels[36];
  public static SkStackChannel Channel37 => Channels[37];
  public static SkStackChannel Channel38 => Channels[38];
  public static SkStackChannel Channel39 => Channels[39];
  public static SkStackChannel Channel40 => Channels[40];
  public static SkStackChannel Channel41 => Channels[41];
  public static SkStackChannel Channel42 => Channels[42];
  public static SkStackChannel Channel43 => Channels[43];
  public static SkStackChannel Channel44 => Channels[44];
  public static SkStackChannel Channel45 => Channels[45];
  public static SkStackChannel Channel46 => Channels[46];
  public static SkStackChannel Channel47 => Channels[47];
  public static SkStackChannel Channel48 => Channels[48];
  public static SkStackChannel Channel49 => Channels[49];
  public static SkStackChannel Channel50 => Channels[50];
  public static SkStackChannel Channel51 => Channels[51];
  public static SkStackChannel Channel52 => Channels[52];
  public static SkStackChannel Channel53 => Channels[53];
  public static SkStackChannel Channel54 => Channels[54];
  public static SkStackChannel Channel55 => Channels[55];
  public static SkStackChannel Channel56 => Channels[56];
  public static SkStackChannel Channel57 => Channels[57];
  public static SkStackChannel Channel58 => Channels[58];
  public static SkStackChannel Channel59 => Channels[59];
  public static SkStackChannel Channel60 => Channels[60];

  internal static SkStackChannel FindByRegisterS02Value(byte registerValue)
    => FindByChannelNumber((int)registerValue);

  internal static SkStackChannel FindByChannelNumber(int channelNumber)
  {
    if (Channels.TryGetValue(channelNumber, out var channel))
      return channel;

    throw new ArgumentOutOfRangeException(nameof(channelNumber), channelNumber, "undefined channel");
  }

  /*
   * instance members
   */
  public int ChannelNumber { get; }
  public decimal FrequencyMHz { get; }
  internal byte RegisterS02Value => (byte)ChannelNumber;

  private SkStackChannel(int channelNumber, decimal frequencyMHz)
  {
    this.ChannelNumber = channelNumber;
    this.FrequencyMHz = frequencyMHz;
  }

  public override bool Equals(object obj)
  {
    if (obj is SkStackChannel channel)
      return Equals(channel);

    return false;
  }

  public bool Equals(SkStackChannel other)
    => this.ChannelNumber == other.ChannelNumber;

  int IComparable<SkStackChannel>.CompareTo(SkStackChannel other)
    => this.ChannelNumber.CompareTo(other.ChannelNumber);

  public override int GetHashCode()
    => ChannelNumber.GetHashCode();

  public override string ToString()
    => $"{ChannelNumber}ch ({nameof(SkStackRegister.S02)}=0x{ChannelNumber:X2}, {FrequencyMHz} MHz)";
}
