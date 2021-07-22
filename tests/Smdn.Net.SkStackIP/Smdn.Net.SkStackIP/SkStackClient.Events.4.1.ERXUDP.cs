// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using Is = Smdn.Test.NUnitExtensions.Constraints.Is;

namespace Smdn.Net.SkStackIP {
  [TestFixture]
  public class SkStackClientEventsERXUDPTests : SkStackClientTestsBase {
    [TestCase(SkStackKnownPortNumbers.EchonetLite)]
    [TestCase(SkStackKnownPortNumbers.Pana)]
    public void StartCapturingUdpReceiveEvents(int port)
    {
      using var stream = new PseudoSkStackStream();
      using var client = SkStackClient.Create(stream, ServiceProvider);

      Assert.DoesNotThrow(() => client.StartCapturingUdpReceiveEvents(port));
    }

    [TestCase(0x0000)]
    [TestCase(0x10000)]
    [TestCase(int.MinValue)]
    [TestCase(int.MaxValue)]
    public void StartCapturingUdpReceiveEvents_PortOutOfRange(int port)
    {
      using var stream = new PseudoSkStackStream();
      using var client = SkStackClient.Create(stream, ServiceProvider);

      Assert.Throws<ArgumentOutOfRangeException>(() => client.StartCapturingUdpReceiveEvents(port));
    }

    [Test]
    public void StartCapturingUdpReceiveEvents_Disposed()
    {
      using var stream = new PseudoSkStackStream();
      using var client = SkStackClient.Create(stream, ServiceProvider);

      client.Close();

      Assert.Throws<ObjectDisposedException>(() => client.StartCapturingUdpReceiveEvents(SkStackKnownPortNumbers.EchonetLite));
    }

    [TestCase(SkStackKnownPortNumbers.EchonetLite)]
    [TestCase(SkStackKnownPortNumbers.Pana)]
    public void StopCapturingUdpReceiveEvents(int port)
    {
      using var stream = new PseudoSkStackStream();
      using var client = SkStackClient.Create(stream, ServiceProvider);

      Assert.DoesNotThrow(() => client.StopCapturingUdpReceiveEvents(port));
    }

    [TestCase(0x0000)]
    [TestCase(0x10000)]
    [TestCase(int.MinValue)]
    [TestCase(int.MaxValue)]
    public void StopCapturingUdpReceiveEvents_PortOutOfRange(int port)
    {
      using var stream = new PseudoSkStackStream();
      using var client = SkStackClient.Create(stream, ServiceProvider);

      Assert.Throws<ArgumentOutOfRangeException>(() => client.StopCapturingUdpReceiveEvents(port));
    }

    [Test]
    public void StopCapturingUdpReceiveEvents_Disposed()
    {
      using var stream = new PseudoSkStackStream();
      using var client = SkStackClient.Create(stream, ServiceProvider);

      client.Close();

      Assert.Throws<ObjectDisposedException>(() => client.StopCapturingUdpReceiveEvents(SkStackKnownPortNumbers.EchonetLite));
    }

    [TestCase(0x0000)]
    [TestCase(0x10000)]
    [TestCase(int.MinValue)]
    [TestCase(int.MaxValue)]
    public void UdpReceiveAsync_PortOutOfRange(int port)
    {
      using var stream = new PseudoSkStackStream();
      using var client = SkStackClient.Create(stream, ServiceProvider);

      Assert.Throws<ArgumentOutOfRangeException>(() => client.UdpReceiveAsync(port));
    }

    [Test]
    public void UdpReceiveAsync_Disposed()
    {
      using var stream = new PseudoSkStackStream();
      using var client = SkStackClient.Create(stream, ServiceProvider);

      client.Close();

      Assert.Throws<ObjectDisposedException>(() => client.UdpReceiveAsync(SkStackKnownPortNumbers.EchonetLite));
    }

    [Test]
    public void UdpReceiveAsync_NotCapturing()
    {
      using var stream = new PseudoSkStackStream();
      using var client = SkStackClient.Create(stream, ServiceProvider);

      Assert.Throws<InvalidOperationException>(() => client.UdpReceiveAsync(SkStackKnownPortNumbers.Pana));
    }

    [Test]
    public void UdpReceiveAsync_CapturingEchonetLiteByDefault()
    {
      using var stream = new PseudoSkStackStream();
      using var client = SkStackClient.Create(stream, ServiceProvider);

      Assert.DoesNotThrow(() => client.UdpReceiveAsync(SkStackKnownPortNumbers.EchonetLite));
    }

    [Test]
    public void UdpReceiveAsync_StoppedCapturing()
    {
      using var stream = new PseudoSkStackStream();
      using var client = SkStackClient.Create(stream, ServiceProvider);

      Assert.DoesNotThrow(() => client.UdpReceiveAsync(SkStackKnownPortNumbers.EchonetLite));

      client.StopCapturingUdpReceiveEvents(SkStackKnownPortNumbers.EchonetLite);

      Assert.Throws<InvalidOperationException>(() => client.UdpReceiveAsync(SkStackKnownPortNumbers.EchonetLite));
    }

    [Test]
    public void UdpReceiveAsync_NoUdpPacketReceived()
    {
      using var stream = new PseudoSkStackStream();
      using var client = SkStackClient.Create(stream, ServiceProvider);

      Assert.ThrowsAsync<OperationCanceledException>(async () => {
        using var cts = new CancellationTokenSource();

        cts.CancelAfter(TimeSpan.FromSeconds(0.2));

        using var result = await client.UdpReceiveAsync(SkStackKnownPortNumbers.EchonetLite, cts.Token);
      });
    }

    [Test]
    public void UdpReceiveAsync_NoUdpPacketReceived_EVENTReceived()
    {
      using var stream = new PseudoSkStackStream();
      using var client = SkStackClient.Create(stream, ServiceProvider);

      stream.ResponseWriter.WriteLine($"EVENT 21 FE80:0000:0000:0000:021D:1290:1234:5678 00");
      stream.ResponseWriter.WriteLine($"EVENT 33 FE80:0000:0000:0000:021D:1290:1234:5678");

      Assert.ThrowsAsync<OperationCanceledException>(async () => {
        using var cts = new CancellationTokenSource();

        cts.CancelAfter(TimeSpan.FromSeconds(0.2));

        using var result = await client.UdpReceiveAsync(SkStackKnownPortNumbers.EchonetLite, cts.Token);
      });
    }

    [Test]
    public void UdpReceiveAsync()
    {
      using var stream = new PseudoSkStackStream();
      using var client = SkStackClient.Create(stream, ServiceProvider);

      stream.ResponseWriter.WriteLine("ERXUDP FE80:0000:0000:0000:021D:1290:1234:5679 FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0E1A 001D129012345679 0 0008 01234567");
      stream.ResponseWriter.WriteLine("ERXUDP FE80:0000:0000:0000:021D:1290:1234:5679 FE80:0000:0000:0000:021D:1290:1234:5678 0E1A 0E1A 001D129012345679 0 0008 89ABCDEF");

      using var cts = new CancellationTokenSource();

      cts.CancelAfter(TimeSpan.FromSeconds(1.0));

      SkStackUdpReceiveResult result1 = null;
      SkStackUdpReceiveResult result2 = null;

      try {
        Assert.DoesNotThrowAsync(async () => {
          result1 = await client.UdpReceiveAsync(SkStackKnownPortNumbers.EchonetLite, cts.Token);
        });

        Assert.IsNotNull(result1);
        Assert.That(
          result1.Buffer,
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
        Assert.DoesNotThrowAsync(async () => {
          result2 = await client.UdpReceiveAsync(SkStackKnownPortNumbers.EchonetLite, cts.Token);
        });

        Assert.IsNotNull(result2);
        Assert.That(
          result2.Buffer,
          Is.EqualTo("89ABCDEF".ToByteSequence()),
          nameof(result2)
        );
      }
      finally {
        result2?.Dispose();
      }
    }

    [Test]
    public void UdpReceiveAsync_MultiplePorts()
    {
      using var stream = new PseudoSkStackStream();
      using var client = SkStackClient.Create(stream, ServiceProvider);

      client.StartCapturingUdpReceiveEvents(SkStackKnownPortNumbers.Pana);

      stream.ResponseWriter.WriteLine($"ERXUDP FE80:0000:0000:0000:021D:1290:1234:5679 FE80:0000:0000:0000:021D:1290:1234:5678 {SkStackKnownPortNumbers.EchonetLite:X4} {SkStackKnownPortNumbers.EchonetLite:X4} 001D129012345679 0 000C ECHONET-LITE");
      stream.ResponseWriter.WriteLine($"ERXUDP FE80:0000:0000:0000:021D:1290:1234:5679 FE80:0000:0000:0000:021D:1290:1234:5678 {SkStackKnownPortNumbers.Pana:X4} {SkStackKnownPortNumbers.Pana:X4} 001D129012345679 0 0004 PANA");

      using var cts = new CancellationTokenSource();

      cts.CancelAfter(TimeSpan.FromSeconds(1.0));

      SkStackUdpReceiveResult resultEchonetLite = null;

      try {
        Assert.DoesNotThrowAsync(async () => {
          resultEchonetLite = await client.UdpReceiveAsync(SkStackKnownPortNumbers.EchonetLite, cts.Token);
        });

        Assert.IsNotNull(resultEchonetLite);
        Assert.That(
          resultEchonetLite.Buffer,
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

      SkStackUdpReceiveResult resultPana = null;

      try {
        Assert.DoesNotThrowAsync(async () => {
          resultPana = await client.UdpReceiveAsync(SkStackKnownPortNumbers.Pana, cts.Token);
        });

        Assert.IsNotNull(resultPana);
        Assert.That(
          resultPana.Buffer,
          Is.EqualTo("PANA".ToByteSequence()),
          nameof(resultPana)
        );
      }
      finally {
        resultPana?.Dispose();
      }
    }

    [Test]
    public void UdpReceiveAsync_IncompleteLine()
    {
      using var stream = new PseudoSkStackStream();
      using var client = SkStackClient.Create(stream, ServiceProvider);

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

      var taskUdpReceive = client.UdpReceiveAsync(SkStackKnownPortNumbers.EchonetLite, cts.Token);

      Assert.DoesNotThrowAsync(async () => {
        await Task.WhenAll(taskUdpReceive.AsTask(), RaiseERXUDPAsync());
      });

      using (var result = taskUdpReceive.Result) {
        Assert.That(
          result.Buffer,
          Is.EqualTo("X".ToByteSequence()),
          nameof(result)
        );
      }
    }

    [Test]
    public void UdpReceiveAsync_SendCommandWhileAwaiting()
    {
      using var stream = new PseudoSkStackStream();
      using var client = SkStackClient.Create(stream, ServiceProvider);

      async Task CompleteResponseAndRaiseERXUDPAsync()
      {
        // TEST status line
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

      var taskUdpReceive = client.UdpReceiveAsync(SkStackKnownPortNumbers.EchonetLite, cts.Token).AsTask();

      Assert.IsFalse(taskUdpReceive.Wait(TimeSpan.FromMilliseconds(100)));

      var taskSendCommand = client.SendCommandAsync(command: "TEST".ToByteSequence());

      Assert.DoesNotThrowAsync(async () => {
        await Task.WhenAll(taskUdpReceive, taskSendCommand.AsTask(), CompleteResponseAndRaiseERXUDPAsync());
      });

      Assert.IsTrue(taskSendCommand.Result.Success);

      using (var result = taskUdpReceive.Result) {
        Assert.That(
          result.Buffer,
          Is.EqualTo("01234567".ToByteSequence()),
          nameof(result)
        );
      }
    }
  }
}