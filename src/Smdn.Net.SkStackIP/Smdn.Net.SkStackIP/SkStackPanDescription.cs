// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Net.NetworkInformation;

namespace Smdn.Net.SkStackIP;

public readonly struct SkStackPanDescription {
  public SkStackChannel Channel { get; }
  public int ChannelPage { get; }
  public int Id { get; }
  public PhysicalAddress MacAddress { get; }
  public double RSSI { get; }
  [CLSCompliant(false)] public uint PairingID { get; }

  internal SkStackPanDescription(
    SkStackChannel channel,
    int channelPage,
    int id,
    PhysicalAddress macAddress,
    double rssi,
    uint pairingId
  )
  {
    this.Channel = channel;
    this.ChannelPage = channelPage;
    this.Id = id;
    this.MacAddress = macAddress;
    this.RSSI = rssi;
    this.PairingID = pairingId;
  }

  public override string ToString()
    => $"{Channel}, Channel page: {ChannelPage}, PAN ID: 0x{Id:X4}, MAC address: {MacAddress}, Pairing ID: {PairingID:X8}, RSSI: {RSSI:N1}";
}
