// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Net;
using System.Threading.Tasks;

using NUnit.Framework;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientCommandsSKREJOINTests : SkStackClientTestsBase {
  [Test]
  public void SKREJOIN()
  {
    const string SelfIPv6Address = "FE80:0000:0000:0000:021D:1290:0003:C890";
    const string PaaIPv6Address = "FE80:0000:0000:0000:1034:5678:ABCD:EF01";

    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    async Task RaisePanaSessionEstablishmentEventsAsync()
    {
      stream.ResponseWriter.WriteLine($"EVENT 21 {SelfIPv6Address} 02");
      stream.ResponseWriter.WriteLine($"EVENT 02 {SelfIPv6Address}");
      stream.ResponseWriter.WriteLine($"ERXUDP {SelfIPv6Address} FE80:0000:0000:0000:021D:1290:1234:5678 02CC 02CC 001D129012345678 0 0001 0");

      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.WriteLine($"EVENT 21 {SelfIPv6Address} 00");
      stream.ResponseWriter.WriteLine($"ERXUDP {SelfIPv6Address} FE80:0000:0000:0000:021D:1290:1234:5678 02CC 02CC 001D129012345678 0 0001 0");

      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.Write($"EVENT 2"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine($"5 {PaaIPv6Address}");
    }

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    Exception? thrownExceptionInEventHandler = null;
    var raisedEventCount = 0;

    client.PanaSessionEstablished += (sender, e) => {
      try {
        Assert.That(sender, Is.SameAs(client), nameof(sender));
        Assert.That(e, Is.Not.Null, nameof(e));
        Assert.That(e.PanaSessionPeerAddress, Is.EqualTo(IPAddress.Parse(PaaIPv6Address)), nameof(e.PanaSessionPeerAddress));
        Assert.That(e.EventNumber, Is.EqualTo(SkStackEventNumber.PanaSessionEstablishmentCompleted), nameof(e.EventNumber));
        Assert.That(e.PanaSessionPeerAddress, Is.EqualTo(client.PanaSessionPeerAddress), nameof(client.PanaSessionPeerAddress));
        Assert.That(client.IsPanaSessionAlive, Is.True, nameof(client.IsPanaSessionAlive));
        raisedEventCount++;
      }
      catch (Exception ex) {
        thrownExceptionInEventHandler = ex;
      }
    };

    Assert.That(client.PanaSessionPeerAddress, Is.Null, nameof(client.PanaSessionPeerAddress));
    Assert.That(client.IsPanaSessionAlive, Is.False, nameof(client.IsPanaSessionAlive));

#pragma warning disable CA2012
    var taskSendCommand = client.SendSKREJOINAsync().AsTask();

    Assert.DoesNotThrowAsync(
      async () => await Task.WhenAll(taskSendCommand, RaisePanaSessionEstablishmentEventsAsync())
    );
#pragma warning restore CA2012

    Assert.That(thrownExceptionInEventHandler, Is.Null, nameof(thrownExceptionInEventHandler));
    Assert.That(raisedEventCount, Is.EqualTo(1), nameof(raisedEventCount));

    var (response, rejoinedSessionPeerAddress) = taskSendCommand.Result;

    Assert.That(response.Success, Is.True);
    Assert.That(IPAddress.Parse(PaaIPv6Address), Is.EqualTo(rejoinedSessionPeerAddress));

    Assert.That(IPAddress.Parse(PaaIPv6Address), Is.EqualTo(client.PanaSessionPeerAddress), nameof(client.PanaSessionPeerAddress));
    Assert.That(client.IsPanaSessionAlive, Is.True, nameof(client.IsPanaSessionAlive));

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKREJOIN\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKREJOIN_FailedByEVENT24()
  {
    const string SelfIPv6Address = "FE80:0000:0000:0000:021D:1290:1234:5678";

    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    async Task RaisePanaSessionEstablishmentEventsAsync()
    {
      stream.ResponseWriter.WriteLine($"EVENT 21 {SelfIPv6Address} 02");
      stream.ResponseWriter.WriteLine($"EVENT 02 {SelfIPv6Address}");
      stream.ResponseWriter.WriteLine($"ERXUDP {SelfIPv6Address} FE80:0000:0000:0000:021D:1290:1234:5678 02CC 02CC 001D129012345678 0 0001 0");

      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.WriteLine($"EVENT 21 {SelfIPv6Address} 00");
      stream.ResponseWriter.WriteLine($"ERXUDP {SelfIPv6Address} FE80:0000:0000:0000:021D:1290:1234:5678 02CC 02CC 001D129012345678 0 0001 0");

      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.Write($"EVEN"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.Write($"T 2"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine($"4 {SelfIPv6Address}");
    }

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    var raisedEventCount = 0;

    client.PanaSessionEstablished += (sender, e) => raisedEventCount++;

    Assert.That(client.PanaSessionPeerAddress, Is.Null, nameof(client.PanaSessionPeerAddress));
    Assert.That(client.IsPanaSessionAlive, Is.False, nameof(client.IsPanaSessionAlive));

#pragma warning disable CA2012
    var taskSendCommand = client.SendSKREJOINAsync().AsTask();

    var ex = Assert.ThrowsAsync<SkStackPanaSessionEstablishmentException>(
      async () => await Task.WhenAll(taskSendCommand, RaisePanaSessionEstablishmentEventsAsync())
    );
#pragma warning restore CA2012

    Assert.That(ex!.EventNumber, Is.EqualTo(SkStackEventNumber.PanaSessionEstablishmentError));
    Assert.That(ex.Address, Is.EqualTo(IPAddress.Parse(SelfIPv6Address)));

    Assert.That(raisedEventCount, Is.EqualTo(0), nameof(raisedEventCount));

    Assert.That(client.PanaSessionPeerAddress, Is.Null, nameof(client.PanaSessionPeerAddress));
    Assert.That(client.IsPanaSessionAlive, Is.False, nameof(client.IsPanaSessionAlive));

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKREJOIN\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKREJOIN_Fail()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("FAIL ER10");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());
    var raisedEventCount = 0;

    client.PanaSessionEstablished += (sender, e) => raisedEventCount++;

    Assert.That(client.PanaSessionPeerAddress, Is.Null, nameof(client.PanaSessionPeerAddress));
    Assert.That(client.IsPanaSessionAlive, Is.False, nameof(client.IsPanaSessionAlive));

    var ex = Assert.ThrowsAsync<SkStackErrorResponseException>(async () => await client.SendSKREJOINAsync());

    Assert.That(ex!.ErrorCode, Is.EqualTo(SkStackErrorCode.ER10));

    Assert.That(raisedEventCount, Is.EqualTo(0), nameof(raisedEventCount));

    Assert.That(client.PanaSessionPeerAddress, Is.Null, nameof(client.PanaSessionPeerAddress));
    Assert.That(client.IsPanaSessionAlive, Is.False, nameof(client.IsPanaSessionAlive));

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo("SKREJOIN\r\n".ToByteSequence())
    );
  }
}
