// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using Is = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientEventsERXUDPTests : SkStackClientTestsBase {
  [TestCase(SkStackERXUDPDataFormat.Raw)]
  [TestCase(SkStackERXUDPDataFormat.HexAsciiText)]
  public void ERXUDPDataFormat(SkStackERXUDPDataFormat format)
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.DoesNotThrow(() => client.ERXUDPDataFormat = format);
    Assert.AreEqual(client.ERXUDPDataFormat, format);
  }

  [TestCase(-1)]
  public void ERXUDPDataFormat_InvalidValue(SkStackERXUDPDataFormat format)
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.Throws<ArgumentException>(() => client.ERXUDPDataFormat = format);
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

  [TestCase(0x0000)]
  [TestCase(0x10000)]
  [TestCase(int.MinValue)]
  [TestCase(int.MaxValue)]
  public void ReceiveUdpAsync_PortOutOfRange(int port)
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.Throws<ArgumentOutOfRangeException>(() => client.ReceiveUdpAsync(port));
#pragma warning restore CA2012
  }

  [Test]
  public void ReceiveUdpAsync_Disposed()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    client.Dispose();

#pragma warning disable CA2012
    Assert.Throws<ObjectDisposedException>(() => client.ReceiveUdpAsync(SkStackKnownPortNumbers.EchonetLite));
#pragma warning restore CA2012
  }

  [Test]
  public void ReceiveUdpAsync_NotCapturing()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.Throws<InvalidOperationException>(() => client.ReceiveUdpAsync(SkStackKnownPortNumbers.Pana));
#pragma warning restore CA2012
  }

  [Test]
  public void ReceiveUdpAsync_CapturingEchonetLiteByDefault()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.DoesNotThrow(() => client.ReceiveUdpAsync(SkStackKnownPortNumbers.EchonetLite));
#pragma warning restore CA2012
  }

  [Test]
  public void ReceiveUdpAsync_StoppedCapturing()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.DoesNotThrow(() => client.ReceiveUdpAsync(SkStackKnownPortNumbers.EchonetLite));
#pragma warning restore CA2012

    client.StopCapturingUdpReceiveEvents(SkStackKnownPortNumbers.EchonetLite);

#pragma warning disable CA2012
    Assert.Throws<InvalidOperationException>(() => client.ReceiveUdpAsync(SkStackKnownPortNumbers.EchonetLite));
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

      using var result = await client.ReceiveUdpAsync(SkStackKnownPortNumbers.EchonetLite, cts.Token);
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

      using var result = await client.ReceiveUdpAsync(SkStackKnownPortNumbers.EchonetLite, cts.Token);
    });
  }

  [Test]
  public void ReceiveUdpAsync()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.AreEqual(client.ERXUDPDataFormat, SkStackERXUDPDataFormat.Raw);

    stream.ResponseWriter.WriteLine("ERXUDP FE80:0000:0000:0000:021D:1290:1234:5679 FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0E1A 001D129012345679 0 0008 01234567");
    stream.ResponseWriter.WriteLine("ERXUDP FE80:0000:0000:0000:021D:1290:1234:5679 FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0E1A 001D129012345679 0 0008 89ABCDEF");

    using var cts = new CancellationTokenSource();

    cts.CancelAfter(TimeSpan.FromSeconds(1.0));

    SkStackReceiveUdpResult result1 = null;
    SkStackReceiveUdpResult result2 = null;

    try {
      Assert.DoesNotThrowAsync(
        async () => result1 = await client.ReceiveUdpAsync(SkStackKnownPortNumbers.EchonetLite, cts.Token)
      );

      Assert.IsNotNull(result1);
      Assert.That(
        result1!.Buffer,
        Is.EqualTo("01234567".ToByteSequence()),
        nameof(result1)
      );

      Assert.DoesNotThrow(() => result1.Dispose(), $"{nameof(result1)}.Dispose #1");
      Assert.Throws<ObjectDisposedException>(() => Assert.AreEqual(result1.Buffer.Length, 0));
      Assert.DoesNotThrow(() => result1.Dispose(), $"{nameof(result1)}.Dispose #2");
    }
    finally {
      result1?.Dispose();
    }

    try {
      Assert.DoesNotThrowAsync(
        async () => result2 = await client.ReceiveUdpAsync(SkStackKnownPortNumbers.EchonetLite, cts.Token)
      );

      Assert.IsNotNull(result2);
      Assert.That(
        result2!.Buffer,
        Is.EqualTo("89ABCDEF".ToByteSequence()),
        nameof(result2)
      );
    }
    finally {
      result2?.Dispose();
    }
  }

  [Test]
  public void ReceiveUdpAsync_MultiplePorts()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.AreEqual(client.ERXUDPDataFormat, SkStackERXUDPDataFormat.Raw);

    client.StartCapturingUdpReceiveEvents(SkStackKnownPortNumbers.Pana);

    stream.ResponseWriter.WriteLine($"ERXUDP FE80:0000:0000:0000:021D:1290:1234:5679 FE80:0000:0000:0000:021D:1290:1234:5678 {SkStackKnownPortNumbers.EchonetLite:X4} {SkStackKnownPortNumbers.EchonetLite:X4} 001D129012345679 0 000C ECHONET-LITE");
    stream.ResponseWriter.WriteLine($"ERXUDP FE80:0000:0000:0000:021D:1290:1234:5679 FE80:0000:0000:0000:021D:1290:1234:5678 {SkStackKnownPortNumbers.Pana:X4} {SkStackKnownPortNumbers.Pana:X4} 001D129012345679 0 0004 PANA");

    using var cts = new CancellationTokenSource();

    cts.CancelAfter(TimeSpan.FromSeconds(1.0));

    SkStackReceiveUdpResult resultEchonetLite = null;

    try {
      Assert.DoesNotThrowAsync(
        async () => resultEchonetLite = await client.ReceiveUdpAsync(SkStackKnownPortNumbers.EchonetLite, cts.Token)
      );

      Assert.IsNotNull(resultEchonetLite);
      Assert.That(
        resultEchonetLite!.Buffer,
        Is.EqualTo("ECHONET-LITE".ToByteSequence()),
        nameof(resultEchonetLite)
      );

      Assert.DoesNotThrow(() => resultEchonetLite.Dispose(), $"{nameof(resultEchonetLite)}.Dispose #1");
      Assert.Throws<ObjectDisposedException>(() => Assert.AreEqual(resultEchonetLite.Buffer.Length, 0));
      Assert.DoesNotThrow(() => resultEchonetLite.Dispose(), $"{nameof(resultEchonetLite)}.Dispose #2");
    }
    finally {
      resultEchonetLite?.Dispose();
    }

    SkStackReceiveUdpResult resultPana = null;

    try {
      Assert.DoesNotThrowAsync(
        async () => resultPana = await client.ReceiveUdpAsync(SkStackKnownPortNumbers.Pana, cts.Token)
      );

      Assert.IsNotNull(resultPana);
      Assert.That(
        resultPana!.Buffer,
        Is.EqualTo("PANA".ToByteSequence()),
        nameof(resultPana)
      );
    }
    finally {
      resultPana?.Dispose();
    }
  }

  [Test]
  public void ReceiveUdpAsync_IncompleteLine()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.AreEqual(client.ERXUDPDataFormat, SkStackERXUDPDataFormat.Raw);

    async Task RaiseERXUDPAsync()
    {
      stream.ResponseWriter.Write("E"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.Write("RXUDP"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.Write(" FE80:0000:0000:0000:021D:1290:1234:5679 "); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.Write("FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0E"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.Write("1A 001D129012345679 0 0001 "); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.Write("X"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine();
    }

    using var cts = new CancellationTokenSource();

    cts.CancelAfter(TimeSpan.FromSeconds(1.0));

#pragma warning disable CA2012
    var taskUdpReceive = client.ReceiveUdpAsync(SkStackKnownPortNumbers.EchonetLite, cts.Token).AsTask();

    Assert.DoesNotThrowAsync(
      async () => await Task.WhenAll(taskUdpReceive, RaiseERXUDPAsync())
    );
#pragma warning restore CA2012

    using var result = taskUdpReceive.Result;

    Assert.That(
      result.Buffer,
      Is.EqualTo("X".ToByteSequence()),
      nameof(result)
    );
  }

  [Test]
  public void ReceiveUdpAsync_IncompleteLine_DataEndsWithCRLF_EndOfLineDelayed()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.AreEqual(client.ERXUDPDataFormat, SkStackERXUDPDataFormat.Raw);

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

#pragma warning disable CA2012
    var taskUdpReceive = client.ReceiveUdpAsync(SkStackKnownPortNumbers.EchonetLite, cts.Token).AsTask();

    Assert.DoesNotThrowAsync(
      async () => await Task.WhenAll(taskUdpReceive, RaiseERXUDPAsync())
    );
#pragma warning restore CA2012

    using var result = taskUdpReceive.Result;

    Assert.That(
      result.Buffer,
      Is.EqualTo("\r\n".ToByteSequence()),
      nameof(result)
    );
  }

  [Test]
  public void ReceiveUdpAsync_SendCommandWhileAwaiting()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.AreEqual(client.ERXUDPDataFormat, SkStackERXUDPDataFormat.Raw);

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

#pragma warning disable CA2012
    var taskUdpReceive = client.ReceiveUdpAsync(SkStackKnownPortNumbers.EchonetLite, cts.Token).AsTask();

    Assert.IsFalse(taskUdpReceive.Wait(TimeSpan.FromMilliseconds(100)));

    var taskSendCommand = client.SendSKVERAsync().AsTask();

    Assert.DoesNotThrowAsync(
      async () => await Task.WhenAll(taskUdpReceive, taskSendCommand, CompleteResponseAndRaiseERXUDPAsync())
    );
#pragma warning restore CA2012

    Assert.IsTrue(taskSendCommand.Result.Success);

    using var result = taskUdpReceive.Result;

    Assert.That(
      result.Buffer,
      Is.EqualTo("01234567".ToByteSequence()),
      nameof(result)
    );
  }

  [Test]
  public void ReceiveUdpAsync_DataFormat_HexASCIIText()
  {
    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    client.ERXUDPDataFormat = SkStackERXUDPDataFormat.HexAsciiText;

    Assert.AreEqual(client.ERXUDPDataFormat, SkStackERXUDPDataFormat.HexAsciiText);

    stream.ResponseWriter.WriteLine("ERXUDP FE80:0000:0000:0000:021D:1290:1234:5679 FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0E1A 001D129012345679 0 0008 0123456789ABCDEF");

    using var cts = new CancellationTokenSource();

    cts.CancelAfter(TimeSpan.FromSeconds(1.0));

    SkStackReceiveUdpResult result = null;

    try {
      Assert.DoesNotThrowAsync(
        async () => result = await client.ReceiveUdpAsync(SkStackKnownPortNumbers.EchonetLite, cts.Token)
      );

      Assert.IsNotNull(result);
      Assert.That(
        result!.Buffer,
        Is.EqualTo(new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF }),
        nameof(result)
      );

      Assert.DoesNotThrow(() => result.Dispose(), $"{nameof(result)}.Dispose #1");
      Assert.Throws<ObjectDisposedException>(() => Assert.AreEqual(result.Buffer.Length, 0));
      Assert.DoesNotThrow(() => result.Dispose(), $"{nameof(result)}.Dispose #2");
    }
    finally {
      result?.Dispose();
    }
  }
}
