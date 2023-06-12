// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

using NUnit.Framework;

using Is = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientCommandsSKSCANTests : SkStackClientTestsBase {
#if false
0 = 19.2 [ms]
1 = 28.8 [ms]
2 = 48.0 [ms]
3 = 86.4 [ms]
4 = 163.2 [ms]
5 = 316.8 [ms]
6 = 624.0 [ms]
7 = 1238.4 [ms]
8 = 2467.2 [ms]
9 = 4924.8 [ms]
10 = 9840.0 [ms]
11 = 19670.4 [ms]
12 = 39331.2 [ms]
13 = 78652.8 [ms]
14 = 157296.0 [ms]
#endif

  [TestCase(2, 0.0)] // must be treated as default value
  [TestCase(0, 19.2)]
  [TestCase(0, 19.2 + 0.1)]
  [TestCase(0, 28.8 - 0.1)]
  [TestCase(1, 28.8)]
  [TestCase(1, 28.8 + 0.1)]
  [TestCase(1, 48.0 - 0.1)]
  [TestCase(2, 48.0)]
  [TestCase(13, 157296.0 - 1.0)]
  [TestCase(14, 157296.0)]
  public void SKSCAN_Duration(int expectedDurationFactor, double durationMilliseconds)
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("FAIL");

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

    Assert.ThrowsAsync<SkStackErrorResponseException>(async () => {
      await client.SendSKSCANEnergyDetectScanAsync(
        duration: TimeSpan.FromMilliseconds(durationMilliseconds)
      );
    });

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo($"SKSCAN 0 FFFFFFFF {expectedDurationFactor:X1}\r\n".ToByteSequence())
    );
  }

  [TestCase(0)]
  [TestCase(1)]
  [TestCase(2)]
  [TestCase(3)]
  [TestCase(4)]
  [TestCase(5)]
  [TestCase(6)]
  [TestCase(7)]
  [TestCase(8)]
  [TestCase(9)]
  [TestCase(10)]
  [TestCase(11)]
  [TestCase(12)]
  [TestCase(13)]
  [TestCase(14)]
  public void SKSCAN_DurationFactor(int durationFactor)
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("FAIL");

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

    Assert.ThrowsAsync<SkStackErrorResponseException>(async () => {
      await client.SendSKSCANEnergyDetectScanAsync(durationFactor: durationFactor);
    });

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo($"SKSCAN 0 FFFFFFFF {durationFactor:X1}\r\n".ToByteSequence())
    );
  }

  [TestCase(-1)]
  [TestCase(19.2 - 0.1)]
  [TestCase(157296 + 1)]
  public void SKSCAN_Duration_OutOfRange(double durationMilliseconds)
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("FAIL");

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

    Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => {
      await client.SendSKSCANEnergyDetectScanAsync(
        duration: TimeSpan.FromMilliseconds(durationMilliseconds)
      );
    });

    Assert.IsEmpty(stream.ReadSentData());
  }

  [TestCase(-1)]
  [TestCase(15)]
  [TestCase(int.MinValue)]
  [TestCase(int.MaxValue)]
  public void SKSCAN_DurationFactor_OutOfRange(int durationFactor)
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("FAIL");

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

    Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => {
      await client.SendSKSCANEnergyDetectScanAsync(durationFactor: durationFactor);
    });

    Assert.IsEmpty(stream.ReadSentData());
  }

  [TestCase((uint)0x00000000)]
  [TestCase((uint)0x12345678)]
  [TestCase((uint)0xDEADBEEF)]
  public void SKSCAN_ChannelMask(uint channelMask)
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("FAIL");

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

    Assert.ThrowsAsync<SkStackErrorResponseException>(async () => {
      await client.SendSKSCANEnergyDetectScanAsync(
        channelMask: channelMask
      );
    });

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo($"SKSCAN 0 {channelMask:X8} 2\r\n".ToByteSequence())
    );
  }

  [TestCase(true)]
  [TestCase(false)]
  public void SKSCAN_EnergyDetectScan(bool extraCRLF)
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");
    stream.ResponseWriter.WriteLine("EVENT 1F FE80:0000:0000:0000:021D:1290:0003:C890");
    stream.ResponseWriter.WriteLine("EEDSCAN");
    stream.ResponseWriter.WriteLine("21 04 22 04 23 03 24 02 25 05 26 06 27 05 28 11 29 10 2A 0B 2B 10 2C 0C 2D 09 2E 0A 2F 07 30 06 31 05 32 06 33 03 34 03 35 03 36 02 37 03 38 03 39 02 3A 04 3B 06 3C 02");

    if (extraCRLF)
      // [VER 1.2.10, APPVER rev26e] EEDSCAN responds extra CRLF
      stream.ResponseWriter.WriteLine();

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());
    IReadOnlyDictionary<SkStackChannel, double> scanResult = default;

    Assert.DoesNotThrowAsync(async () => {
      (_, scanResult) = await client.SendSKSCANEnergyDetectScanAsync();
    });

    Assert.IsNotNull(scanResult);
    Assert.AreEqual(28, scanResult!.Count);

    var expectedValues = new[] {
      new { Channel = 0x21, LQI = 0x04, RSSI = -103.17 },
      new { Channel = 0x22, LQI = 0x04, RSSI = -103.17 },
      new { Channel = 0x23, LQI = 0x03, RSSI = -103.445 },
      new { Channel = 0x24, LQI = 0x02, RSSI = -103.72 },
      new { Channel = 0x25, LQI = 0x05, RSSI = -102.895 },
      new { Channel = 0x26, LQI = 0x06, RSSI = -102.62 },
      new { Channel = 0x27, LQI = 0x05, RSSI = -102.895 },
      new { Channel = 0x28, LQI = 0x11, RSSI = -99.595 },
      new { Channel = 0x29, LQI = 0x10, RSSI = -99.87 },
      new { Channel = 0x2A, LQI = 0x0B, RSSI = -101.245 },
      new { Channel = 0x2B, LQI = 0x10, RSSI = -99.87 },
      new { Channel = 0x2C, LQI = 0x0C, RSSI = -100.97 },
      new { Channel = 0x2D, LQI = 0x09, RSSI = -101.795 },
      new { Channel = 0x2E, LQI = 0x0A, RSSI = -101.52 },
      new { Channel = 0x2F, LQI = 0x07, RSSI = -102.345 },
      new { Channel = 0x30, LQI = 0x06, RSSI = -102.62 },
      new { Channel = 0x31, LQI = 0x05, RSSI = -102.895 },
      new { Channel = 0x32, LQI = 0x06, RSSI = -102.62 },
      new { Channel = 0x33, LQI = 0x03, RSSI = -103.445 },
      new { Channel = 0x34, LQI = 0x03, RSSI = -103.445 },
      new { Channel = 0x35, LQI = 0x03, RSSI = -103.445 },
      new { Channel = 0x36, LQI = 0x02, RSSI = -103.72 },
      new { Channel = 0x37, LQI = 0x03, RSSI = -103.445 },
      new { Channel = 0x38, LQI = 0x03, RSSI = -103.445 },
      new { Channel = 0x39, LQI = 0x02, RSSI = -103.72 },
      new { Channel = 0x3A, LQI = 0x04, RSSI = -103.17 },
      new { Channel = 0x3B, LQI = 0x06, RSSI = -102.62 },
      new { Channel = 0x3C, LQI = 0x02, RSSI = -103.72 },
    };

    foreach (var expectedValue in expectedValues) {
      Assert.IsTrue(scanResult.TryGetValue(SkStackChannel.Channels[expectedValue.Channel], out var rssi), $"channel #{expectedValue.Channel}");
      Assert.That(rssi, Is.EqualTo(expectedValue.RSSI).Within(0.5), $"channel #{expectedValue.Channel} RSSI");
    }

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKSCAN 0 FFFFFFFF 2\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKSCAN_ActiveScanPair()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    async Task RaiseBeaconReceivedEventAsync()
    {
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.WriteLine("EVENT 20 FE80:0000:0000:0000:021D:1290:0003:C890");

      await Task.Delay(ResponseDelayInterval);

#if false
      stream.ResponseWriter.WriteLine("EPANDESC");
      stream.ResponseWriter.WriteLine("  Channel:21");
      stream.ResponseWriter.WriteLine("  Channel Page:09");
      stream.ResponseWriter.WriteLine("  Pan ID:8888");
      stream.ResponseWriter.WriteLine("  Addr:12345678ABCDEF01");
      stream.ResponseWriter.WriteLine("  LQI:E1");
      stream.ResponseWriter.WriteLine("  PairID:AABBCCDD");
#endif
      stream.ResponseWriter.Write("EPAN"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine("DESC");
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.WriteLine("  Channel:21");
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.WriteLine("  Channel Page:09");
      stream.ResponseWriter.WriteLine("  Pan ID:8888");
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.Write("  Addr"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine(":12345678ABCDEF01");
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.WriteLine("  LQI:E1");
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.Write("  Pair"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.Write("ID:"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.Write("AABBCCDD"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine();
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.WriteLine("EVENT 22 FE80:0000:0000:0000:021D:1290:0003:C890");
    }

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

#pragma warning disable CA2012
    var taskSendCommand = client.SendSKSCANActiveScanPairAsync().AsTask();

    Assert.DoesNotThrowAsync(
      async () => await Task.WhenAll(taskSendCommand, RaiseBeaconReceivedEventAsync())
    );
#pragma warning restore CA2012

    var scanResult = taskSendCommand.Result.PanDescriptions;

    Assert.IsNotNull(scanResult);
    Assert.AreEqual(1, scanResult.Count);
    Assert.AreEqual(SkStackChannel.Channels[0x21], scanResult[0].Channel);
    Assert.AreEqual(0x09, scanResult[0].ChannelPage);
    Assert.AreEqual(0x8888, scanResult[0].PanID);
    Assert.AreEqual(new PhysicalAddress(new byte[] {0x12, 0x34, 0x56, 0x78, 0xAB, 0xCD, 0xEF, 0x01}), scanResult[0].MacAddress);
    Assert.That(scanResult[0].RSSI, Is.EqualTo(-42.395).Within(0.001));
    Assert.AreEqual(0xAABBCCDD, scanResult[0].PairingID);

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKSCAN 2 FFFFFFFF 2\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKSCAN_ActiveScanPair_NoPANFound()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    async Task RaiseActiveScanCompletedEventAsync()
    {
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.WriteLine("EVENT 22 FE80:0000:0000:0000:021D:1290:0003:C890");
    }

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

#pragma warning disable CA2012
    var taskSendCommand = client.SendSKSCANActiveScanPairAsync().AsTask();

    Assert.DoesNotThrowAsync(
      async () => await Task.WhenAll(taskSendCommand, RaiseActiveScanCompletedEventAsync())
    );
#pragma warning restore CA2012

    var scanResult = taskSendCommand.Result.PanDescriptions;

    Assert.IsNotNull(scanResult);
    Assert.AreEqual(0, scanResult.Count);

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKSCAN 2 FFFFFFFF 2\r\n".ToByteSequence())
    );
  }

  [Test]
  [Category("not-confirmed-with-actual-behaviour")]
  public void SKSCAN_ActiveScanPair_MultiplePANFound()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    async Task RaiseBeaconReceivedEventAsync()
    {
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.WriteLine("EVENT 20 FE80:0000:0000:0000:021D:1290:0003:C890");
      stream.ResponseWriter.WriteLine("EPANDESC");
      stream.ResponseWriter.WriteLine("  Channel:21");
      stream.ResponseWriter.WriteLine("  Channel Page:09");
      stream.ResponseWriter.WriteLine("  Pan ID:8888");
      stream.ResponseWriter.WriteLine("  Addr:12345678ABCDEF01");
      stream.ResponseWriter.WriteLine("  LQI:E1");
      stream.ResponseWriter.WriteLine("  PairID:AABBCCDD");

      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.WriteLine("EVENT 20 FE80:0000:0000:0000:021D:1290:0003:C890");
      stream.ResponseWriter.WriteLine("EPANDESC");
      stream.ResponseWriter.WriteLine("  Channel:21");
      stream.ResponseWriter.WriteLine("  Channel Page:09");
      stream.ResponseWriter.WriteLine("  Pan ID:9999");
      stream.ResponseWriter.WriteLine("  Addr:ABCDEF0123456789");
      stream.ResponseWriter.WriteLine("  LQI:E1");
      stream.ResponseWriter.WriteLine("  PairID:AABBCCDD");

      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.WriteLine("EVENT 22 FE80:0000:0000:0000:021D:1290:0003:C890");
    }

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

#pragma warning disable CA2012
    var taskSendCommand = client.SendSKSCANActiveScanPairAsync().AsTask();

    Assert.DoesNotThrowAsync(
      async () => await Task.WhenAll(taskSendCommand, RaiseBeaconReceivedEventAsync())
    );
#pragma warning restore CA2012

    var scanResult = taskSendCommand.Result.PanDescriptions;

    Assert.IsNotNull(scanResult);
    Assert.AreEqual(2, scanResult.Count);

    Assert.AreEqual(0x8888, scanResult[0].PanID);
    Assert.AreEqual(new PhysicalAddress(new byte[] { 0x12, 0x34, 0x56, 0x78, 0xAB, 0xCD, 0xEF, 0x01 }), scanResult[0].MacAddress);

    Assert.AreEqual(0x9999, scanResult[1].PanID);
    Assert.AreEqual(new PhysicalAddress(new byte[] { 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67, 0x89 }), scanResult[1].MacAddress);

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKSCAN 2 FFFFFFFF 2\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKSCAN_ActiveScan()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    async Task RaiseBeaconReceivedEventAsync()
    {
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.WriteLine("EVENT 20 FE80:0000:0000:0000:021D:1290:0003:C890");

#if false
      stream.ResponseWriter.WriteLine("EPANDESC");
      stream.ResponseWriter.WriteLine("  Channel:21");
      stream.ResponseWriter.WriteLine("  Channel Page:09");
      stream.ResponseWriter.WriteLine("  Pan ID:8888");
      stream.ResponseWriter.WriteLine("  Addr:12345678ABCDEF01");
      stream.ResponseWriter.WriteLine("  LQI:E1");
#endif

      stream.ResponseWriter.WriteLine("EPANDESC"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.Write("  Channel"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.Write(":21"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine();
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.WriteLine("  Channel Page:09");
      stream.ResponseWriter.WriteLine("  Pan ID:8888");
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.WriteLine("  Addr:12345678ABCDEF01");
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.Write("  LQ"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.Write("I:E"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine("1");
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.WriteLine("EVENT 22 FE80:0000:0000:0000:021D:1290:0003:C890");
    }

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

#pragma warning disable CA2012
    var taskSendCommand = client.SendSKSCANActiveScanAsync().AsTask();

    Assert.DoesNotThrowAsync(
      async () => await Task.WhenAll(taskSendCommand, RaiseBeaconReceivedEventAsync())
    );
#pragma warning restore CA2012

    var scanResult = taskSendCommand.Result.PanDescriptions;

    Assert.IsNotNull(scanResult);
    Assert.AreEqual(1, scanResult.Count);
    Assert.AreEqual(SkStackChannel.Channels[0x21], scanResult[0].Channel);
    Assert.AreEqual(0x09, scanResult[0].ChannelPage);
    Assert.AreEqual(0x8888, scanResult[0].PanID);
    Assert.AreEqual(new PhysicalAddress(new byte[] {0x12, 0x34, 0x56, 0x78, 0xAB, 0xCD, 0xEF, 0x01}), scanResult[0].MacAddress);
    Assert.That(scanResult[0].RSSI, Is.EqualTo(-42.395).Within(0.001));
    Assert.AreEqual(0, scanResult[0].PairingID);

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKSCAN 3 FFFFFFFF 2\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKSCAN_ActiveScan_NoPANFound()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    async Task RaiseActiveScanCompletedEventAsync()
    {
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.WriteLine("EVENT 22 FE80:0000:0000:0000:021D:1290:0003:C890");
    }

    using var client = new SkStackClient(stream, CreateLoggerForTestCase());

#pragma warning disable CA2012
    var taskSendCommand = client.SendSKSCANActiveScanAsync().AsTask();

    Assert.DoesNotThrowAsync(
      async () => await Task.WhenAll(taskSendCommand, RaiseActiveScanCompletedEventAsync())
    );
#pragma warning restore CA2012

    var scanResult = taskSendCommand.Result.PanDescriptions;

    Assert.IsNotNull(scanResult);
    Assert.AreEqual(0, scanResult.Count);

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKSCAN 3 FFFFFFFF 2\r\n".ToByteSequence())
    );
  }
}
