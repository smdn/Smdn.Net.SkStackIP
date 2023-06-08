// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Net;
using System.Threading.Tasks;

using NUnit.Framework;

using Is = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientCommandsSKJOINTests : SkStackClientTestsBase {
  [Test]
  public void SKJOIN()
  {
    const string addressString = "FE80:0000:0000:0000:021D:1290:1234:5678";
    var address = IPAddress.Parse(addressString);

    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    async Task RaisePanaSessionEstablishmentEventsAsync()
    {
      stream.ResponseWriter.WriteLine($"EVENT 21 {addressString} 02");
      stream.ResponseWriter.WriteLine($"EVENT 02 {addressString}");
      stream.ResponseWriter.WriteLine($"ERXUDP {addressString} FE80:0000:0000:0000:021D:1290:1234:5678 02CC 02CC 001D129012345678 0 0001 0");

      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.Write($"EVENT 21 "); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.Write($"{addressString} 00"); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine();
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.WriteLine($"ERXUDP {addressString} FE80:0000:0000:0000:021D:1290:1234:5678 02CC 02CC 001D129012345678 0 0001 0");
      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.Write($"EVENT "); await Task.Delay(ResponseDelayInterval);
      stream.ResponseWriter.WriteLine($"25 {addressString}");
    }

    using var client = new SkStackClient(stream, ServiceProvider);

    Assert.IsNull(client.PanaSessionPeerAddress, nameof(client.PanaSessionPeerAddress));

    Exception thrownExceptionInEventHandler = null;
    var raisedEventCount = 0;

    client.PanaSessionEstablished += (sender, e) => {
      try {
        Assert.AreSame(client, sender, nameof(sender));
        Assert.IsNotNull(e, nameof(e));
        Assert.AreEqual(address, e.PanaSessionPeerAddress, nameof(e.PanaSessionPeerAddress));
        Assert.AreEqual(SkStackEventNumber.PanaSessionEstablishmentCompleted, e.EventNumber, nameof(e.EventNumber));
        Assert.IsNotNull(client.PanaSessionPeerAddress, nameof(client.PanaSessionPeerAddress));
        Assert.AreEqual(client.PanaSessionPeerAddress, e.PanaSessionPeerAddress, nameof(client.PanaSessionPeerAddress));
        raisedEventCount++;
      }
      catch (Exception ex) {
        thrownExceptionInEventHandler = ex;
      }
    };

#pragma warning disable CA2012
    var taskSendCommand = client.SendSKJOINAsync(address).AsTask();

    Assert.DoesNotThrowAsync(
      async () => await Task.WhenAll(taskSendCommand, RaisePanaSessionEstablishmentEventsAsync())
    );
#pragma warning restore CA2012

    Assert.IsNull(thrownExceptionInEventHandler, nameof(thrownExceptionInEventHandler));
    Assert.AreEqual(1, raisedEventCount, nameof(raisedEventCount));

    Assert.AreEqual(client.PanaSessionPeerAddress, address, nameof(client.PanaSessionPeerAddress));

    var response = taskSendCommand.Result;

    Assert.IsTrue(response.Success);

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKJOIN FE80:0000:0000:0000:021D:1290:1234:5678\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKJOIN_FailedByEVENT24()
  {
    const string addressString = "FE80:0000:0000:0000:021D:1290:1234:5678";
    var address = IPAddress.Parse(addressString);

    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    async Task RaisePanaSessionEstablishmentEventsAsync()
    {
      stream.ResponseWriter.WriteLine($"EVENT 21 {addressString} 02");
      stream.ResponseWriter.WriteLine($"EVENT 02 {addressString}");
      stream.ResponseWriter.WriteLine($"ERXUDP {addressString} FE80:0000:0000:0000:021D:1290:1234:5678 02CC 02CC 001D129012345678 0 0001 0");

      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.WriteLine($"EVENT 21 {addressString} 00");
      stream.ResponseWriter.WriteLine($"ERXUDP {addressString} FE80:0000:0000:0000:021D:1290:1234:5678 02CC 02CC 001D129012345678 0 0001 0");

      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.Write($"EVENT 24 ");

      await Task.Delay(ResponseDelayInterval);

      stream.ResponseWriter.WriteLine($"{addressString}");
    }

    using var client = new SkStackClient(stream, ServiceProvider);

    Assert.IsNull(client.PanaSessionPeerAddress, nameof(client.PanaSessionPeerAddress));

    var raisedEventCount = 0;

    client.PanaSessionEstablished += (sender, e) => raisedEventCount++;

    var taskSendCommand = client.SendSKJOINAsync(address).AsTask();

    var ex = Assert.ThrowsAsync<SkStackPanaSessionEstablishmentException>(
      async () => await Task.WhenAll(taskSendCommand, RaisePanaSessionEstablishmentEventsAsync())
    );

    Assert.AreEqual(SkStackEventNumber.PanaSessionEstablishmentError, ex!.EventNumber);
    Assert.AreEqual(address, ex.Address);

    Assert.AreEqual(0, raisedEventCount, nameof(raisedEventCount));

    Assert.IsNull(client.PanaSessionPeerAddress, nameof(client.PanaSessionPeerAddress));

    Assert.That(
      stream.ReadSentData(),
      Is.EqualTo("SKJOIN FE80:0000:0000:0000:021D:1290:1234:5678\r\n".ToByteSequence())
    );
  }

  [Test]
  public void SKJOIN_AddressNull()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, ServiceProvider);

#pragma warning disable CA2012
    Assert.Throws<ArgumentNullException>(() => client.SendSKJOINAsync(ipv6address: null));
#pragma warning restore CA2012

    Assert.IsEmpty(stream.ReadSentData());
  }

  [Test]
  public void SKJOIN_InvalidAddressFamily()
  {
    var stream = new PseudoSkStackStream();

    stream.ResponseWriter.WriteLine("OK");

    using var client = new SkStackClient(stream, ServiceProvider);

#pragma warning disable CA2012
    Assert.Throws<ArgumentException>(() => client.SendSKJOINAsync(ipv6address: IPAddress.Loopback));
#pragma warning restore CA2012

    Assert.IsEmpty(stream.ReadSentData());
  }
}
