// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1815 // TODO: implement equality comparison

using System;
using System.Net.NetworkInformation;

namespace Smdn.Net.SkStackIP;

public readonly struct SkStackPanDescription {
  public SkStackChannel Channel { get; }
  public int ChannelPage { get; }
  public int Id { get; }
  public PhysicalAddress MacAddress { get; }
  public decimal Rssi { get; }
  [CLSCompliant(false)] public uint PairingId { get; }

  internal SkStackPanDescription(
    SkStackChannel channel,
    int channelPage,
    int id,
    PhysicalAddress macAddress,
    decimal rssi,
    uint pairingId
  )
  {
    Channel = channel;
    ChannelPage = channelPage;
    Id = id;
    MacAddress = macAddress;
    Rssi = rssi;
    PairingId = pairingId;
  }

  public override string ToString()
    => $"{Channel}, Channel page: {ChannelPage}, PAN ID: 0x{Id:X4}, MAC address: {MacAddress}, Pairing ID: 0x{PairingId:X8}, RSSI: {Rssi:N2} dB";
}
