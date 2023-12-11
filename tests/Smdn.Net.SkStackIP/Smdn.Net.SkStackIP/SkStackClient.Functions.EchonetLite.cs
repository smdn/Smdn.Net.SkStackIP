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

using Polly;
using Polly.Retry;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

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

    Assert.That(client.ERXUDPDataFormat, Is.EqualTo(SkStackERXUDPDataFormat.Binary));

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

    Assert.That(remoteAddress, Is.Not.Null);
    Assert.That(remoteAddress, Is.EqualTo(IPAddress.Parse(remoteAddressString)), nameof(remoteAddress));
    Assert.That(
      buffer.WrittenMemory,
      SequenceIs.EqualTo("01234567".ToByteSequence()),
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

    Assert.That(response.Payload!.Where(static p => p.Port == SkStackKnownPortNumbers.EchonetLite), Is.Not.Empty);

    Assert.That(client.PanaSessionPeerAddress, Is.Null, nameof(client.PanaSessionPeerAddress));
    Assert.That(client.IsPanaSessionAlive, Is.False, nameof(client.IsPanaSessionAlive));

    Assert.ThrowsAsync<InvalidOperationException>(
      async () => await client.SendUdpEchonetLiteAsync(Array.Empty<byte>())
    );
  }

  [Test]
  public async Task SendUdpEchonetLiteAsync_NoPortHandleAssignedToEchonetLite()
  {
    using var stream = new PseudoSkStackStream();
    using var client = SkStackClientFunctionsPanaTests.CreateClientPanaSessionEstablished(stream, logger: CreateLoggerForTestCase());

    Assert.That(client.PanaSessionPeerAddress, Is.Not.Null, nameof(client.PanaSessionPeerAddress));
    Assert.That(client.IsPanaSessionAlive, Is.True, nameof(client.IsPanaSessionAlive));

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

    Assert.That(response.Payload!.Where(static p => p.Port == SkStackKnownPortNumbers.EchonetLite), Is.Empty);

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

    Assert.That(client.PanaSessionPeerAddress, Is.Not.Null, nameof(client.PanaSessionPeerAddress));
    Assert.That(client.IsPanaSessionAlive, Is.True, nameof(client.IsPanaSessionAlive));

    // SKTABLE E
    stream.ResponseWriter.WriteLine("EPORT");
    for (var handleNumber = 1; handleNumber <= 6; handleNumber++) {
      stream.ResponseWriter.WriteLine(handleNumber == (int)handleForEchonetLite ? $"{SkStackKnownPortNumbers.EchonetLite}" : "0");
    }
    stream.ResponseWriter.WriteLine("OK");

    var response = await client.SendSKTABLEListeningPortListAsync();

    Assert.That(response.Payload!.Where(static p => p.Port == SkStackKnownPortNumbers.EchonetLite), Is.Not.Empty);

    stream.ClearSentData();

    // SKSENDTO
    var senderAddress = client.PanaSessionPeerAddress!.ToLongFormatString();

    stream.ResponseWriter.WriteLine($"EVENT 21 {senderAddress} 00");
    stream.ResponseWriter.WriteLine("OK");

    using var cts = new CancellationTokenSource(DefaultTimeOut);
    var buffer = Encoding.ASCII.GetBytes("012345");

    Assert.DoesNotThrowAsync(
      async () => await client.SendUdpEchonetLiteAsync(buffer, resiliencePipeline: null, cts.Token)
    );

    var expectedDestinationAddress = client.PanaSessionPeerAddress!.ToLongFormatString();

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo($"SKSENDTO {(int)handleForEchonetLite} {expectedDestinationAddress} {SkStackKnownPortNumbers.EchonetLite:X4} {(int)SkStackUdpEncryption.ForceEncrypt} {buffer.Length:X4} {Encoding.ASCII.GetString(buffer)}".ToByteSequence())
    );
  }

  [Test]
  public async Task SendUdpEchonetLiteAsync_FailedWithEVENT21PARAM1()
  {
    const SkStackUdpPortHandle handleForEchonetLite = SkStackUdpPortHandle.Handle1;

    using var stream = new PseudoSkStackStream();
    using var client = SkStackClientFunctionsPanaTests.CreateClientPanaSessionEstablished(stream, logger: CreateLoggerForTestCase());

    Assert.That(client.PanaSessionPeerAddress, Is.Not.Null, nameof(client.PanaSessionPeerAddress));
    Assert.That(client.IsPanaSessionAlive, Is.True, nameof(client.IsPanaSessionAlive));

    // SKTABLE E
    stream.ResponseWriter.WriteLine("EPORT");
    for (var handleNumber = 1; handleNumber <= 6; handleNumber++) {
      stream.ResponseWriter.WriteLine(handleNumber == (int)handleForEchonetLite ? $"{SkStackKnownPortNumbers.EchonetLite}" : "0");
    }
    stream.ResponseWriter.WriteLine("OK");

    var response = await client.SendSKTABLEListeningPortListAsync();

    Assert.That(response.Payload!.Where(static p => p.Port == SkStackKnownPortNumbers.EchonetLite), Is.Not.Empty);

    stream.ClearSentData();

    // SKSENDTO
    var senderAddress = client.PanaSessionPeerAddress!.ToLongFormatString();

    stream.ResponseWriter.WriteLine($"EVENT 21 {senderAddress} 01");
    stream.ResponseWriter.WriteLine("OK");

    using var cts = new CancellationTokenSource(DefaultTimeOut);
    var buffer = Encoding.ASCII.GetBytes("012345");

    var ex = Assert.ThrowsAsync<SkStackUdpSendFailedException>(
      async () => await client.SendUdpEchonetLiteAsync(buffer, resiliencePipeline: null, cts.Token)
    )!;

    Assert.That(ex.PeerAddress, Is.EqualTo(client.PanaSessionPeerAddress!), nameof(ex.PeerAddress));
    Assert.That(ex.PortHandle, Is.EqualTo(handleForEchonetLite), nameof(ex.PortHandle));

    var expectedDestinationAddress = client.PanaSessionPeerAddress!.ToLongFormatString();

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo($"SKSENDTO {(int)handleForEchonetLite} {expectedDestinationAddress} {SkStackKnownPortNumbers.EchonetLite:X4} {(int)SkStackUdpEncryption.ForceEncrypt} {buffer.Length:X4} {Encoding.ASCII.GetString(buffer)}".ToByteSequence())
    );
  }

  [Test]
  public async Task SendUdpEchonetLiteAsync_ResilienceStrategy()
  {
    const SkStackUdpPortHandle handleForEchonetLite = SkStackUdpPortHandle.Handle1;
    const int maxSendAttempt = 3;

    using var stream = new PseudoSkStackStream();
    using var client = SkStackClientFunctionsPanaTests.CreateClientPanaSessionEstablished(stream, logger: CreateLoggerForTestCase());

    Assert.That(client.PanaSessionPeerAddress, Is.Not.Null, nameof(client.PanaSessionPeerAddress));
    Assert.That(client.IsPanaSessionAlive, Is.True, nameof(client.IsPanaSessionAlive));

    // SKTABLE E
    stream.ResponseWriter.WriteLine("EPORT");
    for (var handleNumber = 1; handleNumber <= 6; handleNumber++) {
      stream.ResponseWriter.WriteLine(handleNumber == (int)handleForEchonetLite ? $"{SkStackKnownPortNumbers.EchonetLite}" : "0");
    }
    stream.ResponseWriter.WriteLine("OK");

    var response = await client.SendSKTABLEListeningPortListAsync();

    Assert.That(response.Payload!.Where(static p => p.Port == SkStackKnownPortNumbers.EchonetLite), Is.Not.Empty);

    stream.ClearSentData();

    // SKSENDTO
    var senderAddress = client.PanaSessionPeerAddress!.ToLongFormatString();

    for (var i = 0; i < maxSendAttempt; i++) {
      stream.ResponseWriter.WriteLine($"EVENT 21 {senderAddress} 01");
      stream.ResponseWriter.WriteLine("OK");
    }

    var resiliencePipeline = new ResiliencePipelineBuilder()
      .AddRetry(
        new RetryStrategyOptions {
          ShouldHandle = new PredicateBuilder().Handle<SkStackUdpSendFailedException>(),
          MaxRetryAttempts = maxSendAttempt - 1,
          Delay = TimeSpan.FromSeconds(0),
        }
      )
      .Build();

    using var cts = new CancellationTokenSource(DefaultTimeOut);
    var buffer = Encoding.ASCII.GetBytes("012345");

    var ex = Assert.ThrowsAsync<SkStackUdpSendFailedException>(
      async () => await client.SendUdpEchonetLiteAsync(buffer, resiliencePipeline: resiliencePipeline, cts.Token)
    )!;

    Assert.That(ex.PeerAddress, Is.EqualTo(client.PanaSessionPeerAddress!), nameof(ex.PeerAddress));
    Assert.That(ex.PortHandle, Is.EqualTo(handleForEchonetLite), nameof(ex.PortHandle));

    var expectedDestinationAddress = client.PanaSessionPeerAddress!.ToLongFormatString();

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo(
        string.Concat(
          Enumerable.Repeat(
            $"SKSENDTO {(int)handleForEchonetLite} {expectedDestinationAddress} {SkStackKnownPortNumbers.EchonetLite:X4} {(int)SkStackUdpEncryption.ForceEncrypt} {buffer.Length:X4} {Encoding.ASCII.GetString(buffer)}",
            count: maxSendAttempt
          )
        ).ToByteSequence()
      )
    );
  }
}
