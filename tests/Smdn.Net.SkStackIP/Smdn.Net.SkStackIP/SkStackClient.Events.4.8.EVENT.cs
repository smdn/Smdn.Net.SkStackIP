// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using Is = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Net.SkStackIP;

[TestFixture]
public class SkStackClientEventsEVENTTests : SkStackClientTestsBase {
  private static async Task JoinAsync(
    SkStackClient client,
    PseudoSkStackStream stream,
    string addressString
  )
  {
    var address = IPAddress.Parse(addressString);

    stream.ResponseWriter.WriteLine("OK");
    stream.ResponseWriter.WriteLine($"EVENT 25 {addressString}");

    await client.SendSKJOINAsync(address);

    Assert.AreEqual(client.PanaSessionPeerAddress, address);

    stream.ClearSentData();
  }

  [Test]
  public async Task EVENT_26_RaiseEventPanaSessionTerminated()
  {
    const string addressString = "FE80:0000:0000:0000:021D:1290:1234:5678";
    var address = IPAddress.Parse(addressString);

    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, ServiceProvider);

    await JoinAsync(client, stream, addressString);

    var raisedEventCount = 0;
    Exception thrownExceptionInEventHandler = null;

    client.PanaSessionTerminated += (sender, e) => {
      try {
        Assert.AreSame(client, sender, nameof(sender));
        Assert.IsNotNull(e, nameof(e));
        Assert.AreEqual(address, e.PanaSessionPeerAddress, nameof(e.PanaSessionPeerAddress));
        Assert.AreEqual(SkStackEventNumber.PanaSessionTerminationRequestReceived, e.EventNumber, nameof(e.EventNumber));
        Assert.IsNull(client.PanaSessionPeerAddress, nameof(client.PanaSessionPeerAddress));
        raisedEventCount++;
      }
      catch (Exception ex) {
        thrownExceptionInEventHandler = ex;
      }
    };

    stream.ResponseWriter.WriteLine($"EVENT 26 {addressString}");
    // SKVER
    stream.ResponseWriter.WriteLine("EVER 1.2.10");
    stream.ResponseWriter.WriteLine("OK");

    Assert.IsNotNull(client.PanaSessionPeerAddress, nameof(client.PanaSessionPeerAddress));

    Assert.DoesNotThrowAsync(async () => await client.SendSKVERAsync());

    Assert.IsNull(thrownExceptionInEventHandler, nameof(thrownExceptionInEventHandler));
    Assert.AreEqual(1, raisedEventCount, nameof(raisedEventCount));

    Assert.IsNull(client.PanaSessionPeerAddress, nameof(client.PanaSessionPeerAddress));
  }

  [Test]
  public async Task EVENT_29_RaiseEventPanaSessionExpired()
  {
    const string addressString = "FE80:0000:0000:0000:021D:1290:1234:5678";
    var address = IPAddress.Parse(addressString);

    using var stream = new PseudoSkStackStream();
    using var client = new SkStackClient(stream, ServiceProvider);

    await JoinAsync(client, stream, addressString);

    Exception thrownExceptionInEventHandler = null;
    var raisedEventCount = 0;

    client.PanaSessionExpired += (sender, e) => {
      try {
        Assert.AreSame(client, sender, nameof(sender));
        Assert.IsNotNull(e, nameof(e));
        Assert.AreEqual(address, e.PanaSessionPeerAddress, nameof(e.PanaSessionPeerAddress));
        Assert.AreEqual(SkStackEventNumber.PanaSessionExpired, e.EventNumber, nameof(e.EventNumber));
        Assert.IsNull(client.PanaSessionPeerAddress, nameof(client.PanaSessionPeerAddress));
        raisedEventCount++;
      }
      catch (Exception ex) {
        thrownExceptionInEventHandler = ex;
      }
    };

    stream.ResponseWriter.WriteLine($"EVENT 29 {addressString}");
    // SKVER
    stream.ResponseWriter.WriteLine("EVER 1.2.10");
    stream.ResponseWriter.WriteLine("OK");

    Assert.IsNotNull(client.PanaSessionPeerAddress, nameof(client.PanaSessionPeerAddress));

    Assert.DoesNotThrowAsync(async () => await client.SendSKVERAsync());

    Assert.IsNull(thrownExceptionInEventHandler, nameof(thrownExceptionInEventHandler));
    Assert.AreEqual(1, raisedEventCount, nameof(raisedEventCount));

    Assert.IsNull(client.PanaSessionPeerAddress, nameof(client.PanaSessionPeerAddress));
  }
}
