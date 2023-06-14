// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Smdn.Net.SkStackIP;

var services = new ServiceCollection();

services.AddLogging(
  builder => builder
    .AddSimpleConsole(static options => options.SingleLine = true)
    .AddFilter(static level => LogLevel.Trace <= level)
    //.AddFilter(static _ => true)
);

var logger = services.BuildServiceProvider().GetService<ILoggerFactory>().CreateLogger("SKSTACK-IP");

using var client = new SkStackClient(
  serialPortName: "/dev/ttyACM0",
  logger: logger
);

await client.SendSKVERAsync();
await client.SendSKAPPVERAsync();
await client.SendSKINFOAsync();
