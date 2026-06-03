// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

// この例では、特定チャンネルに対するEDスキャンを実行することにより、
// チャンネルごとの使用状況(電波強度)を取得して表示します
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Smdn.Devices.BP35XX;
using Smdn.Net.SkStackIP;

// BP35A1が接続されているシリアルポートを指定してください
// Windowsでは`COM1`、Linux等では`/dev/ttyACM0`, `/dev/ttyUSB0`といった名前でデバイスを指定してください
const string SerialPort = "/dev/ttyACM0";

using var bp35a1 = await BP35A1.CreateAsync(
  new BP35A1Options() {
    SerialPortName = SerialPort, // Specify a port name such as COM1 on Windows
    UseFlowControl = true,
    TryLoadFlashMemory = true, // Try to load configurations stored in flash memory
  }
);

using var cts = new CancellationTokenSource();

// Requests cancellation to the CancellationToken when
// Ctrl+C is pressed.
Console.CancelKeyPress += (sender, e) => {
  cts.Cancel();
  e.Cancel = true;
};

var scanChannels = new[] {
  SkStackChannel.Channel45,
  SkStackChannel.Channel46,
  SkStackChannel.Channel47,
  SkStackChannel.Channel48,
  SkStackChannel.Channel49,
  SkStackChannel.Channel50,
  SkStackChannel.Channel51,
};
var scanChannelMask = SkStackChannel.CreateMask(scanChannels);

Console.Clear();

var initialCursorPosition = Console.GetCursorPosition();

try {
  for ( ; ;) {
    var (response, scanResult) = await bp35a1.SendSKSCANEnergyDetectScanAsync(
      durationFactor: 5,
      channelMask: scanChannelMask,
      cancellationToken: cts.Token
    );

    Console.SetCursorPosition(initialCursorPosition.Left, initialCursorPosition.Top);

    foreach (var (channel, rssi) in scanResult.Where(pair => scanChannels.Contains(pair.Key))) {
      Console.WriteLine($"{channel}: {GetEmojiForSignalLevel(rssi)} {rssi:N2} dBm   ");
    }
  }
}
catch (OperationCanceledException ex) when (ex.CancellationToken == cts.Token) {
  Console.WriteLine("Cancel key pressed.");
}

static string GetEmojiForSignalLevel(decimal rssi)
  => rssi switch {
    >= -70m => "🟩🔊", // Level 4 (>= -70 dBm): Excellent
    > -81m and <= -71m  => "🟨🔉", // Level 3 (-71 to -80 dBm): Good (Practical Range)
    > -93m and <= -81m  => "🟧🔈", //  Level 2 (-81 to -92 dBm): Unstable (Borderline)
    _ => "🟥🔇" // Level 1 (< -93 dBm): Out of Range / Disconnected
  };
