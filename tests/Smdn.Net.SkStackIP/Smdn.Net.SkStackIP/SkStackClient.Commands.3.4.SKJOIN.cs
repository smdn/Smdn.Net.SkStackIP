// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Net;
using System.Threading.Tasks;

using NUnit.Framework;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientCommandsSKJOINTests : SkStackClientTestsBase {
  [Test]
  public void SKJOIN()
  {
    const string SelfIPv6Address = "FE80:0000:0000:0000:021D:1290:0003:C890";
    const string PaaIPv6Address = "FE80:0000:0000:0000:1034:5678:ABCD:EF01";

    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    async Task RaisePanaSessionEstablishmentEventsAsync()
    {
      stream.ResponseWriter.WriteLine($"EVENT 21 {SelfIPv6Address} 02");
      stream.ResponseWriter.WriteLine($"EVENT 02 {SelfIPv6Address}");
      stream.ResponseWriter.WriteLine($"ERXUDP {SelfIPv6Address} {PaaIPv6Address} 02CC 02CC 001D129012345678 0 0001 0");

      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.Write($"EVENT 21 "); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.Write($"{SelfIPv6Address} 00"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine();
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.WriteLine($"ERXUDP {SelfIPv6Address} {PaaIPv6Address} 02CC 02CC 001D129012345678 0 0001 0");
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.Write($"EVENT "); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine($"25 {PaaIPv6Address}");
    }

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.That(client.PanaSessionPeerAddress, Is.Null, nameof(client.PanaSessionPeerAddress));
    Assert.That(client.IsPanaSessionAlive, Is.False, nameof(client.IsPanaSessionAlive));

    Exception? thrownExceptionInEventHandler = null;
    var raisedEventCount = 0;

    client.PanaSessionEstablished += (sender, e) => {
      try {
        Assert.That(sender, Is.SameAs(client), nameof(sender));
        Assert.That(e, Is.Not.Null, nameof(e));
        Assert.That(e.PanaSessionPeerAddress, Is.EqualTo(IPAddress.Parse(PaaIPv6Address)), nameof(e.PanaSessionPeerAddress));
        Assert.That(e.EventNumber, Is.EqualTo(SkStackEventNumber.PanaSessionEstablishmentCompleted), nameof(e.EventNumber));
        Assert.That(client.PanaSessionPeerAddress, Is.Not.Null, nameof(client.PanaSessionPeerAddress));
        Assert.That(e.PanaSessionPeerAddress, Is.EqualTo(client.PanaSessionPeerAddress), nameof(client.PanaSessionPeerAddress));
        Assert.That(client.IsPanaSessionAlive, Is.True, nameof(client.IsPanaSessionAlive));
        raisedEventCount++;
      }
      catch (Exception ex) {
        thrownExceptionInEventHandler = ex;
      }
    };

#pragma warning disable CA2012
    var taskSendCommand = client.SendSKJOINAsync(IPAddress.Parse(PaaIPv6Address)).AsTask();

    Assert.DoesNotThrowAsync(
      async () => await Task.WhenAll(taskSendCommand, RaisePanaSessionEstablishmentEventsAsync())
    );
#pragma warning restore CA2012

    Assert.That(thrownExceptionInEventHandler, Is.Null, nameof(thrownExceptionInEventHandler));
    Assert.That(raisedEventCount, Is.EqualTo(1), nameof(raisedEventCount));

    Assert.That(IPAddress.Parse(PaaIPv6Address), Is.EqualTo(client.PanaSessionPeerAddress), nameof(client.PanaSessionPeerAddress));
    Assert.That(client.IsPanaSessionAlive, Is.True, nameof(client.IsPanaSessionAlive));

    var response = taskSendCommand.Result;

    Assert.That(response.Success, Is.True);

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo($"SKJOIN {PaaIPv6Address}\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKJOIN_FailedByEVENT24()
  {
    const string SelfIPv6Address = "FE80:0000:0000:0000:021D:1290:0003:C890";
    const string PaaIPv6Address = "FE80:0000:0000:0000:1034:5678:ABCD:EF01";

    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    async Task RaisePanaSessionEstablishmentEventsAsync()
    {
      stream.ResponseWriter.WriteLine($"EVENT 21 {SelfIPv6Address} 02");
      stream.ResponseWriter.WriteLine($"EVENT 02 {SelfIPv6Address}");
      stream.ResponseWriter.WriteLine($"ERXUDP {SelfIPv6Address} {PaaIPv6Address} 02CC 02CC 001D129012345678 0 0001 0");

      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.WriteLine($"EVENT 21 {SelfIPv6Address} 00");
      stream.ResponseWriter.WriteLine($"ERXUDP {SelfIPv6Address} {PaaIPv6Address} 02CC 02CC 001D129012345678 0 0001 0");

      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.Write($"EVENT 24 ");

      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.WriteLine($"{SelfIPv6Address}");
    }

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

    Assert.That(client.PanaSessionPeerAddress, Is.Null, nameof(client.PanaSessionPeerAddress));
    Assert.That(client.IsPanaSessionAlive, Is.False, nameof(client.IsPanaSessionAlive));

    var raisedEventCount = 0;

    client.PanaSessionEstablished += (sender, e) => raisedEventCount++;

    var taskSendCommand = client.SendSKJOINAsync(IPAddress.Parse(PaaIPv6Address)).AsTask();

    var ex = Assert.ThrowsAsync<SkStackPanaSessionEstablishmentException>(
      async () => await Task.WhenAll(taskSendCommand, RaisePanaSessionEstablishmentEventsAsync())
    );

    Assert.That(ex!.EventNumber, Is.EqualTo(SkStackEventNumber.PanaSessionEstablishmentError));
    Assert.That(ex.Address, Is.EqualTo(IPAddress.Parse(SelfIPv6Address)));
    Assert.That(ex.PaaAddress, Is.EqualTo(IPAddress.Parse(PaaIPv6Address)));
    Assert.That(ex.Channel, Is.Null);
    Assert.That(ex.PanId, Is.Null);

    Assert.That(raisedEventCount, Is.EqualTo(0), nameof(raisedEventCount));

    Assert.That(client.PanaSessionPeerAddress, Is.Null, nameof(client.PanaSessionPeerAddress));
    Assert.That(client.IsPanaSessionAlive, Is.False, nameof(client.IsPanaSessionAlive));

    Assert.That(
      stream.ReadSentData(),
      SequenceIs.EqualTo($"SKJOIN {PaaIPv6Address}\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKJOIN_AddressNull()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.Throws<ArgumentNullException>(() => client.SendSKJOINAsync(ipv6address: null!));
#pragma warning restore CA2012

    Assert.That(stream.ReadSentData(), Is.Empty);
  }

  [Test]
  public void SKJOIN_InvalidAddressFamily()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, logger: CreateLoggerForTestCase());

#pragma warning disable CA2012
    Assert.Throws<ArgumentException>(() => client.SendSKJOINAsync(ipv6address: IPAddress.Loopback));
#pragma warning restore CA2012

    Assert.That(stream.ReadSentData(), Is.Empty);
  }
}
