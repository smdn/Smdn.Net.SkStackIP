// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Collections;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

using Microsoft.Extensions.Logging;

using NUnit.Framework;
using NUnit.Framework.Constraints;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

partial class SkStackClientFunctionsPanaTests {
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
    const string selfIPv6Address = "FE80:0000:0000:0000:021D:1290:0003:C890";
    const string selfMacAddress = "001D129012345678";
    const string rbid = "00112233445566778899AABBCCDDEEFF";
    const string password = "0123456789AB";
    const int paaChannel = 0x21;
    const int paaPanId = 0x8888;
    const string paaIPv6Address = "FE80:0000:0000:0000:1034:5678:ABCD:EF01";
    const string paaMacAddress = "10345678ABCDEF01";
    const string paaPairId = "12345678";

    bool exceptPanaSessionEstablishmentException;

    switch (eventNumberOfPanaSessionEstablishment) {
      case SkStackEventNumber.PanaSessionEstablishmentError:
        exceptPanaSessionEstablishmentException = true;
        break;

      case SkStackEventNumber.PanaSessionEstablishmentCompleted:
        exceptPanaSessionEstablishmentException = false;
        break;

      default:
        throw new InvalidOperationException("invalid event number");
    }

    // SKSETRBID
    stream.ResponseWriter.WriteLine("OK");
    // SKSETPWD
    stream.ResponseWriter.WriteLine("OK");
    // SKSCAN 2 FFFFFFFF 3
    stream.ResponseWriter.WriteLine("OK");
    stream.ResponseWriter.WriteLine($"EVENT 20 {selfIPv6Address}");
    stream.ResponseWriter.Write($"EPANDESC\r\n  Channel:{paaChannel:X2}\r\n  Channel Page:09\r\n  Pan ID:{paaPanId:X4}\r\n  Addr:{paaMacAddress}\r\n  LQI:E1\r\n  PairID:{paaPairId}\r\n");
    stream.ResponseWriter.WriteLine($"EVENT 22 {selfIPv6Address}");
    // SKLL64
    stream.ResponseWriter.WriteLine(paaIPv6Address);
    // SKADDNBR
    stream.ResponseWriter.WriteLine("OK");
    // SKINFO
    stream.ResponseWriter.WriteLine($"EINFO {selfIPv6Address} {selfMacAddress} {0x22:X2} {0x9999:X4} FFFE");
    stream.ResponseWriter.WriteLine("OK");
    // SKSREG S02 <paa-channel>
    stream.ResponseWriter.WriteLine("OK");
    // SKSREG S03 <pan-id>
    stream.ResponseWriter.WriteLine("OK");
    // SKJOIN
    stream.ResponseWriter.WriteLine("OK");
    stream.ResponseWriter.WriteLine($"EVENT 21 {selfIPv6Address} 02"); // UDP: Neighbor Solcitation
    stream.ResponseWriter.WriteLine($"EVENT 02 {selfIPv6Address}"); // Neighbor Advertisement received
    stream.ResponseWriter.WriteLine($"ERXUDP {selfIPv6Address} {paaIPv6Address} 02CC 02CC {paaMacAddress} 0 0001 0");
    stream.ResponseWriter.WriteLine($"EVENT 21 {selfIPv6Address} 00"); // UDP: ACK
    stream.ResponseWriter.WriteLine($"ERXUDP {selfIPv6Address} {paaIPv6Address} 02CC 02CC {paaMacAddress} 0 0001 0");
    stream.ResponseWriter.WriteLine($"EVENT {(int)eventNumberOfPanaSessionEstablishment:X2} {selfIPv6Address}"); // PANA Session establishment completed/failed

    var client = new SkStackClient(stream, logger: logger);
    using var cts = new CancellationTokenSource(DefaultTimeOut);

    SkStackPanaSessionInfo? panaSession = null;

    Assert.That(
      async () => panaSession = await client.AuthenticateAsPanaClientAsync(
        rbid: rbid.ToByteSequence(),
        password: password.ToByteSequence(),
        scanOptions: SkStackActiveScanOptions.Default,
        cancellationToken: cts.Token
      ),
      exceptPanaSessionEstablishmentException
        ? Throws.TypeOf<SkStackPanaSessionEstablishmentException>()
        : Throws.Nothing
    );

    if (!exceptPanaSessionEstablishmentException) {
      Assert.IsNotNull(panaSession);
      Assert.AreEqual(IPAddress.Parse(selfIPv6Address), panaSession!.LocalAddress, nameof(panaSession.LocalAddress));
      Assert.AreEqual(PhysicalAddress.Parse(selfMacAddress), panaSession!.LocalMacAddress, nameof(panaSession.LocalMacAddress));
      Assert.AreEqual(IPAddress.Parse(paaIPv6Address), panaSession!.PeerAddress, nameof(panaSession.PeerAddress));
      Assert.AreEqual(PhysicalAddress.Parse(paaMacAddress), panaSession!.PeerMacAddress, nameof(panaSession.PeerMacAddress));
      Assert.AreEqual(SkStackChannel.Channels[paaChannel], panaSession!.Channel, nameof(panaSession.Channel));
      Assert.AreEqual(paaPanId, panaSession!.PanId, nameof(panaSession.PanId));
    }

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo(
        (
          $"SKSETRBID {rbid}\r\n" +
          $"SKSETPWD {password.Length:X} {password}\r\n" +
          "SKSCAN 2 FFFFFFFF 3\r\n" +
          $"SKLL64 {paaMacAddress}\r\n" +
          $"SKADDNBR {paaIPv6Address} {paaMacAddress}\r\n" +
          "SKINFO\r\n" +
          $"SKSREG S02 {paaChannel:X2}\r\n" +
          $"SKSREG S03 {paaPanId:X4}\r\n" +
          $"SKJOIN {paaIPv6Address}\r\n"

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
  public void AuthenticateAsPanaClientAsync_WithPAAAddress()
  {
    const string selfIPv6Address = "FE80:0000:0000:0000:021D:1290:0003:C890";
    const string selfMacAddress = "001D129012345678";
    const string rbid = "00112233445566778899AABBCCDDEEFF";
    const string password = "0123456789AB";
    const int paaChannel = 0x21;
    const int paaPanId = 0x8888;
    const string paaIPv6Address = "FE80:0000:0000:0000:1034:5678:ABCD:EF01";
    const string paaMacAddress = "10345678ABCDEF01";

    using var stream = new PseudoSkStackStream();

    // SKSETRBID
    stream.ResponseWriter.WriteLine("OK");
    // SKSETPWD
    stream.ResponseWriter.WriteLine("OK");
    // SKADDNBR
    stream.ResponseWriter.WriteLine("OK");
    // SKINFO
    stream.ResponseWriter.WriteLine($"EINFO {selfIPv6Address} {selfMacAddress} {0x22:X2} {0x9999:X4} FFFE");
    stream.ResponseWriter.WriteLine("OK");
    // SKSREG S02 <paa-channel>
    stream.ResponseWriter.WriteLine("OK");
    // SKSREG S03 <pan-id>
    stream.ResponseWriter.WriteLine("OK");
    // SKJOIN
    stream.ResponseWriter.WriteLine("OK");
    stream.ResponseWriter.WriteLine($"EVENT 21 {selfIPv6Address} 02"); // UDP: Neighbor Solcitation
    stream.ResponseWriter.WriteLine($"EVENT 02 {selfIPv6Address}"); // Neighbor Advertisement received
    stream.ResponseWriter.WriteLine($"ERXUDP {selfIPv6Address} {paaIPv6Address} 02CC 02CC {paaMacAddress} 0 0001 0");
    stream.ResponseWriter.WriteLine($"EVENT 21 {selfIPv6Address} 00"); // UDP: ACK
    stream.ResponseWriter.WriteLine($"ERXUDP {selfIPv6Address} {paaIPv6Address} 02CC 02CC {paaMacAddress} 0 0001 0");
    stream.ResponseWriter.WriteLine($"EVENT 25 {selfIPv6Address}"); // PANA Session establishment completed/failed

    var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    using var cts = new CancellationTokenSource(DefaultTimeOut);

    SkStackPanaSessionInfo? panaSession = null;

    Assert.DoesNotThrowAsync(
      async () => panaSession = await client.AuthenticateAsPanaClientAsync(
        rbid: rbid.ToByteSequence(),
        password: password.ToByteSequence(),
        paaAddress: IPAddress.Parse(paaIPv6Address),
        channelNumber: paaChannel,
        panId: paaPanId,
        cancellationToken: cts.Token
      )
    );

    Assert.IsNotNull(panaSession);
    Assert.AreEqual(IPAddress.Parse(selfIPv6Address), panaSession!.LocalAddress, nameof(panaSession.LocalAddress));
    Assert.AreEqual(PhysicalAddress.Parse(selfMacAddress), panaSession!.LocalMacAddress, nameof(panaSession.LocalMacAddress));
    Assert.AreEqual(IPAddress.Parse(paaIPv6Address), panaSession!.PeerAddress, nameof(panaSession.PeerAddress));
    Assert.AreEqual(PhysicalAddress.Parse(paaMacAddress), panaSession!.PeerMacAddress, nameof(panaSession.PeerMacAddress));
    Assert.AreEqual(SkStackChannel.Channels[paaChannel], panaSession!.Channel, nameof(panaSession.Channel));
    Assert.AreEqual(paaPanId, panaSession!.PanId, nameof(panaSession.PanId));

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo(
        (
          $"SKSETRBID {rbid}\r\n" +
          $"SKSETPWD {password.Length:X} {password}\r\n" +
          $"SKADDNBR {paaIPv6Address} {paaMacAddress}\r\n" +
          "SKINFO\r\n" +
          $"SKSREG S02 {paaChannel:X2}\r\n" +
          $"SKSREG S03 {paaPanId:X4}\r\n" +
          $"SKJOIN {paaIPv6Address}\r\n"

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

  [TestCase("FAIL ER01 error", "FAIL ER01 error", false)]
  [TestCase("OK", "FAIL ER01 error", true)]
  public void AuthenticateAsPanaClientAsync_SetCredentialFailed(
    string responseSKSETRBID,
    string responseSKSETPWD,
    bool expectSKSETPWD
  )
  {
    const string rbid = "00112233445566778899AABBCCDDEEFF";
    const string password = "0123456789AB";

    using var stream = new PseudoSkStackStream();

    // SKSETRBID
    stream.ResponseWriter.WriteLine(responseSKSETRBID);
    // SKSETPWD
    stream.ResponseWriter.WriteLine(responseSKSETPWD);

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    using var cts = new CancellationTokenSource(DefaultTimeOut);

    Assert.ThrowsAsync<SkStackErrorResponseException>(
#pragma warning disable CA2012
      async () => await client.AuthenticateAsPanaClientAsync(
        rbid: rbid.ToByteSequence(),
        password: password.ToByteSequence(),
        scanOptions: null,
        cancellationToken: cts.Token
      )
#pragma warning restore CA2012
    );

    Assert.That(
      stream.ReadSentData(),
      expectSKSETPWD
        ? SequenceIs.EqualTo($"SKSETRBID {rbid}\r\nSKSETPWD {password.Length:X} {password}\r\n".ToByteSequence())
        : SequenceIs.EqualTo($"SKSETRBID {rbid}\r\n".ToByteSequence())
    );
  }

  [TestCase("FE80:0000:0000:0000:FFFF:FFFF:FFFF:FFFF", "FDFFFFFFFFFFFFFF")]
  [TestCase("FE80:0000:0000:0000:021D:1290:1234:5678", "001D129012345678")]
  public void AuthenticateAsPanaClientAsync_AddLinkLocalAddressToNeighborAddressTable(
    string paaAddress,
    string expectedMacAddress
  )
  {
    const string rbid = "00112233445566778899AABBCCDDEEFF";
    const string password = "0123456789AB";

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
#pragma warning disable CA2012
      async () => await client.AuthenticateAsPanaClientAsync(
        rbid: rbid.ToByteSequence(),
        password: password.ToByteSequence(),
        paaAddress: IPAddress.Parse(paaAddress),
        channel: SkStackChannel.Channel33,
        panId: 0,
        cancellationToken: cts.Token
      )
#pragma warning restore CA2012
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo($"SKSETRBID {rbid}\r\nSKSETPWD {password.Length:X} {password}\r\nSKADDNBR {paaAddress} {expectedMacAddress}\r\n".ToByteSequence())
    );
  }

  [TestCase("FDFFFFFFFFFFFFFF", "FE80:0000:0000:0000:FFFF:FFFF:FFFF:FFFF")]
  [TestCase("001D129012345678", "FE80:0000:0000:0000:021D:1290:1234:5678")]
  public void AuthenticateAsPanaClientAsync_ResolvePaaAddressIfMacAddressSupplied(
    string paaMacAddress,
    string paaAddress
  )
  {
    const string rbid = "00112233445566778899AABBCCDDEEFF";
    const string password = "0123456789AB";

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
#pragma warning disable CA2012
      async () => await client.AuthenticateAsPanaClientAsync(
        rbid: rbid.ToByteSequence(),
        password: password.ToByteSequence(),
        paaMacAddress: PhysicalAddress.Parse(paaMacAddress),
        channel: SkStackChannel.Channel33,
        panId: 0,
        cancellationToken: cts.Token
      )
#pragma warning restore CA2012
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo($"SKLL64 {paaMacAddress}\r\nSKSETRBID {rbid}\r\nSKSETPWD {password.Length:X} {password}\r\nSKADDNBR {paaAddress} {paaMacAddress}\r\n".ToByteSequence())
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithoutPAA_ThrowIfPanaSessionAlreadyEstablished()
  {
    using var stream = new PseudoSkStackStream();
    using var client = CreateClientPanaSessionEstablished(stream, CreateLoggerForTestCase());

    Assert.Throws<InvalidOperationException>(
#pragma warning disable CA2012
      () => client.AuthenticateAsPanaClientAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: "0123456789AB".ToByteSequence(),
        scanOptions: null
      )
#pragma warning restore CA2012
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithoutPAA_ScanOptionDefault()
  {
    const string rbid = "00112233445566778899AABBCCDDEEFF";
    const string password = "0123456789AB";

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
        rbid: rbid.ToByteSequence(),
        password: password.ToByteSequence(),
        scanOptions: null,
        cancellationToken: cts.Token
      )
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo(
        (
          $"SKSETRBID {rbid}\r\n" +
          $"SKSETPWD {password.Length:X} {password}\r\n" +
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
    const string rbid = "00112233445566778899AABBCCDDEEFF";
    const string password = "0123456789AB";

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
        rbid: rbid.ToByteSequence(),
        password: password.ToByteSequence(),
        scanOptions: SkStackActiveScanOptions.Create(new[] { 1 }),
        cancellationToken: cts.Token
      )
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo(
        (
          $"SKSETRBID {rbid}\r\n" +
          $"SKSETPWD {password.Length:X} {password}\r\n" +
          "SKSCAN 2 FFFFFFFF 1\r\n"
        ).ToByteSequence()
      )
    );
  }

  private static IEnumerable YieldTestCases_AuthenticateAsPanaClientAsync_WithoutPAA_AddPAAFoundInScanToNeighborAddressTable()
  {
    const string epandesc0Channel = "21";
    const string epandesc0PanID = "8888";
    const string epandesc0Addr = "12345678ABCDEF01";
    const string epandesc0IPv6Addr = "FE80:0000:0000:0000:021D:1290:1234:5678";

    const string epandesc1Channel = "22";
    const string epandesc1PanID = "9999";
    const string epandesc1Addr = "FDFFFFFFFFFFFFFF";
    const string epandesc1IPv6Addr = "FE80:0000:0000:0000:FFFF:FFFF:FFFF:FFFF";

    const string epandesc0 = $"EPANDESC\r\n  Channel:{epandesc0Channel}\r\n  Channel Page:09\r\n  Pan ID:{epandesc0PanID}\r\n  Addr:{epandesc0Addr}\r\n  LQI:E1\r\n  PairID:12345678\r\n";
    const string epandesc1 = $"EPANDESC\r\n  Channel:{epandesc1Channel}\r\n  Channel Page:09\r\n  Pan ID:{epandesc1PanID}\r\n  Addr:{epandesc1Addr}\r\n  LQI:E1\r\n  PairID:12345678\r\n";

    yield return new object[] {
      new[] { epandesc0, epandesc1 },
      epandesc0Channel,
      epandesc0PanID,
      epandesc0Addr,
      epandesc0IPv6Addr
    };
    yield return new object[] {
      new[] { epandesc1, epandesc0 },
      epandesc1Channel,
      epandesc1PanID,
      epandesc1Addr,
      epandesc1IPv6Addr
    };
  }

  [TestCaseSource(nameof(YieldTestCases_AuthenticateAsPanaClientAsync_WithoutPAA_AddPAAFoundInScanToNeighborAddressTable))]
  public void AuthenticateAsPanaClientAsync_WithoutPAA_AddPAAFoundInScanToNeighborAddressTable(
    string[] epandescs,
    string _discard0,
    string _discard1,
    string selectedPAAMacAddress,
    string selectedPAAIPAddress
  )
  {
    const string rbid = "00112233445566778899AABBCCDDEEFF";
    const string password = "0123456789AB";

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
        rbid: rbid.ToByteSequence(),
        password: password.ToByteSequence(),
        scanOptions: SkStackActiveScanOptions.Create(new[] { 1 }),
        cancellationToken: cts.Token
      )
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo(
        (
          $"SKSETRBID {rbid}\r\n" +
          $"SKSETPWD {password.Length:X} {password}\r\n" +
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
    const string rbid = "00112233445566778899AABBCCDDEEFF";
    const string password = "0123456789AB";
    const string paaAddress = "FE80:0000:0000:0000:021D:1290:1234:ABCD";
    const string paaMacAddress = "001D12901234ABCD";

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
        rbid: rbid.ToByteSequence(),
        password: password.ToByteSequence(),
        paaAddress: IPAddress.Parse(paaAddress),
        channel: SkStackChannel.Channels[channel],
        panId: panId,
        cancellationToken: cts.Token
      )
    );

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo(
        (
          $"SKSETRBID {rbid}\r\n" +
          $"SKSETPWD {password.Length:X} {password}\r\n" +
          $"SKADDNBR {paaAddress} {paaMacAddress}\r\n" +
          "SKINFO\r\n" +
          (channelMustBeSet ? $"SKSREG S02 {channel:X}\r\n" : string.Empty) +
          (panIdMustBeSet ? $"SKSREG S03 {panId:X4}\r\n" : string.Empty) +
          $"SKJOIN {paaAddress}\r\n"
        ).ToByteSequence()
      )
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithPAAAddress_ThrowIfPanaSessionAlreadyEstablished()
  {
    using var stream = new PseudoSkStackStream();
    using var client = CreateClientPanaSessionEstablished(stream, CreateLoggerForTestCase());

    Assert.Throws<InvalidOperationException>(
#pragma warning disable CA2012
      () => client.AuthenticateAsPanaClientAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: "0123456789AB".ToByteSequence(),
        paaAddress: IPAddress.IPv6Any,
        channelNumber: SkStackChannel.Channel33.ChannelNumber,
        panId: SkStackRegister.PanId.MinValue
      )
#pragma warning restore CA2012
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithPAAAddress_ArgumentException_PAAAddressNull()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.Throws<ArgumentNullException>(
#pragma warning disable CA2012
      () => client.AuthenticateAsPanaClientAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: "0123456789AB".ToByteSequence(),
        paaAddress: null!,
        channelNumber: SkStackChannel.Channel33.ChannelNumber,
        panId: SkStackRegister.PanId.MinValue
      )
#pragma warning restore CA2012
    );
  }

  [TestCase(32)]
  [TestCase(61)]
  public void AuthenticateAsPanaClientAsync_WithPAAAddress_ArgumentException_ChannelOutOfRange(int channelNumber)
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.Throws<ArgumentOutOfRangeException>(
#pragma warning disable CA2012
      () => client.AuthenticateAsPanaClientAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: "0123456789AB".ToByteSequence(),
        paaAddress: IPAddress.IPv6Any,
        channelNumber: channelNumber,
        panId: SkStackRegister.PanId.MinValue
      )
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

    Assert.Throws<ArgumentOutOfRangeException>(
#pragma warning disable CA2012
      () => client.AuthenticateAsPanaClientAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: "0123456789AB".ToByteSequence(),
        paaAddress: IPAddress.IPv6Any,
        channelNumber: SkStackChannel.Channel33.ChannelNumber,
        panId: panId
      )
#pragma warning restore CA2012
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithPanDescription_ThrowIfPanaSessionAlreadyEstablished()
  {
    using var stream = new PseudoSkStackStream();
    using var client = CreateClientPanaSessionEstablished(stream, CreateLoggerForTestCase());

    Assert.Throws<InvalidOperationException>(
#pragma warning disable CA2012
      () => client.AuthenticateAsPanaClientAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: "0123456789AB".ToByteSequence(),
        pan: default
      )
#pragma warning restore CA2012
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithPanDescription_ArgumentException()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    var pan = default(SkStackPanDescription);

    Assert.That(
#pragma warning disable CA2012
      () => client.AuthenticateAsPanaClientAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: "0123456789AB".ToByteSequence(),
        pan: pan
      ),
#pragma warning restore CA2012
      Throws.InstanceOf<ArgumentException>()
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithPAAMacAddress_ThrowIfPanaSessionAlreadyEstablished()
  {
    using var stream = new PseudoSkStackStream();
    using var client = CreateClientPanaSessionEstablished(stream, CreateLoggerForTestCase());

    Assert.Throws<InvalidOperationException>(
#pragma warning disable CA2012
      () => client.AuthenticateAsPanaClientAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: "0123456789AB".ToByteSequence(),
        paaMacAddress: PhysicalAddress.None,
        channelNumber: SkStackChannel.Channel33.ChannelNumber,
        panId: SkStackRegister.PanId.MinValue
      )
#pragma warning restore CA2012
    );
  }

  [Test]
  public void AuthenticateAsPanaClientAsync_WithPAAMacAddress_ArgumentException_PAAAddressNull()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.Throws<ArgumentNullException>(
#pragma warning disable CA2012
      () => client.AuthenticateAsPanaClientAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: "0123456789AB".ToByteSequence(),
        paaMacAddress: null!,
        channelNumber: SkStackChannel.Channel33.ChannelNumber,
        panId: SkStackRegister.PanId.MinValue
      )
#pragma warning restore CA2012
    );
  }

  [TestCase(32)]
  [TestCase(61)]
  public void AuthenticateAsPanaClientAsync_WithPAAMacAddress_ArgumentException_ChannelOutOfRange(int channelNumber)
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.Throws<ArgumentOutOfRangeException>(
#pragma warning disable CA2012
      () => client.AuthenticateAsPanaClientAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: "0123456789AB".ToByteSequence(),
        paaMacAddress: PhysicalAddress.None,
        channelNumber: channelNumber,
        panId: SkStackRegister.PanId.MinValue
      )
#pragma warning restore CA2012
    );
  }

  [TestCase(-1)]
  [TestCase(0x_1_0000)]
  public void AuthenticateAsPanaClientAsync_WithPAAMacAddress_ArgumentException_PanIdOutOfRange(int panId)
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.Throws<ArgumentOutOfRangeException>(
#pragma warning disable CA2012
      () => client.AuthenticateAsPanaClientAsync(
        rbid: "00112233445566778899AABBCCDDEEFF".ToByteSequence(),
        password: "0123456789AB".ToByteSequence(),
        paaMacAddress: PhysicalAddress.None,
        channelNumber: SkStackChannel.Channel33.ChannelNumber,
        panId: panId
      )
#pragma warning restore CA2012
    );
  }
}
