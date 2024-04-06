// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using NUnit.Framework;

namespace Smdn.Devices.BP35XX;

[TestFixture]
public class BP35A1CommandsTests {
  private async Task<(BP35A1, PseudoSkStackStream)> CreateDeviceAsync()
  {
    var factory = new PseudoSerialPortStreamFactory();
    var services = new ServiceCollection();

    services.Add(
      ServiceDescriptor.Singleton(
        typeof(IBP35SerialPortStreamFactory),
        factory
      )
    );

    // SKVER
    factory.Stream.ResponseWriter.WriteLine("EVER 1.2.10");
    factory.Stream.ResponseWriter.WriteLine("OK");

    // SKAPPVER
    factory.Stream.ResponseWriter.WriteLine("EAPPVER pseudo-BP35A1");
    factory.Stream.ResponseWriter.WriteLine("OK");

    // SKINFO
    factory.Stream.ResponseWriter.WriteLine("EINFO FE80:0000:0000:0000:021D:1290:1234:5678 001D129012345678 21 8888 FFFE");
    factory.Stream.ResponseWriter.WriteLine("OK");

    // SKLOAD
    factory.Stream.ResponseWriter.WriteLine("OK");

    // SKSREG SFE 0
    factory.Stream.ResponseWriter.WriteLine("OK");

    // ROPT
    factory.Stream.ResponseWriter.Write("OK 00\r");

    var bp35a1 = await BP35A1.CreateAsync(
      new BP35A1Configurations() {
        SerialPortName = "/dev/pseudo-serial-port",
        TryLoadFlashMemory = true,
      },
      services.BuildServiceProvider()
    );

    factory.Stream.ReadSentData(); // clear sent data

    return (bp35a1, factory.Stream);
  }

  [TestCase(BP35UdpReceiveDataFormat.Binary,    0b_0000000_0)]
  [TestCase(BP35UdpReceiveDataFormat.HexAscii,  0b_0000000_1)]
  public async Task SetUdpDataFormatAsync(
    BP35UdpReceiveDataFormat format,
    byte expectedWOPTArg
  )
  {
    var (dev, s) = await CreateDeviceAsync();

    using var bp35a1 = dev;
    using var stream = s;

    // WOPT
    stream.ResponseWriter.Write($"OK\r");

    Assert.DoesNotThrowAsync(async () =>
      await bp35a1.SetUdpDataFormatAsync(
        format: format
      )
    );

    var commands = Encoding.ASCII.GetString(stream.ReadSentData());

    Assert.That(commands, Is.EqualTo($"WOPT {expectedWOPTArg:X2}\r"));
  }

  [TestCase(0b_0000000_0, BP35UdpReceiveDataFormat.Binary)]
  [TestCase(0b_0000000_1, BP35UdpReceiveDataFormat.HexAscii)]
  public async Task GetUdpDataFormatAsync(
    byte statusText,
    BP35UdpReceiveDataFormat expectedFormat
  )
  {
    var (dev, s) = await CreateDeviceAsync();

    using var bp35a1 = dev;
    using var stream = s;

    // ROPT
    stream.ResponseWriter.Write($"OK {statusText:X2}\r");

    BP35UdpReceiveDataFormat actualFormat = default;

    Assert.DoesNotThrowAsync(async () =>
      actualFormat = await bp35a1.GetUdpDataFormatAsync()
    );

    var commands = Encoding.ASCII.GetString(stream.ReadSentData());

    Assert.That(commands, Is.EqualTo("ROPT\r"));

    Assert.That(actualFormat, Is.EqualTo(expectedFormat));
  }

  [TestCase(BP35UartBaudRate.Baud115200,  BP35UartCharacterInterval.None,             BP35UartFlowControl.Disabled, 0b_0_000_0_000)]
  [TestCase(BP35UartBaudRate.Baud9600,    BP35UartCharacterInterval.None,             BP35UartFlowControl.Enabled,  0b_1_000_0_011)]
  [TestCase(BP35UartBaudRate.Baud2400,    BP35UartCharacterInterval.None,             BP35UartFlowControl.Disabled, 0b_0_000_0_001)]
  [TestCase(BP35UartBaudRate.Baud4800,    BP35UartCharacterInterval.None,             BP35UartFlowControl.Disabled, 0b_0_000_0_010)]
  [TestCase(BP35UartBaudRate.Baud9600,    BP35UartCharacterInterval.None,             BP35UartFlowControl.Disabled, 0b_0_000_0_011)]
  [TestCase(BP35UartBaudRate.Baud19200,   BP35UartCharacterInterval.None,             BP35UartFlowControl.Disabled, 0b_0_000_0_100)]
  [TestCase(BP35UartBaudRate.Baud38400,   BP35UartCharacterInterval.None,             BP35UartFlowControl.Disabled, 0b_0_000_0_101)]
  [TestCase(BP35UartBaudRate.Baud57600,   BP35UartCharacterInterval.None,             BP35UartFlowControl.Disabled, 0b_0_000_0_110)]
  [TestCase(BP35UartBaudRate.Baud115200,  BP35UartCharacterInterval.Microseconds100,  BP35UartFlowControl.Disabled, 0b_0_001_0_000)]
  [TestCase(BP35UartBaudRate.Baud115200,  BP35UartCharacterInterval.Microseconds200,  BP35UartFlowControl.Disabled, 0b_0_010_0_000)]
  [TestCase(BP35UartBaudRate.Baud115200,  BP35UartCharacterInterval.Microseconds300,  BP35UartFlowControl.Disabled, 0b_0_011_0_000)]
  [TestCase(BP35UartBaudRate.Baud115200,  BP35UartCharacterInterval.Microseconds400,  BP35UartFlowControl.Disabled, 0b_0_100_0_000)]
  [TestCase(BP35UartBaudRate.Baud115200,  BP35UartCharacterInterval.Microseconds50,   BP35UartFlowControl.Disabled, 0b_0_101_0_000)]
  [TestCase(BP35UartBaudRate.Baud115200,  BP35UartCharacterInterval.None,             BP35UartFlowControl.Enabled,  0b_1_000_0_000)]
  public async Task SetUartOptionsAsync(
    BP35UartBaudRate baudRate,
    BP35UartCharacterInterval characterInterval,
    BP35UartFlowControl flowControl,
    byte expectedWUARTArg
  )
  {
    var (dev, s) = await CreateDeviceAsync();

    using var bp35a1 = dev;
    using var stream = s;

    // WUART
    stream.ResponseWriter.Write($"OK\r");

    Assert.DoesNotThrowAsync(async () =>
      await bp35a1.SetUartOptionsAsync(
        baudRate: baudRate,
        flowControl: flowControl,
        characterInterval: characterInterval
      )
    );

    var commands = Encoding.ASCII.GetString(stream.ReadSentData());

    Assert.That(commands, Is.EqualTo($"WUART {expectedWUARTArg:X2}\r"));
  }

  [TestCase(0b_0_000_0_000, BP35UartBaudRate.Baud115200,  BP35UartCharacterInterval.None,             BP35UartFlowControl.Disabled)]
  [TestCase(0b_1_000_0_011, BP35UartBaudRate.Baud9600,    BP35UartCharacterInterval.None,             BP35UartFlowControl.Enabled)]
  [TestCase(0b_0_000_0_001, BP35UartBaudRate.Baud2400,    BP35UartCharacterInterval.None,             BP35UartFlowControl.Disabled)]
  [TestCase(0b_0_000_0_010, BP35UartBaudRate.Baud4800,    BP35UartCharacterInterval.None,             BP35UartFlowControl.Disabled)]
  [TestCase(0b_0_000_0_011, BP35UartBaudRate.Baud9600,    BP35UartCharacterInterval.None,             BP35UartFlowControl.Disabled)]
  [TestCase(0b_0_000_0_100, BP35UartBaudRate.Baud19200,   BP35UartCharacterInterval.None,             BP35UartFlowControl.Disabled)]
  [TestCase(0b_0_000_0_101, BP35UartBaudRate.Baud38400,   BP35UartCharacterInterval.None,             BP35UartFlowControl.Disabled)]
  [TestCase(0b_0_000_0_110, BP35UartBaudRate.Baud57600,   BP35UartCharacterInterval.None,             BP35UartFlowControl.Disabled)]
  [TestCase(0b_0_001_0_000, BP35UartBaudRate.Baud115200,  BP35UartCharacterInterval.Microseconds100,  BP35UartFlowControl.Disabled)]
  [TestCase(0b_0_010_0_000, BP35UartBaudRate.Baud115200,  BP35UartCharacterInterval.Microseconds200,  BP35UartFlowControl.Disabled)]
  [TestCase(0b_0_011_0_000, BP35UartBaudRate.Baud115200,  BP35UartCharacterInterval.Microseconds300,  BP35UartFlowControl.Disabled)]
  [TestCase(0b_0_100_0_000, BP35UartBaudRate.Baud115200,  BP35UartCharacterInterval.Microseconds400,  BP35UartFlowControl.Disabled)]
  [TestCase(0b_0_101_0_000, BP35UartBaudRate.Baud115200,  BP35UartCharacterInterval.Microseconds50,   BP35UartFlowControl.Disabled)]
  [TestCase(0b_1_000_0_000, BP35UartBaudRate.Baud115200,  BP35UartCharacterInterval.None,             BP35UartFlowControl.Enabled)]
  public async Task GetUartOptionsAsync(
    byte statusText,
    BP35UartBaudRate expectedBaudRate,
    BP35UartCharacterInterval expectedCharacterInterval,
    BP35UartFlowControl expectedFlowControl
  )
  {
    var (dev, s) = await CreateDeviceAsync();

    using var bp35a1 = dev;
    using var stream = s;

    // RUART
    stream.ResponseWriter.Write($"OK {statusText:X2}\r");

    BP35UartBaudRate actualBaudRate = default;
    BP35UartFlowControl actualFlowControl = default;
    BP35UartCharacterInterval actualCharacterInterval = default;

    Assert.DoesNotThrowAsync(async () => {
      (actualBaudRate, actualCharacterInterval, actualFlowControl) = await bp35a1.GetUartOptionsAsync();
    });

    var commands = Encoding.ASCII.GetString(stream.ReadSentData());

    Assert.That(commands, Is.EqualTo("RUART\r"));

    Assert.That(actualBaudRate, Is.EqualTo(expectedBaudRate));
    Assert.That(actualCharacterInterval, Is.EqualTo(expectedCharacterInterval));
    Assert.That(actualFlowControl, Is.EqualTo(expectedFlowControl));
  }
}
