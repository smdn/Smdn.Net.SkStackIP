// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

// この例では、PANA認証エージェント(PAA=スマートメーター)に対して認証を要求し、
// PANAセッションの確立を試みます
using System;
using System.Net;
using System.Text;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Smdn.Devices.BP35XX;
using Smdn.Net.SkStackIP;

var services = new ServiceCollection();

services.AddLogging(
  builder => builder
    .AddSimpleConsole(static options => options.SingleLine = true)
    .AddFilter(static level => LogLevel.Debug <= level)
);

// BルートID(ハイフン・スペースなし)を指定してください
const string RouteBID = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";

// Bルートパスワードを指定してください
const string RouteBPassword = "XXXXXXXXXXXX";

// BP35A1が接続されているシリアルポートを指定してください
// Windowsでは`COM1`、Linux等では`/dev/ttyACM0`, `/dev/ttyUSB0`といった名前でデバイスを指定してください
const string SerialPort = "/dev/ttyACM0";

using var bp35a1 = await BP35A1.CreateAsync(
  serialPortName: SerialPort,
  serviceProvider: services.BuildServiceProvider()
);

// PANAクライアントとして認証を開始します
// PANA認証エージェント(PAA)は、アクティブスキャンによって探索します
var sessionInfo = await bp35a1.AuthenticateAsPanaClientAsync(
  rbid: Encoding.ASCII.GetBytes(RouteBID).AsMemory(),
  password: Encoding.ASCII.GetBytes(RouteBPassword).AsMemory(),
  scanOptions: SkStackActiveScanOptions.ScanUntilFind, // 何らかのPAAが見つかるまでスキャンを続ける
  cancellationToken: default
);

// PAAのIPアドレスもしくはMACアドレス、およびチャンネル・PAN IDが既知の場合は、
// アクティブスキャンを省略して指定したPAAに対して即座に認証を要求することもできます
//
// PAAのIPアドレスが既知の場合
// var sessionInfo = await bp35a1.AuthenticateAsPanaClientAsync(
//   rbid: Encoding.ASCII.GetBytes(RouteBID).AsMemory(),
//   password: Encoding.ASCII.GetBytes(RouteBPassword).AsMemory(),
//   paaAddress: IPAddress.Parse("2001:0DB8:0000:0000:0000:0000:0000:0001"),
//   channel: SkStackChannel.Channels[42],
//   panId: 0x1234,
//   cancellationToken: default
// );
//
// PAAのMACアドレスが既知の場合
// var sessionInfo = await bp35a1.AuthenticateAsPanaClientAsync(
//   rbid: Encoding.ASCII.GetBytes(RouteBID).AsMemory(),
//   password: Encoding.ASCII.GetBytes(RouteBPassword).AsMemory(),
//   paaMacAddress: System.Net.NetworkInformation.PhysicalAddress.Parse("00005EEF10000000"),
//   channel: SkStackChannel.Channels[42],
//   panId: 0x1234,
//   cancellationToken: default
// );

Console.WriteLine("PANA session established");
Console.WriteLine($"  {nameof(bp35a1.PanaSessionPeerAddress)}: {bp35a1.PanaSessionPeerAddress}");
Console.WriteLine($"  {nameof(bp35a1.IsPanaSessionAlive)}: {bp35a1.IsPanaSessionAlive}");

Console.WriteLine("PANA session info");
Console.WriteLine($"    {nameof(sessionInfo.Channel)}: {sessionInfo.Channel}");
Console.WriteLine($"    {nameof(sessionInfo.PanId)}: 0x{sessionInfo.PanId:X4}");
Console.WriteLine($"    {nameof(sessionInfo.PeerAddress)}: {sessionInfo.PeerAddress}");
Console.WriteLine($"    {nameof(sessionInfo.PeerMacAddress)}: {sessionInfo.PeerMacAddress}");
Console.WriteLine($"    {nameof(sessionInfo.LocalAddress)}: {sessionInfo.LocalAddress}");
Console.WriteLine($"    {nameof(sessionInfo.LocalMacAddress)}: {sessionInfo.LocalMacAddress}");

await bp35a1.TerminatePanaSessionAsync();

Console.WriteLine("PANA session terminated");
Console.WriteLine($"  {nameof(bp35a1.PanaSessionPeerAddress)}: {bp35a1.PanaSessionPeerAddress}");
Console.WriteLine($"  {nameof(bp35a1.IsPanaSessionAlive)}: {bp35a1.IsPanaSessionAlive}");
