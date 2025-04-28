using System;

using Smdn.Devices.BP35XX;

using var device = await BP35A1.CreateAsync(
  new BP35A1Configurations() {
    SerialPortName = "/dev/ttyACM0", // Specify a port name such as COM1 on Windows
    TryLoadFlashMemory = true, // Try to load configurations stored in flash memory
  }
);

Console.WriteLine("SkStack information");
Console.WriteLine($"  Version     : {device.SkStackVersion}");
Console.WriteLine($"  App version : {device.SkStackAppVersion}");
Console.WriteLine();

Console.WriteLine("BP35A1 device information");
Console.WriteLine($"  MAC address         : {device.MacAddress}");
Console.WriteLine($"  Link local address  : {device.LinkLocalAddress}");
Console.WriteLine();

Console.WriteLine("ROHM user information");
Console.WriteLine($"  User ID  : {device.RohmUserId}");
Console.WriteLine($"  Password : {device.RohmPassword}");
Console.WriteLine();

var uartOptions = await device.GetUartOptionsAsync();

Console.WriteLine("UART options");
Console.WriteLine($"  Baud rate          : {uartOptions.BaudRate}");
Console.WriteLine($"  Flow control       : {uartOptions.FlowControl}");
Console.WriteLine($"  Character interval : {uartOptions.CharacterInterval}");
Console.WriteLine();

var udpDataFormat = await device.GetUdpDataFormatAsync();

Console.WriteLine($"ERXUDP format: {udpDataFormat}");
Console.WriteLine();
