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
  [CLSCompliant(false)] public uint PairingId { get; }

  internal SkStackPanDescription(
    SkStackChannel channel,
    int channelPage,
    int id,
    PhysicalAddress macAddress,
    double rssi,
    uint pairingId
  )
  {
    Channel = channel;
    ChannelPage = channelPage;
    Id = id;
    MacAddress = macAddress;
    RSSI = rssi;
    PairingId = pairingId;
  }

  public override string ToString()
    => $"{Channel}, Channel page: {ChannelPage}, PAN ID: 0x{Id:X4}, MAC address: {MacAddress}, Pairing ID: {PairingId:X8}, RSSI: {RSSI:N1}";
}
