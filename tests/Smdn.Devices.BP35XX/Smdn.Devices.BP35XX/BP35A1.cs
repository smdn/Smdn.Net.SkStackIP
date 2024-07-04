// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using NUnit.Framework;

using Smdn.Net.SkStackIP;

namespace Smdn.Devices.BP35XX;

[TestFixture]
public class BP35A1Tests {
  [TestCase(true)]
  [TestCase(false)]
  public void CreateAsync(bool tryLoadFlashMemory)
  {
    var factory = new PseudoSerialPortStreamFactory();
    var services = new ServiceCollection();

    services.Add(
      ServiceDescriptor.Singleton(
        typeof(IBP35SerialPortStreamFactory),
        factory
      )
    );

    // SKRESET
    factory.Stream.ResponseWriter.WriteLine("OK");

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
    if (tryLoadFlashMemory)
      factory.Stream.ResponseWriter.WriteLine("OK");

    // SKSREG SFE 0
    factory.Stream.ResponseWriter.WriteLine("OK");

    // ROPT
    factory.Stream.ResponseWriter.Write("OK 00\r");

    Assert.DoesNotThrowAsync(
      async () => {
        using var bp35a1 = await BP35A1.CreateAsync(
          new BP35A1Configurations() {
            SerialPortName = "/dev/pseudo-serial-port",
            TryLoadFlashMemory = tryLoadFlashMemory,
          },
          services.BuildServiceProvider()
        );
      }
    );
  }

  public async Task Properties()
  {
    const string version = "1.2.10";
    const string appVersion = "pseudo-BP35A1";
    const string userId = "FE80";
    const string password = "021D";
    const string linkLocalAddress = $"{userId}:0000:0000:0000:{password}:1290:1234:5678";
    const string macAddress = "001D129012345678";

    var factory = new PseudoSerialPortStreamFactory();
    var services = new ServiceCollection();

    services.Add(
      ServiceDescriptor.Singleton(
        typeof(IBP35SerialPortStreamFactory),
        factory
      )
    );

    // SKRESET
    factory.Stream.ResponseWriter.WriteLine("OK");

    // SKVER
    factory.Stream.ResponseWriter.WriteLine($"EVER {version}");
    factory.Stream.ResponseWriter.WriteLine("OK");

    // SKAPPVER
    factory.Stream.ResponseWriter.WriteLine($"EAPPVER {appVersion}");
    factory.Stream.ResponseWriter.WriteLine("OK");

    // SKINFO
    factory.Stream.ResponseWriter.WriteLine($"EINFO {linkLocalAddress} {macAddress} 21 8888 FFFE");
    factory.Stream.ResponseWriter.WriteLine("OK");

    // SKLOAD
    factory.Stream.ResponseWriter.WriteLine("OK");

    // SKSREG SFE 0
    factory.Stream.ResponseWriter.WriteLine("OK");

    // ROPT
    factory.Stream.ResponseWriter.Write("OK 00\r");

    using var bp35a1 = await BP35A1.CreateAsync(
      new BP35A1Configurations() {
        SerialPortName = "/dev/pseudo-serial-port",
      },
      services.BuildServiceProvider()
    );

    Assert.That(bp35a1.SkStackVersion, Is.EqualTo(Version.Parse(version)));
    Assert.That(bp35a1.SkStackAppVersion, Is.EqualTo(appVersion));
    Assert.That(bp35a1.LinkLocalAddress, Is.EqualTo(IPAddress.Parse(linkLocalAddress)));
    Assert.That(bp35a1.MacAddress, Is.EqualTo(PhysicalAddress.Parse(macAddress)));
    Assert.That(bp35a1.RohmUserId, Is.EqualTo(userId));
    Assert.That(bp35a1.RohmPassword, Is.EqualTo(password));
  }

  [Test]
  public async Task Dispose()
  {
    var factory = new PseudoSerialPortStreamFactory();
    var services = new ServiceCollection();

    services.Add(
      ServiceDescriptor.Singleton(
        typeof(IBP35SerialPortStreamFactory),
        factory
      )
    );

    // SKRESET
    factory.Stream.ResponseWriter.WriteLine("OK");

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

    using var bp35a1 = await BP35A1.CreateAsync(
      new BP35A1Configurations() {
        SerialPortName = "/dev/pseudo-serial-port",
        TryLoadFlashMemory = true,
      },
      services.BuildServiceProvider()
    );

    Assert.DoesNotThrow(() => bp35a1.Dispose(), "Dispose #0");

    Assert.DoesNotThrow(() => Assert.That(bp35a1.SkStackVersion, Is.EqualTo(Version.Parse("1.2.10"))));
    Assert.DoesNotThrow(() => Assert.That(bp35a1.RohmUserId, Is.EqualTo("FE80")));

    Assert.ThrowsAsync<ObjectDisposedException>(
      async () => await bp35a1.AuthenticateAsPanaClientAsync(
        Encoding.ASCII.GetBytes("01234567890123456789012345678901"),
        Encoding.ASCII.GetBytes("pass"),
        SkStackActiveScanOptions.Default
      )
    );
    Assert.ThrowsAsync<ObjectDisposedException>(
      async () => await bp35a1.ActiveScanAsync(
        Encoding.ASCII.GetBytes("01234567890123456789012345678901"),
        Encoding.ASCII.GetBytes("pass"),
        SkStackActiveScanOptions.Default
      )
    );

    Assert.DoesNotThrow(() => bp35a1.Dispose(), "Dispose #1");
  }
}
