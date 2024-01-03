﻿// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.IO.Ports;
using System.Text;

using Smdn.Net.SkStackIP;

using var port = new SerialPort(
  portName: "/dev/ttyACM0",
  baudRate: 115200,
  parity: Parity.None,
  dataBits: 8,
  stopBits: StopBits.One
) {
  Handshake = Handshake.None,
  DtrEnable = false,
  RtsEnable = false,
  NewLine = "\r\n", // CRLF
};

port.Open();
port.DiscardInBuffer();

using var client = new SkStackClient(stream: port.BaseStream);

var respSKVER = await client.SendSKVERAsync();

Console.WriteLine($"VER: {respSKVER.Payload}");

var respSKAPPVER = await client.SendSKAPPVERAsync();

Console.WriteLine($"APPVER: {respSKAPPVER.Payload}");

var respSKINFO = await client.SendSKINFOAsync();

Console.WriteLine($"LinkLocalAddress: {respSKINFO.Payload.LinkLocalAddress}");
Console.WriteLine($"MacAddress: {respSKINFO.Payload.MacAddress}");
Console.WriteLine($"Channel: {respSKINFO.Payload.Channel}");
Console.WriteLine($"PanId: {respSKINFO.Payload.PanId:X4}");
Console.WriteLine();

var respSKSREG_S02 = await client.SendSKSREGAsync(SkStackRegister.S02);

Console.WriteLine($"Register S02: {respSKSREG_S02.Payload}");

var respSKSREG_S0A = await client.SendSKSREGAsync(SkStackRegister.S0A);

Console.WriteLine($"Register S0A: {Encoding.ASCII.GetString(respSKSREG_S0A.Payload.Span)}");

var respSKSREG_S15 = await client.SendSKSREGAsync(SkStackRegister.S15);

Console.WriteLine($"Register S15: {respSKSREG_S15.Payload}");

var respSKSREG_SFE = await client.SendSKSREGAsync(SkStackRegister.SFE);

Console.WriteLine($"Register SFE: {respSKSREG_SFE.Payload}");
