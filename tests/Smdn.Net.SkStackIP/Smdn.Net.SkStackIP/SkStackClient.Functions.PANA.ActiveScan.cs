// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

using NUnit.Framework;

namespace Smdn.Net.SkStackIP;

#pragma warning disable IDE0040
partial class SkStackClientFunctionsPanaTests {
#pragma warning restore IDE0040
  [Test]
  [CancelAfter(DefaultTimeOutInMilliseconds)]
  public void ActiveScanAsync_ArgumentException_RBIDEmpty(CancellationToken cancellationToken)
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.That(
      () => client.ActiveScanAsync(
        rbid: default,
        password: "0123456789AB".ToByteSequence(),
        scanOptions: SkStackActiveScanOptions.Create(new[] { 1 }),
        cancellationToken: cancellationToken
      ),
      Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("rbid")
    );
  }

  [Test]
  [CancelAfter(DefaultTimeOutInMilliseconds)]
  public void ActiveScanAsync_ArgumentException_PasswordEmpty(CancellationToken cancellationToken)
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.That(
      () => client.ActiveScanAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: default,
        cancellationToken: cancellationToken
      ),
      Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("password")
    );
  }

  [Test]
  [CancelAfter(DefaultTimeOutInMilliseconds)]
  public void ActiveScanAsync_ArgumentNullException_WriteRBID(CancellationToken cancellationToken)
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.That(
      () => client.ActiveScanAsync(
        writeRBID: null!,
        writePassword: static writer => throw new NotImplementedException(),
        scanOptions: SkStackActiveScanOptions.Create(new[] { 1 }),
        cancellationToken: cancellationToken
      ),
      Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("writeRBID")
    );
  }

  [Test]
  [CancelAfter(DefaultTimeOutInMilliseconds)]
  public void ActiveScanAsync_ArgumentNullException_WritePassword(CancellationToken cancellationToken)
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.That(
      () => client.ActiveScanAsync(
        writeRBID: static writer => throw new NotImplementedException(),
        writePassword: null!,
        scanOptions: SkStackActiveScanOptions.Create(new[] { 1 }),
        cancellationToken: cancellationToken
      ),
      Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("writePassword")
    );
  }

  [Test]
  [CancelAfter(DefaultTimeOutInMilliseconds)]
  public void ActiveScanAsync_RouteBCredential_ReadOnlyMemory(CancellationToken cancellationToken)
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

    Assert.DoesNotThrowAsync(async () => {
      await client.ActiveScanAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: "0123456789AB".ToByteSequence(),
        scanOptions: SkStackActiveScanOptions.Create(new[] { 1 }),
        cancellationToken: cancellationToken
      );
    });

    var commands = Encoding.ASCII.GetString(stream.ReadSentData());

    Assert.That(commands, Does.Contain("SKSETRBID 00112233445566778899AABBCCDDEEFF"));
    Assert.That(commands, Does.Contain("SKSETPWD C 0123456789AB"));
    Assert.That(commands, Does.Contain("SKSCAN 2 FFFFFFFF 1"));
  }

  [Test]
  [CancelAfter(DefaultTimeOutInMilliseconds)]
  public void ActiveScanAsync_RouteBCredential_Writer(CancellationToken cancellationToken)
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

    Assert.DoesNotThrowAsync(async () => {
      await client.ActiveScanAsync(
        writeRBID: static writer => writer.Write("00112233445566778899AABBCCDDEEFF".ToByteSequence().Span),
        writePassword: static writer => writer.Write("0123456789AB".ToByteSequence().Span),
        scanOptions: SkStackActiveScanOptions.Create(new[] { 1 }),
        cancellationToken: cancellationToken
      );
    });

    var commands = Encoding.ASCII.GetString(stream.ReadSentData());

    Assert.That(commands, Does.Contain("SKSETRBID 00112233445566778899AABBCCDDEEFF"));
    Assert.That(commands, Does.Contain("SKSETPWD C 0123456789AB"));
    Assert.That(commands, Does.Contain("SKSCAN 2 FFFFFFFF 1"));
  }

  [Test]
  [CancelAfter(DefaultTimeOutInMilliseconds)]
  public void ActiveScanAsync_NotFound(CancellationToken cancellationToken)
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
    IReadOnlyList<SkStackPanDescription>? scanResult = null;

    Assert.DoesNotThrowAsync(async () => {
      scanResult = await client.ActiveScanAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: "0123456789AB".ToByteSequence(),
        scanOptions: SkStackActiveScanOptions.Create(new[] { 1 }),
        cancellationToken: cancellationToken
      );
    });

    Assert.That(scanResult, Is.Not.Null, nameof(scanResult));
    Assert.That(scanResult!, Is.Empty, nameof(scanResult));
  }

  [Test]
  [CancelAfter(DefaultTimeOutInMilliseconds)]
  public void ActiveScanAsync_FoundSingle(CancellationToken cancellationToken)
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
    IReadOnlyList<SkStackPanDescription>? scanResult = null;

    Assert.DoesNotThrowAsync(async () => {
      scanResult = await client.ActiveScanAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: "0123456789AB".ToByteSequence(),
        scanOptions: SkStackActiveScanOptions.Create(new[] { 1 }),
        cancellationToken: cancellationToken
      );
    });

    Assert.That(scanResult, Is.Not.Null);
    Assert.That(scanResult!.Count, Is.EqualTo(1));
    Assert.That(
      scanResult[0].MacAddress,
      Is.EqualTo(new PhysicalAddress(new byte[] { 0x12, 0x34, 0x56, 0x78, 0xAB, 0xCD, 0xEF, 0x01 }))
    );
  }

  [Test]
  [CancelAfter(DefaultTimeOutInMilliseconds)]
  public void ActiveScanAsync_FoundMultiple(CancellationToken cancellationToken)
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
    IReadOnlyList<SkStackPanDescription>? scanResult = null;

    Assert.DoesNotThrowAsync(async () => {
      scanResult = await client.ActiveScanAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: "0123456789AB".ToByteSequence(),
        scanOptions: SkStackActiveScanOptions.Create(new[] { 1 }),
        cancellationToken: cancellationToken
      );
    });

    Assert.That(scanResult, Is.Not.Null);
    Assert.That(scanResult!.Count, Is.EqualTo(2));

    Assert.That(
      scanResult[0].MacAddress,
      Is.EqualTo(new PhysicalAddress(new byte[] { 0x12, 0x34, 0x56, 0x78, 0xAB, 0xCD, 0xEF, 0x01 }))
    );
    Assert.That(
      scanResult[1].MacAddress,
      Is.EqualTo(new PhysicalAddress(new byte[] { 0x12, 0x34, 0x56, 0x78, 0xAB, 0xCD, 0xEF, 0x02 }))
    );
  }

  [Test]
  [CancelAfter(DefaultTimeOutInMilliseconds)]
  public void ActiveScanAsync_ScanDurations_NotFound(CancellationToken cancellationToken)
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

    IReadOnlyList<SkStackPanDescription>? scanResult = null;

    Assert.DoesNotThrowAsync(async () => {
      scanResult = await client.ActiveScanAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: "0123456789AB".ToByteSequence(),
        scanOptions: SkStackActiveScanOptions.Create(new[] { 2, 4, 6 }),
        cancellationToken: cancellationToken
      );
    });

    Assert.That(scanResult, Is.Not.Null, nameof(scanResult));
    Assert.That(scanResult!, Is.Empty, nameof(scanResult));

    var commands = Encoding.ASCII.GetString(stream.ReadSentData());

    Assert.That(commands, Does.Contain("SKSCAN 2 FFFFFFFF 2"));
    Assert.That(commands, Does.Contain("SKSCAN 2 FFFFFFFF 4"));
    Assert.That(commands, Does.Contain("SKSCAN 2 FFFFFFFF 6"));
  }

  [Test]
  [CancelAfter(DefaultTimeOutInMilliseconds)]
  public void ActiveScanAsync_ScanDurations_Found(CancellationToken cancellationToken)
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
    IReadOnlyList<SkStackPanDescription>? scanResult = null;

    Assert.DoesNotThrowAsync(async () => {
      scanResult = await client.ActiveScanAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: "0123456789AB".ToByteSequence(),
        scanOptions: SkStackActiveScanOptions.Create(new[] { 2, 4, 6 }),
        cancellationToken: cancellationToken
      );
    });

    Assert.That(scanResult, Is.Not.Null);
    Assert.That(scanResult!.Count, Is.EqualTo(1));

    var commands = Encoding.ASCII.GetString(stream.ReadSentData());

    Assert.That(commands, Does.Contain("SKSCAN 2 FFFFFFFF 2"));
    Assert.That(commands, Does.Contain("SKSCAN 2 FFFFFFFF 4"));
    Assert.That(commands, Does.Not.Contain("SKSCAN 2 FFFFFFFF 6"));
  }

  [TestCase(0x_FFFF_FFFFu)]
  [TestCase(0x_0000_0001u)]
  [TestCase(0x_0800_0000u)]
  [CancelAfter(DefaultTimeOutInMilliseconds)]
  public void ActiveScanAsync_ChannelMask(uint channelMask, CancellationToken cancellationToken)
  {
    using var stream = new PseudoSkStackStream();

    // SKSETRBID
    stream.ResponseWriter.WriteLine("OK");
    // SKSETPWD
    stream.ResponseWriter.WriteLine("OK");
    // SKSCAN 2
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
    IReadOnlyList<SkStackPanDescription>? scanResult = null;

    Assert.DoesNotThrowAsync(async () => {
      scanResult = await client.ActiveScanAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: "0123456789AB".ToByteSequence(),
        scanOptions: SkStackActiveScanOptions.Create(new[] { 2 }, channelMask: channelMask),
        cancellationToken: cancellationToken
      );
    });

    Assert.That(scanResult, Is.Not.Null);
    Assert.That(scanResult!.Count, Is.EqualTo(1));

    var commands = Encoding.ASCII.GetString(stream.ReadSentData());

    Assert.That(commands, Does.Contain($"SKSCAN 2 {channelMask:X8} 2"));
  }
}
