// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#nullable enable

using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

using NUnit.Framework;

namespace Smdn.Net.SkStackIP;

partial class SkStackClientFunctionsPanaTests {
  [Test]
  public void ActiveScanAsync_NotFound()
  {
    using var stream = new PseudoSkStackStream();

    // SKSETRBID
    stream.ResponseWriter.WriteLine("OK");
    // SKSETPWD
    stream.ResponseWriter.WriteLine("OK");
    // SKSCAN
    stream.ResponseWriter.WriteLine("OK");
    stream.ResponseWriter.WriteLine("EVENT 22 FE80:0000:0000:0000:021D:1290:0003:C890");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    using var cts = new CancellationTokenSource(DefaultTimeOut);

    IReadOnlyList<SkStackPanDescription>? scanResult = null;

    Assert.DoesNotThrowAsync(async () => {
      scanResult = await client.ActiveScanAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".AsMemory(),
        password: "0123456789AB".AsMemory(),
        scanOptions: SkStackActiveScanOptions.Create(new[] { 1 }),
        cancellationToken: cts.Token
      );
    });

    Assert.IsNotNull(scanResult, nameof(scanResult));
    Assert.IsEmpty(scanResult!, nameof(scanResult));
  }

  [Test]
  public void ActiveScanAsync_FoundSingle()
  {
    using var stream = new PseudoSkStackStream();

    // SKSETRBID
    stream.ResponseWriter.WriteLine("OK");
    // SKSETPWD
    stream.ResponseWriter.WriteLine("OK");
    // SKSCAN
    stream.ResponseWriter.WriteLine("OK");
    stream.ResponseWriter.WriteLine("EVENT 20 FE80:0000:0000:0000:021D:1290:0003:C890");
    stream.ResponseWriter.WriteLine("EPANDESC");
    stream.ResponseWriter.WriteLine("  Channel:21");
    stream.ResponseWriter.WriteLine("  Channel Page:09");
    stream.ResponseWriter.WriteLine("  Pan ID:8888");
    stream.ResponseWriter.WriteLine("  Addr:12345678ABCDEF01");
    stream.ResponseWriter.WriteLine("  LQI:E1");
    stream.ResponseWriter.WriteLine("  PairID:AABBCCDD");
    stream.ResponseWriter.WriteLine("EVENT 22 FE80:0000:0000:0000:021D:1290:0003:C890");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    using var cts = new CancellationTokenSource(DefaultTimeOut);
    IReadOnlyList<SkStackPanDescription>? scanResult = null;

    Assert.DoesNotThrowAsync(async () => {
      scanResult = await client.ActiveScanAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".AsMemory(),
        password: "0123456789AB".AsMemory(),
        scanOptions: SkStackActiveScanOptions.Create(new[] { 1 }),
        cancellationToken: cts.Token
      );
    });

    Assert.IsNotNull(scanResult);
    Assert.AreEqual(1, scanResult!.Count);
    Assert.AreEqual(
      new PhysicalAddress(new byte[] { 0x12, 0x34, 0x56, 0x78, 0xAB, 0xCD, 0xEF, 0x01 }),
      scanResult[0].MacAddress
    );
  }

  [Test]
  public void ActiveScanAsync_FoundMultiple()
  {
    using var stream = new PseudoSkStackStream();

    // SKSETRBID
    stream.ResponseWriter.WriteLine("OK");
    // SKSETPWD
    stream.ResponseWriter.WriteLine("OK");
    // SKSCAN
    stream.ResponseWriter.WriteLine("OK");
    stream.ResponseWriter.WriteLine("EVENT 20 FE80:0000:0000:0000:021D:1290:0003:C890");
    stream.ResponseWriter.WriteLine("EPANDESC");
    stream.ResponseWriter.WriteLine("  Channel:21");
    stream.ResponseWriter.WriteLine("  Channel Page:09");
    stream.ResponseWriter.WriteLine("  Pan ID:8888");
    stream.ResponseWriter.WriteLine("  Addr:12345678ABCDEF01");
    stream.ResponseWriter.WriteLine("  LQI:E1");
    stream.ResponseWriter.WriteLine("  PairID:AABBCCDD");
    stream.ResponseWriter.WriteLine("EVENT 20 FE80:0000:0000:0000:021D:1290:0003:C890");
    stream.ResponseWriter.WriteLine("EPANDESC");
    stream.ResponseWriter.WriteLine("  Channel:22");
    stream.ResponseWriter.WriteLine("  Channel Page:09");
    stream.ResponseWriter.WriteLine("  Pan ID:9999");
    stream.ResponseWriter.WriteLine("  Addr:12345678ABCDEF02");
    stream.ResponseWriter.WriteLine("  LQI:E1");
    stream.ResponseWriter.WriteLine("  PairID:AABBCCDD");
    stream.ResponseWriter.WriteLine("EVENT 22 FE80:0000:0000:0000:021D:1290:0003:C890");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    using var cts = new CancellationTokenSource(DefaultTimeOut);
    IReadOnlyList<SkStackPanDescription>? scanResult = null;

    Assert.DoesNotThrowAsync(async () => {
      scanResult = await client.ActiveScanAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".AsMemory(),
        password: "0123456789AB".AsMemory(),
        scanOptions: SkStackActiveScanOptions.Create(new[] { 1 }),
        cancellationToken: cts.Token
      );
    });

    Assert.IsNotNull(scanResult);
    Assert.AreEqual(2, scanResult!.Count);

    Assert.AreEqual(
      new PhysicalAddress(new byte[] { 0x12, 0x34, 0x56, 0x78, 0xAB, 0xCD, 0xEF, 0x01 }),
      scanResult[0].MacAddress
    );
    Assert.AreEqual(
      new PhysicalAddress(new byte[] { 0x12, 0x34, 0x56, 0x78, 0xAB, 0xCD, 0xEF, 0x02 }),
      scanResult[1].MacAddress
    );
  }

  [Test]
  public void ActiveScanAsync_ScanDurations_NotFound()
  {
    using var stream = new PseudoSkStackStream();

    // SKSETRBID
    stream.ResponseWriter.WriteLine("OK");
    // SKSETPWD
    stream.ResponseWriter.WriteLine("OK");
    // SKSCAN 2
    stream.ResponseWriter.WriteLine("OK");
    stream.ResponseWriter.WriteLine("EVENT 22 FE80:0000:0000:0000:021D:1290:0003:C890");
    // SKSCAN 4
    stream.ResponseWriter.WriteLine("OK");
    stream.ResponseWriter.WriteLine("EVENT 22 FE80:0000:0000:0000:021D:1290:0003:C890");
    // SKSCAN 6
    stream.ResponseWriter.WriteLine("OK");
    stream.ResponseWriter.WriteLine("EVENT 22 FE80:0000:0000:0000:021D:1290:0003:C890");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    using var cts = new CancellationTokenSource(DefaultTimeOut);

    IReadOnlyList<SkStackPanDescription>? scanResult = null;

    Assert.DoesNotThrowAsync(async () => {
      scanResult = await client.ActiveScanAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".AsMemory(),
        password: "0123456789AB".AsMemory(),
        scanOptions: SkStackActiveScanOptions.Create(new[] { 2, 4, 6 }),
        cancellationToken: cts.Token
      );
    });

    Assert.IsNotNull(scanResult, nameof(scanResult));
    Assert.IsEmpty(scanResult!, nameof(scanResult));

    var commands = Encoding.ASCII.GetString(stream.ReadSentData());

    StringAssert.Contains("SKSCAN 2 FFFFFFFF 2", commands);
    StringAssert.Contains("SKSCAN 2 FFFFFFFF 4", commands);
    StringAssert.Contains("SKSCAN 2 FFFFFFFF 6", commands);
  }

  [Test]
  public void ActiveScanAsync_ScanDurations_Found()
  {
    using var stream = new PseudoSkStackStream();

    // SKSETRBID
    stream.ResponseWriter.WriteLine("OK");
    // SKSETPWD
    stream.ResponseWriter.WriteLine("OK");
    // SKSCAN 2
    stream.ResponseWriter.WriteLine("OK");
    stream.ResponseWriter.WriteLine("EVENT 22 FE80:0000:0000:0000:021D:1290:0003:C890");
    // SKSCAN 4
    stream.ResponseWriter.WriteLine("OK");
    stream.ResponseWriter.WriteLine("EVENT 20 FE80:0000:0000:0000:021D:1290:0003:C890");
    stream.ResponseWriter.WriteLine("EPANDESC");
    stream.ResponseWriter.WriteLine("  Channel:21");
    stream.ResponseWriter.WriteLine("  Channel Page:09");
    stream.ResponseWriter.WriteLine("  Pan ID:8888");
    stream.ResponseWriter.WriteLine("  Addr:12345678ABCDEF01");
    stream.ResponseWriter.WriteLine("  LQI:E1");
    stream.ResponseWriter.WriteLine("  PairID:AABBCCDD");
    stream.ResponseWriter.WriteLine("EVENT 22 FE80:0000:0000:0000:021D:1290:0003:C890");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    using var cts = new CancellationTokenSource(DefaultTimeOut);
    IReadOnlyList<SkStackPanDescription>? scanResult = null;

    Assert.DoesNotThrowAsync(async () => {
      scanResult = await client.ActiveScanAsync(
        rbid  : "00112233445566778899AABBCCDDEEFF".AsMemory(),
        password: "0123456789AB".AsMemory(),
        scanOptions: SkStackActiveScanOptions.Create(new[] { 2, 4, 6 }),
        cancellationToken: cts.Token
      );
    });

    Assert.IsNotNull(scanResult);
    Assert.AreEqual(1, scanResult!.Count);

    var commands = Encoding.ASCII.GetString(stream.ReadSentData());

    StringAssert.Contains("SKSCAN 2 FFFFFFFF 2", commands);
    StringAssert.Contains("SKSCAN 2 FFFFFFFF 4", commands);
    StringAssert.DoesNotContain("SKSCAN 2 FFFFFFFF 6", commands);
  }
}
