// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientFunctionsUdpTests : SkStackClientTestsBase {
  private class ERXUDPDataFormatSkStackClient : SkStackClient {
    public ERXUDPDataFormatSkStackClient()
      : base(
        stream: Stream.Null,
        leaveStreamOpen: true,
        logger: null
      )
    {
    }

    public void SetERXUDPDataFormat(SkStackERXUDPDataFormat newFormat)
      => ERXUDPDataFormat = newFormat;
  }

  [TestCase(SkStackERXUDPDataFormat.Binary)]
  [TestCase(SkStackERXUDPDataFormat.HexAsciiText)]
  public void ERXUDPDataFormat_Set(SkStackERXUDPDataFormat format)
  {
    using var client = new ERXUDPDataFormatSkStackClient();

    Assert.DoesNotThrow(() => client.SetERXUDPDataFormat(format));
    Assert.AreEqual(client.ERXUDPDataFormat, format);
  }

  [TestCase(-1)]
  public void ERXUDPDataFormat_Set_InvalidValue(SkStackERXUDPDataFormat format)
  {
    using var client = new ERXUDPDataFormatSkStackClient();

    var ex = Assert.Throws<ArgumentException>(() => client.SetERXUDPDataFormat(format));

    Assert.AreEqual(nameof(client.ERXUDPDataFormat), ex!.ParamName, nameof(ex.ParamName));
  }

  private static System.Collections.IEnumerable YieldTestCases_ReceiveUdpPollingInterval_Set()
  {
    yield return new object?[] { TimeSpan.FromMilliseconds(1) };
    yield return new object?[] { TimeSpan.FromSeconds(1) };
    yield return new object?[] { TimeSpan.MaxValue };
  }

  [TestCaseSource(nameof(YieldTestCases_ReceiveUdpPollingInterval_Set))]
  public void ReceiveUdpPollingInterval_Set(TimeSpan newValue)
  {
    using var client = new SkStackClient(Stream.Null);

    Assert.DoesNotThrow(() => client.ReceiveUdpPollingInterval = newValue);

    Assert.AreEqual(client.ReceiveUdpPollingInterval, newValue, nameof(client.ReceiveUdpPollingInterval));
  }

  private static System.Collections.IEnumerable YieldTestCases_ReceiveUdpPollingInterval_Set_InvalidValue()
  {
    yield return new object?[] { TimeSpan.Zero };
    yield return new object?[] { TimeSpan.MinValue };
    yield return new object?[] { TimeSpan.FromMilliseconds(-1) };
    yield return new object?[] { TimeSpan.FromSeconds(-1) };
    yield return new object?[] { Timeout.InfiniteTimeSpan };
  }

  [TestCaseSource(nameof(YieldTestCases_ReceiveUdpPollingInterval_Set_InvalidValue))]
  public void ReceiveUdpPollingInterval_Set_InvalidValue(TimeSpan newValue)
  {
    using var client = new SkStackClient(Stream.Null);

    var initialValue = client.ReceiveUdpPollingInterval;

    Assert.Throws<ArgumentOutOfRangeException>(() => client.ReceiveUdpPollingInterval = newValue);

    Assert.AreEqual(client.ReceiveUdpPollingInterval, initialValue, nameof(client.ReceiveUdpPollingInterval));
  }

  [Test]
  public void GetListeningUdpPortListAsync()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    stream.ResponseWriter.WriteLine("EPORT");
    stream.ResponseWriter.WriteLine("1");
    stream.ResponseWriter.WriteLine("0");
    stream.ResponseWriter.WriteLine("3");
    stream.ResponseWriter.WriteLine("4");
    stream.ResponseWriter.WriteLine("0");
    stream.ResponseWriter.WriteLine("0");
    stream.ResponseWriter.WriteLine("OK");

    IReadOnlyList<SkStackUdpPort>? listeningPortList = null;

    Assert.DoesNotThrowAsync(
      async () => listeningPortList = await client.GetListeningUdpPortListAsync()
    );

    Assert.IsNotNull(listeningPortList, nameof(listeningPortList));
    CollectionAssert.AreEqual(
      listeningPortList!.Select(static p => (p.Handle, p.Port)),
      new[] {
        (SkStackUdpPortHandle.Handle1, 1),
        (SkStackUdpPortHandle.Handle3, 3),
        (SkStackUdpPortHandle.Handle4, 4),
      },
      nameof(listeningPortList)
    );
  }

  [Test]
  public void GetListeningUdpPortListAsync_Empty()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    stream.ResponseWriter.WriteLine("EPORT");
    stream.ResponseWriter.WriteLine("0");
    stream.ResponseWriter.WriteLine("0");
    stream.ResponseWriter.WriteLine("0");
    stream.ResponseWriter.WriteLine("0");
    stream.ResponseWriter.WriteLine("0");
    stream.ResponseWriter.WriteLine("0");
    stream.ResponseWriter.WriteLine("OK");

    IReadOnlyList<SkStackUdpPort>? listeningPortList = null;

    Assert.DoesNotThrowAsync(
      async () => listeningPortList = await client.GetListeningUdpPortListAsync()
    );

    Assert.IsNotNull(listeningPortList, nameof(listeningPortList));
    CollectionAssert.IsEmpty(listeningPortList, nameof(listeningPortList));
  }

  [Test]
  public void GetUnusedUdpPortHandleListAsync()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    stream.ResponseWriter.WriteLine("EPORT");
    stream.ResponseWriter.WriteLine("1");
    stream.ResponseWriter.WriteLine("0");
    stream.ResponseWriter.WriteLine("3");
    stream.ResponseWriter.WriteLine("4");
    stream.ResponseWriter.WriteLine("0");
    stream.ResponseWriter.WriteLine("0");
    stream.ResponseWriter.WriteLine("OK");

    IReadOnlyList<SkStackUdpPortHandle>? unusedHandleList = null;

    Assert.DoesNotThrowAsync(
      async () => unusedHandleList = await client.GetUnusedUdpPortHandleListAsync()
    );

    Assert.IsNotNull(unusedHandleList, nameof(unusedHandleList));
    CollectionAssert.AreEqual(
      unusedHandleList,
      new[] {
        SkStackUdpPortHandle.Handle2,
        SkStackUdpPortHandle.Handle5,
        SkStackUdpPortHandle.Handle6,
      },
      nameof(unusedHandleList)
    );
  }

  [Test]
  public void GetUnusedUdpPortHandleListAsync_Empty()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    stream.ResponseWriter.WriteLine("EPORT");
    stream.ResponseWriter.WriteLine("1");
    stream.ResponseWriter.WriteLine("2");
    stream.ResponseWriter.WriteLine("3");
    stream.ResponseWriter.WriteLine("4");
    stream.ResponseWriter.WriteLine("5");
    stream.ResponseWriter.WriteLine("6");
    stream.ResponseWriter.WriteLine("OK");

    IReadOnlyList<SkStackUdpPortHandle>? unusedHandleList = null;

    Assert.DoesNotThrowAsync(
      async () => unusedHandleList = await client.GetUnusedUdpPortHandleListAsync()
    );

    Assert.IsNotNull(unusedHandleList, nameof(unusedHandleList));
    CollectionAssert.IsEmpty(unusedHandleList, nameof(unusedHandleList));
  }

  [TestCase(SkStackKnownPortNumbers.EchonetLite, SkStackUdpPortHandle.Handle2)]
  [TestCase(SkStackKnownPortNumbers.Pana, SkStackUdpPortHandle.Handle2)]
  public void PrepareUdpPortAsync(int port, SkStackUdpPortHandle expectedHandle)
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    // SKTABLE E
    stream.ResponseWriter.WriteLine("EPORT");
    stream.ResponseWriter.WriteLine("9999"); // #1
    stream.ResponseWriter.WriteLine("0"); // #2
    stream.ResponseWriter.WriteLine("0"); // #3
    stream.ResponseWriter.WriteLine("0"); // #4
    stream.ResponseWriter.WriteLine("0"); // #5
    stream.ResponseWriter.WriteLine("0"); // #6
    stream.ResponseWriter.WriteLine("OK");
    // SKUDPPORT
    stream.ResponseWriter.WriteLine("OK");

    SkStackUdpPort preparedPort = default;

    Assert.DoesNotThrowAsync(
      async () => preparedPort = await client.PrepareUdpPortAsync(port)
    );

    Assert.IsFalse(preparedPort.IsUnused);
    Assert.AreEqual(expectedHandle, preparedPort.Handle, nameof(preparedPort.Handle));
    Assert.AreEqual(port, preparedPort.Port, nameof(preparedPort.Port));

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo($"SKTABLE E\r\nSKUDPPORT {(int)expectedHandle} {port:X4}\r\n".ToByteSequence())
    );
  }

  [TestCase(SkStackKnownPortNumbers.EchonetLite, SkStackUdpPortHandle.Handle5)]
  [TestCase(SkStackKnownPortNumbers.Pana, SkStackUdpPortHandle.Handle3)]
  public void PrepareUdpPortAsync_AlreadyListening(int port, SkStackUdpPortHandle expectedHandle)
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    // SKTABLE E
    stream.ResponseWriter.WriteLine("EPORT");
    stream.ResponseWriter.WriteLine("0");     // #1
    stream.ResponseWriter.WriteLine("0");     // #2
    stream.ResponseWriter.WriteLine($"{SkStackKnownPortNumbers.Pana:D}"); // #3
    stream.ResponseWriter.WriteLine("0");     // #4
    stream.ResponseWriter.WriteLine($"{SkStackKnownPortNumbers.EchonetLite:D}"); // #5
    stream.ResponseWriter.WriteLine("0");     // #6
    stream.ResponseWriter.WriteLine("OK");

    SkStackUdpPort preparedPort = default;

    Assert.DoesNotThrowAsync(
      async () => preparedPort = await client.PrepareUdpPortAsync(port)
    );

    Assert.IsFalse(preparedPort.IsUnused);
    Assert.AreEqual(expectedHandle, preparedPort.Handle, nameof(preparedPort.Handle));
    Assert.AreEqual(port, preparedPort.Port, nameof(preparedPort.Port));
  }

  [Test]
  public void PrepareUdpPortAsync_NoUnusedPorts()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    // SKTABLE E
    stream.ResponseWriter.WriteLine("EPORT");
    stream.ResponseWriter.WriteLine("1");
    stream.ResponseWriter.WriteLine("2");
    stream.ResponseWriter.WriteLine("3");
    stream.ResponseWriter.WriteLine("4");
    stream.ResponseWriter.WriteLine("5");
    stream.ResponseWriter.WriteLine("6");
    stream.ResponseWriter.WriteLine("OK");

    Assert.ThrowsAsync<InvalidOperationException>(
      async () => await client.PrepareUdpPortAsync(SkStackKnownPortNumbers.EchonetLite)
    );
  }

  [TestCase(SkStackKnownPortNumbers.EchonetLite)]
  [TestCase(SkStackKnownPortNumbers.Pana)]
  public void StartCapturingUdpReceiveEvents(int port)
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.DoesNotThrow(() => client.StartCapturingUdpReceiveEvents(port));
  }

  [TestCase(0x0000)]
  [TestCase(0x10000)]
  [TestCase(int.MinValue)]
  [TestCase(int.MaxValue)]
  public void StartCapturingUdpReceiveEvents_PortOutOfRange(int port)
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.Throws<ArgumentOutOfRangeException>(() => client.StartCapturingUdpReceiveEvents(port));
  }

  [Test]
  public void StartCapturingUdpReceiveEvents_Disposed()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    client.Dispose();

    Assert.Throws<ObjectDisposedException>(() => client.StartCapturingUdpReceiveEvents(SkStackKnownPortNumbers.EchonetLite));
  }

  [TestCase(SkStackKnownPortNumbers.EchonetLite)]
  [TestCase(SkStackKnownPortNumbers.Pana)]
  public void StopCapturingUdpReceiveEvents(int port)
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.DoesNotThrow(() => client.StopCapturingUdpReceiveEvents(port));
  }

  [TestCase(0x0000)]
  [TestCase(0x10000)]
  [TestCase(int.MinValue)]
  [TestCase(int.MaxValue)]
  public void StopCapturingUdpReceiveEvents_PortOutOfRange(int port)
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.Throws<ArgumentOutOfRangeException>(() => client.StopCapturingUdpReceiveEvents(port));
  }

  [Test]
  public void StopCapturingUdpReceiveEvents_Disposed()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    client.Dispose();

    Assert.Throws<ObjectDisposedException>(() => client.StopCapturingUdpReceiveEvents(SkStackKnownPortNumbers.EchonetLite));
  }

  private class NullBufferWriter : IBufferWriter<byte> {
    public void Advance(int count) { /* do nothing */}
    public Span<byte> GetSpan(int sizeHint) => new byte[sizeHint];
    public Memory<byte> GetMemory(int sizeHint) => new byte[sizeHint];
  }

  private static IBufferWriter<byte> CreateNullBufferWriter() => new NullBufferWriter();

  [TestCase(0x0000)]
  [TestCase(0x10000)]
  [TestCase(int.MinValue)]
  [TestCase(int.MaxValue)]
  public void ReceiveUdpAsync_PortOutOfRange(int port)
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.Throws<ArgumentOutOfRangeException>(() => client.ReceiveUdpAsync(port: port, buffer: CreateNullBufferWriter()));
#pragma warning restore CA2012
  }

  [Test]
  public void ReceiveUdpAsync_BufferNull()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.Throws<ArgumentNullException>(() => client.ReceiveUdpAsync(port: 1, buffer: null!));
#pragma warning restore CA2012
  }

  [Test]
  public void ReceiveUdpAsync_Disposed()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    client.Dispose();

#pragma warning disable CA2012
    Assert.Throws<ObjectDisposedException>(
      () => client.ReceiveUdpAsync(port: SkStackKnownPortNumbers.EchonetLite, buffer: CreateNullBufferWriter())
    );
#pragma warning restore CA2012
  }

  [Test]
  public void ReceiveUdpAsync_NotCapturing()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.Throws<InvalidOperationException>(
      () => client.ReceiveUdpAsync(port: SkStackKnownPortNumbers.Pana, buffer: CreateNullBufferWriter())
    );
#pragma warning restore CA2012
  }

  [Test]
  public void ReceiveUdpAsync_CapturingEchonetLiteByDefault()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.DoesNotThrow(
      () => client.ReceiveUdpAsync(port: SkStackKnownPortNumbers.EchonetLite, buffer: CreateNullBufferWriter())
    );
#pragma warning restore CA2012
  }

  [Test]
  public void ReceiveUdpAsync_StoppedCapturing()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.DoesNotThrow(
      () => client.ReceiveUdpAsync(port: SkStackKnownPortNumbers.EchonetLite, buffer: CreateNullBufferWriter())
    );
#pragma warning restore CA2012

    client.StopCapturingUdpReceiveEvents(SkStackKnownPortNumbers.EchonetLite);

#pragma warning disable CA2012
    Assert.Throws<InvalidOperationException>(
      () => client.ReceiveUdpAsync(port: SkStackKnownPortNumbers.EchonetLite, buffer: CreateNullBufferWriter())
    );
#pragma warning restore CA2012
  }

  [Test]
  public void ReceiveUdpAsync_NoUdpPacketReceived()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.ThrowsAsync<OperationCanceledException>(async () => {
      using var cts = new CancellationTokenSource();

      cts.CancelAfter(TimeSpan.FromSeconds(0.2));

      _ = await client.ReceiveUdpAsync(
        port: SkStackKnownPortNumbers.EchonetLite,
        buffer: CreateNullBufferWriter(),
        cts.Token
      );
    });
  }

  [Test]
  public void ReceiveUdpAsync_NoUdpPacketReceived_EVENTReceived()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    stream.ResponseWriter.WriteLine($"EVENT 21 FE80:0000:0000:0000:021D:1290:1234:5678 00");
    stream.ResponseWriter.WriteLine($"EVENT 33 FE80:0000:0000:0000:021D:1290:1234:5678");

    Assert.ThrowsAsync<OperationCanceledException>(async () => {
      using var cts = new CancellationTokenSource();

      cts.CancelAfter(TimeSpan.FromSeconds(0.2));

      _ = await client.ReceiveUdpAsync(
        port: SkStackKnownPortNumbers.EchonetLite,
        buffer: CreateNullBufferWriter(),
        cts.Token
      );
    });
  }

  [Test]
  public void ReceiveUdpAsync()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.AreEqual(client.ERXUDPDataFormat, SkStackERXUDPDataFormat.Binary);

    const string remoteAddressString1 = "FE80:0000:0000:0000:021D:1290:1111:2222";
    const string remoteAddressString2 = "FE80:0000:0000:0000:021D:1290:3333:4444";

    stream.ResponseWriter.WriteLine($"ERXUDP {remoteAddressString1} FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0E1A 001D129012345679 0 0008 01234567");
    stream.ResponseWriter.WriteLine($"ERXUDP {remoteAddressString2} FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0E1A 001D129012345679 0 0008 89ABCDEF");

    using var cts = new CancellationTokenSource();

    cts.CancelAfter(TimeSpan.FromSeconds(1.0));

    var buffer = new ArrayBufferWriter<byte>();
    IPAddress? remoteAddress1 = null;

    Assert.DoesNotThrowAsync(
      async () => remoteAddress1 = await client.ReceiveUdpAsync(
        port: SkStackKnownPortNumbers.EchonetLite,
        buffer: buffer,
        cts.Token
      )
    );

    Assert.IsNotNull(remoteAddress1);
    Assert.AreEqual(IPAddress.Parse(remoteAddressString1), remoteAddress1, nameof(remoteAddress1));
    Assert.That(
      buffer.WrittenMemory,
      SequenceIs.EqualTo("01234567".ToByteSequence()),
      nameof(buffer.WrittenMemory)
    );

    buffer.Clear();

    IPAddress? remoteAddress2 = null;

    Assert.DoesNotThrowAsync(
      async () => remoteAddress2 = await client.ReceiveUdpAsync(
        port: SkStackKnownPortNumbers.EchonetLite,
        buffer: buffer,
        cts.Token
      )
    );

    Assert.IsNotNull(remoteAddress2);
    Assert.AreEqual(IPAddress.Parse(remoteAddressString2), remoteAddress2, nameof(remoteAddress2));
    Assert.That(
      buffer.WrittenMemory,
      SequenceIs.EqualTo("89ABCDEF".ToByteSequence()),
      nameof(buffer.WrittenMemory)
    );
  }

  [Test]
  public void ReceiveUdpAsync_MultiplePorts()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.AreEqual(client.ERXUDPDataFormat, SkStackERXUDPDataFormat.Binary);

    client.StartCapturingUdpReceiveEvents(SkStackKnownPortNumbers.Pana);

    const string remoteAddressStringEchonetLite = "FE80:0000:0000:0000:021D:1290:1111:2222";
    const string remoteAddressStringPana = "FE80:0000:0000:0000:021D:1290:3333:4444";

    stream.ResponseWriter.WriteLine($"ERXUDP {remoteAddressStringEchonetLite} FE80:0000:0000:0000:021D:1290:1234:5678 {SkStackKnownPortNumbers.EchonetLite:X4} {SkStackKnownPortNumbers.EchonetLite:X4} 001D129012345679 0 000C ECHONET-LITE");
    stream.ResponseWriter.WriteLine($"ERXUDP {remoteAddressStringPana} FE80:0000:0000:0000:021D:1290:1234:5678 {SkStackKnownPortNumbers.Pana:X4} {SkStackKnownPortNumbers.Pana:X4} 001D129012345679 0 0004 PANA");

    using var cts = new CancellationTokenSource();

    cts.CancelAfter(TimeSpan.FromSeconds(1.0));

    var buffer = new ArrayBufferWriter<byte>();

    IPAddress? remoteAddressEchonetLite = null;

    Assert.DoesNotThrowAsync(
      async () => remoteAddressEchonetLite = await client.ReceiveUdpAsync(
        port: SkStackKnownPortNumbers.EchonetLite,
        buffer: buffer,
        cts.Token
      )
    );

    Assert.IsNotNull(remoteAddressEchonetLite);
    Assert.AreEqual(IPAddress.Parse(remoteAddressStringEchonetLite), remoteAddressEchonetLite, nameof(remoteAddressEchonetLite));
    Assert.That(
      buffer.WrittenMemory,
      SequenceIs.EqualTo("ECHONET-LITE".ToByteSequence()),
      nameof(buffer.WrittenMemory)
    );

    buffer.Clear();

    IPAddress? remoteAddressPana = null;

    Assert.DoesNotThrowAsync(
      async () => remoteAddressPana = await client.ReceiveUdpAsync(
        port: SkStackKnownPortNumbers.Pana,
        buffer: buffer,
        cts.Token
      )
    );

    Assert.IsNotNull(remoteAddressPana);
    Assert.AreEqual(IPAddress.Parse(remoteAddressStringPana), remoteAddressPana, nameof(remoteAddressPana));
    Assert.That(
      buffer.WrittenMemory,
      SequenceIs.EqualTo("PANA".ToByteSequence()),
      nameof(buffer.WrittenMemory)
    );
  }

  [Test]
  public void ReceiveUdpAsync_IncompleteLine()
  {
    const string remoteAddressString = "FE80:0000:0000:0000:021D:1290:1111:2222";

    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.AreEqual(client.ERXUDPDataFormat, SkStackERXUDPDataFormat.Binary);

    async Task RaiseERXUDPAsync()
    {
      stream.ResponseWriter.Write("E"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.Write("RXUDP"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.Write($" {remoteAddressString} "); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.Write("FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0E"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.Write("1A 001D129012345679 0 0001 "); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.Write("X"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine();
    }

    using var cts = new CancellationTokenSource();

    cts.CancelAfter(TimeSpan.FromSeconds(1.0));

    var buffer = new ArrayBufferWriter<byte>();

#pragma warning disable CA2012
    var taskUdpReceive = client.ReceiveUdpAsync(
      port: SkStackKnownPortNumbers.EchonetLite,
      buffer: buffer,
      cts.Token
    ).AsTask();

    Assert.DoesNotThrowAsync(
      async () => await Task.WhenAll(taskUdpReceive, RaiseERXUDPAsync())
    );
#pragma warning restore CA2012

    var remoteAddress = taskUdpReceive.Result;

    Assert.IsNotNull(remoteAddress);
    Assert.AreEqual(IPAddress.Parse(remoteAddressString), remoteAddress, nameof(remoteAddress));
    Assert.That(
      buffer.WrittenMemory,
      SequenceIs.EqualTo("X".ToByteSequence()),
      nameof(buffer.WrittenMemory)
    );
  }

  [Test]
  public void ReceiveUdpAsync_IncompleteLine_DataEndsWithCRLF_EndOfLineDelayed()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.AreEqual(client.ERXUDPDataFormat, SkStackERXUDPDataFormat.Binary);

    async Task RaiseERXUDPAsync()
    {
      stream.ResponseWriter.Write("ERXUDP FE80:0000:0000:0000:021D:1290:1234:5679 FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0E1A 001D129012345679 0 0002 \r\n");
      await Task.Delay(TimeSpan.FromMilliseconds(1000));

      stream.ResponseWriter.Write("\r");
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.Write("\n");
      await Task.Delay(ResponseDelayInterval);
    }

    using var cts = new CancellationTokenSource();

    cts.CancelAfter(TimeSpan.FromSeconds(3.0));

    var buffer = new ArrayBufferWriter<byte>();

#pragma warning disable CA2012
    var taskUdpReceive = client.ReceiveUdpAsync(
      port: SkStackKnownPortNumbers.EchonetLite,
      buffer: buffer,
      cts.Token
    ).AsTask();

    Assert.DoesNotThrowAsync(
      async () => await Task.WhenAll(taskUdpReceive, RaiseERXUDPAsync())
    );
#pragma warning restore CA2012

    Assert.That(
      buffer.WrittenMemory,
      SequenceIs.EqualTo("\r\n".ToByteSequence()),
      nameof(buffer.WrittenMemory)
    );
  }

  [Test]
  public void ReceiveUdpAsync_SendCommandWhileAwaiting()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.AreEqual(client.ERXUDPDataFormat, SkStackERXUDPDataFormat.Binary);

    async Task CompleteResponseAndRaiseERXUDPAsync()
    {
      // SKVER EVER event line
      stream.ResponseWriter.WriteLine("EVER 1.2.10");
      await Task.Delay(ResponseDelayInterval);

      // SKVER status line
      stream.ResponseWriter.Write("O"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine("K");
      await Task.Delay(ResponseDelayInterval);

      // ERXUDP
      stream.ResponseWriter.Write("ERXUDP FE80:0000:0000:0000:021D:1290:1234:5679 "); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.Write("FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0E1A 001D129012345679 0 0008 0"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine("1234567"); await Task.Delay(ResponseDelayInterval);
      await Task.Delay(ResponseDelayInterval);
    }

    using var cts = new CancellationTokenSource();

    cts.CancelAfter(TimeSpan.FromSeconds(2.0));

    var buffer = new ArrayBufferWriter<byte>();

#pragma warning disable CA2012
    var taskUdpReceive = client.ReceiveUdpAsync(
      port: SkStackKnownPortNumbers.EchonetLite,
      buffer: buffer,
      cts.Token
    ).AsTask();

    Assert.IsFalse(taskUdpReceive.Wait(TimeSpan.FromMilliseconds(100)));

    var taskSendCommand = client.SendSKVERAsync().AsTask();

    Assert.DoesNotThrowAsync(
      async () => await Task.WhenAll(taskUdpReceive, taskSendCommand, CompleteResponseAndRaiseERXUDPAsync())
    );
#pragma warning restore CA2012

    Assert.IsTrue(taskSendCommand.Result.Success);

    Assert.That(
      buffer.WrittenMemory,
      SequenceIs.EqualTo("01234567".ToByteSequence()),
      nameof(buffer.WrittenMemory)
    );
  }

  [Test]
  public void ReceiveUdpAsync_DataFormat_HexASCIIText()
  {
    const string remoteAddressString = "FE80:0000:0000:0000:021D:1290:1111:2222";

    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, erxudpDataFormat: SkStackERXUDPDataFormat.HexAsciiText, logger: CreateLoggerForTestCase());

    Assert.AreEqual(client.ERXUDPDataFormat, SkStackERXUDPDataFormat.HexAsciiText);

    stream.ResponseWriter.WriteLine($"ERXUDP {remoteAddressString} FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0E1A 001D129012345679 0 0008 0123456789ABCDEF");

    using var cts = new CancellationTokenSource();

    cts.CancelAfter(TimeSpan.FromSeconds(1.0));

    var buffer = new ArrayBufferWriter<byte>();

    IPAddress? remoteAddress = null;

    Assert.DoesNotThrowAsync(
      async () => remoteAddress = await client.ReceiveUdpAsync(
        port: SkStackKnownPortNumbers.EchonetLite,
        buffer: buffer,
        cts.Token
      )
    );

    Assert.IsNotNull(remoteAddress);
    Assert.AreEqual(IPAddress.Parse(remoteAddressString), remoteAddress, nameof(remoteAddress));
    Assert.That(
      buffer.WrittenMemory,
      SequenceIs.EqualTo(new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF }),
      nameof(buffer.WrittenMemory)
    );
  }
}
