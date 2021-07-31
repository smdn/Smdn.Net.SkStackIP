// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

using Smdn.Net.SkStackIP;

var services = new ServiceCollection();

services.AddLogging(
  builder => builder
    .AddSimpleConsole(static options => options.SingleLine = true)
    .AddFilter(static level => level == LogLevel.Trace)
    //.AddFilter(static _ => true)
);

using var client = new SkStackClient(
  serialPortName: "/dev/ttyACM0",
  serviceProvider:  services.BuildServiceProvider()
);

await client.SendSKVERAsync();
await client.SendSKAPPVERAsync();
await client.SendSKINFOAsync();
