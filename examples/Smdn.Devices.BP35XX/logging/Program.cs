// SPDX-FileCopyrightText: 2024 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Smdn.Devices.BP35XX;

var services = new ServiceCollection();

services.AddLogging(
  builder => builder
    .AddSimpleConsole(static options => options.SingleLine = true)
    .AddFilter(static level => LogLevel.Information <= level)
);

using var device = await BP35A1.CreateAsync(
  new BP35A1Configurations() {
    SerialPortName = "/dev/ttyACM0", // Specify a port name such as COM1 on Windows
    TryLoadFlashMemory = true, // Try to load configurations stored in flash memory
  },
  serviceProvider: services.BuildServiceProvider()
);
