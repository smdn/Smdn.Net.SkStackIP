// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.IO.Ports;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Smdn.Net.SkStackIP;

var services = new ServiceCollection();

services.AddLogging(
  builder => builder
    .AddSimpleConsole(static options => options.SingleLine = true)
    .AddFilter(static level => LogLevel.Trace <= level)
);

using var serviceProvider = services.BuildServiceProvider();

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

using var client = new SkStackClient(
  stream: port.BaseStream,
  logger: serviceProvider.GetService<ILoggerFactory>().CreateLogger("SKSTACK-IP")
);

await client.SendSKVERAsync();
await client.SendSKAPPVERAsync();
await client.SendSKINFOAsync();
