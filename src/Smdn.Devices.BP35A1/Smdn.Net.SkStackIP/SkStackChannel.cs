// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Text;

namespace Smdn.Net.SkStackIP {
  public readonly struct SkStackChannel : IEquatable<SkStackChannel> {
    public static readonly SkStackChannel Channel33 = new(channelNumber: 33, frequencyMHz: 922.5m);
    public static readonly SkStackChannel Channel34 = new(channelNumber: 34, frequencyMHz: 922.7m);
    public static readonly SkStackChannel Channel35 = new(channelNumber: 35, frequencyMHz: 922.9m);
    public static readonly SkStackChannel Channel36 = new(channelNumber: 36, frequencyMHz: 923.1m);
    public static readonly SkStackChannel Channel37 = new(channelNumber: 37, frequencyMHz: 923.3m);
    public static readonly SkStackChannel Channel38 = new(channelNumber: 38, frequencyMHz: 923.5m);
    public static readonly SkStackChannel Channel39 = new(channelNumber: 39, frequencyMHz: 923.7m);
    public static readonly SkStackChannel Channel40 = new(channelNumber: 40, frequencyMHz: 923.9m);
    public static readonly SkStackChannel Channel41 = new(channelNumber: 41, frequencyMHz: 924.1m);
    public static readonly SkStackChannel Channel42 = new(channelNumber: 42, frequencyMHz: 924.3m);
    public static readonly SkStackChannel Channel43 = new(channelNumber: 43, frequencyMHz: 924.5m);
    public static readonly SkStackChannel Channel44 = new(channelNumber: 44, frequencyMHz: 924.7m);
    public static readonly SkStackChannel Channel45 = new(channelNumber: 45, frequencyMHz: 924.9m);
    public static readonly SkStackChannel Channel46 = new(channelNumber: 46, frequencyMHz: 925.1m);
    public static readonly SkStackChannel Channel47 = new(channelNumber: 47, frequencyMHz: 925.3m);
    public static readonly SkStackChannel Channel48 = new(channelNumber: 48, frequencyMHz: 925.5m);
    public static readonly SkStackChannel Channel49 = new(channelNumber: 49, frequencyMHz: 925.7m);
    public static readonly SkStackChannel Channel50 = new(channelNumber: 50, frequencyMHz: 925.9m);
    public static readonly SkStackChannel Channel51 = new(channelNumber: 51, frequencyMHz: 926.1m);
    public static readonly SkStackChannel Channel52 = new(channelNumber: 52, frequencyMHz: 926.3m);
    public static readonly SkStackChannel Channel53 = new(channelNumber: 53, frequencyMHz: 926.5m);
    public static readonly SkStackChannel Channel54 = new(channelNumber: 54, frequencyMHz: 926.7m);
    public static readonly SkStackChannel Channel55 = new(channelNumber: 55, frequencyMHz: 926.9m);
    public static readonly SkStackChannel Channel56 = new(channelNumber: 56, frequencyMHz: 927.1m);
    public static readonly SkStackChannel Channel57 = new(channelNumber: 57, frequencyMHz: 927.3m);
    public static readonly SkStackChannel Channel58 = new(channelNumber: 58, frequencyMHz: 927.5m);
    public static readonly SkStackChannel Channel59 = new(channelNumber: 59, frequencyMHz: 927.7m);
    public static readonly SkStackChannel Channel60 = new(channelNumber: 60, frequencyMHz: 927.9m);

    internal static SkStackChannel FindByRegisterS02Value(byte registerValue)
      => FindByChannelNumber((int)registerValue);

    internal static SkStackChannel FindByChannelNumber(int channelNumber)
      => channelNumber switch {
        33 => Channel33,
        34 => Channel34,
        35 => Channel35,
        36 => Channel36,
        37 => Channel37,
        38 => Channel38,
        39 => Channel39,
        40 => Channel40,
        41 => Channel41,
        42 => Channel42,
        43 => Channel43,
        44 => Channel44,
        45 => Channel45,
        46 => Channel46,
        47 => Channel47,
        48 => Channel48,
        49 => Channel49,
        50 => Channel50,
        51 => Channel51,
        52 => Channel52,
        53 => Channel53,
        54 => Channel54,
        55 => Channel55,
        56 => Channel56,
        57 => Channel57,
        58 => Channel58,
        59 => Channel59,
        60 => Channel60,
        _ => throw new ArgumentOutOfRangeException(nameof(channelNumber), channelNumber, "undefined channel"),
      };

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

    public override int GetHashCode()
      => ChannelNumber.GetHashCode();

    public override string ToString()
      => $"Channel #{ChannelNumber} ({FrequencyMHz} MHz)";
  }
}