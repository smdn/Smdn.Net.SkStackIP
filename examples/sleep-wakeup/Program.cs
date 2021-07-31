﻿// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Device.Gpio;
using System.Threading.Tasks;

using Smdn.Net.SkStackIP;

using GpioController gpio = new();

const int pinWakeUp = 5;

gpio.OpenPin(pinWakeUp, PinMode.Output);

using var client = SkStackClient.Create(serialPortName: "/dev/ttyACM0");

client.Slept += static (sender, e) => Console.WriteLine(">> now in sleeping state");
client.WokeUp += static (sender, e) => Console.WriteLine(">> now in wake-up state");

await client.SendSKRESETAsync();

Console.WriteLine("start sleeping");

for (;;) {
  gpio.Write(pinWakeUp, PinValue.High);

  var taskSleep = client.SendSKDSLEEPAsync(waitUntilWakeUp: true).AsTask();
  var taskWakeUp = Task.Run(() => {
    Console.WriteLine("press ENTER key to activate wake-up signal");
    Console.ReadLine();

    gpio.Write(pinWakeUp, PinValue.Low);
  });

  await Task.WhenAll(taskSleep, taskWakeUp);

  Console.WriteLine("press ENTER key to restart sleeping");
  Console.ReadLine();
}


