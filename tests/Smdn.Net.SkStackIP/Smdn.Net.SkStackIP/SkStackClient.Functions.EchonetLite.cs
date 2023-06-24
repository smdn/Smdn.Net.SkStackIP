// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using Is = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientFunctionsEchonetLiteTests : SkStackClientTestsBase {
  private static readonly TimeSpan DefaultTimeOut = TimeSpan.FromSeconds(5);

  [Test]
  public void ReceiveUdpEchonetLiteAsync_BufferNull()
  {
    using var client = new SkStackClient(Stream.Null, logger: CreateLoggerForTestCase());

    Assert.ThrowsAsync<ArgumentNullException>(
      async () => await client.ReceiveUdpEchonetLiteAsync(buffer: null!)
    );
  }

  [TestCase(true)]
  [TestCase(false)]
  public void ReceiveUdpEchonetLiteAsync(bool startCapturingExplicitly)
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    if (startCapturingExplicitly)
      client.StartCapturingUdpReceiveEvents(SkStackKnownPortNumbers.EchonetLite);

    Assert.AreEqual(client.ERXUDPDataFormat, SkStackERXUDPDataFormat.Binary);

    const string remoteAddressString = "FE80:0000:0000:0000:021D:1290:1111:2222";

    stream.ResponseWriter.WriteLine($"ERXUDP {remoteAddressString} FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0E1A 001D129012345679 0 0008 01234567");

    using var cts = new CancellationTokenSource(DefaultTimeOut);
    var buffer = new ArrayBufferWriter<byte>();

    IPAddress? remoteAddress = null;

    Assert.DoesNotThrowAsync(
      async () => remoteAddress = await client.ReceiveUdpEchonetLiteAsync(
        buffer: buffer,
        cts.Token
      )
    );

    Assert.IsNotNull(remoteAddress);
    Assert.AreEqual(IPAddress.Parse(remoteAddressString), remoteAddress, nameof(remoteAddress));
    Assert.That(
      buffer.WrittenMemory,
      Is.EqualTo("01234567".ToByteSequence()),
      nameof(buffer.WrittenMemory)
    );
  }

  [Test]
  public async Task SendUdpEchonetLiteAsync_PanaSessionNotEstablished()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    // SKTABLE E
    stream.ResponseWriter.WriteLine("EPORT");
    stream.ResponseWriter.WriteLine($"{SkStackKnownPortNumbers.EchonetLite}");
    stream.ResponseWriter.WriteLine("0");
    stream.ResponseWriter.WriteLine("0");
    stream.ResponseWriter.WriteLine("0");
    stream.ResponseWriter.WriteLine("0");
    stream.ResponseWriter.WriteLine("0");
    stream.ResponseWriter.WriteLine("OK");

    var response = await client.SendSKTABLEListeningPortListAsync();

    CollectionAssert.IsNotEmpty(response.Payload!.Where(static p => p.Port == SkStackKnownPortNumbers.EchonetLite));

    Assert.IsNull(client.PanaSessionPeerAddress, nameof(client.PanaSessionPeerAddress));
    Assert.IsFalse(client.IsPanaSessionAlive, nameof(client.IsPanaSessionAlive));

    Assert.ThrowsAsync<InvalidOperationException>(
      async () => await client.SendUdpEchonetLiteAsync(Array.Empty<byte>())
    );
  }

  [Test]
  public async Task SendUdpEchonetLiteAsync_NoPortHandleAssignedToEchonetLite()
  {
    using var stream = new PseudoSkStackStream();
    using var client = SkStackClientFunctionsPanaTests.CreateClientPanaSessionEstablished(stream, logger: CreateLoggerForTestCase());

    Assert.IsNotNull(client.PanaSessionPeerAddress, nameof(client.PanaSessionPeerAddress));
    Assert.IsTrue(client.IsPanaSessionAlive, nameof(client.IsPanaSessionAlive));

    // SKTABLE E
    stream.ResponseWriter.WriteLine("EPORT");
    stream.ResponseWriter.WriteLine("0");
    stream.ResponseWriter.WriteLine("0");
    stream.ResponseWriter.WriteLine("0");
    stream.ResponseWriter.WriteLine("0");
    stream.ResponseWriter.WriteLine("0");
    stream.ResponseWriter.WriteLine("0");
    stream.ResponseWriter.WriteLine("OK");

    var response = await client.SendSKTABLEListeningPortListAsync();

    CollectionAssert.IsEmpty(response.Payload!.Where(static p => p.Port == SkStackKnownPortNumbers.EchonetLite));

    Assert.ThrowsAsync<InvalidOperationException>(
      async () => await client.SendUdpEchonetLiteAsync(Array.Empty<byte>())
    );
  }

  [Test]
  public async Task SendUdpEchonetLiteAsync(
    [Values(SkStackUdpPortHandle.Handle1, SkStackUdpPortHandle.Handle3, SkStackUdpPortHandle.Handle6)] SkStackUdpPortHandle handleForEchonetLite
  )
  {
    using var stream = new PseudoSkStackStream();
    using var client = SkStackClientFunctionsPanaTests.CreateClientPanaSessionEstablished(stream, logger: CreateLoggerForTestCase());

    Assert.IsNotNull(client.PanaSessionPeerAddress, nameof(client.PanaSessionPeerAddress));
    Assert.IsTrue(client.IsPanaSessionAlive, nameof(client.IsPanaSessionAlive));

    // SKTABLE E
    stream.ResponseWriter.WriteLine("EPORT");
    for (var handleNumber = 1; handleNumber <= 6; handleNumber++) {
      stream.ResponseWriter.WriteLine(handleNumber == (int)handleForEchonetLite ? $"{SkStackKnownPortNumbers.EchonetLite}" : "0");
    }
    stream.ResponseWriter.WriteLine("OK");

    var response = await client.SendSKTABLEListeningPortListAsync();

    CollectionAssert.IsNotEmpty(response.Payload!.Where(static p => p.Port == SkStackKnownPortNumbers.EchonetLite));

    stream.ClearSentData();

    // SKSENDTO
    stream.ResponseWriter.WriteLine("OK");

    using var cts = new CancellationTokenSource(DefaultTimeOut);
    var buffer = Encoding.ASCII.GetBytes("012345");

    Assert.DoesNotThrowAsync(
      async () => await client.SendUdpEchonetLiteAsync(buffer, cts.Token)
    );

    var expectedDestinationAddress = client.PanaSessionPeerAddress!.ToLongFormatString();

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo($"SKSENDTO {(int)handleForEchonetLite} {expectedDestinationAddress} {SkStackKnownPortNumbers.EchonetLite:X4} {(int)SkStackUdpEncryption.ForceEncrypt} {buffer.Length:X4} {Encoding.ASCII.GetString(buffer)}".ToByteSequence())
    );
  }
}
