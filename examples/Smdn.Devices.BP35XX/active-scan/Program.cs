// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

// この例では、アクティブスキャンを実行することにより、
// ネットワーク内にあるPANA認証エージェント(PAA=スマートメーター)を探索します
using System;
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

try {
  // アクティブスキャンを実行します
  // この例では、スキャン間隔7(1チャンネルあたり約1.2秒)のスキャンを最大3回まで試行します
  // スキャン動作は、SkStackActiveScanOptionsによってカスタマイズすることができます
  var scanResult = await bp35a1.ActiveScanAsync(
    rbid: Encoding.ASCII.GetBytes(RouteBID).AsMemory(),
    password: Encoding.ASCII.GetBytes(RouteBPassword).AsMemory(),
    scanOptions: SkStackActiveScanOptions.Create([ 7, 7, 7 ])
  );

  // スキャン結果を表示します
  for (var i = 0; i < scanResult.Count; i++) {
    Console.WriteLine($"[{i}] {scanResult[i]}");
  }
}
catch (Exception ex) {
  Console.Error.WriteLine("スキャン中にエラーが発生しました");
  Console.Error.WriteLine(ex);
}
