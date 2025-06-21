// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
using System.Collections;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

using Microsoft.Extensions.Logging;

using NUnit.Framework;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

#pragma warning disable IDE0040
partial class SkStackClientFunctionsPanaTests {
#pragma warning restore IDE0040
  internal static SkStackClient CreateClientPanaSessionEstablished(PseudoSkStackStream stream, ILogger logger)
    => CreateClientAndAuthenticateAsPanaClient(
      stream,
      SkStackEventNumber.PanaSessionEstablishmentCompleted,
      logger
    );

  private static SkStackClient CreateClientAndAuthenticateAsPanaClient(
    PseudoSkStackStream stream,
    SkStackEventNumber eventNumberOfPanaSessionEstablishment,
    ILogger logger
  )
  {
    const string SelfIPv6Address = "FE80:0000:0000:0000:021D:1290:0003:C890";
    const string SelfMacAddress = "001D129012345678";
    const string RBID = "00112233445566778899AABBCCDDEEFF";
    const string Password = "0123456789AB";
    const int PaaChannel = 0x21;
    const int PaaPanId = 0x8888;
    const string PaaIPv6Address = "FE80:0000:0000:0000:1034:5678:ABCD:EF01";
    const string PaaMacAddress = "10345678ABCDEF01";
    const string PaaPairId = "12345678";

    var exceptPanaSessionEstablishmentException = eventNumberOfPanaSessionEstablishment switch {
      SkStackEventNumber.PanaSessionEstablishmentError => true,
      SkStackEventNumber.PanaSessionEstablishmentCompleted => false,
      _ => throw new InvalidOperationException("invalid event number"),
    };

    // SKSETRBID
    stream.ResponseWriter.WriteLine("OK");
    // SKSETPWD
    stream.ResponseWriter.WriteLine("OK");
    // SKSCAN 2 FFFFFFFF 3
    stream.ResponseWriter.WriteLine("OK");
    stream.ResponseWriter.WriteLine($"EVENT 20 {SelfIPv6Address}");
    stream.ResponseWriter.Write($"EPANDESC\r\n  Channel:{PaaChannel:X2}\r\n  Channel Page:09\r\n  Pan ID:{PaaPanId:X4}\r\n  Addr:{PaaMacAddress}\r\n  LQI:E1\r\n  PairID:{PaaPairId}\r\n");
    stream.ResponseWriter.WriteLine($"EVENT 22 {SelfIPv6Address}");
    // SKLL64
    stream.ResponseWriter.WriteLine(PaaIPv6Address);
    // SKADDNBR
    stream.ResponseWriter.WriteLine("OK");
    // SKINFO
    stream.ResponseWriter.WriteLine($"EINFO {SelfIPv6Address} {SelfMacAddress} {0x22:X2} {0x9999:X4} FFFE");
    stream.ResponseWriter.WriteLine("OK");
    // SKSREG S02 <paa-channel>
    stream.ResponseWriter.WriteLine("OK");
    // SKSREG S03 <pan-id>
    stream.ResponseWriter.WriteLine("OK");
    // SKJOIN
    stream.ResponseWriter.WriteLine("OK");
    stream.ResponseWriter.WriteLine($"EVENT 21 {SelfIPv6Address} 02"); // UDP: Neighbor Solicitation
    stream.ResponseWriter.WriteLine($"EVENT 02 {SelfIPv6Address}"); // Neighbor Advertisement received
    stream.ResponseWriter.WriteLine($"ERXUDP {SelfIPv6Address} {PaaIPv6Address} 02CC 02CC {PaaMacAddress} 0 0001 0");
    stream.ResponseWriter.WriteLine($"EVENT 21 {SelfIPv6Address} 00"); // UDP: ACK
    stream.ResponseWriter.WriteLine($"ERXUDP {SelfIPv6Address} {PaaIPv6Address} 02CC 02CC {PaaMacAddress} 0 0001 0");
    stream.ResponseWriter.WriteLine($"EVENT {(int)eventNumberOfPanaSessionEstablishment:X2} {SelfIPv6Address}"); // PANA Session establishment completed/failed

    var client = new SkStackClient(stream, logger: logger);
    using var cts = new CancellationTokenSource(DefaultTimeOut);

    SkStackPanaSessionInfo? panaSession = null;

    Assert.That(
      async () => panaSession = await client.AuthenticateAsPanaClientAsync(
        rbid: RBID.ToByteSequence(),
        password: Password.ToByteSequence(),
        scanOptions: SkStackActiveScanOptions.Default,
        cancellationToken: cts.Token
      ),
      exceptPanaSessionEstablishmentException
        ? Throws
            .TypeOf<SkStackPanaSessionEstablishmentException>()
            .And.Property(nameof(SkStackPanaSessionEstablishmentException.PaaAddress)).EqualTo(IPAddress.Parse(PaaIPv6Address))
            .And.Property(nameof(SkStackPanaSessionEstablishmentException.Channel)).EqualTo(SkStackChannel.Channels[PaaChannel])
            .And.Property(nameof(SkStackPanaSessionEstablishmentException.PanId)).EqualTo(PaaPanId)
            .And.Property(nameof(SkStackPanaSessionEstablishmentException.EventNumber)).EqualTo(eventNumberOfPanaSessionEstablishment)
            .And.Property(nameof(SkStackPanaSessionEstablishmentException.Address)).EqualTo(IPAddress.Parse(SelfIPv6Address))
        : Throws.Nothing
    );

    if (!exceptPanaSessionEstablishmentException) {
      Assert.That(panaSession, Is.Not.Null);
      Assert.That(panaSession!.LocalAddress, Is.EqualTo(IPAddress.Parse(SelfIPv6Address)), nameof(panaSession.LocalAddress));
      Assert.That(panaSession!.LocalMacAddress, Is.EqualTo(PhysicalAddress.Parse(SelfMacAddress)), nameof(panaSession.LocalMacAddress));
      Assert.That(panaSession!.PeerAddress, Is.EqualTo(IPAddress.Parse(PaaIPv6Address)), nameof(panaSession.PeerAddress));
      Assert.That(panaSession!.PeerMacAddress, Is.EqualTo(PhysicalAddress.Parse(PaaMacAddress)), nameof(panaSession.PeerMacAddress));
      Assert.That(panaSession!.Channel, Is.EqualTo(SkStackChannel.Channels[PaaChannel]), nameof(panaSession.Channel));
      Assert.That(panaSession!.PanId, Is.EqualTo(PaaPanId), nameof(panaSession.PanId));
    }

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo(
        (
          $"SKSETRBID {RBID}\r\n" +
          $"SKSETPWD {Password.Length:X} {Password}\r\n" +
          "SKSCAN 2 FFFFFFFF 3\r\n" +
          $"SKLL64 {PaaMacAddress}\r\n" +
          $"SKADDNBR {PaaIPv6Address} {PaaMacAddress}\r\n" +
          "SKINFO\r\n" +
          $"SKSREG S02 {PaaChannel:X2}\r\n" +
          $"SKSREG S03 {PaaPanId:X4}\r\n" +
          $"SKJOIN {PaaIPv6Address}\r\n"

        ).ToByteSequence()
      )
    );

    return client;
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithoutPAA()
  {
    using var stream = new PseudoSkStackStream();
    using var client = CreateClientAndAuthenticateAsPanaClient(
      stream,
      SkStackEventNumber.PanaSessionEstablishmentCompleted,
      CreateLoggerForTestCase()
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithoutPAA_ArgumentException_RBIDEmpty()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    using var cts = new CancellationTokenSource(DefaultTimeOut);

    Assert.That(
      () => client.AuthenticateAsPanaClientAsync(
        rbid: default,
        password: "0123456789AB".ToByteSequence(),
        scanOptions: SkStackActiveScanOptions.Default,
        cancellationToken: cts.Token
      ),
      Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("rbid")
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithoutPAA_ArgumentException_PasswordEmpty()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    using var cts = new CancellationTokenSource(DefaultTimeOut);

    Assert.That(
      () => client.AuthenticateAsPanaClientAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: default,
        scanOptions: SkStackActiveScanOptions.Default,
        cancellationToken: cts.Token
      ),
      Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("password")
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithoutPAA_ArgumentNullException_WriteRBIDNull()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    using var cts = new CancellationTokenSource(DefaultTimeOut);

    Assert.That(
      () => client.AuthenticateAsPanaClientAsync(
        writeRBID: null!,
        writePassword: static _ => throw new NotImplementedException(),
        scanOptions: SkStackActiveScanOptions.Default,
        cancellationToken: cts.Token
      ),
      Throws.ArgumentNullException.With.Property(nameof(ArgumentNullException.ParamName)).EqualTo("writeRBID")
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithoutPAA_ArgumentNullException_WritePasswordNull()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    using var cts = new CancellationTokenSource(DefaultTimeOut);

    Assert.That(
      () => client.AuthenticateAsPanaClientAsync(
        writeRBID: static _ => throw new NotImplementedException(),
        writePassword: null!,
        scanOptions: SkStackActiveScanOptions.Default,
        cancellationToken: cts.Token
      ),
      Throws.ArgumentNullException.With.Property(nameof(ArgumentNullException.ParamName)).EqualTo("writePassword")
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithPAAAddress()
  {
    const string SelfIPv6Address = "FE80:0000:0000:0000:021D:1290:0003:C890";
    const string SelfMacAddress = "001D129012345678";
    const string RBID = "00112233445566778899AABBCCDDEEFF";
    const string Password = "0123456789AB";
    const int PaaChannel = 0x21;
    const int PaaPanId = 0x8888;
    const string PaaIPv6Address = "FE80:0000:0000:0000:1034:5678:ABCD:EF01";
    const string PaaMacAddress = "10345678ABCDEF01";

    using var stream = new PseudoSkStackStream();

    // SKSETRBID
    stream.ResponseWriter.WriteLine("OK");
    // SKSETPWD
    stream.ResponseWriter.WriteLine("OK");
    // SKADDNBR
    stream.ResponseWriter.WriteLine("OK");
    // SKINFO
    stream.ResponseWriter.WriteLine($"EINFO {SelfIPv6Address} {SelfMacAddress} {0x22:X2} {0x9999:X4} FFFE");
    stream.ResponseWriter.WriteLine("OK");
    // SKSREG S02 <paa-channel>
    stream.ResponseWriter.WriteLine("OK");
    // SKSREG S03 <pan-id>
    stream.ResponseWriter.WriteLine("OK");
    // SKJOIN
    stream.ResponseWriter.WriteLine("OK");
    stream.ResponseWriter.WriteLine($"EVENT 21 {SelfIPv6Address} 02"); // UDP: Neighbor Solicitation
    stream.ResponseWriter.WriteLine($"EVENT 02 {SelfIPv6Address}"); // Neighbor Advertisement received
    stream.ResponseWriter.WriteLine($"ERXUDP {SelfIPv6Address} {PaaIPv6Address} 02CC 02CC {PaaMacAddress} 0 0001 0");
    stream.ResponseWriter.WriteLine($"EVENT 21 {SelfIPv6Address} 00"); // UDP: ACK
    stream.ResponseWriter.WriteLine($"ERXUDP {SelfIPv6Address} {PaaIPv6Address} 02CC 02CC {PaaMacAddress} 0 0001 0");
    stream.ResponseWriter.WriteLine($"EVENT 25 {SelfIPv6Address}"); // PANA Session establishment completed/failed

    var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    using var cts = new CancellationTokenSource(DefaultTimeOut);

    SkStackPanaSessionInfo? panaSession = null;

    Assert.DoesNotThrowAsync(
      async () => panaSession = await client.AuthenticateAsPanaClientAsync(
        rbid: RBID.ToByteSequence(),
        password: Password.ToByteSequence(),
        paaAddress: IPAddress.Parse(PaaIPv6Address),
        channelNumber: PaaChannel,
        panId: PaaPanId,
        cancellationToken: cts.Token
      )
    );

    Assert.That(panaSession, Is.Not.Null);
    Assert.That(panaSession!.LocalAddress, Is.EqualTo(IPAddress.Parse(SelfIPv6Address)), nameof(panaSession.LocalAddress));
    Assert.That(panaSession!.LocalMacAddress, Is.EqualTo(PhysicalAddress.Parse(SelfMacAddress)), nameof(panaSession.LocalMacAddress));
    Assert.That(panaSession!.PeerAddress, Is.EqualTo(IPAddress.Parse(PaaIPv6Address)), nameof(panaSession.PeerAddress));
    Assert.That(panaSession!.PeerMacAddress, Is.EqualTo(PhysicalAddress.Parse(PaaMacAddress)), nameof(panaSession.PeerMacAddress));
    Assert.That(panaSession!.Channel, Is.EqualTo(SkStackChannel.Channels[PaaChannel]), nameof(panaSession.Channel));
    Assert.That(panaSession!.PanId, Is.EqualTo(PaaPanId), nameof(panaSession.PanId));

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo(
        (
          $"SKSETRBID {RBID}\r\n" +
          $"SKSETPWD {Password.Length:X} {Password}\r\n" +
          $"SKADDNBR {PaaIPv6Address} {PaaMacAddress}\r\n" +
          "SKINFO\r\n" +
          $"SKSREG S02 {PaaChannel:X2}\r\n" +
          $"SKSREG S03 {PaaPanId:X4}\r\n" +
          $"SKJOIN {PaaIPv6Address}\r\n"

        ).ToByteSequence()
      )
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_PanaSessionCouldNotBeEstablished()
  {
    using var stream = new PseudoSkStackStream();
    using var client = CreateClientAndAuthenticateAsPanaClient(
      stream,
      SkStackEventNumber.PanaSessionEstablishmentError,
      CreateLoggerForTestCase()
    );
  }

  private static IEnumerable YieldTestCases_AuthenticateAsPanaClientAsync_SetRouteBCredential_Fail()
  {
    foreach (var useBufferWriterToSupplyCredential in new[] { true, false }) {
      foreach (var (responseSKSETRBID, responseSKSETPWD, expectSKSETPWD) in new[] {
        ("FAIL ER01 error", "FAIL ER01 error", false),
        ("OK", "FAIL ER01 error", true),
      }) {
        yield return new object?[] { useBufferWriterToSupplyCredential, responseSKSETRBID, responseSKSETPWD, expectSKSETPWD };
      }
    }
  }

  [TestCaseSource(nameof(YieldTestCases_AuthenticateAsPanaClientAsync_SetRouteBCredential_Fail))]
  public void AuthenticateAsPanaClientAsync_SetRouteBCredential_Fail(
    bool useBufferWriterToSupplyCredential,
    string responseSKSETRBID,
    string responseSKSETPWD,
    bool expectSKSETPWD
  )
  {
    const string RBID = "00112233445566778899AABBCCDDEEFF";
    const string Password = "0123456789AB";

    using var stream = new PseudoSkStackStream();

    // SKSETRBID
    stream.ResponseWriter.WriteLine(responseSKSETRBID);
    // SKSETPWD
    stream.ResponseWriter.WriteLine(responseSKSETPWD);

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    using var cts = new CancellationTokenSource(DefaultTimeOut);

    Assert.ThrowsAsync<SkStackErrorResponseException>(
      async () => {
        if (useBufferWriterToSupplyCredential) {
          await client.AuthenticateAsPanaClientAsync(
            writeRBID: writer => writer.Write(RBID.ToByteSequence().Span),
            writePassword: writer => writer.Write(Password.ToByteSequence().Span),
            scanOptions: null,
            cancellationToken: cts.Token
          );
        }
        else {
          await client.AuthenticateAsPanaClientAsync(
            rbid: RBID.ToByteSequence(),
            password: Password.ToByteSequence(),
            scanOptions: null,
            cancellationToken: cts.Token
          );
        }
      }
    );

    Assert.That(
      stream.ReadSentData(),
      expectSKSETPWD
        ? SequenceIs.EqualTo($"SKSETRBID {RBID}\r\nSKSETPWD {Password.Length:X} {Password}\r\n".ToByteSequence())
        : SequenceIs.EqualTo($"SKSETRBID {RBID}\r\n".ToByteSequence())
    );
  }

  [TestCase("FE80:0000:0000:0000:FFFF:FFFF:FFFF:FFFF", "FDFFFFFFFFFFFFFF")]
  [TestCase("FE80:0000:0000:0000:021D:1290:1234:5678", "001D129012345678")]
  public void AuthenticateAsPanaClientAsync_AddLinkLocalAddressToNeighborAddressTable(
    string paaAddress,
    string expectedMacAddress
  )
  {
    const string RBID = "00112233445566778899AABBCCDDEEFF";
    const string Password = "0123456789AB";

    using var stream = new PseudoSkStackStream();

    // SKSETRBID
    stream.ResponseWriter.WriteLine("OK");
    // SKSETPWD
    stream.ResponseWriter.WriteLine("OK");
    // SKADDNBR
    stream.ResponseWriter.WriteLine("FAIL ER01 error");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    using var cts = new CancellationTokenSource(DefaultTimeOut);

    Assert.ThrowsAsync<SkStackErrorResponseException>(
      async () => await client.AuthenticateAsPanaClientAsync(
        rbid: RBID.ToByteSequence(),
        password: Password.ToByteSequence(),
        paaAddress: IPAddress.Parse(paaAddress),
        channel: SkStackChannel.Channel33,
        panId: 0,
        cancellationToken: cts.Token
      )
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo($"SKSETRBID {RBID}\r\nSKSETPWD {Password.Length:X} {Password}\r\nSKADDNBR {paaAddress} {expectedMacAddress}\r\n".ToByteSequence())
    );
  }

  [TestCase("FDFFFFFFFFFFFFFF", "FE80:0000:0000:0000:FFFF:FFFF:FFFF:FFFF")]
  [TestCase("001D129012345678", "FE80:0000:0000:0000:021D:1290:1234:5678")]
  public void AuthenticateAsPanaClientAsync_ResolvePaaAddressIfMacAddressSupplied(
    string paaMacAddress,
    string paaAddress
  )
  {
    const string RBID = "00112233445566778899AABBCCDDEEFF";
    const string Password = "0123456789AB";

    using var stream = new PseudoSkStackStream();

    // SKLL64
    stream.ResponseWriter.WriteLine(paaAddress);
    // SKSETRBID
    stream.ResponseWriter.WriteLine("OK");
    // SKSETPWD
    stream.ResponseWriter.WriteLine("OK");
    // SKADDNBR
    stream.ResponseWriter.WriteLine("FAIL ER01 error");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    using var cts = new CancellationTokenSource(DefaultTimeOut);

    Assert.ThrowsAsync<SkStackErrorResponseException>(
      async () => await client.AuthenticateAsPanaClientAsync(
        rbid: RBID.ToByteSequence(),
        password: Password.ToByteSequence(),
        paaMacAddress: PhysicalAddress.Parse(paaMacAddress),
        channel: SkStackChannel.Channel33,
        panId: 0,
        cancellationToken: cts.Token
      )
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo($"SKLL64 {paaMacAddress}\r\nSKSETRBID {RBID}\r\nSKSETPWD {Password.Length:X} {Password}\r\nSKADDNBR {paaAddress} {paaMacAddress}\r\n".ToByteSequence())
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithoutPAA_ThrowIfPanaSessionAlreadyEstablished()
  {
    using var stream = new PseudoSkStackStream();
    using var client = CreateClientPanaSessionEstablished(stream, CreateLoggerForTestCase());

    Assert.That(
#pragma warning disable CA2012
      () => client.AuthenticateAsPanaClientAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: "0123456789AB".ToByteSequence(),
        scanOptions: null
      ),
      Throws.TypeOf<SkStackPanaSessionStateException>()
#pragma warning restore CA2012
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithoutPAA_ScanOptionDefault()
  {
    const string RBID = "00112233445566778899AABBCCDDEEFF";
    const string Password = "0123456789AB";

    using var stream = new PseudoSkStackStream();

    // SKSETRBID
    stream.ResponseWriter.WriteLine("OK");
    // SKSETPWD
    stream.ResponseWriter.WriteLine("OK");
    // PanaScanOptions.Default is used
    //   SKSCAN 2 FFFFFFFF 3
    //   SKSCAN 2 FFFFFFFF 4
    //   SKSCAN 2 FFFFFFFF 5
    //   SKSCAN 2 FFFFFFFF 6
    //   SKSCAN 2 FFFFFFFF 6
    //   SKSCAN 2 FFFFFFFF 6
    stream.ResponseWriter.WriteLine("OK");
    stream.ResponseWriter.WriteLine("EVENT 22 FE80:0000:0000:0000:021D:1290:0003:C890");
    stream.ResponseWriter.WriteLine("OK");
    stream.ResponseWriter.WriteLine("EVENT 22 FE80:0000:0000:0000:021D:1290:0003:C890");
    stream.ResponseWriter.WriteLine("OK");
    stream.ResponseWriter.WriteLine("EVENT 22 FE80:0000:0000:0000:021D:1290:0003:C890");
    stream.ResponseWriter.WriteLine("OK");
    stream.ResponseWriter.WriteLine("EVENT 22 FE80:0000:0000:0000:021D:1290:0003:C890");
    stream.ResponseWriter.WriteLine("OK");
    stream.ResponseWriter.WriteLine("EVENT 22 FE80:0000:0000:0000:021D:1290:0003:C890");
    stream.ResponseWriter.WriteLine("OK");
    stream.ResponseWriter.WriteLine("EVENT 22 FE80:0000:0000:0000:021D:1290:0003:C890");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    using var cts = new CancellationTokenSource(DefaultTimeOut);

    Assert.ThrowsAsync<InvalidOperationException>(
      async () => await client.AuthenticateAsPanaClientAsync(
        rbid: RBID.ToByteSequence(),
        password: Password.ToByteSequence(),
        scanOptions: null,
        cancellationToken: cts.Token
      )
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo(
        (
          $"SKSETRBID {RBID}\r\n" +
          $"SKSETPWD {Password.Length:X} {Password}\r\n" +
          "SKSCAN 2 FFFFFFFF 3\r\n" +
          "SKSCAN 2 FFFFFFFF 4\r\n" +
          "SKSCAN 2 FFFFFFFF 5\r\n" +
          "SKSCAN 2 FFFFFFFF 6\r\n" +
          "SKSCAN 2 FFFFFFFF 6\r\n" +
          "SKSCAN 2 FFFFFFFF 6\r\n"
        ).ToByteSequence()
      )
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithoutPAA_ScanOptionSupplied()
  {
    const string RBID = "00112233445566778899AABBCCDDEEFF";
    const string Password = "0123456789AB";

    using var stream = new PseudoSkStackStream();

    // SKSETRBID
    stream.ResponseWriter.WriteLine("OK");
    // SKSETPWD
    stream.ResponseWriter.WriteLine("OK");
    // SKSCAN 2 FFFFFFFF 1
    stream.ResponseWriter.WriteLine("OK");
    stream.ResponseWriter.WriteLine("EVENT 22 FE80:0000:0000:0000:021D:1290:0003:C890");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    using var cts = new CancellationTokenSource(DefaultTimeOut);

    Assert.ThrowsAsync<InvalidOperationException>(
      async () => await client.AuthenticateAsPanaClientAsync(
        rbid: RBID.ToByteSequence(),
        password: Password.ToByteSequence(),
        scanOptions: SkStackActiveScanOptions.Create(new[] { 1 }),
        cancellationToken: cts.Token
      )
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo(
        (
          $"SKSETRBID {RBID}\r\n" +
          $"SKSETPWD {Password.Length:X} {Password}\r\n" +
          "SKSCAN 2 FFFFFFFF 1\r\n"
        ).ToByteSequence()
      )
    );
  }

  private static IEnumerable YieldTestCases_AuthenticateAsPanaClientAsync_WithoutPAA_AddPAAFoundInScanToNeighborAddressTable()
  {
    const string EPANDESC0Channel = "21";
    const string EPANDESC0PanID = "8888";
    const string EPANDESC0Addr = "12345678ABCDEF01";
    const string EPANDESC0IPv6Addr = "FE80:0000:0000:0000:021D:1290:1234:5678";

    const string EPANDESC1Channel = "22";
    const string EPANDESC1PanID = "9999";
    const string EPANDESC1Addr = "FDFFFFFFFFFFFFFF";
    const string EPANDESC1IPv6Addr = "FE80:0000:0000:0000:FFFF:FFFF:FFFF:FFFF";

    const string EPANDESC0 = $"EPANDESC\r\n  Channel:{EPANDESC0Channel}\r\n  Channel Page:09\r\n  Pan ID:{EPANDESC0PanID}\r\n  Addr:{EPANDESC0Addr}\r\n  LQI:E1\r\n  PairID:12345678\r\n";
    const string EPANDESC1 = $"EPANDESC\r\n  Channel:{EPANDESC1Channel}\r\n  Channel Page:09\r\n  Pan ID:{EPANDESC1PanID}\r\n  Addr:{EPANDESC1Addr}\r\n  LQI:E1\r\n  PairID:12345678\r\n";

    yield return new object[] {
      new[] { EPANDESC0, EPANDESC1 },
      EPANDESC0Channel,
      EPANDESC0PanID,
      EPANDESC0Addr,
      EPANDESC0IPv6Addr
    };
    yield return new object[] {
      new[] { EPANDESC1, EPANDESC0 },
      EPANDESC1Channel,
      EPANDESC1PanID,
      EPANDESC1Addr,
      EPANDESC1IPv6Addr
    };
  }

  [TestCaseSource(nameof(YieldTestCases_AuthenticateAsPanaClientAsync_WithoutPAA_AddPAAFoundInScanToNeighborAddressTable))]
  public void AuthenticateAsPanaClientAsync_WithoutPAA_AddPAAFoundInScanToNeighborAddressTable(
    string[] epandescs,
#pragma warning disable IDE0060
    string discard0,
    string discard1,
#pragma warning restore IDE0060
    string selectedPAAMacAddress,
    string selectedPAAIPAddress
  )
  {
    const string RBID = "00112233445566778899AABBCCDDEEFF";
    const string Password = "0123456789AB";

    using var stream = new PseudoSkStackStream();

    // SKSETRBID
    stream.ResponseWriter.WriteLine("OK");
    // SKSETPWD
    stream.ResponseWriter.WriteLine("OK");
    // SKSCAN 2 FFFFFFFF 1
    stream.ResponseWriter.WriteLine("OK");
    foreach (var epandesc in epandescs) {
      stream.ResponseWriter.WriteLine("EVENT 20 FE80:0000:0000:0000:021D:1290:0003:C890");
      stream.ResponseWriter.Write(epandesc);
    }
    stream.ResponseWriter.WriteLine("EVENT 22 FE80:0000:0000:0000:021D:1290:0003:C890");
    // SKLL64
    stream.ResponseWriter.WriteLine(selectedPAAIPAddress);
    // SKADDNBR
    stream.ResponseWriter.WriteLine("FAIL ER01 error");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    using var cts = new CancellationTokenSource(DefaultTimeOut);

    Assert.ThrowsAsync<SkStackErrorResponseException>(
      async () => await client.AuthenticateAsPanaClientAsync(
        rbid: RBID.ToByteSequence(),
        password: Password.ToByteSequence(),
        scanOptions: SkStackActiveScanOptions.Create(new[] { 1 }),
        cancellationToken: cts.Token
      )
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo(
        (
          $"SKSETRBID {RBID}\r\n" +
          $"SKSETPWD {Password.Length:X} {Password}\r\n" +
          "SKSCAN 2 FFFFFFFF 1\r\n" +
          $"SKLL64 {selectedPAAMacAddress}\r\n" +
          $"SKADDNBR {selectedPAAIPAddress} {selectedPAAMacAddress}\r\n"
        ).ToByteSequence()
      )
    );
  }

  [TestCase(33, 0x8888, 33, 0x8888)]
  [TestCase(34, 0x8888, 33, 0x8888)]
  [TestCase(33, 0x9999, 33, 0x8888)]
  [TestCase(34, 0x9999, 33, 0x8888)]
  public void AuthenticateAsPanaClientAsync_SetChannelAndPanIdIfNeeded(
    int channel,
    int panId,
    int currentChannel,
    int currentPanId
  )
  {
    const string RBID = "00112233445566778899AABBCCDDEEFF";
    const string Password = "0123456789AB";
    const string PaaAddress = "FE80:0000:0000:0000:021D:1290:1234:ABCD";
    const string PaaMacAddress = "001D12901234ABCD";

    using var stream = new PseudoSkStackStream();

    var channelMustBeSet = channel != currentChannel;
    var panIdMustBeSet = panId != currentPanId;

    // SKSETRBID
    stream.ResponseWriter.WriteLine("OK");
    // SKSETPWD
    stream.ResponseWriter.WriteLine("OK");
    // SKADDNBR
    stream.ResponseWriter.WriteLine("OK");
    // SKINFO
    stream.ResponseWriter.WriteLine($"EINFO FE80:0000:0000:0000:021D:1290:1234:5678 001D129012345678 {currentChannel:X} {currentPanId:X} FFFE");
    stream.ResponseWriter.WriteLine("OK");
    if (channelMustBeSet)
      // SKSREG S02 ????
      stream.ResponseWriter.WriteLine("OK");
    if (panIdMustBeSet)
      // SKSREG S03 ????
      stream.ResponseWriter.WriteLine("OK");
    // SKJOIN
    stream.ResponseWriter.WriteLine("FAIL ER01 error");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    using var cts = new CancellationTokenSource(DefaultTimeOut);

    Assert.ThrowsAsync<SkStackErrorResponseException>(
      async () => await client.AuthenticateAsPanaClientAsync(
        rbid: RBID.ToByteSequence(),
        password: Password.ToByteSequence(),
        paaAddress: IPAddress.Parse(PaaAddress),
        channel: SkStackChannel.Channels[channel],
        panId: panId,
        cancellationToken: cts.Token
      )
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo(
        (
          $"SKSETRBID {RBID}\r\n" +
          $"SKSETPWD {Password.Length:X} {Password}\r\n" +
          $"SKADDNBR {PaaAddress} {PaaMacAddress}\r\n" +
          "SKINFO\r\n" +
          (channelMustBeSet ? $"SKSREG S02 {channel:X}\r\n" : string.Empty) +
          (panIdMustBeSet ? $"SKSREG S03 {panId:X4}\r\n" : string.Empty) +
          $"SKJOIN {PaaAddress}\r\n"
        ).ToByteSequence()
      )
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithPAAAddress_ThrowIfPanaSessionAlreadyEstablished()
  {
    using var stream = new PseudoSkStackStream();
    using var client = CreateClientPanaSessionEstablished(stream, CreateLoggerForTestCase());

    Assert.That(
#pragma warning disable CA2012
      () => client.AuthenticateAsPanaClientAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: "0123456789AB".ToByteSequence(),
        paaAddress: IPAddress.IPv6Any,
        channelNumber: SkStackChannel.Channel33.ChannelNumber,
        panId: SkStackRegister.PanId.MinValue
      ),
      Throws.TypeOf<SkStackPanaSessionStateException>()
#pragma warning restore CA2012
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithPAAAddress_ArgumentException_PAAAddressNull()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.That(
#pragma warning disable CA2012
      () => client.AuthenticateAsPanaClientAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: "0123456789AB".ToByteSequence(),
        paaAddress: null!,
        channelNumber: SkStackChannel.Channel33.ChannelNumber,
        panId: SkStackRegister.PanId.MinValue
      ),
      Throws.ArgumentNullException
#pragma warning restore CA2012
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithPAAAddress_ArgumentException_RBIDEmpty()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    using var cts = new CancellationTokenSource(DefaultTimeOut);

    Assert.That(
      () => client.AuthenticateAsPanaClientAsync(
        rbid: default,
        password: "0123456789AB".ToByteSequence(),
        paaAddress: IPAddress.IPv6Any,
        channelNumber: SkStackChannel.Channel33.ChannelNumber,
        panId: SkStackRegister.PanId.MinValue,
        cancellationToken: cts.Token
      ),
      Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("rbid")
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithPAAAddress_ArgumentException_PasswordEmpty()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    using var cts = new CancellationTokenSource(DefaultTimeOut);

    Assert.That(
      () => client.AuthenticateAsPanaClientAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: default,
        paaAddress: IPAddress.IPv6Any,
        channelNumber: SkStackChannel.Channel33.ChannelNumber,
        panId: SkStackRegister.PanId.MinValue,
        cancellationToken: cts.Token
      ),
      Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("password")
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithPAAAddress_ArgumentNullException_WriteRBIDNull()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    using var cts = new CancellationTokenSource(DefaultTimeOut);

    Assert.That(
      () => client.AuthenticateAsPanaClientAsync(
        writeRBID: null!,
        writePassword: static _ => throw new NotImplementedException(),
        paaAddress: IPAddress.IPv6Any,
        channel: SkStackChannel.Channel33,
        panId: SkStackRegister.PanId.MinValue,
        cancellationToken: cts.Token
      ),
      Throws.ArgumentNullException.With.Property(nameof(ArgumentNullException.ParamName)).EqualTo("writeRBID")
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithPAAAddress_ArgumentNullException_WritePasswordNull()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    using var cts = new CancellationTokenSource(DefaultTimeOut);

    Assert.That(
      () => client.AuthenticateAsPanaClientAsync(
        writeRBID: static _ => throw new NotImplementedException(),
        writePassword: null!,
        paaAddress: IPAddress.IPv6Any,
        channel: SkStackChannel.Channel33,
        panId: SkStackRegister.PanId.MinValue,
        cancellationToken: cts.Token
      ),
      Throws.ArgumentNullException.With.Property(nameof(ArgumentNullException.ParamName)).EqualTo("writePassword")
    );
  }

  [TestCase(32)]
  [TestCase(61)]
  public void AuthenticateAsPanaClientAsync_WithPAAAddress_ArgumentException_ChannelOutOfRange(int channelNumber)
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.That(
#pragma warning disable CA2012
      () => client.AuthenticateAsPanaClientAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: "0123456789AB".ToByteSequence(),
        paaAddress: IPAddress.IPv6Any,
        channelNumber: channelNumber,
        panId: SkStackRegister.PanId.MinValue
      ),
      Throws.TypeOf<ArgumentOutOfRangeException>()
#pragma warning restore CA2012
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_AuthenticateAsPanaClientAsync_WithPAAAddress_NotSupportedException_AddressIsNotIPv6LinkLocal()
  {
    yield return new object[] { IPAddress.None };
    yield return new object[] { IPAddress.Loopback };
    yield return new object[] { IPAddress.IPv6None };
    yield return new object[] { IPAddress.IPv6Loopback };
  }

  [TestCaseSource(nameof(YieldTestCases_AuthenticateAsPanaClientAsync_WithPAAAddress_NotSupportedException_AddressIsNotIPv6LinkLocal))]
  public void AuthenticateAsPanaClientAsync_WithPAAAddress_NotSupportedException_AddressIsNotIPv6LinkLocal(
    IPAddress paaAddress
  )
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.ThrowsAsync<NotSupportedException>(
      async () => await client.AuthenticateAsPanaClientAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: "0123456789AB".ToByteSequence(),
        paaAddress: paaAddress,
        channelNumber: SkStackChannel.Channel33.ChannelNumber,
        panId: 0x1234
      )
    );
  }

  [TestCase(-1)]
  [TestCase(0x_1_0000)]
  public void AuthenticateAsPanaClientAsync_WithPAAAddress_ArgumentException_PanIdOutOfRange(int panId)
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.That(
#pragma warning disable CA2012
      () => client.AuthenticateAsPanaClientAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: "0123456789AB".ToByteSequence(),
        paaAddress: IPAddress.IPv6Any,
        channelNumber: SkStackChannel.Channel33.ChannelNumber,
        panId: panId
      ),
      Throws.TypeOf<ArgumentOutOfRangeException>()
#pragma warning restore CA2012
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithPanDescription_ThrowIfPanaSessionAlreadyEstablished()
  {
    using var stream = new PseudoSkStackStream();
    using var client = CreateClientPanaSessionEstablished(stream, CreateLoggerForTestCase());

    Assert.That(
#pragma warning disable CA2012
      () => client.AuthenticateAsPanaClientAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: "0123456789AB".ToByteSequence(),
        pan: default
      ),
      Throws.TypeOf<SkStackPanaSessionStateException>()
#pragma warning restore CA2012
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithPanDescription_ArgumentException_InvalidPanDescription()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    var pan = default(SkStackPanDescription);

    Assert.That(
      () => client.AuthenticateAsPanaClientAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: "0123456789AB".ToByteSequence(),
        pan: pan
      ),
      Throws.InstanceOf<ArgumentException>()
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithPanDescription_ArgumentException_RBIDEmpty()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    using var cts = new CancellationTokenSource(DefaultTimeOut);

    Assert.That(
      () => client.AuthenticateAsPanaClientAsync(
        rbid: default,
        password: "0123456789AB".ToByteSequence(),
        pan: default,
        cancellationToken: cts.Token
      ),
      Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("rbid")
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithPanDescription_ArgumentException_PasswordEmpty()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    using var cts = new CancellationTokenSource(DefaultTimeOut);

    Assert.That(
      () => client.AuthenticateAsPanaClientAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: default,
        pan: default,
        cancellationToken: cts.Token
      ),
      Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("password")
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithPanDescription_ArgumentNullException_WriteRBIDNull()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    using var cts = new CancellationTokenSource(DefaultTimeOut);

    Assert.That(
      () => client.AuthenticateAsPanaClientAsync(
        writeRBID: null!,
        writePassword: static _ => throw new NotImplementedException(),
        pan: default,
        cancellationToken: cts.Token
      ),
      Throws.ArgumentNullException.With.Property(nameof(ArgumentNullException.ParamName)).EqualTo("writeRBID")
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithPanDescription_ArgumentNullException_WritePasswordNull()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    using var cts = new CancellationTokenSource(DefaultTimeOut);

    Assert.That(
      () => client.AuthenticateAsPanaClientAsync(
        writeRBID: static _ => throw new NotImplementedException(),
        writePassword: null!,
        pan: default,
        cancellationToken: cts.Token
      ),
      Throws.ArgumentNullException.With.Property(nameof(ArgumentNullException.ParamName)).EqualTo("writePassword")
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithPAAMacAddress_ThrowIfPanaSessionAlreadyEstablished()
  {
    using var stream = new PseudoSkStackStream();
    using var client = CreateClientPanaSessionEstablished(stream, CreateLoggerForTestCase());

    Assert.That(
#pragma warning disable CA2012
      () => client.AuthenticateAsPanaClientAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: "0123456789AB".ToByteSequence(),
        paaMacAddress: PhysicalAddress.None,
        channelNumber: SkStackChannel.Channel33.ChannelNumber,
        panId: SkStackRegister.PanId.MinValue
      ),
      Throws.TypeOf<SkStackPanaSessionStateException>()
#pragma warning restore CA2012
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithPAAMacAddress_ArgumentException_RBIDEmpty()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    using var cts = new CancellationTokenSource(DefaultTimeOut);

    Assert.That(
      () => client.AuthenticateAsPanaClientAsync(
        rbid: default,
        password: "0123456789AB".ToByteSequence(),
        paaMacAddress: PhysicalAddress.None,
        channelNumber: SkStackChannel.Channel33.ChannelNumber,
        panId: SkStackRegister.PanId.MinValue,
        cancellationToken: cts.Token
      ),
      Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("rbid")
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithPAAMacAddress_ArgumentException_PasswordEmpty()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    using var cts = new CancellationTokenSource(DefaultTimeOut);

    Assert.That(
      () => client.AuthenticateAsPanaClientAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: default,
        paaMacAddress: PhysicalAddress.None,
        channelNumber: SkStackChannel.Channel33.ChannelNumber,
        panId: SkStackRegister.PanId.MinValue,
        cancellationToken: cts.Token
      ),
      Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("password")
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithPAAMacAddress_ArgumentNullException_WriteRBIDNull()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    using var cts = new CancellationTokenSource(DefaultTimeOut);

    Assert.That(
      () => client.AuthenticateAsPanaClientAsync(
        writeRBID: null!,
        writePassword: static _ => throw new NotImplementedException(),
        paaMacAddress: PhysicalAddress.None,
        channel: SkStackChannel.Channel33,
        panId: SkStackRegister.PanId.MinValue,
        cancellationToken: cts.Token
      ),
      Throws.ArgumentNullException.With.Property(nameof(ArgumentNullException.ParamName)).EqualTo("writeRBID")
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithPAAMacAddress_ArgumentNullException_WritePasswordNull()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    using var cts = new CancellationTokenSource(DefaultTimeOut);

    Assert.That(
      () => client.AuthenticateAsPanaClientAsync(
        writeRBID: static _ => throw new NotImplementedException(),
        writePassword: null!,
        paaMacAddress: PhysicalAddress.None,
        channel: SkStackChannel.Channel33,
        panId: SkStackRegister.PanId.MinValue,
        cancellationToken: cts.Token
      ),
      Throws.ArgumentNullException.With.Property(nameof(ArgumentNullException.ParamName)).EqualTo("writePassword")
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithPAAMacAddress_ArgumentException_PAAAddressNull()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.That(
#pragma warning disable CA2012
      () => client.AuthenticateAsPanaClientAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: "0123456789AB".ToByteSequence(),
        paaMacAddress: null!,
        channelNumber: SkStackChannel.Channel33.ChannelNumber,
        panId: SkStackRegister.PanId.MinValue
      ),
      Throws.ArgumentNullException
#pragma warning restore CA2012
    );
  }

  [TestCase(32)]
  [TestCase(61)]
  public void AuthenticateAsPanaClientAsync_WithPAAMacAddress_ArgumentException_ChannelOutOfRange(int channelNumber)
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.That(
#pragma warning disable CA2012
      () => client.AuthenticateAsPanaClientAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: "0123456789AB".ToByteSequence(),
        paaMacAddress: PhysicalAddress.None,
        channelNumber: channelNumber,
        panId: SkStackRegister.PanId.MinValue
      ),
      Throws.TypeOf<ArgumentOutOfRangeException>()
#pragma warning restore CA2012
    );
  }

  [TestCase(-1)]
  [TestCase(0x_1_0000)]
  public void AuthenticateAsPanaClientAsync_WithPAAMacAddress_ArgumentException_PanIdOutOfRange(int panId)
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.That(
#pragma warning disable CA2012
      () => client.AuthenticateAsPanaClientAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: "0123456789AB".ToByteSequence(),
        paaMacAddress: PhysicalAddress.None,
        channelNumber: SkStackChannel.Channel33.ChannelNumber,
        panId: panId
      ),
      Throws.TypeOf<ArgumentOutOfRangeException>()
#pragma warning restore CA2012
    );
  }
}
